using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using Project1.Module.Models.Customers;
using Project1.Module.Models.Notes;
using Project1.Core.Enums;

namespace Project1.Module.Controllers.Customers
{
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
                yeniNot.Kisi = objectSpace.GetObject(seciliKisi);
                if (seciliKisi.Musteri != null)
                {
                    yeniNot.Musteri = objectSpace.GetObject(seciliKisi.Musteri);
                }
                yeniNot.IsKisiHidden = true;
                yeniNot.IsMusteriHidden = true;
            }

            e.View = Application.CreateDetailView(objectSpace, yeniNot);
        }

        private void NotEkleAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowView?.CurrentObject is Not)
            {
                e.PopupWindowView.ObjectSpace.CommitChanges();
                View.ObjectSpace.Refresh();
            }
        }
    }
}
