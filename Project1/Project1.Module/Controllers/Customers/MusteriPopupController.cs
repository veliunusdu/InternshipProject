using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using Project1.Module.BusinessObjects.Customers;
using Project1.Module.BusinessObjects.Enums;
using Project1.Module.BusinessObjects.Notes;

namespace Project1.Module.Controllers.Customers
{
    /// <summary>
    /// Müşteri detay veya liste sayfalarındaki "Not Ekle" ve "Kişi Ekle" açılır pencere (popup) butonlarını yönetir.
    /// </summary>
    public sealed class MusteriPopupController : ObjectViewController<ObjectView, Musteri>
    {
        private readonly PopupWindowShowAction notEkleAction;
        private readonly PopupWindowShowAction kisiEkleAction;

        public MusteriPopupController()
        {
            notEkleAction = new PopupWindowShowAction(this, "MusteriNotEkleAction", PredefinedCategory.RecordEdit)
            {
                Caption = "Not Ekle",
                ImageName = "Crm_Not",
                TargetObjectType = typeof(Musteri),
                TargetViewType = ViewType.Any,
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            notEkleAction.CustomizePopupWindowParams += NotEkleAction_CustomizePopupWindowParams;
            notEkleAction.Execute += NotEkleAction_Execute;

            kisiEkleAction = new PopupWindowShowAction(this, "MusteriKisiEkleAction", PredefinedCategory.RecordEdit)
            {
                Caption = "Kişi Ekle",
                ImageName = "BO_Person",
                TargetObjectType = typeof(Musteri),
                TargetViewType = ViewType.Any,
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            kisiEkleAction.CustomizePopupWindowParams += KisiEkleAction_CustomizePopupWindowParams;
            kisiEkleAction.Execute += KisiEkleAction_Execute;
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
                yeniNot.IsMusteriHidden = true;
            }

            e.View = PopupHelper.CreateEditableDetailView(Application, objectSpace, yeniNot);
        }

        private void NotEkleAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            PopupHelper.CommitAndRefresh(e, View);
        }

        private void KisiEkleAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            IObjectSpace objectSpace = Application.CreateObjectSpace(typeof(Kisi));

            Kisi yeniKisi = objectSpace.CreateObject<Kisi>();

            if (View.CurrentObject is Musteri seciliMusteri)
            {
                var musteriInSpace = objectSpace.GetObject(seciliMusteri);
                yeniKisi.Musteri = musteriInSpace;
                yeniKisi.IsMusteriHidden = true;
            }

            e.View = PopupHelper.CreateEditableDetailView(Application, objectSpace, yeniKisi);
        }

        private void KisiEkleAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            PopupHelper.CommitAndRefresh(e, View);
        }
    }
}
