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
        bool IsEmailSent
    );

    public record CreateNoteRequestDto(
        string Baslik,
        string Icerik,
        int Derece,
        Guid MusteriOid,
        Guid KisiOid
    );
}
