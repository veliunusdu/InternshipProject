#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Project1.Blazor.Server.Controllers;
using Project1.Core.Services.Interfaces;
using Xunit;

namespace Project1.Module.Tests.Api
{
    public class MailTrackingControllerTests
    {
        private static MailTrackingController CreateControllerWithContext(
            IMailTrackingService mailTrackingService,
            out DefaultHttpContext httpContext)
        {
            var controller = new MailTrackingController(mailTrackingService);
            httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            return controller;
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenServiceIsNull()
        {
            // Act
            Action act = () => new MailTrackingController(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("mailTrackingService");
        }

        [Fact]
        public async Task TrackDelivered_ShouldCallService_AndReturnTransparentGifWithHeaders()
        {
            // Arrange
            var serviceMock = new Mock<IMailTrackingService>();
            var noteId = Guid.NewGuid();
            serviceMock
                .Setup(s => s.ProcessDeliveredAsync(noteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var controller = CreateControllerWithContext(serviceMock.Object, out var httpContext);

            // Act
            var result = await controller.TrackDelivered(noteId);

            // Assert
            result.Should().BeOfType<FileContentResult>();
            var fileResult = (FileContentResult)result;
            fileResult.ContentType.Should().Be("image/gif");
            fileResult.FileContents.Length.Should().Be(43, "1x1 şeffaf GIF 43 byte olmalıdır");

            serviceMock.Verify(s => s.ProcessDeliveredAsync(noteId, It.IsAny<CancellationToken>()), Times.Once);

            httpContext.Response.Headers.CacheControl.ToString().Should().Contain("no-cache");
            httpContext.Response.Headers.Pragma.ToString().Should().Contain("no-cache");
        }

        [Fact]
        public async Task TrackRead_ShouldCallService_AndReturnTransparentGifWithHeaders_WhenRedirectIsFalse()
        {
            // Arrange
            var serviceMock = new Mock<IMailTrackingService>();
            var noteId = Guid.NewGuid();
            serviceMock
                .Setup(s => s.ProcessReadAsync(noteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var controller = CreateControllerWithContext(serviceMock.Object, out var httpContext);

            // Act
            var result = await controller.TrackRead(noteId, redirect: false);

            // Assert
            result.Should().BeOfType<FileContentResult>();
            var fileResult = (FileContentResult)result;
            fileResult.ContentType.Should().Be("image/gif");
            fileResult.FileContents.Length.Should().Be(43, "1x1 şeffaf GIF 43 byte olmalıdır");

            serviceMock.Verify(s => s.ProcessReadAsync(noteId, It.IsAny<CancellationToken>()), Times.Once);

            httpContext.Response.Headers.CacheControl.ToString().Should().Contain("no-cache");
            httpContext.Response.Headers.Pragma.ToString().Should().Contain("no-cache");
        }

        [Fact]
        public async Task TrackRead_ShouldCallService_AndReturnRedirect_WhenRedirectIsTrue()
        {
            // Arrange
            var serviceMock = new Mock<IMailTrackingService>();
            var noteId = Guid.NewGuid();
            serviceMock
                .Setup(s => s.ProcessReadAsync(noteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var controller = CreateControllerWithContext(serviceMock.Object, out var httpContext);

            // Act
            var result = await controller.TrackRead(noteId, redirect: true);

            // Assert
            result.Should().BeOfType<RedirectResult>();
            var redirectResult = (RedirectResult)result;
            redirectResult.Url.Should().Be("/#Not_ListView");

            serviceMock.Verify(s => s.ProcessReadAsync(noteId, It.IsAny<CancellationToken>()), Times.Once);

            httpContext.Response.Headers.CacheControl.ToString().Should().Contain("no-cache");
        }
    }
}
