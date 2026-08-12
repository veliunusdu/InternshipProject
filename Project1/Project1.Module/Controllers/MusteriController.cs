using System;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;

namespace Project1.Module.Controllers
{
    /// <summary>
    /// Müşteri ve genel navigasyon/güvenlik menülerini yöneten denetleyici.
    /// Standart kullanıcılar için menüdeki teknik güvenlik öğelerini (Roller, Kullanıcılar) gizler.
    /// </summary>
    public sealed class MusteriController : WindowController
    {
        public MusteriController()
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
                bool isAdmin = string.Equals(Application?.Security?.UserName, "Admin", StringComparison.OrdinalIgnoreCase);
                if (!isAdmin)
                {
                    ChoiceActionItem defaultGroup = navigationController.ShowNavigationItemAction.Items.FirstOrDefault(i => i.Id == "Default");
                    if (defaultGroup != null)
                    {
                        ChoiceActionItem roleItem = defaultGroup.Items.FirstOrDefault(i => i.Id == "Role");
                        if (roleItem != null)
                        {
                            defaultGroup.Items.Remove(roleItem);
                        }

                        ChoiceActionItem userItem = defaultGroup.Items.FirstOrDefault(i => i.Id == "User");
                        if (userItem != null)
                        {
                            defaultGroup.Items.Remove(userItem);
                        }
                    }
                }
            }
        }
    }
}
