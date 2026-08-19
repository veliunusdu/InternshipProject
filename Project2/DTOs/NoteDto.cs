using System;
using System.Collections.Generic;

namespace Project2.DTOs
{
    public record NoteDto(
        Guid Oid,
        string Baslik,
        string Icerik,
        string Derece,
        string Musteri,
        string Kisi,
        bool IsEmailSent,
        DateTime CreatedDate,
        string MailDurumu,
        DateTime? MailGonderilmeTarihi,
        DateTime? MailIletilmeTarihi,
        DateTime? MailOkunmaTarihi,
        bool IsSharedWithProject2,
        NoteAttachmentDto? Ek
    )
    {
        public string MusteriUnvan => Musteri;
        public string KisiAdSoyad => Kisi;
    }
}
