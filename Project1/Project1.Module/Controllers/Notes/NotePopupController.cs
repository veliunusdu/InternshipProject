using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using Project1.Module.BusinessObjects.Customers;
using Project1.Module.BusinessObjects.Enums;
using Project1.Module.BusinessObjects.Notes;

namespace Project1.Module.Controllers.Notes
{
    /// <summary>
    /// Müşteri detay sayfasındaki "Not Ekle" ve "Kişi Ekle" açılır pencere (popup) butonlarını yönetir.
    /// </summary>
    public sealed class NotePopupController : ObjectViewController<DetailView, Musteri>
    {
        private readonly PopupWindowShowAction notEkleAction;
        private readonly PopupWindowShowAction kisiEkleAction;

        public NotePopupController()
        {
            notEkleAction = new PopupWindowShowAction(this, "MusteriNotEkleAction", PredefinedCategory.View)
            {
                Caption = "Not Ekle",
                ImageName = "Crm_Not",
                TargetObjectType = typeof(Musteri),
                TargetViewType = ViewType.DetailView,
                SelectionDependencyType = SelectionDependencyType.Independent
            };
            notEkleAction.CustomizePopupWindowParams += NotEkleAction_CustomizePopupWindowParams;
            notEkleAction.Execute += NotEkleAction_Execute;

            kisiEkleAction = new PopupWindowShowAction(this, "MusteriKisiEkleAction", PredefinedCategory.View)
            {
                Caption = "Kişi Ekle",
                ImageName = "BO_Person",
                TargetObjectType = typeof(Musteri),
                TargetViewType = ViewType.DetailView,
                SelectionDependencyType = SelectionDependencyType.Independent
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
            }

            DetailView popUpView = Application.CreateDetailView(objectSpace, yeniNot);
            popUpView.ViewEditMode = ViewEditMode.Edit;

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
                    View?.ObjectSpace?.Refresh();
                }
            }
        }

        private void KisiEkleAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            IObjectSpace objectSpace = Application.CreateObjectSpace(typeof(Kisi));

            Kisi yeniKisi = objectSpace.CreateObject<Kisi>();

            if (View.CurrentObject is Musteri seciliMusteri)
            {
                var musteriInSpace = objectSpace.GetObject(seciliMusteri);
                yeniKisi.Musteri = musteriInSpace;
            }

            DetailView popUpView = Application.CreateDetailView(objectSpace, yeniKisi);
            popUpView.ViewEditMode = ViewEditMode.Edit;

            e.View = popUpView;
        }

        private void KisiEkleAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowViewCurrentObject is Kisi)
            {
                IObjectSpace popupObjectSpace = e.PopupWindowView?.ObjectSpace;
                if (popupObjectSpace?.IsModified == true)
                {
                    popupObjectSpace.CommitChanges();
                    View?.ObjectSpace?.Refresh();
                }
            }
        }
    }
}
