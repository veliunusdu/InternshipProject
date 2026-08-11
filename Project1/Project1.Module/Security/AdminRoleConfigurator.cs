using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using Project1.Module.Models.Entities;

namespace Project1.Module.Security
{
    /// <summary>
    /// Admin iş verilerini ve e-posta yetkilerini yönetir; güvenlik rolleri üzerinde yetkili değildir.
    /// </summary>
    public static class AdminRoleConfigurator
    {
        public static void Configure(PermissionPolicyRole role)
        {
            role.IsAdministrative = false;
            role.CanEditModel = false;
            role.PermissionPolicy = SecurityPermissionPolicy.DenyAllByDefault;

            role.SetTypePermission<Musteri>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
            role.SetTypePermission<Kisi>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
            role.SetTypePermission<Not>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
            role.SetTypePermission<UserEmailPermission>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);

            // Kullanıcı yalnızca e-posta yetkisi ekranındaki seçim alanında okunabilir.
            role.SetTypePermission<PermissionPolicyUser>(SecurityOperations.Read, SecurityPermissionState.Allow);

            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/Dashboard_View",
                SecurityPermissionState.Allow);
            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/Musteri_ListView",
                SecurityPermissionState.Allow);
            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/Kisi_ListView",
                SecurityPermissionState.Allow);
            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/Not_ListView",
                SecurityPermissionState.Allow);
            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/Yonetim",
                SecurityPermissionState.Allow);
            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/Yonetim/Items/UserEmailPermission_ListView",
                SecurityPermissionState.Allow);
        }
    }
}
