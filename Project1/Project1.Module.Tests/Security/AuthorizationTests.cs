using System;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using FluentAssertions;
using Project1.Module.Models.Audit;
using Project1.Module.Models.Customers;
using Project1.Module.Models.Notes;
using Project1.Module.Models.Tenants;
using Project1.Module.BusinessObjects.Security;
using Project1.Module.Security;
using Xunit;

namespace Project1.Module.Tests.Security
{
    public class AuthorizationTests
    {
        private Session CreateInMemorySession()
        {
            var typesInfoSource = XpoTypesInfoHelper.GetXpoTypeInfoSource();
            var typesInfo = XpoTypesInfoHelper.GetTypesInfo();
            typesInfo.RegisterEntity(typeof(Firma));
            typesInfo.RegisterEntity(typeof(Musteri));
            typesInfo.RegisterEntity(typeof(Kisi));
            typesInfo.RegisterEntity(typeof(Not));
            typesInfo.RegisterEntity(typeof(FileData));
            typesInfo.RegisterEntity(typeof(AuditLog));
            typesInfo.RegisterEntity(typeof(UserEmailPermission));
            typesInfo.RegisterEntity(typeof(PermissionPolicyUser));
            typesInfo.RegisterEntity(typeof(ApplicationUser));
            typesInfo.RegisterEntity(typeof(PermissionPolicyRole));
            typesInfo.RegisterEntity(typeof(PermissionPolicyTypePermissionObject));
            typesInfo.RegisterEntity(typeof(PermissionPolicyObjectPermissionsObject));
            typesInfo.RegisterEntity(typeof(PermissionPolicyNavigationPermissionObject));

            var dataStore = new InMemoryDataStore(AutoCreateOption.DatabaseAndSchema);
            var dataLayer = new SimpleDataLayer(dataStore);
            return new Session(dataLayer);
        }

        [Fact]
        public void AdminRole_ShouldHaveNavigationPermissions_ForAdminDashboardAndManagement()
        {
            // Arrange
            using var session = CreateInMemorySession();
            var adminRole = new PermissionPolicyRole(session)
            {
                Name = SecurityConstants.AdministratorRoleName
            };

            // Act
            AdminRoleConfigurator.Configure(adminRole);

            // Assert
            var navItems = adminRole.NavigationPermissions.Select(p => p.ItemPath).ToList();
            navItems.Should().Contain("Application/NavigationItems/Items/Default/Items/AdminDashboard_View");
            navItems.Should().Contain("Application/NavigationItems/Items/Default/Items/Firma_ListView");
            navItems.Should().Contain("Application/NavigationItems/Items/Default/Items/Yonetim");
            navItems.Should().Contain("Application/NavigationItems/Items/Default/Items/Yonetim/Items/UserEmailPermission_ListView");
            navItems.Should().NotContain("Application/NavigationItems/Items/Default/Items/UserDashboard_View");
        }

        [Fact]
        public void AdminRole_ShouldHaveTypePermissions_ForAuditLogAndEntities()
        {
            // Arrange
            using var session = CreateInMemorySession();
            var adminRole = new PermissionPolicyRole(session)
            {
                Name = SecurityConstants.AdministratorRoleName
            };

            // Act
            AdminRoleConfigurator.Configure(adminRole);

            // Assert
            var typeNames = adminRole.TypePermissions
                .Select(p => p.TargetType?.Name ?? p.ToString())
                .ToList();
            typeNames.Should().Contain(nameof(Firma));
            typeNames.Should().Contain(nameof(Musteri));
            typeNames.Should().Contain(nameof(Kisi));
            typeNames.Should().Contain(nameof(Not));
            typeNames.Should().Contain(nameof(FileData));
            typeNames.Should().Contain(nameof(AuditLog));
            typeNames.Should().Contain(nameof(UserEmailPermission));
            typeNames.Should().Contain(nameof(PermissionPolicyUser));
            typeNames.Should().Contain(nameof(ApplicationUser));
        }

        [Fact]
        public void StandardUserRole_ShouldHaveNavigationPermissions_ForUserDashboardOnly()
        {
            // Arrange
            using var session = CreateInMemorySession();
            var standardRole = new PermissionPolicyRole(session)
            {
                Name = SecurityConstants.StandardUserRoleName
            };

            // Act
            StandardUserRoleConfigurator.Configure(standardRole);

            // Assert
            var navItems = standardRole.NavigationPermissions.Select(p => p.ItemPath).ToList();
            navItems.Should().Contain("Application/NavigationItems/Items/Default/Items/UserDashboard_View");
            navItems.Should().NotContain("Application/NavigationItems/Items/Default/Items/AdminDashboard_View");
            navItems.Should().NotContain("Application/NavigationItems/Items/Default/Items/Yonetim");
            navItems.Should().NotContain("Application/NavigationItems/Items/Default/Items/Yonetim/Items/UserEmailPermission_ListView");
        }

        [Fact]
        public void StandardUserRole_ShouldHaveTypePermissions_ForAuditLogAndEntities()
        {
            // Arrange
            using var session = CreateInMemorySession();
            var standardRole = new PermissionPolicyRole(session)
            {
                Name = SecurityConstants.StandardUserRoleName
            };

            // Act
            StandardUserRoleConfigurator.Configure(standardRole);

            // Assert
            var typeNames = standardRole.TypePermissions
                .Select(p => p.TargetType?.Name ?? p.ToString())
                .ToList();
            typeNames.Should().Contain(nameof(Firma));
            typeNames.Should().Contain(nameof(Musteri));
            typeNames.Should().Contain(nameof(Kisi));
            typeNames.Should().Contain(nameof(Not));
            typeNames.Should().Contain(nameof(FileData));
            typeNames.Should().Contain(nameof(AuditLog));
            typeNames.Should().Contain(nameof(UserEmailPermission));
            typeNames.Should().Contain(nameof(PermissionPolicyUser));
            typeNames.Should().Contain(nameof(ApplicationUser));
        }
    }
}
