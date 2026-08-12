using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.Base;
using Project1.Module.Models.Entities;
using Project1.Module.Models.Enums;
using Project1.Module.Services;

namespace Project1.Module.Controllers
{
    /// <summary>
    /// Not nesnesi kaydedildiğinde tetiklenir; ilgili Kişi'ye otomatik HTML e-posta bildirimi gönderir.
    /// </summary>
    public sealed class NotEmailNotificationController : ObjectViewController<ObjectView, Not>
    {
        private static readonly Dictionary<string, DateTime> _recentlySentNoteKeys = new Dictionary<string, DateTime>();
        private bool _showToastNotification = false;
        private bool _emailPermissionDenied = false;

        protected override void OnActivated()
        {
            base.OnActivated();
            ObjectSpace.Committing += ObjectSpace_Committing;
            ObjectSpace.Committed += ObjectSpace_Committed;
        }

        protected override void OnDeactivated()
        {
            ObjectSpace.Committing -= ObjectSpace_Committing;
            ObjectSpace.Committed -= ObjectSpace_Committed;
            base.OnDeactivated();
        }

        private void ObjectSpace_Committing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _showToastNotification = false;
            _emailPermissionDenied = false;

            bool canSendEmail = true;
            object currentUserId = Application?.Security?.UserId;
            if (currentUserId != null && ObjectSpace.GetObjectByKey<ApplicationUser>(currentUserId) is ApplicationUser currentUser)
            {
                if (currentUser.UserName == "Admin")
                {
                    canSendEmail = true;
                }
                else
                {
                    canSendEmail = currentUser.CanSendEmailOnNoteCreation;
                }
            }

            if (!canSendEmail)
            {
                return;
            }

            var pendingNotes = ObjectSpace.ModifiedObjects
                .OfType<Not>()
                .Where(n => !n.IsEmailSent)
                .ToList();

            if (pendingNotes.Count > 0 && !CanCurrentUserSendEmail())
            {
                _emailPermissionDenied = true;
                return;
            }

            foreach (var note in pendingNotes)
            {
                Kisi recipient = note.Kisi;
                if (recipient == null || string.IsNullOrWhiteSpace(recipient.Email))
                    continue;

                string deduplicationKey = $"{note.Oid}_{recipient.Email.ToLowerInvariant()}";

                lock (_recentlySentNoteKeys)
                {
                    var expiredKeys = _recentlySentNoteKeys
                        .Where(kvp => (DateTime.UtcNow - kvp.Value).TotalSeconds > 15)
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (var key in expiredKeys)
                    {
                        _recentlySentNoteKeys.Remove(key);
                    }

                    if (_recentlySentNoteKeys.ContainsKey(deduplicationKey))
                    {
                        continue;
                    }

                    _recentlySentNoteKeys[deduplicationKey] = DateTime.UtcNow;
                }

                var (success, errorMessage) = EmailService.SendNoteNotificationEmail(
                    recipient.Email,
                    recipient.AdSoyad,
                    note.Baslik,
                    note.Icerik,
                    note.Derece.ToString(),
                    note.Musteri?.Ad);

                if (success)
                {
                    note.IsEmailSent = true;
                    _showToastNotification = true;
                }
                else
                {
                    Application?.ShowViewStrategy?.ShowMessage(new MessageOptions
                    {
                        Message = $"E-posta gönderilemedi ({recipient.Email}): {errorMessage}",
                        Type = InformationType.Warning,
                        Duration = 8000
                    });
                }
            }
        }

        private void ObjectSpace_Committed(object sender, EventArgs e)
        {
            if (_emailPermissionDenied)
            {
                _emailPermissionDenied = false;
                Application?.ShowViewStrategy?.ShowMessage(new MessageOptions
                {
                    Message = "Not kaydedildi; e-posta gönderme yetkiniz kapalı olduğu için bildirim gönderilmedi.",
                    Type = InformationType.Warning,
                    Duration = 5000
                });
            }

            if (_showToastNotification)
            {
                _showToastNotification = false;
                Application?.ShowViewStrategy?.ShowMessage(new MessageOptions
                {
                    Message = "Not bu kişiye e-posta olarak gönderildi.",
                    Type = InformationType.Success,
                    Duration = 2000
                });
            }
        }

        private bool CanCurrentUserSendEmail()
        {
            ISecurityStrategyBase security = Application?.Security;
            if (security?.IsAuthenticated != true || security.UserId == null)
            {
                return false;
            }

            if (string.Equals(
                security.UserName,
                Security.SecurityConstants.AdministratorUserName,
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (Application == null)
            {
                return false;
            }

            INonSecuredObjectSpaceFactory objectSpaceFactory = Application.ServiceProvider?
                .GetService(typeof(INonSecuredObjectSpaceFactory)) as INonSecuredObjectSpaceFactory;

            if (objectSpaceFactory != null)
            {
                using IObjectSpace permissionObjectSpace =
                    objectSpaceFactory.CreateNonSecuredObjectSpace(typeof(UserEmailPermission));
                return GetEmailPermission(permissionObjectSpace, security.UserId);
            }

            using IObjectSpace fallbackObjectSpace =
                Application.CreateObjectSpace(typeof(UserEmailPermission));
            return GetEmailPermission(fallbackObjectSpace, security.UserId);
        }

        private static bool GetEmailPermission(IObjectSpace objectSpace, object userId)
        {
            UserEmailPermission permission = objectSpace.FindObject<UserEmailPermission>(
                CriteriaOperator.Parse("User.Oid = ?", userId));
            return permission?.CanSendEmail == true;
        }
    }

    /// <summary>
    /// Müşteri detay sayfasındaki "Not Ekle" açılır pencere (popup) butonunu yönetir.
    /// </summary>
    public sealed class NotDetailPopupController : ObjectViewController<DetailView, Musteri>
    {
        private readonly PopupWindowShowAction notEkleAction;

        public NotDetailPopupController()
        {
            notEkleAction = new PopupWindowShowAction(this, "MusteriNotEkleAction", PredefinedCategory.View)
            {
                Caption = "Not Ekle",
                ImageName = "Action_New",
                TargetObjectType = typeof(Musteri),
                TargetViewType = ViewType.DetailView,
                SelectionDependencyType = SelectionDependencyType.Independent
            };
            notEkleAction.CustomizePopupWindowParams += NotEkleAction_CustomizePopupWindowParams;
            notEkleAction.Execute += NotEkleAction_Execute;
        }

        private void NotEkleAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            IObjectSpace objectSpace = Application.CreateObjectSpace(typeof(Not));

            Not yeniNot = objectSpace.CreateObject<Not>();
            yeniNot.Derece = NotDerecesi.Normal;

            if (View.CurrentObject is Musteri seciliMusteri)
            {
                var musteriInSpace = objectSpace.GetObject(seciliMusteri);
                yeniNot.Musteri = musteriInSpace;
            }

            DetailView popUpView = Application.CreateDetailView(objectSpace, yeniNot);
            popUpView.ViewEditMode = ViewEditMode.Edit;
            if (popUpView.FindItem(nameof(Not.Musteri)) is IAppearanceVisibility musteriEditor)
            {
                musteriEditor.Visibility = ViewItemVisibility.Hide;
            }

            e.View = popUpView;
        }

        private void NotEkleAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowViewCurrentObject is Not)
            {
                IObjectSpace popupObjectSpace = e.PopupWindowView?.ObjectSpace;
                if (popupObjectSpace?.IsModified == true)
                {
                    popupObjectSpace.CommitChanges();
                }

                View.ObjectSpace?.Refresh();
            }
        }
    }

    /// <summary>
    /// Müşteri ve kişi detaylarındaki alt listelerin ekleme işlemlerini sayfa geçişi
    /// yerine küçük kayıt pencerelerinde (popup) açar.
    /// </summary>
    public sealed class RelatedRecordPopupController : ViewController<ListView>
    {
        private const string MusteriKisilerListViewId = "Musteri_Kisiler_ListView";
        private const string MusteriNotlarListViewId = "Musteri_Notlar_ListView";
        private const string KisiNotlarListViewId = "Kisi_Notlar_ListView";

        private readonly PopupWindowShowAction kisiEkleAction;
        private readonly PopupWindowShowAction musteriNotEkleAction;
        private readonly PopupWindowShowAction kisiNotEkleAction;

        public RelatedRecordPopupController()
        {
            kisiEkleAction = CreatePopupAction("MusteriDetayKisiEkle", "Kişi Ekle", typeof(Kisi), MusteriKisilerListViewId);
            kisiEkleAction.CustomizePopupWindowParams += KisiEkleAction_CustomizePopupWindowParams;
            kisiEkleAction.Execute += PopupAction_Execute;

            musteriNotEkleAction = CreatePopupAction("MusteriDetayNotEkle", "Not Ekle", typeof(Not), MusteriNotlarListViewId);
            musteriNotEkleAction.CustomizePopupWindowParams += MusteriNotEkleAction_CustomizePopupWindowParams;
            musteriNotEkleAction.Execute += PopupAction_Execute;

            kisiNotEkleAction = CreatePopupAction("KisiDetayNotEkle", "Not Ekle", typeof(Not), KisiNotlarListViewId);
            kisiNotEkleAction.CustomizePopupWindowParams += KisiNotEkleAction_CustomizePopupWindowParams;
            kisiNotEkleAction.Execute += PopupAction_Execute;
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            NewObjectViewController newObjectController = Frame.GetController<NewObjectViewController>();
            if (newObjectController != null && IsRelatedRecordListView(View.Id))
            {
                newObjectController.NewObjectAction.Active.SetItemValue(nameof(RelatedRecordPopupController), false);
            }
        }

        protected override void OnDeactivated()
        {
            NewObjectViewController newObjectController = Frame.GetController<NewObjectViewController>();
            newObjectController?.NewObjectAction.Active.RemoveItem(nameof(RelatedRecordPopupController));
            base.OnDeactivated();
        }

        private PopupWindowShowAction CreatePopupAction(string id, string caption, Type objectType, string targetViewId)
        {
            return new PopupWindowShowAction(this, id, PredefinedCategory.Edit)
            {
                Caption = caption,
                ImageName = "Action_New",
                TargetObjectType = objectType,
                TargetViewType = ViewType.ListView,
                TargetViewNesting = Nesting.Nested,
                TargetViewId = targetViewId
            };
        }

        private void KisiEkleAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            IObjectSpace objectSpace = Application.CreateObjectSpace(typeof(Kisi));
            Kisi yeniKisi = objectSpace.CreateObject<Kisi>();

            if (View.CollectionSource is PropertyCollectionSource collectionSource && collectionSource.MasterObject is Musteri musteri)
            {
                yeniKisi.Musteri = objectSpace.GetObject(musteri);
            }

            DetailView popUpView = Application.CreateDetailView(objectSpace, yeniKisi);
            popUpView.ViewEditMode = ViewEditMode.Edit;
            if (popUpView.FindItem(nameof(Kisi.Musteri)) is IAppearanceVisibility musteriEditor)
            {
                musteriEditor.Visibility = ViewItemVisibility.Hide;
            }
            e.View = popUpView;
        }

        private void MusteriNotEkleAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            IObjectSpace objectSpace = Application.CreateObjectSpace(typeof(Not));
            Not yeniNot = objectSpace.CreateObject<Not>();
            yeniNot.Derece = NotDerecesi.Normal;

            if (View.CollectionSource is PropertyCollectionSource collectionSource && collectionSource.MasterObject is Musteri musteri)
            {
                yeniNot.Musteri = objectSpace.GetObject(musteri);
            }

            DetailView popUpView = CreateEditableDetailView(objectSpace, yeniNot);
            HideEditor(popUpView, nameof(Not.Musteri));
            e.View = popUpView;
        }

        private void KisiNotEkleAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            IObjectSpace objectSpace = Application.CreateObjectSpace(typeof(Not));
            Not yeniNot = objectSpace.CreateObject<Not>();
            yeniNot.Derece = NotDerecesi.Normal;

            if (View.CollectionSource is PropertyCollectionSource collectionSource &&
                collectionSource.MasterObject is Kisi kisi)
            {
                yeniNot.Kisi = objectSpace.GetObject(kisi);
                yeniNot.Musteri = objectSpace.GetObject(kisi.Musteri);
            }

            DetailView popUpView = CreateEditableDetailView(objectSpace, yeniNot);
            HideEditor(popUpView, nameof(Not.Musteri));
            HideEditor(popUpView, nameof(Not.Kisi));
            e.View = popUpView;
        }

        private DetailView CreateEditableDetailView(IObjectSpace objectSpace, object currentObject)
        {
            DetailView detailView = Application.CreateDetailView(objectSpace, currentObject);
            detailView.ViewEditMode = ViewEditMode.Edit;
            return detailView;
        }

        private static void HideEditor(DetailView detailView, string itemId)
        {
            if (detailView.FindItem(itemId) is IAppearanceVisibility editor)
            {
                editor.Visibility = ViewItemVisibility.Hide;
            }
        }

        private void PopupAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            IObjectSpace popupObjectSpace = e.PopupWindowView?.ObjectSpace;
            if (popupObjectSpace?.IsModified == true)
            {
                popupObjectSpace.CommitChanges();
            }

            View.ObjectSpace.Refresh();
        }

        private static bool IsRelatedRecordListView(string viewId)
        {
            return viewId == MusteriKisilerListViewId ||
                   viewId == MusteriNotlarListViewId ||
                   viewId == KisiNotlarListViewId;
        }
    }
}
