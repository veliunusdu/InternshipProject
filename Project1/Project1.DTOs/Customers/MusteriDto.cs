using System;
using System.ComponentModel.DataAnnotations;

namespace Project1.DTOs.Customers
{
    public record MusteriDto(
        Guid Oid,
        string Ad,
        string Telefon,
        string Adres
    );

    public record CreateMusteriRequestDto(
        [property: Required(ErrorMessage = "Müşteri adı zorunludur.")]
        string Ad,
        string Telefon,
        string Adres
    );
}
