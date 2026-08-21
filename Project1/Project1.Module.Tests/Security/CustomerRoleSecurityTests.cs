#nullable enable
using System;
using System.Linq;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using FluentAssertions;
using Project1.Module.BusinessObjects.Security;
using Project1.Module.Models.Audit;
using Project1.Module.Models.Customers;
using Project1.Module.Models.Notes;
using Project1.Module.Security;
using Xunit;

namespace Project1.Module.Tests.Security
{
    public class CustomerRoleSecurityTests
    {
        private Session CreateInMemorySession()
        {
            var typesInfoSource = XpoTypesInfoHelper.GetXpoTypeInfoSource();
            var typesInfo = XpoTypesInfoHelper.GetTypesInfo();
            typesInfo.RegisterEntity(typeof(Project1.Module.Models.Tenants.Firma));
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
            typesInfo.RegisterEntity(typeof(PermissionPolicyNavigationPermissionObject));

            var dataStore = new InMemoryDataStore(AutoCreateOption.DatabaseAndSchema);
            var dataLayer = new SimpleDataLayer(dataStore);
            return new Session(dataLayer);
        }

        [Fact]
        public void CustomerRoleConfigurator_ShouldSetDenyAllByDefault()
        {
            // Arrange
            using var session = CreateInMemorySession();
            var role = new PermissionPolicyRole(session)
            {
                Name = SecurityConstants.CustomerRoleName
            };

            // Act
            CustomerRoleConfigurator.Configure(role);

            // Assert
            role.IsAdministrative.Should().BeFalse();
            role.CanEditModel.Should().BeFalse();
            role.PermissionPolicy.Should().Be(SecurityPermissionPolicy.DenyAllByDefault);
        }

        [Fact]
        public void CustomerRoleConfigurator_ShouldConfigureObjectPermissions_ForMusteriKisiNot()
        {
            // Arrange
            using var session = CreateInMemorySession();
            var role = new PermissionPolicyRole(session)
            {
                Name = SecurityConstants.CustomerRoleName
            };

            // Act
            CustomerRoleConfigurator.Configure(role);

            // Assert
            var typePermissions = role.TypePermissions.ToList();
            typePermissions.Should().NotBeEmpty();

            // Should have object-level criteria containing ApplicationUser / CurrentUserId
            var allCriteria = typePermissions.SelectMany(tp => tp.ObjectPermissions).Select(op => op.Criteria).ToList();
            allCriteria.Should().Contain(c => c.Contains("ApplicationUser") && c.Contains("CurrentUserId"));

            // Should grant Read permission on ApplicationUser
            typePermissions.Should().Contain(tp => tp.TargetType == typeof(ApplicationUser) && tp.ReadState == SecurityPermissionState.Allow);
        }

        [Fact]
        public void CustomerRoleConfigurator_ShouldAllowNavigationToCustomerViews()
        {
            // Arrange
            using var session = CreateInMemorySession();
            var role = new PermissionPolicyRole(session)
            {
                Name = SecurityConstants.CustomerRoleName
            };

            // Act
            CustomerRoleConfigurator.Configure(role);

            // Assert
            var navPermissions = role.NavigationPermissions.ToList();
            navPermissions.Should().NotBeEmpty();
            navPermissions.Should().Contain(np => np.ItemPath.Contains("UserDashboard_View"));
            navPermissions.Should().NotContain(np => np.ItemPath.Contains("Musteri_ListView"));
            navPermissions.Should().Contain(np => np.ItemPath.Contains("Kisi_ListView"));
            navPermissions.Should().Contain(np => np.ItemPath.Contains("Not_ListView"));
            navPermissions.Should().NotContain(np => np.ItemPath.Contains("Yonetim"));
            navPermissions.Should().NotContain(np => np.ItemPath.Contains("AdminDashboard_View"));
        }

        [Fact]
        public void CurrentCustomerOidOperator_ShouldHaveValidProperties()
        {
            // Arrange & Act
            var op = new CurrentCustomerOidOperator();

            // Assert
            op.Name.Should().Be("CurrentCustomerOid");
            op.MinOperandCount.Should().Be(0);
            op.MaxOperandCount.Should().Be(0);
            op.ResultType().Should().Be(typeof(Guid?));
        }
    }
}
