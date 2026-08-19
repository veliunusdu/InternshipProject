#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using FluentAssertions;
using Moq;
using Project1.DTOs.Notes;
using Project1.Module.Models.Customers;
using Project1.Module.Models.Notes;
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
                KisiOid: kisi.Oid,
                Project2IlePaylas: false
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
                KisiOid: null,
                Project2IlePaylas: false
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

        [Fact]
        public async Task GetNotesAsync_ShouldFilterByOnlyShared_WhenRequested()
        {
            // Arrange
            var dataStore = new InMemoryDataStore(AutoCreateOption.DatabaseAndSchema);
            var dataLayer = new SimpleDataLayer(dataStore);
            var typesInfoSource = XpoTypesInfoHelper.GetXpoTypeInfoSource();
            var typesInfo = XpoTypesInfoHelper.GetTypesInfo();
            typesInfo.RegisterEntity(typeof(Not));
            typesInfo.RegisterEntity(typeof(Musteri));
            typesInfo.RegisterEntity(typeof(Kisi));

            using (var uow = new UnitOfWork(dataLayer))
            {
                var n1 = new Not(uow) { Baslik = "Paylaşılan Not", Icerik = "A", Project2IlePaylas = true };
                var n2 = new Not(uow) { Baslik = "Gizli Not", Icerik = "B", Project2IlePaylas = false };
                n1.Save();
                n2.Save();
                uow.CommitChanges();
            }

            var mockFactory = new Mock<IObjectSpaceFactory>();
            mockFactory
                .Setup(f => f.CreateObjectSpace(It.IsAny<Type>()))
                .Returns(() => new XPObjectSpace(typesInfo, typesInfoSource, () => new UnitOfWork(dataLayer)));

            var noteService = new NoteService(mockFactory.Object);

            // Act
            var sharedNotes = (await noteService.GetNotesAsync(onlyShared: true)).ToList();
            var allNotes = (await noteService.GetNotesAsync()).ToList();

            // Assert
            sharedNotes.Should().HaveCount(1);
            sharedNotes[0].Baslik.Should().Be("Paylaşılan Not");
            sharedNotes[0].IsSharedWithProject2.Should().BeTrue();

            allNotes.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetNotesAsync_ShouldIncludeAttachmentInfo_WhenNoteHasDosya()
        {
            var dataStore = new InMemoryDataStore(AutoCreateOption.DatabaseAndSchema);
            var dataLayer = new SimpleDataLayer(dataStore);
            var typesInfoSource = XpoTypesInfoHelper.GetXpoTypeInfoSource();
            var typesInfo = XpoTypesInfoHelper.GetTypesInfo();
            typesInfo.RegisterEntity(typeof(Not));
            typesInfo.RegisterEntity(typeof(Musteri));
            typesInfo.RegisterEntity(typeof(Kisi));

            Guid noteId;
            using (var uow = new UnitOfWork(dataLayer))
            {
                var fileData = new FileData(uow);
                fileData.LoadFromStream("rapor.pdf", new MemoryStream(new byte[] { 10, 20, 30 }));

                var n = new Not(uow)
                {
                    Baslik = "Rapor Notu",
                    Icerik = "Ekli dosya testi",
                    Project2IlePaylas = true,
                    Dosya = fileData
                };
                n.Save();
                uow.CommitChanges();
                noteId = n.Oid;
            }

            var mockFactory = new Mock<IObjectSpaceFactory>();
            mockFactory
                .Setup(f => f.CreateObjectSpace(It.IsAny<Type>()))
                .Returns(() => new XPObjectSpace(typesInfo, typesInfoSource, () => new UnitOfWork(dataLayer)));

            var noteService = new NoteService(mockFactory.Object);

            var notes = (await noteService.GetNotesAsync(onlyShared: true)).ToList();

            notes.Should().HaveCount(1);
            var noteEk = notes[0].Ek;
            noteEk.Should().NotBeNull();
            noteEk!.DosyaAdi.Should().Be("rapor.pdf");
            noteEk.DownloadUrl.Should().Be($"/api/attachments/{noteId}/download");
            noteEk.IsPdf.Should().BeTrue();
        }

        [Fact]
        public async Task GetAttachmentFileAsync_ShouldReturnFileBytes_WhenNoteHasDosya()
        {
            var dataStore = new InMemoryDataStore(AutoCreateOption.DatabaseAndSchema);
            var dataLayer = new SimpleDataLayer(dataStore);
            var typesInfoSource = XpoTypesInfoHelper.GetXpoTypeInfoSource();
            var typesInfo = XpoTypesInfoHelper.GetTypesInfo();
            typesInfo.RegisterEntity(typeof(Not));
            typesInfo.RegisterEntity(typeof(Musteri));
            typesInfo.RegisterEntity(typeof(Kisi));

            Guid noteId;
            var expectedBytes = new byte[] { 1, 2, 3, 4, 5 };
            using (var uow = new UnitOfWork(dataLayer))
            {
                var fileData = new FileData(uow);
                fileData.LoadFromStream("resim.png", new MemoryStream(expectedBytes));

                var n = new Not(uow)
                {
                    Baslik = "Resimli Not",
                    Icerik = "Görsel testi",
                    Dosya = fileData
                };
                n.Save();
                uow.CommitChanges();
                noteId = n.Oid;
            }

            var mockFactory = new Mock<IObjectSpaceFactory>();
            mockFactory
                .Setup(f => f.CreateObjectSpace(It.IsAny<Type>()))
                .Returns(() => new XPObjectSpace(typesInfo, typesInfoSource, () => new UnitOfWork(dataLayer)));

            var noteService = new NoteService(mockFactory.Object);

            var file = await noteService.GetAttachmentFileAsync(noteId);

            file.Should().NotBeNull();
            var (bytes, fileName, contentType) = file!.Value;
            fileName.Should().Be("resim.png");
            contentType.Should().Be("image/png");
            bytes.Should().BeEquivalentTo(expectedBytes);
        }

        [Fact]
        public async Task GetAttachmentFileAsync_ShouldReturnNull_WhenAttachmentDoesNotExist()
        {
            // Arrange
            var (objectSpace, _) = CreateInMemoryObjectSpace();
            var mockFactory = new Mock<IObjectSpaceFactory>();
            mockFactory.Setup(f => f.CreateObjectSpace(It.IsAny<Type>())).Returns(objectSpace);

            var noteService = new NoteService(mockFactory.Object);

            // Act
            var file = await noteService.GetAttachmentFileAsync(Guid.NewGuid());

            // Assert
            file.Should().BeNull();
        }
    }
}
