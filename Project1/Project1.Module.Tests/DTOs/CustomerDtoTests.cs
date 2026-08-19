using System;
using FluentAssertions;
using Project1.DTOs.Customers;

namespace Project1.Module.Tests.DTOs
{
    public class CustomerDtoTests
    {
        [Fact]
        public void MusteriDto_ShouldExposeCustomerData()
        {
            Guid oid = Guid.Parse("11111111-1111-1111-1111-111111111111");

            var dto = new MusteriDto(oid, "Acme A.Ş.", "555 000 00 00", "İstanbul");

            dto.Oid.Should().Be(oid);
            dto.Ad.Should().Be("Acme A.Ş.");
            dto.Telefon.Should().Be("555 000 00 00");
            dto.Adres.Should().Be("İstanbul");
        }

        [Fact]
        public void CreateMusteriRequestDto_ShouldPreserveInput()
        {
            var dto = new CreateMusteriRequestDto("Acme A.Ş.", "555 000 00 00", "İstanbul");

            dto.Ad.Should().Be("Acme A.Ş.");
            dto.Telefon.Should().Be("555 000 00 00");
            dto.Adres.Should().Be("İstanbul");
        }

        [Fact]
        public void KisiDto_ShouldExposeItsCustomerRelationship()
        {
            Guid kisiOid = Guid.Parse("22222222-2222-2222-2222-222222222222");
            Guid musteriOid = Guid.Parse("11111111-1111-1111-1111-111111111111");

            var dto = new KisiDto(kisiOid, "Ayşe", "Yılmaz", "Ayşe Yılmaz", "ayse@example.com", "555 111 11 11", musteriOid);

            dto.Oid.Should().Be(kisiOid);
            dto.AdSoyad.Should().Be("Ayşe Yılmaz");
            dto.MusteriOid.Should().Be(musteriOid);
        }

        [Fact]
        public void CreateKisiRequestDto_ShouldPreserveCustomerRelationship()
        {
            Guid musteriOid = Guid.Parse("11111111-1111-1111-1111-111111111111");

            var dto = new CreateKisiRequestDto("Ayşe", "Yılmaz", "ayse@example.com", "555 111 11 11", musteriOid);

            dto.Email.Should().Be("ayse@example.com");
            dto.MusteriOid.Should().Be(musteriOid);
        }
    }
}
