#nullable enable
using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Project1.Blazor.Server.Controllers;
using Project1.Module.BusinessObjects.Enums;
using Project1.Module.Models.Audit;
using Project1.Module.Models.Enums;
using Project1.Module.Models.Notes;
using Xunit;

namespace Project1.Module.Tests.Api
{
    public class MailTrackingControllerTests
    {
        private (IObjectSpace ObjectSpace, UnitOfWork UnitOfWork) CreateRealObjectSpace()
        {
            var dataStore = new InMemoryDataStore(AutoCreateOption.DatabaseAndSchema);
            var dataLayer = new SimpleDataLayer(dataStore);
            var uow = new UnitOfWork(dataLayer);
            var objectSpaceMock = new Mock<IObjectSpace>();

            objectSpaceMock
                .Setup(os => os.GetObjectByKey<Not>(It.IsAny<object>()))
                .Returns((object key) => uow.GetObjectByKey<Not>(key));

            objectSpaceMock
                .Setup(os => os.CreateObject<AuditLog>())
                .Returns(() => new AuditLog(uow));

            objectSpaceMock
                .Setup(os => os.CommitChanges())
                .Callback(() => uow.CommitChanges());

            return (objectSpaceMock.Object, uow);
        }

        private MailTrackingController CreateControllerWithContext(
            IObjectSpace objectSpace,
            out DefaultHttpContext httpContext,
            Mock<ILogger<MailTrackingController>>? loggerMock = null)
        {
            var factoryMock = new Mock<IObjectSpaceFactory>();
            factoryMock
                .Setup(f => f.CreateObjectSpace(typeof(Not)))
                .Returns(objectSpace);

            loggerMock ??= new Mock<ILogger<MailTrackingController>>();
            var controller = new MailTrackingController(factoryMock.Object, null, loggerMock.Object);

            httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        [Fact]
        public void TrackRead_ShouldSetMailDurumuToOkundu_AndSetOkunmaTarihi_WhenNoteExists()
        {
            // Arrange
            var (objectSpace, uow) = CreateRealObjectSpace();
            var not = new Not(uow)
            {
                Baslik = "Test Notu",
                Icerik = "İçerik",
                Derece = NotDerecesi.Normal,
                MailDurumu = MailDurumu.Iletildi,
                MailGonderilmeTarihi = DateTime.Now.AddMinutes(-10),
                MailIletilmeTarihi = DateTime.Now.AddMinutes(-9)
            };
            uow.CommitChanges();

            var controller = CreateControllerWithContext(objectSpace, out var httpContext);

            // Act
            var result = controller.TrackRead(not.Oid);

            // Assert
            result.Should().BeOfType<FileContentResult>();
            var fileResult = (FileContentResult)result;
            fileResult.ContentType.Should().Be("image/gif");
            fileResult.FileContents.Length.Should().Be(43, "1x1 şeffaf GIF 43 byte olmalıdır");

            // Not güncellenmiş olmalı
            not.MailDurumu.Should().Be(MailDurumu.Okundu);
            not.MailOkunmaTarihi.Should().NotBeNull();
            not.MailOkunmaTarihi.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));

            // Cache-Control başlıkları kontrolü
            httpContext.Response.Headers.CacheControl.ToString().Should().Contain("no-cache");
            httpContext.Response.Headers.Pragma.ToString().Should().Contain("no-cache");
        }

        [Fact]
        public void TrackRead_ShouldPreserveInitialOkunmaTarihi_WhenCalledMultipleTimes()
        {
            // Arrange: Zaten 1 saat önce okunmuş bir not
            var (objectSpace, uow) = CreateRealObjectSpace();
            var initialReadDate = DateTime.Now.AddHours(-1);
            var not = new Not(uow)
            {
                Baslik = "Zaten Okunmuş Not",
                Icerik = "İçerik",
                Derece = NotDerecesi.Normal,
                MailDurumu = MailDurumu.Okundu,
                MailOkunmaTarihi = initialReadDate
            };
            uow.CommitChanges();

            var controller = CreateControllerWithContext(objectSpace, out _);

            // Act: Piksel ikinci kez çağrılıyor
            var result = controller.TrackRead(not.Oid);

            // Assert: Okunma tarihi değişmemeli (Idempotency)
            result.Should().BeOfType<FileContentResult>();
            not.MailDurumu.Should().Be(MailDurumu.Okundu);
            not.MailOkunmaTarihi.Should().Be(initialReadDate, "İkinci açılışta ilk okuma tarihi ezilmemelidir");
        }

        [Fact]
        public void TrackRead_ShouldReturnTransparentGif_EvenWhenNoteNotFound()
        {
            // Arrange
            var (objectSpace, _) = CreateRealObjectSpace();
            var controller = CreateControllerWithContext(objectSpace, out _);
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = controller.TrackRead(nonExistentId);

            // Assert: Hata fırlatılmamalı, yine de şeffaf GIF dönmeli
            result.Should().BeOfType<FileContentResult>();
            var fileResult = (FileContentResult)result;
            fileResult.ContentType.Should().Be("image/gif");
        }

        [Fact]
        public void TrackDelivered_ShouldSetMailDurumuToIletildi_AndSetIletilmeTarihi_WhenNoteExists()
        {
            // Arrange
            var (objectSpace, uow) = CreateRealObjectSpace();
            var not = new Not(uow)
            {
                Baslik = "İletilecek Not",
                Icerik = "İçerik",
                Derece = NotDerecesi.Normal,
                MailDurumu = MailDurumu.Gonderildi,
                MailGonderilmeTarihi = DateTime.Now.AddMinutes(-5)
            };
            uow.CommitChanges();

            var controller = CreateControllerWithContext(objectSpace, out var httpContext);

            // Act
            var result = controller.TrackDelivered(not.Oid);

            // Assert
            result.Should().BeOfType<FileContentResult>();
            not.MailDurumu.Should().Be(MailDurumu.Iletildi);
            not.MailIletilmeTarihi.Should().NotBeNull();
            not.MailIletilmeTarihi.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void TrackRead_ShouldLogWarning_AndReturnGif_WhenExceptionOccurs()
        {
            // Arrange: ObjectSpace hata fırlatacak şekilde Mock'lanıyor
            var objectSpaceMock = new Mock<IObjectSpace>();
            objectSpaceMock
                .Setup(os => os.GetObjectByKey<Not>(It.IsAny<object>()))
                .Throws(new InvalidOperationException("Simulated Database Connection Failure"));

            var loggerMock = new Mock<ILogger<MailTrackingController>>();
            var controller = CreateControllerWithContext(objectSpaceMock.Object, out _, loggerMock);

            // Act
            var result = controller.TrackRead(Guid.NewGuid());

            // Assert: Exception alıcıya sızmamalı, loglanmalı ve GIF dönmeli
            result.Should().BeOfType<FileContentResult>();
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Mail tracking pikseli işlenirken hata oluştu")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
