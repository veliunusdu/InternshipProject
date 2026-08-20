#nullable enable
using System;
using System.Threading.Tasks;
using DevExpress.ExpressApp;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Project1.Business.Services.Implementations;
using Project1.Core.Enums;
using Project1.Core.Services.Interfaces;
using Project1.Module.Models.Audit;
using Project1.Module.Models.Notes;
using Xunit;

namespace Project1.Module.Tests.Services
{
    public class MailTrackingServiceTests
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

        private MailTrackingService CreateService(
            IObjectSpace objectSpace,
            Mock<ICrmNotificationService>? notificationMock = null,
            Mock<ILogger<MailTrackingService>>? loggerMock = null)
        {
            var factoryMock = new Mock<IObjectSpaceFactory>();
            factoryMock
                .Setup(f => f.CreateObjectSpace(typeof(Not)))
                .Returns(objectSpace);

            return new MailTrackingService(
                factoryMock.Object,
                nonSecuredObjectSpaceFactory: null,
                notificationService: notificationMock?.Object,
                logger: loggerMock?.Object);
        }

        [Fact]
        public async Task ProcessDeliveredAsync_ShouldSetMailDurumuToIletildi_AndSetIletilmeTarihi_WhenNoteExists()
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

            var service = CreateService(objectSpace);

            // Act
            var result = await service.ProcessDeliveredAsync(not.Oid);

            // Assert
            result.Should().BeTrue();
            not.MailDurumu.Should().Be(MailDurumu.Iletildi);
            not.MailIletilmeTarihi.Should().NotBeNull();
            not.MailIletilmeTarihi.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task ProcessDeliveredAsync_ShouldReturnFalse_WhenNoteDoesNotExist()
        {
            // Arrange
            var (objectSpace, _) = CreateRealObjectSpace();
            var service = CreateService(objectSpace);

            // Act
            var result = await service.ProcessDeliveredAsync(Guid.NewGuid());

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ProcessDeliveredAsync_ShouldNotModifyNote_WhenAlreadyDeliveredOrRead()
        {
            // Arrange
            var (objectSpace, uow) = CreateRealObjectSpace();
            var deliveryDate = DateTime.Now.AddMinutes(-30);
            var not = new Not(uow)
            {
                Baslik = "Zaten İletilmiş Not",
                Icerik = "İçerik",
                Derece = NotDerecesi.Normal,
                MailDurumu = MailDurumu.Iletildi,
                MailIletilmeTarihi = deliveryDate
            };
            uow.CommitChanges();

            var service = CreateService(objectSpace);

            // Act
            var result = await service.ProcessDeliveredAsync(not.Oid);

            // Assert
            result.Should().BeTrue();
            not.MailDurumu.Should().Be(MailDurumu.Iletildi);
            not.MailIletilmeTarihi.Should().Be(deliveryDate);
        }

        [Fact]
        public async Task ProcessReadAsync_ShouldSetMailDurumuToOkundu_AndPublishNotification_WhenNoteExists()
        {
            // Arrange
            var (objectSpace, uow) = CreateRealObjectSpace();
            var not = new Not(uow)
            {
                Baslik = "Okunacak Not",
                Icerik = "İçerik",
                Derece = NotDerecesi.Onemli,
                MailDurumu = MailDurumu.Iletildi,
                MailGonderilmeTarihi = DateTime.Now.AddMinutes(-10),
                MailIletilmeTarihi = DateTime.Now.AddMinutes(-9)
            };
            uow.CommitChanges();

            var notificationMock = new Mock<ICrmNotificationService>();
            var service = CreateService(objectSpace, notificationMock);

            // Act
            var result = await service.ProcessReadAsync(not.Oid);

            // Assert
            result.Should().BeTrue();
            not.MailDurumu.Should().Be(MailDurumu.Okundu);
            not.MailOkunmaTarihi.Should().NotBeNull();
            not.MailOkunmaTarihi.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));

            notificationMock.Verify(
                n => n.PublishNoteRead(It.Is<NoteReadNotificationEvent>(e => e.NoteId == not.Oid && e.Title == "Okunacak Not")),
                Times.Once);
        }

        [Fact]
        public async Task ProcessReadAsync_ShouldPreserveInitialOkunmaTarihi_WhenCalledMultipleTimes()
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

            var notificationMock = new Mock<ICrmNotificationService>();
            var service = CreateService(objectSpace, notificationMock);

            // Act
            var result = await service.ProcessReadAsync(not.Oid);

            // Assert
            result.Should().BeTrue();
            not.MailDurumu.Should().Be(MailDurumu.Okundu);
            not.MailOkunmaTarihi.Should().Be(initialReadDate, "İkinci açılışta ilk okuma tarihi ezilmemelidir");

            // Notification should still be published on each hit
            notificationMock.Verify(
                n => n.PublishNoteRead(It.Is<NoteReadNotificationEvent>(e => e.NoteId == not.Oid)),
                Times.Once);
        }

        [Fact]
        public async Task ProcessReadAsync_ShouldReturnFalse_WhenNoteDoesNotExist()
        {
            // Arrange
            var (objectSpace, _) = CreateRealObjectSpace();
            var service = CreateService(objectSpace);

            // Act
            var result = await service.ProcessReadAsync(Guid.NewGuid());

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ProcessReadAsync_ShouldLogWarning_AndReturnFalse_WhenExceptionOccurs()
        {
            // Arrange
            var objectSpaceMock = new Mock<IObjectSpace>();
            objectSpaceMock
                .Setup(os => os.GetObjectByKey<Not>(It.IsAny<object>()))
                .Throws(new InvalidOperationException("Simulated Database Error"));

            var loggerMock = new Mock<ILogger<MailTrackingService>>();
            var service = CreateService(objectSpaceMock.Object, loggerMock: loggerMock);

            // Act
            var result = await service.ProcessReadAsync(Guid.NewGuid());

            // Assert
            result.Should().BeFalse();
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
