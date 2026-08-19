#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.ExpressApp;
using Project1.DTOs.Customers;
using Project1.Module.Models.Customers;

namespace Project1.Mapping.Customers
{
    public static class ContactMappingExtensions
    {
        public static KisiDto ToDto(this Kisi kisi)
        {
            if (kisi == null) throw new ArgumentNullException(nameof(kisi));

            return new KisiDto(
                kisi.Oid,
                kisi.Ad ?? string.Empty,
                kisi.Soyad ?? string.Empty,
                kisi.AdSoyad ?? $"{kisi.Ad} {kisi.Soyad}".Trim(),
                kisi.Email ?? string.Empty,
                kisi.Telefon ?? string.Empty,
                kisi.Musteri?.Oid
            );
        }

        public static IEnumerable<KisiDto> ToDtoList(this IEnumerable<Kisi> kisiler)
        {
            if (kisiler == null) return Enumerable.Empty<KisiDto>();
            return kisiler.Select(k => k.ToDto()).ToList();
        }

        public static Kisi ToEntity(this CreateKisiRequestDto dto, IObjectSpace objectSpace)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (objectSpace == null) throw new ArgumentNullException(nameof(objectSpace));

            var kisi = objectSpace.CreateObject<Kisi>();
            kisi.Ad = dto.Ad;
            kisi.Soyad = dto.Soyad;
            kisi.Email = dto.Email;
            kisi.Telefon = dto.Telefon;

            if (dto.MusteriOid.HasValue && dto.MusteriOid.Value != Guid.Empty)
            {
                kisi.Musteri = objectSpace.GetObjectByKey<Musteri>(dto.MusteriOid.Value);
            }

            return kisi;
        }
    }
}
