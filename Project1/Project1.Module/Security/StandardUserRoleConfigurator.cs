using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using Project1.Module.Models.Entities;

namespace Project1.Module.Security
{
    /// <summary>
    /// Standart kullanıcının iş kayıtları üzerindeki izinlerini tanımlar.
    /// </summary>
    public static class StandardUserRoleConfigurator
    {
        public static void Configure(PermissionPolicyRole role)
        {
            role.IsAdministrative = false;
            role.CanEditModel = false;
            role.PermissionPolicy = SecurityPermissionPolicy.DenyAllByDefault;

            GrantWorkAccess<Musteri>(role);
            GrantWorkAccess<Kisi>(role);
            GrantWorkAccess<Not>(role);

            // Kullanıcı bu kaydı ekleyemez, değiştiremez veya silemez. Ancak not kaydederken
            // kendi e-posta izninin sunucu tarafında okunabilmesi gerekir.
            role.SetTypePermission<UserEmailPermission>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<UserEmailPermission>(SecurityOperations.Create, SecurityPermissionState.Deny);
            role.SetTypePermission<UserEmailPermission>(SecurityOperations.Write, SecurityPermissionState.Deny);
            role.SetTypePermission<UserEmailPermission>(SecurityOperations.Delete, SecurityPermissionState.Deny);

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
        }

        private static void GrantWorkAccess<T>(PermissionPolicyRole role)
            where T : class
        {
            role.SetTypePermission<T>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
        }
    }
}
