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
        DateTime CreatedDate = default,
        string MailDurumu = "Gonderilmedi",
        DateTime? MailGonderilmeTarihi = null,
        DateTime? MailIletilmeTarihi = null,
        DateTime? MailOkunmaTarihi = null,
        bool IsSharedWithProject2 = false,
        NoteAttachmentDto? Ek = null
    )
    {
        public string MusteriUnvan => Musteri;
        public string KisiAdSoyad => Kisi;
    }
}
