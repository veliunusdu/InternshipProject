using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;
using Project1.Module.Models.Customers;
using Project1.Module.Models.Notes;
using Project1.Core.Enums;
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
            Type paramType = View.ObjectTypeInfo.Type == typeof(Kisi) ? typeof(CreateKisiParameters) : typeof(Not);
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
            else if (newObject is Not yeniNot)
            {
                yeniNot.Derece = NotDerecesi.Normal;
                
                if (masterObject is Musteri musteri)
                {
                    yeniNot.Musteri = popupObjectSpace.GetObject(musteri);
                    yeniNot.IsMusteriHidden = true;
                }
                else if (masterObject is Kisi kisi)
                {
                    yeniNot.Kisi = popupObjectSpace.GetObject(kisi);
                    if (kisi.Musteri != null)
                    {
                        yeniNot.Musteri = popupObjectSpace.GetObject(kisi.Musteri);
                    }
                    yeniNot.IsKisiHidden = true;
                    yeniNot.IsMusteriHidden = true;
                }
            }

            e.View = Application.CreateDetailView(popupObjectSpace, newObject);
        }

        private async void PopupEkleAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowView.CurrentObject is CreateKisiParameters kisiParams)
            {
                var mediator = Application.ServiceProvider.GetRequiredService<IMediator>();
                var command = new CreateKisiCommand(
                    kisiParams.Ad, 
                    kisiParams.Soyad, 
                    kisiParams.Email,
                    kisiParams.Telefon,
                    kisiParams.Musteri?.Oid);
                await mediator.Send(command);
            }
            else if (e.PopupWindowView.CurrentObject is Not)
            {
                e.PopupWindowView.ObjectSpace.CommitChanges();
            }

            View.ObjectSpace.Refresh();
        }
    }
}
