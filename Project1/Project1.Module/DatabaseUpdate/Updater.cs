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

            PermissionPolicyUser adminUser = EnsureUser(SecurityConstants.AdministratorUserName, adminRole,
                SecurityConstants.AdminInitialPasswordConfigurationKey);
            PermissionPolicyUser standardUser = EnsureUser(SecurityConstants.StandardUserName, standardUserRole,
                SecurityConstants.UserInitialPasswordConfigurationKey);

            EnsureEmailPermission(standardUser, true);
            RemoveAdminEmailPermission(adminUser);
            RemoveLegacyOwnerPermissions();

            ObjectSpace.CommitChanges();
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

        private PermissionPolicyUser EnsureUser(string userName, PermissionPolicyRole role, string passwordConfigurationKey)
        {
            PermissionPolicyUser user = ObjectSpace.FirstOrDefault<PermissionPolicyUser>(
                u => u.UserName == userName);
            bool isNewUser = user == null;

            if (isNewUser)
            {
                user = ObjectSpace.CreateObject<PermissionPolicyUser>();
                user.UserName = userName;
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
