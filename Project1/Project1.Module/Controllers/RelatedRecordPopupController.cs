using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.Base;
using Project1.Module.Models.Entities;
using Project1.Module.Models.Enums;

namespace Project1.Module.Controllers
{
    /// <summary>
    /// Müşteri ve kişi detaylarındaki alt listelerin ekleme işlemlerini sayfa geçişi
    /// yerine küçük kayıt pencerelerinde açar.
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
