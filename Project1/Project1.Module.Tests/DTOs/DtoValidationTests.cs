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

        private static IReadOnlyCollection<ValidationResult> Validate(object instance)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(instance, new ValidationContext(instance), results, validateAllProperties: true);
            return results;
        }
    }
}
