using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Updating;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using Project1.Module.Models.Entities;

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

            ApplicationUser adminUser = ObjectSpace.FirstOrDefault<ApplicationUser>(u => u.UserName == "Admin");
            if (adminUser == null)
            {
                adminUser = ObjectSpace.CreateObject<ApplicationUser>();
                adminUser.UserName = "Admin";
                adminUser.SetPassword("");
                adminUser.Roles.Add(adminRole);
                adminUser.CanSendEmailOnNoteCreation = true;
            }

            // 2. Seed Default User Role & User User
            PermissionPolicyRole defaultRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(r => r.Name == "Default User");
            if (defaultRole == null)
            {
                defaultRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                defaultRole.Name = "Default User";
                defaultRole.PermissionPolicy = SecurityPermissionPolicy.AllowAllByDefault;
            }

            ApplicationUser standardUser = ObjectSpace.FirstOrDefault<ApplicationUser>(u => u.UserName == "User");
            if (standardUser == null)
            {
                standardUser = ObjectSpace.CreateObject<ApplicationUser>();
                standardUser.UserName = "User";
                standardUser.SetPassword("");
                standardUser.Roles.Add(defaultRole);
                standardUser.CanSendEmailOnNoteCreation = true;
            }

            ObjectSpace.CommitChanges();
        }

        public override void UpdateDatabaseBeforeUpdateSchema()
        {
            base.UpdateDatabaseBeforeUpdateSchema();
        }
    }
}
