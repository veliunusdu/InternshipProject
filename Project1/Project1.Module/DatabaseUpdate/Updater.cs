using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Updating;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;

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

            // 1. Seed Administrators Role & Admin User
            PermissionPolicyRole adminRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(r => r.Name == "Administrators");
            if (adminRole == null)
            {
                adminRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                adminRole.Name = "Administrators";
                adminRole.IsAdministrative = true;
            }

            PermissionPolicyUser adminUser = ObjectSpace.FirstOrDefault<PermissionPolicyUser>(u => u.UserName == "Admin");
            if (adminUser == null)
            {
                adminUser = ObjectSpace.CreateObject<PermissionPolicyUser>();
                adminUser.UserName = "Admin";
                adminUser.SetPassword("");
                adminUser.Roles.Add(adminRole);
            }

            // 2. Seed Default User Role & User User
            PermissionPolicyRole defaultRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(r => r.Name == "Default User");
            if (defaultRole == null)
            {
                defaultRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                defaultRole.Name = "Default User";
                defaultRole.PermissionPolicy = SecurityPermissionPolicy.AllowAllByDefault;
            }

            PermissionPolicyUser standardUser = ObjectSpace.FirstOrDefault<PermissionPolicyUser>(u => u.UserName == "User");
            if (standardUser == null)
            {
                standardUser = ObjectSpace.CreateObject<PermissionPolicyUser>();
                standardUser.UserName = "User";
                standardUser.SetPassword("");
                standardUser.Roles.Add(defaultRole);
            }

            ObjectSpace.CommitChanges();
        }

        public override void UpdateDatabaseBeforeUpdateSchema()
        {
            base.UpdateDatabaseBeforeUpdateSchema();
        }
    }
}
