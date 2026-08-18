#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Project1.Module.Services.Implementations;
using Project1.Core.Services.Interfaces;
using Xunit;

namespace Project1.Module.Tests.Services
{
    public class EmailServiceTests
    {
        [Fact]
        public async Task SendNoteNotificationEmailAsync_ShouldReturnFailure_WhenConfigurationIsInvalid()
        {
            // Arrange: Geçersiz SMTP host konfigürasyonu
            var invalidSettings = new EmailSettings
            {
                SmtpHost = "", // Boş SMTP host
                SmtpPort = 587,
                SenderEmail = "test@example.com",
                SenderPassword = "password"
            };
            var emailService = new EmailService(invalidSettings);
            var request = new SendNoteNotificationRequest(
                NoteId: Guid.NewGuid(),
                ToEmail: "recipient@example.com",
                RecipientName: "Ahmet Yılmaz",
                Title: "Test Notu",
                Content: "Test İçeriği",
                Severity: "Normal",
                CustomerName: "Test Müşteri"
            );

            // Act
            EmailResult result = await emailService.SendNoteNotificationEmailAsync(request);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("SMTP sunucusu yapılandırılmamış");
        }

        [Fact]
        public async Task SendNoteNotificationEmailAsync_ShouldReturnFailure_WhenToEmailIsEmpty()
        {
            // Arrange: Geçerli konfigürasyon fakat boş alıcı e-posta adresi
            var settings = new EmailSettings
            {
                SmtpHost = "smtp.test.com",
                SmtpPort = 587,
                SenderEmail = "sender@example.com",
                SenderPassword = "password"
            };
            var emailService = new EmailService(settings);
            var request = new SendNoteNotificationRequest(
                NoteId: Guid.NewGuid(),
                ToEmail: "", // Boş e-posta adresi
                RecipientName: "Ahmet Yılmaz",
                Title: "Test Notu",
                Content: "Test İçeriği",
                Severity: "Normal",
                CustomerName: "Test Müşteri"
            );

            // Act
            EmailResult result = await emailService.SendNoteNotificationEmailAsync(request);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("E-posta adresi belirtilmemiş.");
        }

        [Fact]
        public async Task SendNoteNotificationEmailAsync_ShouldThrowArgumentNullException_WhenRequestIsNull()
        {
            // Arrange
            var settings = new EmailSettings
            {
                SmtpHost = "smtp.test.com",
                SmtpPort = 587,
                SenderEmail = "sender@example.com",
                SenderPassword = "password"
            };
            var emailService = new EmailService(settings);

            // Act
            Func<Task> act = async () => await emailService.SendNoteNotificationEmailAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("request");
        }

        [Fact]
        public async Task SendNoteNotificationEmailAsync_ShouldReturnFailure_WhenCancellationTokenIsCancelled()
        {
            // Arrange
            var settings = new EmailSettings
            {
                SmtpHost = "smtp.test.com",
                SmtpPort = 587,
                SenderEmail = "sender@example.com",
                SenderPassword = "password"
            };
            var emailService = new EmailService(settings);
            var request = new SendNoteNotificationRequest(
                NoteId: Guid.NewGuid(),
                ToEmail: "recipient@example.com",
                RecipientName: "Ahmet Yılmaz",
                Title: "Test Notu",
                Content: "Test İçeriği",
                Severity: "Normal",
                CustomerName: "Test Müşteri"
            );

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            EmailResult result = await emailService.SendNoteNotificationEmailAsync(request, cts.Token);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("iptal");
        }

        [Fact]
        public async Task SendNoteNotificationEmailAsync_ShouldLogWarning_WhenCancellationTokenIsCancelled()
        {
            // Arrange
            var settings = new EmailSettings
            {
                SmtpHost = "smtp.test.com",
                SmtpPort = 587,
                SenderEmail = "sender@example.com",
                SenderPassword = "password"
            };
            var mockLogger = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<EmailService>>();
            var emailService = new EmailService(settings, mockLogger.Object);
            var request = new SendNoteNotificationRequest(
                NoteId: Guid.NewGuid(),
                ToEmail: "recipient@example.com",
                RecipientName: "Ahmet Yılmaz",
                Title: "Test Notu",
                Content: "Test İçeriği",
                Severity: "Normal",
                CustomerName: "Test Müşteri"
            );

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            var result = await emailService.SendNoteNotificationEmailAsync(request, cts.Token);

            // Assert
            result.Success.Should().BeFalse();
            mockLogger.Verify(
                x => x.Log(
                    Microsoft.Extensions.Logging.LogLevel.Warning,
                    Moq.It.IsAny<Microsoft.Extensions.Logging.EventId>(),
                    Moq.It.Is<Moq.It.IsAnyType>((v, t) => v.ToString()!.Contains("iptal")),
                    Moq.It.IsAny<Exception>(),
                    Moq.It.IsAny<Func<Moq.It.IsAnyType, Exception?, string>>()),
                Moq.Times.AtLeastOnce);
        }

        [Fact]
        public async Task SendNoteNotificationEmailAsync_ShouldLogWarning_WhenConfigurationIsInvalid()
        {
            // Arrange
            var settings = new EmailSettings
            {
                SmtpHost = "",
                SmtpPort = 587,
                SenderEmail = "sender@example.com",
                SenderPassword = "password"
            };
            var mockLogger = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<EmailService>>();
            var emailService = new EmailService(settings, mockLogger.Object);
            var request = new SendNoteNotificationRequest(
                NoteId: Guid.NewGuid(),
                ToEmail: "recipient@example.com",
                RecipientName: "Ahmet Yılmaz",
                Title: "Test Notu",
                Content: "Test İçeriği",
                Severity: "Normal",
                CustomerName: "Test Müşteri"
            );

            // Act
            var result = await emailService.SendNoteNotificationEmailAsync(request);

            // Assert
            result.Success.Should().BeFalse();
            mockLogger.Verify(
                x => x.Log(
                    Microsoft.Extensions.Logging.LogLevel.Warning,
                    Moq.It.IsAny<Microsoft.Extensions.Logging.EventId>(),
                    Moq.It.Is<Moq.It.IsAnyType>((v, t) => v.ToString()!.Contains("yapılandırması geçersiz")),
                    Moq.It.IsAny<Exception>(),
                    Moq.It.IsAny<Func<Moq.It.IsAnyType, Exception?, string>>()),
                Moq.Times.AtLeastOnce);
        }
    }
}
