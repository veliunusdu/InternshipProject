using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Project1.DTOs.Customers;
using Project1.DTOs.Notes;

namespace Project1.Module.Tests.DTOs
{
    public class DtoValidationTests
    {
        [Fact]
        public void CreateMusteriRequestDto_ShouldRejectAnEmptyName()
        {
            var request = new CreateMusteriRequestDto("", "555 000 00 00", "İstanbul");

            Validate(request).Should().Contain(result => result.MemberNames.Contains(nameof(CreateMusteriRequestDto.Ad)));
        }

        [Fact]
        public void CreateKisiRequestDto_ShouldRejectMissingNamesAndAnInvalidEmail()
        {
            var request = new CreateKisiRequestDto("", "", "gecersiz-email", "555 111 11 11", null);

            var errors = Validate(request);

            errors.Should().Contain(result => result.MemberNames.Contains(nameof(CreateKisiRequestDto.Ad)));
            errors.Should().Contain(result => result.MemberNames.Contains(nameof(CreateKisiRequestDto.Soyad)));
            errors.Should().Contain(result => result.MemberNames.Contains(nameof(CreateKisiRequestDto.Email)));
        }

        [Fact]
        public void CreateNoteRequestDto_ShouldRejectAnEmptyTitle()
        {
            var request = new CreateNoteRequestDto("", "İçerik", 0, Guid.Empty, Guid.Empty);

            Validate(request).Should().Contain(result => result.MemberNames.Contains(nameof(CreateNoteRequestDto.Baslik)));
        }

        [Fact]
        public void CreateNoteRequestDto_ShouldRejectAnUnsupportedSeverity()
        {
            var request = new CreateNoteRequestDto("Başlık", "İçerik", 42, Guid.Empty, Guid.Empty);

            Validate(request).Should().Contain(result => result.MemberNames.Contains(nameof(CreateNoteRequestDto.Derece)));
        }

        [Fact]
        public void NoteAttachmentDto_ShouldCorrectlyInstantiateAndHoldValues()
        {
            var id = Guid.NewGuid();
            var now = DateTime.Now;
            var dto = new NoteAttachmentDto(
                Oid: id,
                DosyaAdi: "rapor.pdf",
                ContentType: "application/pdf",
                BoyutBytes: 1024,
                YuklemeTarihi: now,
                DownloadUrl: "/api/attachments/" + id + "/download",
                IsImage: false,
                IsPdf: true
            );

            dto.Oid.Should().Be(id);
            dto.DosyaAdi.Should().Be("rapor.pdf");
            dto.ContentType.Should().Be("application/pdf");
            dto.BoyutBytes.Should().Be(1024);
            dto.IsPdf.Should().BeTrue();
            dto.IsImage.Should().BeFalse();
        }

        [Fact]
        public void NoteDto_ShouldSupportEkAndSharedFlag()
        {
            var id = Guid.NewGuid();
            var attId = Guid.NewGuid();
            var att = new NoteAttachmentDto(attId, "resim.png", "image/png", 2048, DateTime.Now, "/api/attachments/" + attId + "/download", true, false);
            var note = new NoteDto(
                Oid: id,
                Baslik: "Not Başlığı",
                Icerik: "İçerik",
                Derece: "Orta",
                Musteri: "Müşteri A",
                Kisi: "Kişi B",
                IsEmailSent: false,
                IsSharedWithProject2: true,
                Ek: att
            );

            note.IsSharedWithProject2.Should().BeTrue();
            note.Ek.Should().NotBeNull();
            note.Ek!.DosyaAdi.Should().Be("resim.png");
            note.Ek.IsImage.Should().BeTrue();
        }

        [Fact]
        public void NoteDto_ShouldHoldSingleAttachmentDto()
        {
            var ek = new NoteAttachmentDto(
                Guid.NewGuid(),
                "belge.pdf",
                "application/pdf",
                1024,
                DateTime.Now,
                "/api/attachments/123/download",
                false,
                true
            );

            var noteDto = new NoteDto(
                Guid.NewGuid(),
                "Başlık",
                "İçerik",
                "Normal",
                "Müşteri A",
                "Kişi B",
                false,
                DateTime.Now,
                "Gonderilmedi",
                null,
                null,
                null,
                true,
                ek
            );

            noteDto.Ek.Should().NotBeNull();
            noteDto.Ek!.DosyaAdi.Should().Be("belge.pdf");
            noteDto.Ek.IsPdf.Should().BeTrue();
            noteDto.IsSharedWithProject2.Should().BeTrue();
        }

        private static IReadOnlyCollection<ValidationResult> Validate(object instance)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(instance, new ValidationContext(instance), results, validateAllProperties: true);
            return results;
        }
    }
}
