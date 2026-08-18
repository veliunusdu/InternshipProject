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
    public class SoftDeleteDomainTests
    {
        private UnitOfWork CreateInMemoryUnitOfWork()
        {
            var dataStore = new InMemoryDataStore(AutoCreateOption.DatabaseAndSchema);
            var dataLayer = new SimpleDataLayer(dataStore);
            return new UnitOfWork(dataLayer);
        }

        [Fact]
        public void Delete_ShouldPerformSoftDelete_AndExcludeFromQueries_WhenNoteIsDeleted()
        {
            // Arrange
            using var uow = CreateInMemoryUnitOfWork();
            var not = new Not(uow)
            {
                Baslik = "Silinecek Not",
                Icerik = "İçerik",
                Derece = NotDerecesi.Normal
            };
            uow.CommitChanges();

            // Act: Silme işlemi yapılıyor
            not.Delete();
            uow.CommitChanges();

            // Assert: Normal sorguda silinen not gelmemeli (Deferred Deletion / Soft Delete)
            var activeNotes = uow.Query<Not>().Where(n => n.Baslik == "Silinecek Not").ToList();
            activeNotes.Should().BeEmpty("Silinen not aktif sorgularda listelenmemeli (Soft Delete)");

            // Not nesnesi IsDeleted işaretli olmalı
            not.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public void Delete_ShouldCreateAuditLog_WithSoftDeleteAction_WhenNoteIsDeleted()
        {
            // Arrange
            using var uow = CreateInMemoryUnitOfWork();
            var not = new Not(uow)
            {
                Baslik = "Audit Soft Delete Notu",
                Icerik = "İçerik",
                Derece = NotDerecesi.Onemli
            };
            uow.CommitChanges();

            // Act
            not.Delete();
            uow.CommitChanges();

            // Assert: AuditLog tablosunda 'Silindi (Soft Delete)' kaydı oluşmalı
            var deleteAuditLog = uow.Query<AuditLog>()
                .FirstOrDefault(l => l.VarlikId == not.Oid && l.IslemTuru == "Silindi (Soft Delete)");

            deleteAuditLog.Should().NotBeNull();
            deleteAuditLog!.VarlikTipi.Should().Be("Not");
            deleteAuditLog.Aciklama.Should().Contain("Audit Soft Delete Notu");
        }

        [Fact]
        public void Delete_ShouldPerformSoftDelete_WhenMusteriIsDeleted()
        {
            // Arrange
            using var uow = CreateInMemoryUnitOfWork();
            var musteri = new Musteri(uow)
            {
                Ad = "Test Şirketi",
                Telefon = "05551234567"
            };
            uow.CommitChanges();

            // Act
            musteri.Delete();
            uow.CommitChanges();

            // Assert
            var activeCustomers = uow.Query<Musteri>().Where(m => m.Ad == "Test Şirketi").ToList();
            activeCustomers.Should().BeEmpty();
            musteri.IsDeleted.Should().BeTrue();

            var auditLog = uow.Query<AuditLog>()
                .FirstOrDefault(l => l.VarlikId == musteri.Oid && l.IslemTuru == "Silindi (Soft Delete)");
            auditLog.Should().NotBeNull();
            auditLog!.VarlikTipi.Should().Be("Müşteri");
        }

        [Fact]
        public void Delete_ShouldPerformSoftDelete_WhenKisiIsDeleted()
        {
            // Arrange
            using var uow = CreateInMemoryUnitOfWork();
            var kisi = new Kisi(uow)
            {
                Ad = "Mehmet",
                Soyad = "Kaya",
                Email = "mehmet@example.com"
            };
            uow.CommitChanges();

            // Act
            kisi.Delete();
            uow.CommitChanges();

            // Assert
            var activePeople = uow.Query<Kisi>().Where(k => k.Email == "mehmet@example.com").ToList();
            activePeople.Should().BeEmpty();
            kisi.IsDeleted.Should().BeTrue();

            var auditLog = uow.Query<AuditLog>()
                .FirstOrDefault(l => l.VarlikId == kisi.Oid && l.IslemTuru == "Silindi (Soft Delete)");
            auditLog.Should().NotBeNull();
            auditLog!.VarlikTipi.Should().Be("Kişi");
        }
    }
}
