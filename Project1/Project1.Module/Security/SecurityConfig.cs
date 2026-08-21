using System;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using Project1.Module.Models.Customers;
using Project1.Module.Models.Notes;
using Project1.Module.Models.Audit;
using Project1.Module.Models.Tenants;
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
        public const string FirmaKullanicisiRoleName = "FirmaKullanicisiRole";
        public const string CustomerRoleName = "Customer";

        public const string AdministratorUserName = "Admin";
        public const string StandardUserName = "User";
        public const string DefaultCustomerUserName = "customer_acme";

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
    /// Admin iş verilerini ve e-posta yetkilerini yönetir; tüm firmaların verilerine tam erişim yetkisine sahiptir.
    /// </summary>
    public static class AdminRoleConfigurator
    {
        public static void Configure(PermissionPolicyRole role)
        {
            role.IsAdministrative = false;
            role.CanEditModel = false;
            role.PermissionPolicy = SecurityPermissionPolicy.DenyAllByDefault;

            role.SetTypePermission<Firma>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
            role.SetTypePermission<Musteri>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
            role.SetTypePermission<Kisi>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
            role.SetTypePermission<Not>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
            role.SetTypePermission<FileData>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
            role.SetTypePermission<AuditLog>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
            role.SetTypePermission<UserEmailPermission>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);

            role.SetTypePermission<PermissionPolicyUser>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<ApplicationUser>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);

            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/AdminDashboard_View",
                SecurityPermissionState.Allow);
            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/Firma_ListView",
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
    /// Firma kullanıcısının (FirmaKullanicisiRole) yalnızca kendi firmasına bağlı kayıtlar üzerindeki izinlerini tanımlar.
    /// Kriter: [Firma] = CurrentUserFirma()
    /// </summary>
    public static class FirmaKullanicisiRoleConfigurator
    {
        public static void Configure(PermissionPolicyRole role)
        {
            role.IsAdministrative = false;
            role.CanEditModel = false;
            role.PermissionPolicy = SecurityPermissionPolicy.DenyAllByDefault;

            // 1. Firma İzni: Yalnızca kendi firmasını okur
            role.SetTypePermission<Firma>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
            role.AddObjectPermission<Firma>(
                SecurityOperations.ReadOnlyAccess,
                "[Oid] = CurrentUserFirma() or [Oid] = CurrentFirmaOid()",
                SecurityPermissionState.Allow);

            // 2. Müşteri İzni: Read, Write, Create, Delete -> [Firma.Oid] = CurrentUserFirma()
            role.SetTypePermission<Musteri>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
            role.AddObjectPermission<Musteri>(
                "Read;Write;Delete;Navigate",
                "[Firma.Oid] = CurrentUserFirma() or [Firma.Oid] = CurrentFirmaOid()",
                SecurityPermissionState.Allow);
            role.AddMemberPermission<Musteri>(
                SecurityOperations.Write,
                nameof(Musteri.Firma),
                null,
                SecurityPermissionState.Deny);

            // 3. Kişi İzni: Read, Write, Create, Delete -> [Firma.Oid] = CurrentUserFirma()
            role.SetTypePermission<Kisi>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
            role.AddObjectPermission<Kisi>(
                "Read;Write;Delete;Navigate",
                "[Firma.Oid] = CurrentUserFirma() or [Firma.Oid] = CurrentFirmaOid()",
                SecurityPermissionState.Allow);
            role.AddMemberPermission<Kisi>(
                SecurityOperations.Write,
                nameof(Kisi.Firma),
                null,
                SecurityPermissionState.Deny);

            // 4. Not İzni: Read, Write, Create, Delete -> [Firma.Oid] = CurrentUserFirma()
            role.SetTypePermission<Not>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
            role.AddObjectPermission<Not>(
                "Read;Write;Delete;Navigate",
                "[Firma.Oid] = CurrentUserFirma() or [Firma.Oid] = CurrentFirmaOid()",
                SecurityPermissionState.Allow);
            role.AddMemberPermission<Not>(
                SecurityOperations.Write,
                nameof(Not.Firma),
                null,
                SecurityPermissionState.Deny);

            // 5. Dosya ve Denetim İzinleri
            role.SetTypePermission<FileData>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
            role.SetTypePermission<AuditLog>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);

            // 6. Kullanıcı İzinleri
            role.SetTypePermission<PermissionPolicyUser>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<ApplicationUser>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<PermissionPolicyTypePermissionObject>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<PermissionPolicyNavigationPermissionObject>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<PermissionPolicyMemberPermissionsObject>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<PermissionPolicyObjectPermissionsObject>(SecurityOperations.Read, SecurityPermissionState.Allow);

            role.SetTypePermission<UserEmailPermission>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<UserEmailPermission>(SecurityOperations.Create, SecurityPermissionState.Deny);
            role.SetTypePermission<UserEmailPermission>(SecurityOperations.Write, SecurityPermissionState.Deny);
            role.SetTypePermission<UserEmailPermission>(SecurityOperations.Delete, SecurityPermissionState.Deny);

            // 7. Navigasyon İzinleri (Müşteriler menüsü gizlenir, sadece Kişiler ve Notlar görünür)
            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/UserDashboard_View",
                SecurityPermissionState.Allow);
            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/Kisi_ListView",
                SecurityPermissionState.Allow);
            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/Not_ListView",
                SecurityPermissionState.Allow);
        }
    }

    /// <summary>
    /// Standart kullanıcı rolü - FirmaKullanicisiRole ile aynı kurallara sahiptir.
    /// </summary>
    public static class StandardUserRoleConfigurator
    {
        public static void Configure(PermissionPolicyRole role)
        {
            FirmaKullanicisiRoleConfigurator.Configure(role);
        }
    }

    /// <summary>
    /// Müşteri rolünün kendi şirketine ait kayıtlar üzerindeki satır seviyesi (Row-Level) izinlerini tanımlar.
    /// </summary>
    public static class CustomerRoleConfigurator
    {
        public static void Configure(PermissionPolicyRole role)
        {
            role.IsAdministrative = false;
            role.CanEditModel = false;
            role.PermissionPolicy = SecurityPermissionPolicy.DenyAllByDefault;

            // 1. Müşteri İzni: Yalnızca kendi firmasını okur ve günceller (Tüm müşterileri görmesini engellemek için Type-Level izin verilmez)
            role.SetTypePermission<Musteri>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
            role.AddObjectPermission<Musteri>(
                SecurityOperations.ReadWriteAccess, 
                "[Oid] = CurrentCustomerOid() or [<ApplicationUser>][Oid = CurrentUserId() and Musteri.Oid = ^.Oid]", 
                SecurityPermissionState.Allow);

            // 2. Kişi İzni: Yeni kişi oluşturabilir (Create) ve silebilir (Delete), ancak yalnızca kendi müşterisine bağlı kişileri yönetir
            role.SetTypePermission<Kisi>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
            role.AddObjectPermission<Kisi>(
                "Read;Write;Delete;Navigate", 
                "[Musteri.Oid] = CurrentCustomerOid() or [<ApplicationUser>][Oid = CurrentUserId() and Musteri.Oid = ^.Musteri.Oid]", 
                SecurityPermissionState.Allow);

            // 3. Not İzni: Yalnızca kendi müşterisine ve kişilerine açılmış notları okur
            role.SetTypePermission<Not>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
            role.AddObjectPermission<Not>(
                SecurityOperations.ReadOnlyAccess, 
                "[Musteri.Oid] = CurrentCustomerOid() or [Kisi.Musteri.Oid] = CurrentCustomerOid() or [<ApplicationUser>][Oid = CurrentUserId() and (Musteri.Oid = ^.Musteri.Oid or Musteri.Oid = ^.Kisi.Musteri.Oid)]", 
                SecurityPermissionState.Allow);

            // 4. Dosya Eki İzni: Kendi notlarının eklerini indirir
            role.SetTypePermission<FileData>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);

            // 5. Denetim İzni: Otomatik sistem loglarının yazılabilmesi için izin verilir
            role.SetTypePermission<AuditLog>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);

            // 6. Firma İzni: Bağlı olduğu firmayı okur
            role.SetTypePermission<Firma>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
            role.AddObjectPermission<Firma>(
                SecurityOperations.ReadOnlyAccess, 
                "[Oid] = CurrentUserFirma() or [Oid] = CurrentFirmaOid()", 
                SecurityPermissionState.Allow);

            // 7. Kullanıcı ve Güvenlik Nesnelerini Okuma İzinleri
            role.SetTypePermission<PermissionPolicyUser>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<ApplicationUser>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<PermissionPolicyTypePermissionObject>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<PermissionPolicyNavigationPermissionObject>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<PermissionPolicyMemberPermissionsObject>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<PermissionPolicyObjectPermissionsObject>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<UserEmailPermission>(SecurityOperations.FullAccess, SecurityPermissionState.Deny);

            // 8. Navigasyon (Menü) İzinleri
            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/UserDashboard_View",
                SecurityPermissionState.Allow);
            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/Kisi_ListView",
                SecurityPermissionState.Allow);
            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/Not_ListView",
                SecurityPermissionState.Allow);
        }
    }
}
