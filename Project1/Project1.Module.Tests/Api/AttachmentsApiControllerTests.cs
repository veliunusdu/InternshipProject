#nullable enable
using System;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Project1.Blazor.Server.Controllers;
using Project1.Core.Services.Interfaces;
using Xunit;

namespace Project1.Module.Tests.Api
{
    public class AttachmentsApiControllerTests
    {
        [Fact]
        public async Task DownloadAttachment_ShouldReturnFile_WhenAttachmentExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dummyBytes = Encoding.UTF8.GetBytes("fake pdf content");
            var mockNoteService = new Mock<INoteService>();
            mockNoteService
                .Setup(s => s.GetAttachmentFileAsync(id, default))
                .ReturnsAsync((dummyBytes, "test.pdf", "application/pdf"));

            var controller = new AttachmentsApiController(mockNoteService.Object);

            // Act
            var result = await controller.DownloadAttachment(id);

            // Assert
            result.Should().BeOfType<FileContentResult>();
            var fileResult = (FileContentResult)result;
            fileResult.ContentType.Should().Be("application/pdf");
            fileResult.FileDownloadName.Should().Be("test.pdf");
            fileResult.FileContents.Should().BeEquivalentTo(dummyBytes);
        }

        [Fact]
        public async Task DownloadAttachment_ShouldReturnNotFound_WhenAttachmentDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            var mockNoteService = new Mock<INoteService>();
            mockNoteService
                .Setup(s => s.GetAttachmentFileAsync(id, default))
                .ReturnsAsync(((byte[] Bytes, string FileName, string ContentType)?)null);

            var controller = new AttachmentsApiController(mockNoteService.Object);

            // Act
            var result = await controller.DownloadAttachment(id);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
