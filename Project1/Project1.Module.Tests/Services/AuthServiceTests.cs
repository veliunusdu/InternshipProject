#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.ExpressApp;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Project1.Business.Services.Implementations;
using Project1.Core.Services.Interfaces;
using Project1.DTOs.Auth;
using Project1.Module.BusinessObjects.Security;
using Project1.Module.Models.Customers;
using Xunit;

namespace Project1.Module.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IObjectSpaceFactory> _mockObjectSpaceFactory;
        private readonly Mock<IObjectSpace> _mockObjectSpace;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly EmailSettings _emailSettings;
        private readonly Mock<ILogger<AuthService>> _mockLogger;

        public AuthServiceTests()
        {
            _mockObjectSpaceFactory = new Mock<IObjectSpaceFactory>();
            _mockObjectSpace = new Mock<IObjectSpace>();
            _mockEmailService = new Mock<IEmailService>();
            _emailSettings = new EmailSettings
            {
                BaseUrl = "https://localhost:5001",
                SenderEmail = "system@crm.com"
            };
            _mockLogger = new Mock<ILogger<AuthService>>();

            _mockObjectSpaceFactory
                .Setup(f => f.CreateObjectSpace(It.IsAny<Type>()))
                .Returns(_mockObjectSpace.Object);
        }

        [Fact]
        public async Task ConfirmEmailAsync_WithEmptyUserIdOrToken_ShouldReturnFail()
        {
            // Arrange
            var service = new AuthService(
                _mockObjectSpaceFactory.Object,
                _mockEmailService.Object,
                _emailSettings,
                nonSecuredObjectSpaceFactory: null,
                httpContextAccessor: null,
                logger: _mockLogger.Object);

            // Act
            var resultEmptyGuid = await service.ConfirmEmailAsync(Guid.Empty, "some-token");
            var resultEmptyToken = await service.ConfirmEmailAsync(Guid.NewGuid(), "");

            // Assert
            resultEmptyGuid.Success.Should().BeFalse();
            resultEmptyToken.Success.Should().BeFalse();
        }

        [Fact]
        public async Task ConfirmEmailAsync_WhenUserNotFound_ShouldReturnFail()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockObjectSpace
                .Setup(os => os.GetObjectByKey<ApplicationUser>(userId))
                .Returns((ApplicationUser)null!);

            var service = new AuthService(
                _mockObjectSpaceFactory.Object,
                _mockEmailService.Object,
                _emailSettings,
                nonSecuredObjectSpaceFactory: null,
                httpContextAccessor: null,
                logger: _mockLogger.Object);

            // Act
            var result = await service.ConfirmEmailAsync(userId, "valid-token");

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("bulunamadı");
        }

        [Fact]
        public async Task RegisterCustomerAsync_WithNullRequest_ShouldThrowArgumentNullException()
        {
            // Arrange
            var service = new AuthService(
                _mockObjectSpaceFactory.Object,
                _mockEmailService.Object,
                _emailSettings,
                nonSecuredObjectSpaceFactory: null,
                httpContextAccessor: null,
                logger: _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                service.RegisterCustomerAsync(null!));
        }
    }
}
