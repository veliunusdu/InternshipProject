#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.ExpressApp;
using Project1.DTOs.Notes;
using Project1.Module.BusinessObjects.Enums;
using Project1.Module.Models.Customers;
using Project1.Module.Models.Notes;

namespace Project1.Mapping.Notes
{
    public static class NoteMappingExtensions
    {
        public static NoteAttachmentDto? ToAttachmentDto(this Not note)
        {
            if (note.Dosya == null || string.IsNullOrEmpty(note.Dosya.FileName))
            {
                return null;
            }

            return new NoteAttachmentDto(
                note.Oid,
                note.DosyaAdi,
                note.ContentType,
                note.BoyutBytes,
                note.CreatedDate,
                $"/api/attachments/{note.Oid}/download",
                note.IsImage,
                note.IsPdf
            );
        }

        public static NoteDto ToDto(this Not note)
        {
            if (note == null) throw new ArgumentNullException(nameof(note));

            return new NoteDto(
                note.Oid,
                note.Baslik ?? string.Empty,
                note.Icerik ?? string.Empty,
                note.Derece.ToString(),
                note.Musteri != null ? note.Musteri.Ad : string.Empty,
                note.Kisi != null ? (note.Kisi.Ad + " " + note.Kisi.Soyad).Trim() : string.Empty,
                note.IsEmailSent,
                note.CreatedDate,
                note.MailDurumu.ToString(),
                note.MailGonderilmeTarihi,
                note.MailIletilmeTarihi,
                note.MailOkunmaTarihi,
                note.Project2IlePaylas,
                note.ToAttachmentDto()
            );
        }

        public static IEnumerable<NoteDto> ToDtoList(this IEnumerable<Not> notes)
        {
            if (notes == null) return Enumerable.Empty<NoteDto>();
            return notes.Select(n => n.ToDto()).ToList();
        }

        public static Not ToEntity(this CreateNoteRequestDto dto, IObjectSpace objectSpace)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (objectSpace == null) throw new ArgumentNullException(nameof(objectSpace));

            var note = objectSpace.CreateObject<Not>();
            note.Baslik = dto.Baslik;
            note.Icerik = dto.Icerik;
            note.Derece = (NotDerecesi)dto.Derece;
            note.Project2IlePaylas = dto.Project2IlePaylas;

            if (dto.MusteriOid.HasValue && dto.MusteriOid.Value != Guid.Empty)
            {
                note.Musteri = objectSpace.GetObjectByKey<Musteri>(dto.MusteriOid.Value);
            }

            if (dto.KisiOid.HasValue && dto.KisiOid.Value != Guid.Empty)
            {
                note.Kisi = objectSpace.GetObjectByKey<Kisi>(dto.KisiOid.Value);
            }

            return note;
        }
    }
}
