using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;
using Project1.Module.BusinessObjects.Customers;
using Project1.Module.BusinessObjects.Notes;
using Project1.Module.BusinessObjects.Enums;
using Project1.Module.BusinessObjects.NonPersistent;
using Project1.Core.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using DevExpress.Persistent.Base;

namespace Project1.Module.Controllers.Customers
{
    public class NestedListPopupController : ViewController<ListView>
    {
        private PopupWindowShowAction popupEkleAction;

        public NestedListPopupController()
        {
            TargetViewNesting = Nesting.Nested;
            
            popupEkleAction = new PopupWindowShowAction(this, "NestedListPopupEkleAction", PredefinedCategory.ObjectsCreation)
            {
                Caption = "Ekle",
                ImageName = "Action_New"
            };
            popupEkleAction.CustomizePopupWindowParams += PopupEkleAction_CustomizePopupWindowParams;
            popupEkleAction.Execute += PopupEkleAction_Execute;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            
            // Sadece Kisi ve Not listelerinde aktif olsun
            if (View.ObjectTypeInfo.Type != typeof(Kisi) && View.ObjectTypeInfo.Type != typeof(Not))
            {
                popupEkleAction.Active.SetItemValue("ValidType", false);
                return;
            }
            else
            {
                popupEkleAction.Active.RemoveItem("ValidType");
            }

            // Varsayılan New butonunu devre dışı bırak
            var newObjectController = Frame.GetController<NewObjectViewController>();
            if (newObjectController != null)
            {
                newObjectController.NewObjectAction.Active.SetItemValue("ReplacedByPopup", false);
            }
        }

        protected override void OnDeactivated()
        {
            var newObjectController = Frame.GetController<NewObjectViewController>();
            if (newObjectController != null)
            {
                newObjectController.NewObjectAction.Active.RemoveItem("ReplacedByPopup");
            }
            base.OnDeactivated();
        }

        private void PopupEkleAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            Type paramType = View.ObjectTypeInfo.Type == typeof(Kisi) ? typeof(CreateKisiParameters) : typeof(CreateNoteParameters);
            IObjectSpace popupObjectSpace = Application.CreateObjectSpace(paramType);
            object newObject = popupObjectSpace.CreateObject(paramType);

            // Master objeyi al (Müşteri veya Kişi)
            PropertyCollectionSource collectionSource = View.CollectionSource as PropertyCollectionSource;
            object masterObject = collectionSource?.MasterObject;

            if (newObject is CreateKisiParameters kisiParams)
            {
                if (masterObject is Musteri musteri)
                {
                    kisiParams.Musteri = popupObjectSpace.GetObject(musteri);
                }
            }
            else if (newObject is CreateNoteParameters noteParams)
            {
                noteParams.Derece = NotDerecesi.Normal;
                
                if (masterObject is Musteri musteri)
                {
                    noteParams.Musteri = popupObjectSpace.GetObject(musteri);
                }
                else if (masterObject is Kisi kisi)
                {
                    noteParams.Kisi = popupObjectSpace.GetObject(kisi);
                    if (kisi.Musteri != null)
                    {
                        noteParams.Musteri = popupObjectSpace.GetObject(kisi.Musteri);
                    }
                }
            }

            e.View = Application.CreateDetailView(popupObjectSpace, newObject);
        }

        private void PopupEkleAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            var mediator = Application.ServiceProvider.GetRequiredService<IMediator>();

            if (e.PopupWindowView.CurrentObject is CreateKisiParameters kisiParams)
            {
                var command = new CreateKisiCommand(
                    kisiParams.Ad, 
                    kisiParams.Soyad, 
                    kisiParams.Email,
                    kisiParams.Telefon,
                    kisiParams.Musteri?.Oid);
                mediator.Send(command).Wait();
            }
            else if (e.PopupWindowView.CurrentObject is CreateNoteParameters noteParams)
            {
                var command = new CreateNoteCommand(
                    noteParams.Baslik, 
                    noteParams.Icerik, 
                    (int)noteParams.Derece, 
                    noteParams.Musteri?.Oid, 
                    noteParams.Kisi?.Oid);
                mediator.Send(command).Wait();
            }

            View.ObjectSpace.Refresh();
        }
    }
}
