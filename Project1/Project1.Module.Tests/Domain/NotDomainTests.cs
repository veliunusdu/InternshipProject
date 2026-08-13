#nullable enable
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using FluentAssertions;
using Project1.Module.BusinessObjects.Customers;
using Project1.Module.BusinessObjects.Notes;
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
    }
}
