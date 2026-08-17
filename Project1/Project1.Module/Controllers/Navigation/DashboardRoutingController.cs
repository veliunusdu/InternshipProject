using System;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;
using Project1.Module.Security;

namespace Project1.Module.Controllers.Navigation
{
    /// <summary>
    /// Oturum açan kullanıcının rolüne göre başlangıç Dashboard görünümünü belirler.
    /// Admin ise AdminDashboard_View, Standart kullanıcı ise UserDashboard_View aktif edilir.
    /// </summary>
    public sealed class DashboardRoutingController : WindowController
    {
        public const string AdminDashboardViewId = "AdminDashboard_View";
        public const string UserDashboardViewId = "UserDashboard_View";

        public DashboardRoutingController()
        {
            TargetWindowType = WindowType.Main;
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            ShowNavigationItemController navigationController = Frame.GetController<ShowNavigationItemController>();
            if (navigationController != null)
            {
                navigationController.ItemsInitialized += NavigationController_ItemsInitialized;
            }
        }

        private void NavigationController_ItemsInitialized(object sender, EventArgs e)
        {
            if (sender is ShowNavigationItemController navigationController)
            {
                bool isAdmin = string.Equals(Application?.Security?.UserName, SecurityConstants.AdministratorUserName, StringComparison.OrdinalIgnoreCase);

                string targetDashboardId = isAdmin ? AdminDashboardViewId : UserDashboardViewId;

                ChoiceActionItem defaultGroup = navigationController.ShowNavigationItemAction.Items.FirstOrDefault(i => i.Id == "Default");
                if (defaultGroup != null)
                {
                    ChoiceActionItem targetItem = defaultGroup.Items.FirstOrDefault(i => i.Id == targetDashboardId);
                    if (targetItem != null)
                    {
                        navigationController.ShowNavigationItemAction.SelectedItem = targetItem;
                    }
                }
            }
        }
    }
}
