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
using Project1.Module.BusinessObjects;
using Project1.Module.BusinessObjects.Security;
using Project1.Module.Models.Audit;
using Project1.Module.Models.Customers;
using Project1.Module.Models.Notes;
using Project1.Module.Models.Tenants;
using Project1.Module.Security;
using Xunit;

namespace Project1.Module.Tests.Security
{
    public class FirmaRowLevelSecurityTests
    {
        static FirmaRowLevelSecurityTests()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var assemblyName = new System.Reflection.AssemblyName(args.Name).Name;
                var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName + ".dll");
                if (System.IO.File.Exists(path))
                {
                    return System.Reflection.Assembly.LoadFrom(path);
                }
                return null;
            };
        }

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
        public void CurrentUserFirmaOperator_ShouldHaveValidProperties()
        {
            // Arrange & Act
            var op = new CurrentUserFirmaOperator();

            // Assert
            op.Name.Should().Be("CurrentUserFirma");
            op.ResultType().Should().Be(typeof(object));
        }

        [Fact]
        public void CurrentFirmaOidOperator_ShouldHaveValidProperties()
        {
            // Arrange & Act
            var op = new CurrentFirmaOidOperator();

            // Assert
            op.Name.Should().Be("CurrentFirmaOid");
            op.MinOperandCount.Should().Be(0);
            op.MaxOperandCount.Should().Be(0);
            op.ResultType().Should().Be(typeof(Guid?));
        }

        [Fact]
        public void FirmaKullanicisiRole_ShouldConfigureCriteriaAndDenyWriteOnFirmaMember()
        {
            // Arrange
            using var session = CreateInMemorySession();
            var role = new PermissionPolicyRole(session)
            {
                Name = SecurityConstants.FirmaKullanicisiRoleName
            };

            // Act
            FirmaKullanicisiRoleConfigurator.Configure(role);

            // Assert
            role.IsAdministrative.Should().BeFalse();
            role.PermissionPolicy.Should().Be(SecurityPermissionPolicy.DenyAllByDefault);

            var typePermissions = role.TypePermissions.ToList();
            var objectCriteria = typePermissions.SelectMany(tp => tp.ObjectPermissions).Select(op => op.Criteria).ToList();
            objectCriteria.Should().Contain(c => c.Contains("CurrentUserFirma") || c.Contains("CurrentFirmaOid"));

            // Check member permissions for Musteri, Kisi, Not -> Firma Write Deny
            var memberPermissions = typePermissions.SelectMany(tp => tp.MemberPermissions).ToList();
            memberPermissions.Should().Contain(mp => mp.Members == nameof(Musteri.Firma) && mp.WriteState == SecurityPermissionState.Deny);
        }

        [Fact]
        public void StandardUserRole_ShouldEnforceRowLevelSecurityCriteria_ForFirmaAwareEntities()
        {
            // Arrange
            using var session = CreateInMemorySession();
            var role = new PermissionPolicyRole(session)
            {
                Name = SecurityConstants.StandardUserRoleName
            };

            // Act
            StandardUserRoleConfigurator.Configure(role);

            // Assert
            role.IsAdministrative.Should().BeFalse();
            role.PermissionPolicy.Should().Be(SecurityPermissionPolicy.DenyAllByDefault);

            var typePermissions = role.TypePermissions.ToList();
            typePermissions.Should().NotBeEmpty();

            // Musteri, Kisi, Not ve Firma için Object Permission kriterleri olmalı
            var objectPermissions = typePermissions.SelectMany(tp => tp.ObjectPermissions).ToList();
            objectPermissions.Should().NotBeEmpty();

            var criteriaList = objectPermissions.Select(op => op.Criteria).ToList();
            criteriaList.Should().Contain(c => c.Contains("CurrentFirmaOid") || c.Contains("Firma.Oid"));

            // Firma nesnesi için izin kontrolü
            typePermissions.Should().Contain(tp => tp.TargetType == typeof(Firma));
            typePermissions.Should().Contain(tp => tp.TargetType == typeof(Musteri));
            typePermissions.Should().Contain(tp => tp.TargetType == typeof(Kisi));
            typePermissions.Should().Contain(tp => tp.TargetType == typeof(Not));
        }

        [Fact]
        public void AdminRole_ShouldHaveFullAccess_ToFirmaAndEntities()
        {
            // Arrange
            using var session = CreateInMemorySession();
            var role = new PermissionPolicyRole(session)
            {
                Name = SecurityConstants.AdministratorRoleName
            };

            // Act
            AdminRoleConfigurator.Configure(role);

            // Assert
            role.IsAdministrative.Should().BeFalse();
            var typePermissions = role.TypePermissions.ToList();

            var typeNames = typePermissions.Select(p => p.TargetType?.Name ?? p.ToString()).ToList();
            typeNames.Should().Contain(nameof(Firma));
            typeNames.Should().Contain(nameof(Musteri));
            typeNames.Should().Contain(nameof(Kisi));
            typeNames.Should().Contain(nameof(Not));

            var firmaPerm = typePermissions.FirstOrDefault(tp => tp.TargetType == typeof(Firma));
            firmaPerm.Should().NotBeNull();
            firmaPerm!.ReadState.Should().Be(SecurityPermissionState.Allow);
            firmaPerm.WriteState.Should().Be(SecurityPermissionState.Allow);
            firmaPerm.CreateState.Should().Be(SecurityPermissionState.Allow);
            firmaPerm.DeleteState.Should().Be(SecurityPermissionState.Allow);
        }

        [Fact]
        public void DomainObjects_ShouldImplementIFirmaAware_AndHaveFirmaAssociation()
        {
            // Arrange
            using var session = CreateInMemorySession();
            var firma = new Firma(session)
            {
                Ad = "Acme Teknoloji A.Ş.",
                VergiNo = "1234567890"
            };

            var user = new ApplicationUser(session)
            {
                UserName = "test_user",
                Firma = firma
            };

            var musteri = new Musteri(session)
            {
                Ad = "Hedef Müşteri",
                Firma = firma
            };

            var kisi = new Kisi(session)
            {
                Ad = "Ali",
                Soyad = "Veli",
                Firma = firma,
                Musteri = musteri
            };

            var not = new Not(session)
            {
                Baslik = "Test Notu",
                Icerik = "İçerik",
                Firma = firma,
                Musteri = musteri,
                Kisi = kisi
            };

            // Assert
            musteri.Should().BeAssignableTo<IFirmaAware>();
            kisi.Should().BeAssignableTo<IFirmaAware>();
            not.Should().BeAssignableTo<IFirmaAware>();

            musteri.Firma.Should().Be(firma);
            kisi.Firma.Should().Be(firma);
            not.Firma.Should().Be(firma);
            user.Firma.Should().Be(firma);

            firma.Musteriler.Should().Contain(musteri);
            firma.Kisiler.Should().Contain(kisi);
            firma.Notlar.Should().Contain(not);
            firma.Kullanicilar.Should().Contain(user);
        }
    }
}
