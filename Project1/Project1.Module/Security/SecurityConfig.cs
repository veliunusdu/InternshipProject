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
    /// ADMIN: Tüm sistem ve verilere tam erişim.
    /// </summary>
    public static class AdminRoleConfigurator
    {
        public static void Configure(PermissionPolicyRole role)
        {
            role.IsAdministrative = false;
            role.CanEditModel = true;
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
            role.SetTypePermission<PermissionPolicyRole>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
            role.SetTypePermission<PermissionPolicyTypePermissionObject>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
            role.SetTypePermission<PermissionPolicyNavigationPermissionObject>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
            role.SetTypePermission<PermissionPolicyMemberPermissionsObject>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
            role.SetTypePermission<PermissionPolicyObjectPermissionsObject>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);

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
    /// FIRMA KULLANICISI: Sadece kendi firmasına (Firma) bağlı kayıtları görebilir ve düzenleyebilir.
    /// KURAL: İş sınıfları (Firma, Musteri, Kisi, Not) için SetTypePermission (Create hariç) doğrudan geniş izin verilmez.
    /// Nesne seviyesinde AddObjectPermission ve alan seviyesinde AddMemberPermission kullanılır.
    /// </summary>
    public static class FirmaKullanicisiRoleConfigurator
    {
        public static void Configure(PermissionPolicyRole role)
        {
            role.IsAdministrative = false;
            role.CanEditModel = false;
            role.PermissionPolicy = SecurityPermissionPolicy.DenyAllByDefault;

            // 1. Firma: Sadece kendi firmasını görebilir
            role.AddObjectPermission<Firma>(SecurityOperations.ReadOnlyAccess, "[Oid] = CurrentFirmaOid()", SecurityPermissionState.Allow);

            // 2. Müşteri: Kendi firmasına ait müşterileri yönetebilir
            role.SetTypePermission<Musteri>(SecurityOperations.Create, SecurityPermissionState.Allow);
            role.AddObjectPermission<Musteri>("Read;Write;Delete;Navigate", "[Firma.Oid] = CurrentFirmaOid()", SecurityPermissionState.Allow);
            // Firma alanının elle değiştirilmesini engelle (FirmaAtamaViewController zaten otomatik atıyor)
            role.AddMemberPermission<Musteri>(SecurityOperations.Write, nameof(Musteri.Firma), "[Firma.Oid] = CurrentFirmaOid()", SecurityPermissionState.Deny);

            // 3. Kişi: Sadece kendi firmasına ait kişileri yönetir
            role.SetTypePermission<Kisi>(SecurityOperations.Create, SecurityPermissionState.Allow);
            role.AddObjectPermission<Kisi>("Read;Write;Delete;Navigate", "[Firma.Oid] = CurrentFirmaOid()", SecurityPermissionState.Allow);
            role.AddMemberPermission<Kisi>(SecurityOperations.Write, nameof(Kisi.Firma), "[Firma.Oid] = CurrentFirmaOid()", SecurityPermissionState.Deny);

            // 4. Not: Sadece kendi firmasına ait notları yönetir
            role.SetTypePermission<Not>(SecurityOperations.Create, SecurityPermissionState.Allow);
            role.AddObjectPermission<Not>("Read;Write;Delete;Navigate", "[Firma.Oid] = CurrentFirmaOid()", SecurityPermissionState.Allow);
            role.AddMemberPermission<Not>(SecurityOperations.Write, nameof(Not.Firma), "[Firma.Oid] = CurrentFirmaOid()", SecurityPermissionState.Deny);

            // 5. Sistem Nesneleri (Dosya, Log, Kullanıcı Bilgisi)
            role.SetTypePermission<FileData>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
            role.SetTypePermission<AuditLog>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
            role.SetTypePermission<PermissionPolicyUser>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<ApplicationUser>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.AddObjectPermission<ApplicationUser>(SecurityOperations.ReadWriteAccess, "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);

            role.SetTypePermission<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<PermissionPolicyTypePermissionObject>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<PermissionPolicyNavigationPermissionObject>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<PermissionPolicyMemberPermissionsObject>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<PermissionPolicyObjectPermissionsObject>(SecurityOperations.Read, SecurityPermissionState.Allow);

            role.SetTypePermission<UserEmailPermission>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<UserEmailPermission>(SecurityOperations.Create, SecurityPermissionState.Deny);
            role.SetTypePermission<UserEmailPermission>(SecurityOperations.Write, SecurityPermissionState.Deny);
            role.SetTypePermission<UserEmailPermission>(SecurityOperations.Delete, SecurityPermissionState.Deny);

            // 6. Menü (Navigasyon) İzinleri
            role.AddNavigationPermission("Application/NavigationItems/Items/Default/Items/UserDashboard_View", SecurityPermissionState.Allow);
            role.AddNavigationPermission("Application/NavigationItems/Items/Default/Items/Musteri_ListView", SecurityPermissionState.Allow);
            role.AddNavigationPermission("Application/NavigationItems/Items/Default/Items/Kisi_ListView", SecurityPermissionState.Allow);
            role.AddNavigationPermission("Application/NavigationItems/Items/Default/Items/Not_ListView", SecurityPermissionState.Allow);
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
    /// MÜŞTERİ (END-USER): Sisteme giren son kullanıcı. SADECE kendi kaydını, kendi kişilerini ve notlarını görmelidir.
    /// </summary>
    public static class CustomerRoleConfigurator
    {
        public static void Configure(PermissionPolicyRole role)
        {
            role.IsAdministrative = false;
            role.CanEditModel = false;
            role.PermissionPolicy = SecurityPermissionPolicy.DenyAllByDefault;

            // 1. Müşteri İzni: Yalnızca kendi firmasını okur ve günceller (Tüm müşterileri görmesini engellemek için Type-Level izin verilmez)
            role.AddObjectPermission<Musteri>(
                SecurityOperations.ReadWriteAccess, 
                "[<ApplicationUser>][Oid = CurrentUserId() and Musteri.Oid = ^.Oid]", 
                SecurityPermissionState.Allow);

            // 2. Kişi İzni: Yeni kişi oluşturabilir (Create), ancak yalnızca kendi firmasına bağlı kişileri yönetir/görür
            role.SetTypePermission<Kisi>(SecurityOperations.Create, SecurityPermissionState.Allow);
            role.AddObjectPermission<Kisi>(
                "Read;Write;Delete;Navigate", 
                "[<ApplicationUser>][Oid = CurrentUserId() and Musteri.Oid = ^.Musteri.Oid]", 
                SecurityPermissionState.Allow);

            // 3. Not İzni: Yalnızca kendi firmasına ve kişilerine açılmış notları okur
            role.AddObjectPermission<Not>(
                SecurityOperations.ReadOnlyAccess, 
                "[<ApplicationUser>][Oid = CurrentUserId() and (Musteri.Oid = ^.Musteri.Oid or Musteri.Oid = ^.Kisi.Musteri.Oid)]", 
                SecurityPermissionState.Allow);

            // 4. Dosya Eki İzni: Kendi notlarının eklerini indirir
            role.SetTypePermission<FileData>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);

            // 5. Firma İzni: Kendi firmasını okur
            role.AddObjectPermission<Firma>(SecurityOperations.ReadOnlyAccess, "[Oid] = CurrentFirmaOid()", SecurityPermissionState.Allow);

            // 6. Kullanıcı ve Güvenlik Nesnelerini Okuma İzinleri
            role.SetTypePermission<PermissionPolicyUser>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<ApplicationUser>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.AddObjectPermission<ApplicationUser>(SecurityOperations.ReadWriteAccess, "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
            role.SetTypePermission<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<PermissionPolicyTypePermissionObject>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<PermissionPolicyNavigationPermissionObject>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<PermissionPolicyMemberPermissionsObject>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<PermissionPolicyObjectPermissionsObject>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<UserEmailPermission>(SecurityOperations.FullAccess, SecurityPermissionState.Deny);
            role.SetTypePermission<AuditLog>(SecurityOperations.FullAccess, SecurityPermissionState.Deny);

            // 7. Navigasyon (Menü) İzinleri
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

