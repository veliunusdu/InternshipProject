using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Updating;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using Project1.Module.Models.Customers;
using Project1.Module.Models.Notes;
using Project1.Module.Models.Tenants;
using Project1.Module.BusinessObjects.Security;
using Project1.Module.Security;

namespace Project1.Module.DatabaseUpdate
{
    public class Updater : ModuleUpdater
    {
        public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
            base(objectSpace, currentDBVersion)
        {
        }

        public override void UpdateDatabaseAfterUpdateSchema()
        {
            base.UpdateDatabaseAfterUpdateSchema();

            PermissionPolicyRole adminRole = EnsureAdministratorRole();
            PermissionPolicyRole standardUserRole = EnsureStandardUserRole();
            PermissionPolicyRole firmaUserRole = EnsureFirmaKullanicisiRole();
            PermissionPolicyRole customerRole = EnsureCustomerRole();

            var (firmaAcme, firmaBeta) = EnsureDefaultFirmalar();

            ApplicationUser adminUser = EnsureUser(SecurityConstants.AdministratorUserName, adminRole,
                SecurityConstants.AdminInitialPasswordConfigurationKey);
            adminUser.Firma = null; // Admin tüm firmaları görür

            ApplicationUser standardUser = EnsureUser(SecurityConstants.StandardUserName, standardUserRole,
                SecurityConstants.UserInitialPasswordConfigurationKey);
            standardUser.Firma = firmaAcme;
            if (!standardUser.Roles.Contains(firmaUserRole)) standardUser.Roles.Add(firmaUserRole);

            ApplicationUser userBeta = EnsureUser("User_Beta", standardUserRole,
                SecurityConstants.UserInitialPasswordConfigurationKey);
            userBeta.Firma = firmaBeta;
            if (!userBeta.Roles.Contains(firmaUserRole)) userBeta.Roles.Add(firmaUserRole);

            EnsureDefaultCustomerAndUser(customerRole, firmaAcme);
            EnsureSampleData(firmaAcme, firmaBeta);

            EnsureEmailPermission(standardUser, true);
            EnsureEmailPermission(userBeta, true);
            RemoveAdminEmailPermission(adminUser);
            RemoveLegacyOwnerPermissions();

            ObjectSpace.CommitChanges();
        }

        private (Firma acme, Firma beta) EnsureDefaultFirmalar()
        {
            Firma firmaAcme = ObjectSpace.FirstOrDefault<Firma>(f => f.Ad == "Acme Teknoloji A.Ş.");
            if (firmaAcme == null)
            {
                firmaAcme = ObjectSpace.CreateObject<Firma>();
                firmaAcme.Ad = "Acme Teknoloji A.Ş.";
                firmaAcme.Unvan = "Acme Teknoloji ve Yazılım San. Tic. A.Ş.";
                firmaAcme.VergiNo = "1112223334";
                firmaAcme.Telefon = "0212 555 10 20";
                firmaAcme.Adres = "Maslak, Sarıyer, İstanbul";
            }

            Firma firmaBeta = ObjectSpace.FirstOrDefault<Firma>(f => f.Ad == "Beta Lojistik A.Ş.");
            if (firmaBeta == null)
            {
                firmaBeta = ObjectSpace.CreateObject<Firma>();
                firmaBeta.Ad = "Beta Lojistik A.Ş.";
                firmaBeta.Unvan = "Beta Uluslararası Taşımacılık ve Lojistik Ltd. Şti.";
                firmaBeta.VergiNo = "5556667778";
                firmaBeta.Telefon = "0232 444 30 40";
                firmaBeta.Adres = "Alsancak, Konak, İzmir";
            }

            return (firmaAcme, firmaBeta);
        }

        private void EnsureSampleData(Firma firmaAcme, Firma firmaBeta)
        {
            // Acme Müşteri ve Not
            Musteri musteriAcme = ObjectSpace.FirstOrDefault<Musteri>(m => m.Ad == "Mega Perakende A.Ş.");
            if (musteriAcme == null)
            {
                musteriAcme = ObjectSpace.CreateObject<Musteri>();
                musteriAcme.Ad = "Mega Perakende A.Ş.";
                musteriAcme.Telefon = "0212 333 44 55";
                musteriAcme.Adres = "Levent, İstanbul";
                musteriAcme.Firma = firmaAcme;

                Kisi kisiAcme = ObjectSpace.CreateObject<Kisi>();
                kisiAcme.Ad = "Ahmet";
                kisiAcme.Soyad = "Yılmaz";
                kisiAcme.Email = "ahmet.yilmaz@megaperakende.com";
                kisiAcme.Telefon = "0532 111 22 33";
                kisiAcme.Musteri = musteriAcme;
                kisiAcme.Firma = firmaAcme;

                Not notAcme = ObjectSpace.CreateObject<Not>();
                notAcme.Baslik = "Acme Yıllık Sözleşme Görüşmesi";
                notAcme.Icerik = "2026 yılı bakım ve destek sözleşmesi detayları görüşüldü.";
                notAcme.Derece = Project1.Core.Enums.NotDerecesi.Onemli;
                notAcme.Musteri = musteriAcme;
                notAcme.Kisi = kisiAcme;
                notAcme.Firma = firmaAcme;
            }
            else if (musteriAcme.Firma == null)
            {
                musteriAcme.Firma = firmaAcme;
            }

            // Beta Müşteri ve Not
            Musteri musteriBeta = ObjectSpace.FirstOrDefault<Musteri>(m => m.Ad == "Ege Dağıtım Ltd.");
            if (musteriBeta == null)
            {
                musteriBeta = ObjectSpace.CreateObject<Musteri>();
                musteriBeta.Ad = "Ege Dağıtım Ltd.";
                musteriBeta.Telefon = "0232 222 33 44";
                musteriBeta.Adres = "Bornova, İzmir";
                musteriBeta.Firma = firmaBeta;

                Kisi kisiBeta = ObjectSpace.CreateObject<Kisi>();
                kisiBeta.Ad = "Ayşe";
                kisiBeta.Soyad = "Kaya";
                kisiBeta.Email = "ayse.kaya@egedagitim.com";
                kisiBeta.Telefon = "0542 999 88 77";
                kisiBeta.Musteri = musteriBeta;
                kisiBeta.Firma = firmaBeta;

                Not notBeta = ObjectSpace.CreateObject<Not>();
                notBeta.Baslik = "Beta Filo Teslimatı Notu";
                notBeta.Icerik = "Yeni nesil elektrikli dağıtım araçlarının teslimatı tamamlandı.";
                notBeta.Derece = Project1.Core.Enums.NotDerecesi.Normal;
                notBeta.Musteri = musteriBeta;
                notBeta.Kisi = kisiBeta;
                notBeta.Firma = firmaBeta;
            }
            else if (musteriBeta.Firma == null)
            {
                musteriBeta.Firma = firmaBeta;
            }
        }

        private PermissionPolicyRole EnsureCustomerRole()
        {
            PermissionPolicyRole role = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(
                r => r.Name == SecurityConstants.CustomerRoleName);

            if (role == null)
            {
                role = ObjectSpace.CreateObject<PermissionPolicyRole>();
                role.Name = SecurityConstants.CustomerRoleName;
            }

            ResetRolePermissions(role);
            CustomerRoleConfigurator.Configure(role);
            return role;
        }

        private void EnsureDefaultCustomerAndUser(PermissionPolicyRole customerRole, Firma firmaAcme)
        {
            Musteri defaultMusteri = ObjectSpace.FirstOrDefault<Musteri>(m => m.Ad == "Acme Lojistik A.Ş.");
            if (defaultMusteri == null)
            {
                defaultMusteri = ObjectSpace.CreateObject<Musteri>();
                defaultMusteri.Ad = "Acme Lojistik A.Ş.";
                defaultMusteri.Telefon = "0555 123 45 67";
                defaultMusteri.Adres = "İstanbul, Türkiye";
                defaultMusteri.Firma = firmaAcme;
            }
            else if (defaultMusteri.Firma == null)
            {
                defaultMusteri.Firma = firmaAcme;
            }

            ApplicationUser customerUser = ObjectSpace.FirstOrDefault<ApplicationUser>(
                u => u.UserName == SecurityConstants.DefaultCustomerUserName);

            if (customerUser == null)
            {
                customerUser = ObjectSpace.CreateObject<ApplicationUser>();
                customerUser.UserName = SecurityConstants.DefaultCustomerUserName;
                customerUser.Email = "customer@acme.com";
                customerUser.SetPassword("1234");
            }

            customerUser.Musteri = defaultMusteri;
            customerUser.Firma = firmaAcme;
            customerUser.EmailConfirmed = true;
            customerUser.IsActive = true;

            foreach (var r in customerUser.Roles.ToList())
            {
                if (r.Name != SecurityConstants.CustomerRoleName)
                {
                    customerUser.Roles.Remove(r);
                }
            }

            if (!customerUser.Roles.Contains(customerRole))
            {
                customerUser.Roles.Add(customerRole);
            }
        }

        private PermissionPolicyRole EnsureAdministratorRole()
        {
            PermissionPolicyRole role = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(
                r => r.Name == SecurityConstants.AdministratorRoleName);

            if (role == null)
            {
                role = ObjectSpace.CreateObject<PermissionPolicyRole>();
                role.Name = SecurityConstants.AdministratorRoleName;
            }

            ResetRolePermissions(role);
            AdminRoleConfigurator.Configure(role);
            return role;
        }

        private PermissionPolicyRole EnsureStandardUserRole()
        {
            PermissionPolicyRole role = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(
                r => r.Name == SecurityConstants.StandardUserRoleName);

            if (role == null)
            {
                role = ObjectSpace.CreateObject<PermissionPolicyRole>();
                role.Name = SecurityConstants.StandardUserRoleName;
            }

            ResetRolePermissions(role);
            StandardUserRoleConfigurator.Configure(role);
            return role;
        }

        private PermissionPolicyRole EnsureFirmaKullanicisiRole()
        {
            PermissionPolicyRole role = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(
                r => r.Name == SecurityConstants.FirmaKullanicisiRoleName);

            if (role == null)
            {
                role = ObjectSpace.CreateObject<PermissionPolicyRole>();
                role.Name = SecurityConstants.FirmaKullanicisiRoleName;
            }

            ResetRolePermissions(role);
            FirmaKullanicisiRoleConfigurator.Configure(role);
            return role;
        }

        private void ResetRolePermissions(PermissionPolicyRole role)
        {
            foreach (PermissionPolicyTypePermissionObject permission in role.TypePermissions.ToList())
            {
                foreach (PermissionPolicyObjectPermissionsObject objPerm in permission.ObjectPermissions.ToList())
                {
                    ObjectSpace.Delete(objPerm);
                }
                foreach (PermissionPolicyMemberPermissionsObject memberPerm in permission.MemberPermissions.ToList())
                {
                    ObjectSpace.Delete(memberPerm);
                }
                ObjectSpace.Delete(permission);
            }

            foreach (PermissionPolicyNavigationPermissionObject permission in role.NavigationPermissions.ToList())
            {
                ObjectSpace.Delete(permission);
            }

            foreach (PermissionPolicyActionPermissionObject permission in role.ActionPermissions.ToList())
            {
                ObjectSpace.Delete(permission);
            }
        }

        private ApplicationUser EnsureUser(string userName, PermissionPolicyRole role, string passwordConfigurationKey)
        {
            ApplicationUser user = ObjectSpace.FirstOrDefault<ApplicationUser>(
                u => u.UserName == userName);
            bool isNewUser = user == null;

            if (isNewUser)
            {
                user = ObjectSpace.CreateObject<ApplicationUser>();
                user.UserName = userName;
                user.IsActive = true;
                user.EmailConfirmed = true;
            }

            if (!user.Roles.Contains(role))
            {
                user.Roles.Add(role);
            }

            RemoveLegacyDefaultUserRole(user, userName);

            if (isNewUser || InitialUserPasswordProvider.ShouldResetInitialPasswords())
            {
                user.SetPassword(InitialUserPasswordProvider.GetRequiredPassword(passwordConfigurationKey, userName));
            }

            return user;
        }

        private void EnsureEmailPermission(PermissionPolicyUser user, bool defaultValue)
        {
            UserEmailPermission permission = ObjectSpace.FirstOrDefault<UserEmailPermission>(
                item => item.User == user);
            if (permission == null)
            {
                permission = ObjectSpace.CreateObject<UserEmailPermission>();
                permission.User = user;
                permission.CanSendEmail = defaultValue;
            }
        }

        private void RemoveAdminEmailPermission(PermissionPolicyUser adminUser)
        {
            UserEmailPermission permission = ObjectSpace.FirstOrDefault<UserEmailPermission>(
                item => item.User == adminUser);
            if (permission != null)
            {
                ObjectSpace.Delete(permission);
            }
        }

        private void RemoveLegacyOwnerPermissions()
        {
            foreach (PermissionPolicyObjectPermissionsObject permission in
                     ObjectSpace.GetObjects<PermissionPolicyObjectPermissionsObject>().ToList())
            {
                if (permission.Criteria?.Contains("Owner", StringComparison.OrdinalIgnoreCase) == true)
                {
                    ObjectSpace.Delete(permission);
                }
            }
        }

        private void RemoveLegacyDefaultUserRole(PermissionPolicyUser user, string userName)
        {
            if (userName != SecurityConstants.StandardUserName)
            {
                return;
            }

            PermissionPolicyRole legacyRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(
                r => r.Name == "Default User");
            if (legacyRole != null && user.Roles.Contains(legacyRole))
            {
                user.Roles.Remove(legacyRole);
            }
        }

    }
}
