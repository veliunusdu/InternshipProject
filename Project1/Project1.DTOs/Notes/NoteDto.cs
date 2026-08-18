using System;

namespace Project1.DTOs.Notes
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
        DateTime? MailOkunmaTarihi = null
    );

    public record CreateNoteRequestDto(
        string Baslik,
        string Icerik,
        int Derece,
        Guid? MusteriOid = null,
        Guid? KisiOid = null
    );
}
