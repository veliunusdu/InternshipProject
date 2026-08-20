#nullable enable
using System;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using FluentAssertions;
using Project1.Mapping.Customers;
using Project1.Mapping.Notes;
using Project1.Module.Models.Customers;
using Project1.Module.Models.Notes;
using Xunit;

namespace Project1.Module.Tests.Mapping
{
    public class EntityMapperTests
    {
        private static UnitOfWork CreateUnitOfWork()
        {
            var dataStore = new InMemoryDataStore(AutoCreateOption.DatabaseAndSchema);
            return new UnitOfWork(new SimpleDataLayer(dataStore));
        }

        [Fact]
        public void NoteMapper_ShouldMapAllBasicFieldsWithoutAttachment()
        {
            using var unitOfWork = CreateUnitOfWork();
            var customer = new Musteri(unitOfWork) { Ad = "Acme" };
            var contact = new Kisi(unitOfWork)
            {
                Ad = "Ada",
                Soyad = "Lovelace",
                Musteri = customer
            };
            var note = new Not(unitOfWork)
            {
                Baslik = "Plan",
                Icerik = "Mapping entegrasyonu",
                Musteri = customer,
                Kisi = contact,
                Project2IlePaylas = true
            };

            var result = new NoteMapper().Map(note);

            result.Oid.Should().Be(note.Oid);
            result.Baslik.Should().Be("Plan");
            result.Icerik.Should().Be("Mapping entegrasyonu");
            result.Musteri.Should().Be("Acme");
            result.Kisi.Should().Be("Ada Lovelace");
            result.IsSharedWithProject2.Should().BeTrue();
            result.Ek.Should().BeNull();
        }

        [Fact]
        public void MusteriMapper_ShouldMapCustomerFields()
        {
            using var unitOfWork = CreateUnitOfWork();
            var customer = new Musteri(unitOfWork)
            {
                Ad = "Acme",
                Telefon = "555",
                Adres = "İstanbul"
            };

            var result = new MusteriMapper().Map(customer);

            result.Oid.Should().Be(customer.Oid);
            result.Ad.Should().Be("Acme");
            result.Telefon.Should().Be("555");
            result.Adres.Should().Be("İstanbul");
        }

        [Fact]
        public void KisiMapper_ShouldMapContactAndCustomerId()
        {
            using var unitOfWork = CreateUnitOfWork();
            var customer = new Musteri(unitOfWork) { Ad = "Acme" };
            var contact = new Kisi(unitOfWork)
            {
                Ad = "Ada",
                Soyad = "Lovelace",
                Email = "ada@example.com",
                Telefon = "555",
                Musteri = customer
            };

            var result = new KisiMapper().Map(contact);

            result.Oid.Should().Be(contact.Oid);
            result.AdSoyad.Should().Be("Ada Lovelace");
            result.Email.Should().Be("ada@example.com");
            result.Telefon.Should().Be("555");
            result.MusteriOid.Should().Be(customer.Oid);
        }

        [Fact]
        public void Mappers_ShouldRejectNullSources()
        {
            Action mapNote = () => new NoteMapper().Map(null!);
            Action mapCustomer = () => new MusteriMapper().Map(null!);
            Action mapContact = () => new KisiMapper().Map(null!);

            mapNote.Should().Throw<ArgumentNullException>();
            mapCustomer.Should().Throw<ArgumentNullException>();
            mapContact.Should().Throw<ArgumentNullException>();
        }
    }
}
