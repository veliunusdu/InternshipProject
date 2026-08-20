#nullable enable
using System;
using Project1.Core.Mapping;
using Project1.DTOs.Notes;
using Project1.Module.Models.Notes;

namespace Project1.Mapping.Notes
{
    public sealed class NoteMapper : IMapper<Not, NoteDto>
    {
        public NoteDto Map(Not source)
        {
            ArgumentNullException.ThrowIfNull(source);

            return new NoteDto(
                source.Oid,
                source.Baslik ?? string.Empty,
                source.Icerik ?? string.Empty,
                source.Derece.ToString(),
                source.Musteri?.Ad ?? string.Empty,
                source.Kisi is null ? string.Empty : $"{source.Kisi.Ad} {source.Kisi.Soyad}".Trim(),
                source.IsEmailSent,
                source.CreatedDate,
                source.MailDurumu.ToString(),
                source.MailGonderilmeTarihi,
                source.MailIletilmeTarihi,
                source.MailOkunmaTarihi,
                source.Project2IlePaylas,
                MapAttachment(source));
        }

        private static NoteAttachmentDto? MapAttachment(Not source)
        {
            if (source.Dosya is null || string.IsNullOrEmpty(source.Dosya.FileName))
            {
                return null;
            }

            return new NoteAttachmentDto(
                source.Oid,
                source.DosyaAdi,
                source.ContentType,
                source.BoyutBytes,
                source.CreatedDate,
                $"/api/attachments/{source.Oid}/download",
                source.IsImage,
                source.IsPdf);
        }
    }
}
