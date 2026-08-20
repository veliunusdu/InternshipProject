#nullable enable
using System;
using Project1.Core.Mapping;
using Project1.DTOs.Customers;
using Project1.Module.Models.Customers;

namespace Project1.Mapping.Customers
{
    public sealed class KisiMapper : IMapper<Kisi, KisiDto>
    {
        public KisiDto Map(Kisi source)
        {
            ArgumentNullException.ThrowIfNull(source);

            return new KisiDto(
                source.Oid,
                source.Ad ?? string.Empty,
                source.Soyad ?? string.Empty,
                source.AdSoyad ?? $"{source.Ad} {source.Soyad}".Trim(),
                source.Email ?? string.Empty,
                source.Telefon ?? string.Empty,
                source.Musteri?.Oid);
        }
    }
}
