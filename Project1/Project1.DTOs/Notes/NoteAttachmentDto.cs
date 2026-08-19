using System;

namespace Project1.DTOs.Notes
{
    public record NoteAttachmentDto(
        Guid Oid,
        string DosyaAdi,
        string ContentType,
        long BoyutBytes,
        DateTime YuklemeTarihi,
        string DownloadUrl,
        bool IsImage,
        bool IsPdf
    );
}
