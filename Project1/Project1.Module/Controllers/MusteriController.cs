using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using Project1.Module.Models.Entities;
using Project1.Module.Models.Enums;

namespace Project1.Module.Controllers
{
    public sealed class MusteriController : ObjectViewController<DetailView, Musteri>
    {
        private readonly PopupWindowShowAction notEkleAction;

        public MusteriController()
        {
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
            HideMusteriEditor(popUpView);

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

                if (View.ObjectSpace != null)
                {
                    View.ObjectSpace.Refresh();
                }
            }
        }

        private static void HideMusteriEditor(DetailView detailView)
        {
            if (detailView.FindItem(nameof(Not.Musteri)) is IAppearanceVisibility musteriEditor)
            {
                musteriEditor.Visibility = ViewItemVisibility.Hide;
            }
        }
    }
}
