#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.ExpressApp;
using Project1.DTOs.Customers;
using Project1.Module.Models.Customers;

namespace Project1.Mapping.Customers
{
    public static class CustomerMappingExtensions
    {
        public static MusteriDto ToDto(this Musteri musteri)
        {
            if (musteri == null) throw new ArgumentNullException(nameof(musteri));

            return new MusteriDto(
                musteri.Oid,
                musteri.Ad ?? string.Empty,
                musteri.Telefon ?? string.Empty,
                musteri.Adres ?? string.Empty
            );
        }

        public static IEnumerable<MusteriDto> ToDtoList(this IEnumerable<Musteri> musteriler)
        {
            if (musteriler == null) return Enumerable.Empty<MusteriDto>();
            return musteriler.Select(m => m.ToDto()).ToList();
        }

        public static Musteri ToEntity(this CreateMusteriRequestDto dto, IObjectSpace objectSpace)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (objectSpace == null) throw new ArgumentNullException(nameof(objectSpace));

            var musteri = objectSpace.CreateObject<Musteri>();
            musteri.Ad = dto.Ad;
            musteri.Telefon = dto.Telefon;
            musteri.Adres = dto.Adres;
            return musteri;
        }
    }
}
