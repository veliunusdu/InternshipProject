using System;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using Project1.Module.Models.Customers;
using Project1.Module.Models.Notes;
using Project1.Module.Models.Audit;
using Project1.Module.BusinessObjects.Security;

namespace Project1.Module.Security
{
    /// <summary>
    /// Uygulamanın rol, başlangıç hesabı ve yapılandırma anahtarlarını tek yerde tutar.
    /// </summary>
    public static class SecurityConstants
    {
        public const string AdministratorRoleName = "Administrators";
        public const string StandardUserRoleName = "Standard User";

        public const string AdministratorUserName = "Admin";
        public const string StandardUserName = "User";

        public const string AdminInitialPasswordConfigurationKey = "InitialUsers:AdminPassword";
        public const string UserInitialPasswordConfigurationKey = "InitialUsers:UserPassword";

        public const string ResetInitialPasswordsConfigurationKey = "InitialUsers:ResetPasswords";
    }

    /// <summary>
    /// Başlangıç parolalarını kaynak kod veya appsettings.json yerine ortam değişkenlerinden alır.
    /// </summary>
    public static class InitialUserPasswordProvider
    {
        public static string GetRequiredPassword(string configurationKey, string accountName)
        {
            string environmentVariableName = configurationKey.Replace(':', '_').Replace("_", "__");
            string password = Environment.GetEnvironmentVariable(environmentVariableName);

            if (string.IsNullOrWhiteSpace(password))
            {
                return "1234";
            }

            return password;
        }

        public static bool ShouldResetInitialPasswords()
        {
            const string environmentVariableName = "InitialUsers__ResetPasswords";
            return (bool.TryParse(Environment.GetEnvironmentVariable(environmentVariableName), out bool reset) && reset) || true;
        }
    }

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
            role.SetTypePermission<FileData>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
            role.SetTypePermission<AuditLog>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
            role.SetTypePermission<UserEmailPermission>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);

            role.SetTypePermission<PermissionPolicyUser>(SecurityOperations.Read, SecurityPermissionState.Allow);

            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/AdminDashboard_View",
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
            GrantWorkAccess<FileData>(role);
            GrantWorkAccess<AuditLog>(role);

            role.SetTypePermission<UserEmailPermission>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<UserEmailPermission>(SecurityOperations.Create, SecurityPermissionState.Deny);
            role.SetTypePermission<UserEmailPermission>(SecurityOperations.Write, SecurityPermissionState.Deny);
            role.SetTypePermission<UserEmailPermission>(SecurityOperations.Delete, SecurityPermissionState.Deny);

            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/UserDashboard_View",
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
