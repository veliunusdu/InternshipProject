using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using Project1.Module.Models.Customers;
using Project1.Module.BusinessObjects.Enums;
using Project1.Module.BusinessObjects.NonPersistent;
using Project1.Core.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

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
            IObjectSpace objectSpace = Application.CreateObjectSpace(typeof(CreateNoteParameters));
            CreateNoteParameters parameters = objectSpace.CreateObject<CreateNoteParameters>();
            parameters.Derece = NotDerecesi.Normal;

            if (View.CurrentObject is Kisi seciliKisi)
            {
                parameters.Kisi = objectSpace.GetObject(seciliKisi);
                if (seciliKisi.Musteri != null)
                {
                    parameters.Musteri = objectSpace.GetObject(seciliKisi.Musteri);
                }
            }

            e.View = Application.CreateDetailView(objectSpace, parameters);
        }

        private async void NotEkleAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowView?.CurrentObject is not CreateNoteParameters parameters)
            {
                return;
            }

            var command = new CreateNoteCommand(
                parameters.Baslik,
                parameters.Icerik,
                (int)parameters.Derece,
                parameters.Musteri?.Oid,
                parameters.Kisi?.Oid);

            var mediator = Application.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(command);

            View.ObjectSpace.Refresh();
        }
    }
}
