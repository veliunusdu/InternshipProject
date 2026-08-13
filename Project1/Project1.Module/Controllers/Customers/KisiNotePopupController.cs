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
    /// Kişi detay sayfasından veya kişi listesinden "Not Ekle" popup işlemini yönetir.
    /// </summary>
    public sealed class KisiNotePopupController : ObjectViewController<ObjectView, Kisi>
    {
        private readonly PopupWindowShowAction notEkleAction;

        public KisiNotePopupController()
        {
            notEkleAction = new PopupWindowShowAction(this, "KisiNotEkleAction", PredefinedCategory.RecordEdit)
            {
                Caption = "Not Ekle",
                ImageName = "Crm_Not",
                TargetObjectType = typeof(Kisi),
                TargetViewType = ViewType.Any,
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            notEkleAction.CustomizePopupWindowParams += NotEkleAction_CustomizePopupWindowParams;
            notEkleAction.Execute += NotEkleAction_Execute;
        }

        private void NotEkleAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            IObjectSpace objectSpace = Application.CreateObjectSpace(typeof(Not));
            Not yeniNot = objectSpace.CreateObject<Not>();
            yeniNot.Derece = NotDerecesi.Normal;

            if (View.CurrentObject is Kisi seciliKisi)
            {
                var kisiInSpace = objectSpace.GetObject(seciliKisi);
                yeniNot.Kisi = kisiInSpace;
                
                if (kisiInSpace.Musteri != null)
                {
                    yeniNot.Musteri = kisiInSpace.Musteri;
                }
                
                yeniNot.IsMusteriHidden = true;
                yeniNot.IsKisiHidden = true;
            }

            e.View = PopupHelper.CreateEditableDetailView(Application, objectSpace, yeniNot);
        }

        private void NotEkleAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            PopupHelper.CommitAndRefresh(e, View);
        }
    }
}
