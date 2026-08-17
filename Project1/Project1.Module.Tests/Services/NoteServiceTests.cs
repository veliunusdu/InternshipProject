#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using FluentAssertions;
using Moq;
using Project1.DTOs.Notes;
using Project1.Module.BusinessObjects.Customers;
using Project1.Module.BusinessObjects.Notes;
using Project1.Module.Services.Implementations;
using Xunit;

namespace Project1.Module.Tests.Services
{
    public class NoteServiceTests
    {
        private (XPObjectSpace ObjectSpace, UnitOfWork UnitOfWork) CreateInMemoryObjectSpace()
        {
            var dataStore = new InMemoryDataStore(AutoCreateOption.DatabaseAndSchema);
            var dataLayer = new SimpleDataLayer(dataStore);
            var uow = new UnitOfWork(dataLayer);
            var typesInfoSource = XpoTypesInfoHelper.GetXpoTypeInfoSource();
            var typesInfo = XpoTypesInfoHelper.GetTypesInfo();
            typesInfo.RegisterEntity(typeof(Not));
            typesInfo.RegisterEntity(typeof(Musteri));
            typesInfo.RegisterEntity(typeof(Kisi));
            var objectSpace = new XPObjectSpace(typesInfo, typesInfoSource, () => uow);
            return (objectSpace, uow);
        }

        [Fact]
        public async Task CreateNoteAsync_ShouldLinkMusteriAndKisi_WhenValidIdsProvided()
        {
            // Arrange
            var (objectSpace, uow) = CreateInMemoryObjectSpace();
            var musteri = new Musteri(uow) { Ad = "Test Şirketi" };
            musteri.Save();
            var kisi = new Kisi(uow) { Ad = "Ali", Soyad = "Veli", Email = "ali@veli.com" };
            kisi.Save();
            uow.CommitChanges();

            var mockFactory = new Mock<IObjectSpaceFactory>();
            mockFactory.Setup(f => f.CreateObjectSpace(It.IsAny<Type>())).Returns(objectSpace);

            var noteService = new NoteService(mockFactory.Object);
            var request = new CreateNoteRequestDto(
                Baslik: "Önemli Not",
                Icerik: "Not içeriği detayları",
                Derece: 1,
                MusteriOid: musteri.Oid,
                KisiOid: kisi.Oid
            );

            // Act
            NoteDto result = await noteService.CreateNoteAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Baslik.Should().Be("Önemli Not");
            result.Icerik.Should().Be("Not içeriği detayları");
            result.Musteri.Should().Be("Test Şirketi");
            result.Kisi.Should().Be("Ali Veli");
            result.CreatedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CreateNoteAsync_ShouldCreateNoteWithoutLinks_WhenIdsAreNull()
        {
            // Arrange
            var (objectSpace, _) = CreateInMemoryObjectSpace();

            var mockFactory = new Mock<IObjectSpaceFactory>();
            mockFactory.Setup(f => f.CreateObjectSpace(It.IsAny<Type>())).Returns(objectSpace);

            var noteService = new NoteService(mockFactory.Object);
            var request = new CreateNoteRequestDto(
                Baslik: "Genel Not",
                Icerik: "Herhangi bir müşteriye bağlı değil",
                Derece: 0,
                MusteriOid: null,
                KisiOid: null
            );

            // Act
            NoteDto result = await noteService.CreateNoteAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Baslik.Should().Be("Genel Not");
            result.Musteri.Should().BeEmpty();
            result.Kisi.Should().BeEmpty();
            result.CreatedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        }
    }
}
