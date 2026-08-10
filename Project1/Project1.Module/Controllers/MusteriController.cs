using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.Base;
using System.ComponentModel;
using Project1.Module.Models.Entities;
using Project1.Module.Models.Enums;

namespace Project1.Module.Controllers
{
    public class MusteriController : ViewController
    {
        private readonly PopupWindowShowAction kisiEkleAction;
        private readonly PopupWindowShowAction notEkleAction;
        private NewObjectViewController _newObjectViewController;

        public MusteriController()
        {
            kisiEkleAction = new PopupWindowShowAction(this, "MusteriKisiEkleAction", PredefinedCategory.View)
            {
                Caption = "Kişi Ekle",
                ImageName = "Action_New",
                TargetObjectType = typeof(Musteri),
                TargetViewType = ViewType.DetailView
            };
            kisiEkleAction.CustomizePopupWindowParams += KisiEkleAction_CustomizePopupWindowParams;
            kisiEkleAction.Execute += KisiEkleAction_Execute;

            notEkleAction = new PopupWindowShowAction(this, "MusteriNotEkleAction", PredefinedCategory.View)
            {
                Caption = "Not Ekle",
                ImageName = "Action_New",
                TargetObjectType = typeof(Musteri),
                TargetViewType = ViewType.DetailView,
                SelectionDependencyType = SelectionDependencyType.Independent
            };
            notEkleAction.CustomizePopupWindowParams += NotEkleAction_CustomizePopupWindowParams;
            notEkleAction.Execute += NotEkleAction_Execute;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            ObjectSpace.Committed += ObjectSpace_Committed;

            _newObjectViewController = Frame.GetController<NewObjectViewController>();
            if (_newObjectViewController != null)
            {
                _newObjectViewController.NewObjectAction.Executing += NewObjectAction_Executing;
            }
        }

        protected override void OnDeactivated()
        {
            ObjectSpace.Committed -= ObjectSpace_Committed;

            if (_newObjectViewController != null)
            {
                _newObjectViewController.NewObjectAction.Executing -= NewObjectAction_Executing;
            }

            base.OnDeactivated();
        }

        private void NewObjectAction_Executing(object sender, CancelEventArgs e)
        {
            if (View?.Id == "Musteri_Notlar_ListView")
            {
                e.Cancel = true;

                if (View.ObjectSpace?.Owner is DetailView parentDetailView && parentDetailView.CurrentObject is Musteri parentMusteri)
                {
                    IObjectSpace objectSpace = Application.CreateObjectSpace(typeof(Not));
                    Not yeniNot = objectSpace.CreateObject<Not>();
                    yeniNot.Derece = NotDerecesi.Normal;
                    yeniNot.Musteri = objectSpace.GetObject(parentMusteri);

                    DetailView popUpView = Application.CreateDetailView(objectSpace, yeniNot);
                    popUpView.ViewEditMode = ViewEditMode.Edit;

                    ShowViewParameters svp = new ShowViewParameters(popUpView)
                    {
                        TargetWindow = TargetWindow.NewModalWindow,
                        Context = TemplateContext.PopupWindow
                    };

                    DialogController dialogController = Application.CreateController<DialogController>();
                    dialogController.AcceptAction.Execute += (s, args) =>
                    {
                        if (objectSpace.IsModified)
                        {
                            objectSpace.CommitChanges();
                        }
                        if (parentDetailView.ObjectSpace != null)
                        {
                            parentDetailView.ObjectSpace.Refresh();
                        }
                    };
                    svp.Controllers.Add(dialogController);

                    Application.ShowViewStrategy.ShowView(svp, new ShowViewSource(Frame, null));
                }
            }
        }

        private void ObjectSpace_Committed(object sender, System.EventArgs e)
        {
            try
            {
                if (View is ListView listView && listView.CollectionSource != null)
                {
                    listView.CollectionSource.Reload();
                }
                else if (View != null && View.ObjectSpace != null)
                {
                    View.ObjectSpace.Refresh();
                }
            }
            catch
            {
                // Fallback
            }
        }

        private void KisiEkleAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            IObjectSpace nestedObjectSpace = View.ObjectSpace.CreateNestedObjectSpace();
            Kisi yeniKisi = nestedObjectSpace.CreateObject<Kisi>();

            if (View.CurrentObject is Musteri mevcutMusteri)
            {
                yeniKisi.Musteri = nestedObjectSpace.GetObject(mevcutMusteri);
            }

            DetailView popUpView = Application.CreateDetailView(nestedObjectSpace, yeniKisi);
            popUpView.ViewEditMode = ViewEditMode.Edit;
            e.View = popUpView;
        }

        private void KisiEkleAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowViewCurrentObject is Kisi yeniKisi)
            {
                IObjectSpace popupObjectSpace = e.PopupWindowView.ObjectSpace;
                if (popupObjectSpace != null && popupObjectSpace.IsModified)
                {
                    popupObjectSpace.CommitChanges();
                }

                if (View != null && View.ObjectSpace != null)
                {
                    if (View.ObjectSpace.IsModified)
                    {
                        View.ObjectSpace.CommitChanges();
                    }
                    View.ObjectSpace.Refresh();
                }
            }
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
            if (e.PopupWindowViewCurrentObject is Not yeniNot)
            {
                IObjectSpace popupObjectSpace = e.PopupWindowView.ObjectSpace;
                if (popupObjectSpace != null)
                {
                    if (popupObjectSpace.IsModified)
                    {
                        popupObjectSpace.CommitChanges();
                    }
                }

                if (View is ListView listView && listView.CollectionSource != null)
                {
                    listView.CollectionSource.Reload();
                }
                else if (View != null && View.ObjectSpace != null)
                {
                    View.ObjectSpace.Refresh();
                }
            }
        }
    }
}
