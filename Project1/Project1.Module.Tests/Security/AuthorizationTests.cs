using System.Linq;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using FluentAssertions;
using Project1.Module.Security;
using Xunit;

namespace Project1.Module.Tests.Security
{
    public class AuthorizationTests
    {
        private Session CreateInMemorySession()
        {
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
            navItems.Should().Contain("Application/NavigationItems/Items/Default/Items/Yonetim");
            navItems.Should().Contain("Application/NavigationItems/Items/Default/Items/Yonetim/Items/UserEmailPermission_ListView");
            navItems.Should().NotContain("Application/NavigationItems/Items/Default/Items/UserDashboard_View");
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
    }
}
