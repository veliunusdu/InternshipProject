using System;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using Project1.Module.Security;

namespace Project1.Module.Controllers.Navigation
{
    /// <summary>
    /// Standart kullanıcılar için menüdeki teknik güvenlik öğelerini ve yönetici panelini gizler.
    /// Admin kullanıcılar için standart kullanıcı panelini menüden ayıklar.
    /// </summary>
    public sealed class MenuSecurityController : WindowController
    {
        public MenuSecurityController()
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
                ChoiceActionItem defaultGroup = navigationController.ShowNavigationItemAction.Items.FirstOrDefault(i => i.Id == "Default");

                if (defaultGroup != null)
                {
                    if (!isAdmin)
                    {
                        // Standart / Firma kullanıcısı: Yönetici paneli, Yönetim grubu ve Müşteriler menüsünü tamamen kaldır
                        RemoveItem(defaultGroup, "AdminDashboard_View");
                        RemoveItem(defaultGroup, "Yonetim");
                        RemoveItem(defaultGroup, "Role");
                        RemoveItem(defaultGroup, "User");
                        RemoveItem(defaultGroup, "Firma_ListView");
                        RemoveItem(defaultGroup, "Musteri_ListView");
                    }
                    else
                    {
                        // Admin kullanıcı: Kullanıcı panelini menüden kaldır (Yönetici Paneli kullanılır)
                        RemoveItem(defaultGroup, "UserDashboard_View");
                    }
                }
            }
        }

        private static void RemoveItem(ChoiceActionItem group, string itemId)
        {
            ChoiceActionItem item = group.Items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                group.Items.Remove(item);
            }
        }
    }
}
