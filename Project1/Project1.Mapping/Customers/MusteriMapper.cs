#nullable enable
using System;
using Project1.Core.Mapping;
using Project1.DTOs.Customers;
using Project1.Module.Models.Customers;

namespace Project1.Mapping.Customers
{
    public sealed class MusteriMapper : IMapper<Musteri, MusteriDto>
    {
        public MusteriDto Map(Musteri source)
        {
            ArgumentNullException.ThrowIfNull(source);

            return new MusteriDto(
                source.Oid,
                source.Ad ?? string.Empty,
                source.Telefon ?? string.Empty,
                source.Adres ?? string.Empty);
        }
    }
}
