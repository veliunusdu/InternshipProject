using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;
using Project1.Module.BusinessObjects.Customers;
using Project1.Module.BusinessObjects.Notes;
using Project1.Module.BusinessObjects.Enums;
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
            IObjectSpace popupObjectSpace = Application.CreateObjectSpace(View.ObjectTypeInfo.Type);
            object newObject = popupObjectSpace.CreateObject(View.ObjectTypeInfo.Type);

            // Master objeyi al (Müşteri veya Kişi)
            PropertyCollectionSource collectionSource = View.CollectionSource as PropertyCollectionSource;
            object masterObject = collectionSource?.MasterObject;

            if (newObject is Kisi yeniKisi)
            {
                if (masterObject is Musteri musteri)
                {
                    yeniKisi.Musteri = popupObjectSpace.GetObject(musteri);
                }
                yeniKisi.IsMusteriHidden = true;
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

            e.View = PopupHelper.CreateEditableDetailView(Application, popupObjectSpace, newObject);
        }

        private void PopupEkleAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            PopupHelper.CommitAndRefresh(e, View);
        }
    }
}
