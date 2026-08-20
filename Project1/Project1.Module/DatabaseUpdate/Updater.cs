using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Updating;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using Project1.Module.Models.Customers;
using Project1.Module.Models.Notes;
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
            PermissionPolicyRole customerRole = EnsureCustomerRole();

            ApplicationUser adminUser = EnsureUser(SecurityConstants.AdministratorUserName, adminRole,
                SecurityConstants.AdminInitialPasswordConfigurationKey);
            ApplicationUser standardUser = EnsureUser(SecurityConstants.StandardUserName, standardUserRole,
                SecurityConstants.UserInitialPasswordConfigurationKey);

            EnsureDefaultCustomerAndUser(customerRole);

            EnsureEmailPermission(standardUser, true);
            RemoveAdminEmailPermission(adminUser);
            RemoveLegacyOwnerPermissions();

            ObjectSpace.CommitChanges();
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

        private void EnsureDefaultCustomerAndUser(PermissionPolicyRole customerRole)
        {
            Musteri defaultMusteri = ObjectSpace.FirstOrDefault<Musteri>(m => m.Ad == "Acme Lojistik A.Ş.");
            if (defaultMusteri == null)
            {
                defaultMusteri = ObjectSpace.CreateObject<Musteri>();
                defaultMusteri.Ad = "Acme Lojistik A.Ş.";
                defaultMusteri.Telefon = "0555 123 45 67";
                defaultMusteri.Adres = "İstanbul, Türkiye";
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
            customerUser.EmailConfirmed = true;
            customerUser.IsActive = true;

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
