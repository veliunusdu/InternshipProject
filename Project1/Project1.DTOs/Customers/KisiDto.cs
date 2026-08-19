using System;
using System.ComponentModel.DataAnnotations;

namespace Project1.DTOs.Customers
{
    public record KisiDto(
        Guid Oid,
        string Ad,
        string Soyad,
        string AdSoyad,
        string Email,
        string Telefon,
        Guid? MusteriOid
    );

    public record CreateKisiRequestDto(
        [property: Required(ErrorMessage = "Kişi adı zorunludur.")]
        string Ad,
        [property: Required(ErrorMessage = "Kişi soyadı zorunludur.")]
        string Soyad,
        [property: EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        string Email,
        string Telefon,
        Guid? MusteriOid
    );
}
