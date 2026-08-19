using System;
using System.ComponentModel.DataAnnotations;

namespace Project1.DTOs.Customers
{
    public class KisiDto
    {
        public Guid Oid { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string AdSoyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public Guid? MusteriOid { get; set; }

        public KisiDto() { }

        public KisiDto(Guid oid, string ad, string soyad, string adSoyad, string email, string telefon, Guid? musteriOid)
        {
            Oid = oid;
            Ad = ad;
            Soyad = soyad;
            AdSoyad = adSoyad;
            Email = email;
            Telefon = telefon;
            MusteriOid = musteriOid;
        }
    }

    public class CreateKisiRequestDto
    {
        [Required(ErrorMessage = "Kişi adı zorunludur.")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kişi soyadı zorunludur.")]
        public string Soyad { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        public string Telefon { get; set; } = string.Empty;
        public Guid? MusteriOid { get; set; }

        public CreateKisiRequestDto() { }

        public CreateKisiRequestDto(string ad, string soyad, string email, string telefon, Guid? musteriOid)
        {
            Ad = ad;
            Soyad = soyad;
            Email = email;
            Telefon = telefon;
            MusteriOid = musteriOid;
        }
    }
}
