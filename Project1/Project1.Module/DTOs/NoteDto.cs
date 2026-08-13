using System;

namespace Project1.Module.DTOs
{
    public record NoteDto(
        Guid Oid,
        string Baslik,
        string Icerik,
        string Derece,
        string MusteriUnvan,
        string KisiAdSoyad,
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
