using System;

namespace Project2.DTOs
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
}
