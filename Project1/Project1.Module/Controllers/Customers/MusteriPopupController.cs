using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using Project1.Module.Models.Customers;
using Project1.Module.Models.Notes;
using Project1.Core.Enums;
using Project1.Module.BusinessObjects.NonPersistent;
using Project1.Core.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Project1.Module.Controllers.Customers
{
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
                yeniNot.Musteri = objectSpace.GetObject(seciliMusteri);
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

        private void KisiEkleAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            IObjectSpace objectSpace = Application.CreateObjectSpace(typeof(CreateKisiParameters));
            CreateKisiParameters parameters = objectSpace.CreateObject<CreateKisiParameters>();

            if (View.CurrentObject is Musteri seciliMusteri)
            {
                parameters.Musteri = objectSpace.GetObject(seciliMusteri);
            }

            e.View = Application.CreateDetailView(objectSpace, parameters);
        }

        private async void KisiEkleAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowView?.CurrentObject is not CreateKisiParameters parameters)
            {
                return;
            }

            var command = new CreateKisiCommand(
                parameters.Ad, 
                parameters.Soyad, 
                parameters.Email,
                parameters.Telefon,
                parameters.Musteri?.Oid);

            var mediator = Application.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(command);

            View.ObjectSpace.Refresh();
        }
    }
}
