using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using Project1.Module.BusinessObjects.Customers;
using Project1.Module.BusinessObjects.Enums;
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
            IObjectSpace objectSpace = Application.CreateObjectSpace(typeof(CreateNoteParameters));
            CreateNoteParameters parameters = objectSpace.CreateObject<CreateNoteParameters>();
            parameters.Derece = NotDerecesi.Normal;

            if (View.CurrentObject is Musteri seciliMusteri)
            {
                // We keep a reference to pass the ID later
                parameters.Musteri = objectSpace.GetObject(seciliMusteri);
            }

            e.View = Application.CreateDetailView(objectSpace, parameters);
        }

        private void NotEkleAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            var parameters = (CreateNoteParameters)e.PopupWindowView.CurrentObject;
            
            var command = new CreateNoteCommand(
                parameters.Baslik, 
                parameters.Icerik, 
                (int)parameters.Derece, 
                parameters.Musteri?.Oid, 
                parameters.Kisi?.Oid);

            var mediator = Application.ServiceProvider.GetRequiredService<IMediator>();
            mediator.Send(command).Wait();

            View.ObjectSpace.Refresh();
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

        private void KisiEkleAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            var parameters = (CreateKisiParameters)e.PopupWindowView.CurrentObject;

            var command = new CreateKisiCommand(
                parameters.Ad, 
                parameters.Soyad, 
                parameters.Email,
                parameters.Telefon,
                parameters.Musteri?.Oid);

            var mediator = Application.ServiceProvider.GetRequiredService<IMediator>();
            mediator.Send(command).Wait();

            View.ObjectSpace.Refresh();
        }
    }
}
