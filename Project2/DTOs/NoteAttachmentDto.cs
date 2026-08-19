using System;

namespace Project2.DTOs
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
