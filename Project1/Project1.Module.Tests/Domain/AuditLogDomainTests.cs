#nullable enable
using System.Linq;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using FluentAssertions;
using Project1.Module.BusinessObjects.Enums;
using Project1.Module.Models.Audit;
using Project1.Module.Models.Customers;
using Project1.Module.Models.Notes;
using Xunit;

namespace Project1.Module.Tests.Domain
{
    public class AuditLogDomainTests
    {
        private UnitOfWork CreateInMemoryUnitOfWork()
        {
            var dataStore = new InMemoryDataStore(AutoCreateOption.DatabaseAndSchema);
            var dataLayer = new SimpleDataLayer(dataStore);
            return new UnitOfWork(dataLayer);
        }

        [Fact]
        public void CreateNote_ShouldCreateAuditLog_WithCreatedAction()
        {
            // Arrange
            using var uow = CreateInMemoryUnitOfWork();

            // Act
            var not = new Not(uow)
            {
                Baslik = "Yeni Deneme Notu",
                Icerik = "İçerik",
                Derece = NotDerecesi.Acil
            };
            uow.CommitChanges();

            // Assert
            var log = uow.Query<AuditLog>().FirstOrDefault(l => l.VarlikId == not.Oid);
            log.Should().NotBeNull();
            log!.IslemTuru.Should().Be("Oluşturuldu");
            log.VarlikTipi.Should().Be("Not");
            log.Aciklama.Should().Contain("Yeni Deneme Notu");
        }

        [Fact]
        public void UpdateNote_ShouldCreateAuditLog_WithUpdatedAction()
        {
            // Arrange
            using var uow = CreateInMemoryUnitOfWork();
            var not = new Not(uow)
            {
                Baslik = "Orijinal Başlık",
                Icerik = "İçerik",
                Derece = NotDerecesi.Normal
            };
            uow.CommitChanges();

            // Act
            not.Baslik = "Değiştirilmiş Başlık";
            uow.CommitChanges();

            // Assert
            var logs = uow.Query<AuditLog>().Where(l => l.VarlikId == not.Oid).ToList();
            logs.Should().HaveCountGreaterThanOrEqualTo(2);
            logs.Should().Contain(l => l.IslemTuru == "Oluşturuldu");
            logs.Should().Contain(l => l.IslemTuru == "Güncellendi" && l.Aciklama.Contains("Değiştirilmiş Başlık"));
        }

        [Fact]
        public void CreateMusteri_ShouldCreateAuditLog_WithCreatedAction()
        {
            // Arrange
            using var uow = CreateInMemoryUnitOfWork();

            // Act
            var musteri = new Musteri(uow)
            {
                Ad = "Deneme A.Ş.",
                Telefon = "02120000000"
            };
            uow.CommitChanges();

            // Assert
            var log = uow.Query<AuditLog>().FirstOrDefault(l => l.VarlikId == musteri.Oid);
            log.Should().NotBeNull();
            log!.IslemTuru.Should().Be("Oluşturuldu");
            log.VarlikTipi.Should().Be("Müşteri");
            log.Aciklama.Should().Contain("Deneme A.Ş.");
        }

        [Fact]
        public void CreateKisi_ShouldCreateAuditLog_WithCreatedAction()
        {
            // Arrange
            using var uow = CreateInMemoryUnitOfWork();

            // Act
            var kisi = new Kisi(uow)
            {
                Ad = "Ali",
                Soyad = "Veli",
                Email = "ali.veli@example.com"
            };
            uow.CommitChanges();

            // Assert
            var log = uow.Query<AuditLog>().FirstOrDefault(l => l.VarlikId == kisi.Oid);
            log.Should().NotBeNull();
            log!.IslemTuru.Should().Be("Oluşturuldu");
            log.VarlikTipi.Should().Be("Kişi");
            log.Aciklama.Should().Contain("Ali Veli");
        }
    }
}
