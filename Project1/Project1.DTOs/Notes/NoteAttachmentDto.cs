using System;

namespace Project1.DTOs.Notes
{
    public class NoteAttachmentDto
    {
        public Guid Oid { get; set; }
        public string DosyaAdi { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long BoyutBytes { get; set; }
        public DateTime YuklemeTarihi { get; set; }
        public string DownloadUrl { get; set; } = string.Empty;
        public bool IsImage { get; set; }
        public bool IsPdf { get; set; }

        public NoteAttachmentDto() { }

        public NoteAttachmentDto(
            Guid Oid,
            string DosyaAdi,
            string ContentType,
            long BoyutBytes,
            DateTime YuklemeTarihi,
            string DownloadUrl,
            bool IsImage,
            bool IsPdf)
        {
            this.Oid = Oid;
            this.DosyaAdi = DosyaAdi;
            this.ContentType = ContentType;
            this.BoyutBytes = BoyutBytes;
            this.YuklemeTarihi = YuklemeTarihi;
            this.DownloadUrl = DownloadUrl;
            this.IsImage = IsImage;
            this.IsPdf = IsPdf;
        }
    }
}
