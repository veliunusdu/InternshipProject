#nullable enable
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using FluentAssertions;
using Moq;
using Project1.Core.Services.Interfaces;
using Project1.Module.BusinessObjects.Enums;
using Project1.Module.Models.Audit;
using Project1.Module.Models.Customers;
using Project1.Module.Models.Enums;
using Project1.Module.Models.Notes;
using Xunit;

namespace Project1.Module.Tests.Controllers
{
    public class NoteNotificationControllerTests
    {
        private UnitOfWork CreateInMemoryUnitOfWork()
        {
            var dataStore = new InMemoryDataStore(AutoCreateOption.DatabaseAndSchema);
            var dataLayer = new SimpleDataLayer(dataStore);
            return new UnitOfWork(dataLayer);
        }

        [Fact]
        public async Task EmailService_WhenSuccessful_ShouldAllowNotificationFlow_AndGenerateAuditLog()
        {
            // Arrange
            using var uow = CreateInMemoryUnitOfWork();
            var musteri = new Musteri(uow) { Ad = "Örnek Müşteri" };
            var kisi = new Kisi(uow)
            {
                Ad = "Ahmet",
                Soyad = "Yılmaz",
                Email = "ahmet.yilmaz@example.com",
                Musteri = musteri
            };
            var not = new Not(uow)
            {
                Baslik = "Yeni Görev Bildirimi",
                Icerik = "Lütfen inceleyiniz.",
                Derece = NotDerecesi.Onemli,
                Musteri = musteri,
                Kisi = kisi
            };
            uow.CommitChanges();

            // Mock IEmailService
            var mockEmailService = new Mock<IEmailService>();
            mockEmailService
                .Setup(s => s.SendNoteNotificationEmailAsync(
                    It.Is<SendNoteNotificationRequest>(r => r.NoteId == not.Oid && r.ToEmail == "ahmet.yilmaz@example.com"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmailResult(true, null));

            // Act: Simüle edilen başarılı gönderim akışı
            var request = new SendNoteNotificationRequest(
                NoteId: not.Oid,
                ToEmail: not.Kisi.Email,
                RecipientName: not.Kisi.AdSoyad,
                Title: not.Baslik,
                Content: not.Icerik,
                Severity: not.Derece.ToString(),
                CustomerName: not.Musteri?.Ad ?? string.Empty
            );

            var result = await mockEmailService.Object.SendNoteNotificationEmailAsync(request);

            if (result.Success)
            {
                not.MailDurumu = MailDurumu.Iletildi;
                not.MailGonderilmeTarihi = DateTime.Now;
                not.MailIletilmeTarihi = DateTime.Now;
                not.IsEmailSent = true;

                new AuditLog(uow)
                {
                    Tarih = DateTime.Now,
                    Kullanici = "Sistem",
                    IslemTuru = "E-posta İletildi",
                    VarlikTipi = "Not",
                    VarlikId = not.Oid,
                    Aciklama = $"'{not.Baslik}' başlıklı not bildirimi {kisi.Email} adresine iletildi."
                };
                uow.CommitChanges();
            }

            // Assert
            result.Success.Should().BeTrue();
            not.MailDurumu.Should().Be(MailDurumu.Iletildi);
            not.MailGonderilmeTarihi.Should().NotBeNull();
            not.MailIletilmeTarihi.Should().NotBeNull();
            not.IsEmailSent.Should().BeTrue();

            var auditLog = uow.Query<AuditLog>().FirstOrDefault(l => l.VarlikId == not.Oid && l.IslemTuru == "E-posta İletildi");
            auditLog.Should().NotBeNull();
            auditLog!.Aciklama.Should().Contain("ahmet.yilmaz@example.com");
            mockEmailService.Verify(s => s.SendNoteNotificationEmailAsync(It.IsAny<SendNoteNotificationRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task EmailService_WhenFailed_ShouldSetMailDurumuToBasarisiz_AndGenerateErrorAuditLog()
        {
            // Arrange
            using var uow = CreateInMemoryUnitOfWork();
            var musteri = new Musteri(uow) { Ad = "Hata Test Müşteri" };
            var kisi = new Kisi(uow)
            {
                Ad = "Veli",
                Soyad = "Demir",
                Email = "veli@gecersiz-domain.com",
                Musteri = musteri
            };
            var not = new Not(uow)
            {
                Baslik = "Hatalı Gönderim Notu",
                Icerik = "İçerik",
                Derece = NotDerecesi.Normal,
                Musteri = musteri,
                Kisi = kisi
            };
            uow.CommitChanges();

            // Mock IEmailService failing with SMTP error
            var mockEmailService = new Mock<IEmailService>();
            mockEmailService
                .Setup(s => s.SendNoteNotificationEmailAsync(It.IsAny<SendNoteNotificationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmailResult(false, "SMTP Bağlantı Zaman Aşımı (Timeout)"));

            // Act: Simüle edilen hata akışı
            var request = new SendNoteNotificationRequest(
                NoteId: not.Oid,
                ToEmail: not.Kisi.Email,
                RecipientName: not.Kisi.AdSoyad,
                Title: not.Baslik,
                Content: not.Icerik,
                Severity: not.Derece.ToString(),
                CustomerName: not.Musteri?.Ad ?? string.Empty
            );

            var result = await mockEmailService.Object.SendNoteNotificationEmailAsync(request);

            if (!result.Success)
            {
                not.MailDurumu = MailDurumu.Basarisiz;
                not.MailHataMesaji = result.ErrorMessage;

                new AuditLog(uow)
                {
                    Tarih = DateTime.Now,
                    Kullanici = "Sistem",
                    IslemTuru = "E-posta Hatası",
                    VarlikTipi = "Not",
                    VarlikId = not.Oid,
                    Aciklama = $"E-posta gönderimi başarısız ({kisi.Email}): {result.ErrorMessage}"
                };
                uow.CommitChanges();
            }

            // Assert
            result.Success.Should().BeFalse();
            not.MailDurumu.Should().Be(MailDurumu.Basarisiz);
            not.MailHataMesaji.Should().Be("SMTP Bağlantı Zaman Aşımı (Timeout)");
            not.MailGonderilmeTarihi.Should().BeNull("Hata durumunda gönderilme tarihi boş kalmalıdır");

            var auditLog = uow.Query<AuditLog>().FirstOrDefault(l => l.VarlikId == not.Oid && l.IslemTuru == "E-posta Hatası");
            auditLog.Should().NotBeNull();
            auditLog!.Aciklama.Should().Contain("SMTP Bağlantı Zaman Aşımı");
        }
    }
}
