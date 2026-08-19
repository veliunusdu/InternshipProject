#nullable enable
using System;
using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace Project1.Module.Models.Notes
{
    [DefaultClassOptions]
    [DefaultProperty(nameof(DosyaAdi))]
    [ImageName("BO_FileAttachment")]
    [XafDisplayName("Not Eki")]
    public class NotEk : BaseObject
    {
        public NotEk(Session session) : base(session)
        {
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            YuklemeTarihi = DateTime.Now;
        }

        private Not? _not;
        [Association("Not-Ekler")]
        [XafDisplayName("Not")]
        public Not? Not
        {
            get => _not;
            set => SetPropertyValue(nameof(Not), ref _not, value);
        }

        private FileData? _dosya;
        [DevExpress.Xpo.Aggregated]
        [FileTypeFilter("PDF ve Görseller", "*.pdf;*.png;*.jpg;*.jpeg;*.gif;*.webp;*.bmp")]
        [XafDisplayName("Dosya")]
        public FileData? Dosya
        {
            get => _dosya;
            set => SetPropertyValue(nameof(Dosya), ref _dosya, value);
        }

        private string _aciklama = string.Empty;
        [XafDisplayName("Açıklama")]
        [Size(500)]
        public string Aciklama
        {
            get => _aciklama;
            set => SetPropertyValue(nameof(Aciklama), ref _aciklama, value);
        }

        private DateTime _yuklemeTarihi;
        [XafDisplayName("Yükleme Tarihi")]
        [ReadOnly(true)]
        public DateTime YuklemeTarihi
        {
            get => _yuklemeTarihi;
            set => SetPropertyValue(nameof(YuklemeTarihi), ref _yuklemeTarihi, value);
        }

        [XafDisplayName("Dosya Adı")]
        public string DosyaAdi => Dosya?.FileName ?? string.Empty;

        [XafDisplayName("Boyut (Bytes)")]
        public long BoyutBytes => Dosya?.Size ?? 0;

        public string ContentType => GetContentType(Dosya?.FileName);

        public bool IsImage => ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

        public bool IsPdf => ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase);

        public static string GetContentType(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return "application/octet-stream";
            string ext = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream"
            };
        }
    }
}
