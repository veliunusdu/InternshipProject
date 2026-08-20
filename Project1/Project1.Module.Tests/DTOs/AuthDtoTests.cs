#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using FluentAssertions;
using Project1.DTOs.Auth;
using Xunit;

namespace Project1.Module.Tests.DTOs
{
    public class AuthDtoTests
    {
        private static IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void RegisterCustomerRequest_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var request = new RegisterCustomerRequest(
                musteriAdi: "Acme Logistics A.Ş.",
                userName: "acme_logistics",
                email: "info@acme.com",
                password: "SecurePassword123!",
                telefon: "05551234567",
                adres: "İstanbul, Türkiye"
            );

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Theory]
        [InlineData("", "acme_user", "info@acme.com", "pass123", "555")] // Empty Name
        [InlineData("Acme", "", "info@acme.com", "pass123", "555")] // Empty UserName
        [InlineData("Acme", "acme_user", "invalid-email", "pass123", "555")] // Invalid Email
        [InlineData("Acme", "acme_user", "info@acme.com", "123", "555")] // Short Password (< 6 chars)
        [InlineData("Acme", "acme_user", "info@acme.com", "pass123", "")] // Empty Phone
        public void RegisterCustomerRequest_WithInvalidData_ShouldFailValidation(
            string name, string userName, string email, string password, string phone)
        {
            // Arrange
            var request = new RegisterCustomerRequest(
                musteriAdi: name,
                userName: userName,
                email: email,
                password: password,
                telefon: phone
            );

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().NotBeEmpty();
        }

        [Fact]
        public void AuthResults_FactoryMethods_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var userId = Guid.NewGuid();
            var regSuccess = RegisterResult.Ok(userId);
            var regFail = RegisterResult.Fail("Hata");

            var confirmSuccess = ConfirmEmailResult.Ok("Tebrikler");
            var confirmFail = ConfirmEmailResult.Fail("Geçersiz token");

            // Assert
            regSuccess.Success.Should().BeTrue();
            regSuccess.UserId.Should().Be(userId);
            regSuccess.ErrorMessage.Should().BeNull();

            regFail.Success.Should().BeFalse();
            regFail.ErrorMessage.Should().Be("Hata");

            confirmSuccess.Success.Should().BeTrue();
            confirmSuccess.Message.Should().Be("Tebrikler");

            confirmFail.Success.Should().BeFalse();
            confirmFail.ErrorMessage.Should().Be("Geçersiz token");
        }
    }
}
