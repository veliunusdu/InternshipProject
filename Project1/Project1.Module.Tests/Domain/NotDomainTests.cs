#nullable enable
using System;
using System.IO;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using FluentAssertions;
using Project1.Core.Enums;
using Project1.Module.Models.Customers;
using Project1.Module.Models.Notes;
using Xunit;

namespace Project1.Module.Tests.Domain
{
    public class NotDomainTests
    {
        private Session CreateInMemorySession()
        {
            var dataStore = new InMemoryDataStore(AutoCreateOption.DatabaseAndSchema);
            var dataLayer = new SimpleDataLayer(dataStore);
            return new Session(dataLayer);
        }

        [Fact]
        public void EmailGonderilebilir_ShouldReturnFalse_WhenEmailIsAlreadySent()
        {
            // Arrange
            using var session = CreateInMemorySession();
            var kisi = new Kisi(session) { Ad = "Ahmet", Soyad = "Yılmaz", Email = "ahmet@example.com" };
            var not = new Not(session)
            {
                Baslik = "Test Notu",
                Icerik = "İçerik",
                Kisi = kisi,
                IsEmailSent = true // E-posta zaten gönderilmiş
            };

            // Act & Assert
            not.EmailGonderilebilir.Should().BeFalse();
        }

        [Fact]
        public void EmailGonderilebilir_ShouldReturnFalse_WhenKisiIsNull()
        {
            // Arrange
            using var session = CreateInMemorySession();
            var not = new Not(session)
            {
                Baslik = "Test Notu",
                Icerik = "İçerik",
                Kisi = null,
                IsEmailSent = false
            };

            // Act & Assert
            not.EmailGonderilebilir.Should().BeFalse();
        }

        [Fact]
        public void EmailGonderilebilir_ShouldReturnFalse_WhenKisiEmailIsEmpty()
        {
            // Arrange
            using var session = CreateInMemorySession();
            var kisi = new Kisi(session) { Ad = "Ahmet", Soyad = "Yılmaz", Email = "" }; // Boş e-posta
            var not = new Not(session)
            {
                Baslik = "Test Notu",
                Icerik = "İçerik",
                Kisi = kisi,
                IsEmailSent = false
            };

            // Act & Assert
            not.EmailGonderilebilir.Should().BeFalse();
        }

        [Fact]
        public void EmailGonderilebilir_ShouldReturnTrue_WhenNoteIsNotSentAndKisiHasValidEmail()
        {
            // Arrange
            using var session = CreateInMemorySession();
            var kisi = new Kisi(session) { Ad = "Ahmet", Soyad = "Yılmaz", Email = "ahmet@example.com" };
            var not = new Not(session)
            {
                Baslik = "Test Notu",
                Icerik = "İçerik",
                Kisi = kisi,
                IsEmailSent = false
            };

            // Act & Assert
            not.EmailGonderilebilir.Should().BeTrue();
        }

        [Fact]
        public void Not_ShouldHaveInitialMailDurumuAsGonderilmedi()
        {
            // Arrange & Act
            using var session = CreateInMemorySession();
            var not = new Not(session);

            // Assert
            not.MailDurumu.Should().Be(MailDurumu.Gonderilmedi);
            not.MailGonderilmeTarihi.Should().BeNull();
            not.MailIletilmeTarihi.Should().BeNull();
            not.MailOkunmaTarihi.Should().BeNull();
            not.MailHataMesaji.Should().BeNull();
        }

        [Fact]
        public void Not_ShouldComputeContentTypeAndFlags_ForPdfAndImages()
        {
            Not.GetContentType("dokuman.pdf").Should().Be("application/pdf");
            Not.GetContentType("resim.png").Should().Be("image/png");
            Not.GetContentType("foto.jpg").Should().Be("image/jpeg");
            Not.GetContentType("foto.jpeg").Should().Be("image/jpeg");
            Not.GetContentType("animasyon.gif").Should().Be("image/gif");
            Not.GetContentType("vektor.svg").Should().Be("image/svg+xml");
            Not.GetContentType("diger.xyz").Should().Be("application/octet-stream");
        }

        [Fact]
        public void Not_ShouldSetDosyaProperty_AndExtractProperties()
        {
            using var session = CreateInMemorySession();
            var not = new Not(session)
            {
                Baslik = "Dosyalı Not",
                Icerik = "İçerik",
                Project2IlePaylas = true
            };

            var fileData = new FileData(session);
            fileData.LoadFromStream("sample.pdf", new MemoryStream(new byte[] { 1, 2, 3 }));
            not.Dosya = fileData;
            not.Save();

            not.DosyaAdi.Should().Be("sample.pdf");
            not.BoyutBytes.Should().Be(3);
            not.IsPdf.Should().BeTrue();
            not.IsImage.Should().BeFalse();
            not.Project2IlePaylas.Should().BeTrue();
        }
    }
}
