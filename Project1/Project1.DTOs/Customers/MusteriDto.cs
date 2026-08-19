using System;
using System.ComponentModel.DataAnnotations;

namespace Project1.DTOs.Customers
{
    public class MusteriDto
    {
        public Guid Oid { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public string Adres { get; set; } = string.Empty;

        public MusteriDto() { }

        public MusteriDto(Guid oid, string ad, string telefon, string adres)
        {
            Oid = oid;
            Ad = ad;
            Telefon = telefon;
            Adres = adres;
        }
    }

    public class CreateMusteriRequestDto
    {
        [Required(ErrorMessage = "Müşteri adı zorunludur.")]
        public string Ad { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public string Adres { get; set; } = string.Empty;

        public CreateMusteriRequestDto() { }

        public CreateMusteriRequestDto(string ad, string telefon, string adres)
        {
            Ad = ad;
            Telefon = telefon;
            Adres = adres;
        }
    }
}
