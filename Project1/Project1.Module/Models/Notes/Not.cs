using System;
using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Project1.Module.Models.Customers;
using Project1.Core.Enums;
using Project1.Module.Models.Audit;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;

namespace Project1.Module.Models.Notes
{
    [DefaultClassOptions]
    [DefaultProperty(nameof(Baslik))]
    [DeferredDeletion(true)]
    [ImageName("Crm_Not")]
    [XafDisplayName("Not")]
    [Appearance("HideMusteriInPopup", TargetItems = "Musteri", Criteria = "[IsMusteriHidden] = True", Context = "DetailView", Visibility = ViewItemVisibility.Hide)]
    [Appearance("HideKisiInPopup", TargetItems = "Kisi", Criteria = "[IsKisiHidden] = True", Context = "DetailView", Visibility = ViewItemVisibility.Hide)]
    public class Not : BaseObject
    {
        public Not(Session session) : base(session)
        {
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            CreatedDate = DateTime.Now;
        }

        protected override void OnSaving()
        {
            base.OnSaving();
            if (CreatedDate == default)
            {
                CreatedDate = DateTime.Now;
            }

            if (!Session.IsObjectsLoading && !IsDeleted)
            {
                string user = GetCurrentUserName();
                if (Session.IsNewObject(this))
                {
                    new AuditLog(Session)
                    {
                        Tarih = DateTime.Now,
                        Kullanici = user,
                        IslemTuru = "Oluşturuldu",
                        VarlikTipi = "Not",
                        VarlikId = Oid,
                        Aciklama = $"'{Baslik}' başlıklı yeni not oluşturuldu. (Müşteri: {Musteri?.Ad ?? "-"})"
                    };
                }
                else
                {
                    new AuditLog(Session)
                    {
                        Tarih = DateTime.Now,
                        Kullanici = user,
                        IslemTuru = "Güncellendi",
                        VarlikTipi = "Not",
                        VarlikId = Oid,
                        Aciklama = $"'{Baslik}' başlıklı not güncellendi."
                    };
                }
            }
        }

        protected override void OnDeleting()
        {
            base.OnDeleting();
            string user = GetCurrentUserName();
            new AuditLog(Session)
            {
                Tarih = DateTime.Now,
                Kullanici = user,
                IslemTuru = "Silindi (Soft Delete)",
                VarlikTipi = "Not",
                VarlikId = Oid,
                Aciklama = $"'{Baslik}' başlıklı not silindi."
            };
        }

        private static string GetCurrentUserName()
        {
            try
            {
                return SecuritySystem.CurrentUserName ?? "Sistem";
            }
            catch
            {
                return "Sistem";
            }
        }

        private string _baslik;
        [XafDisplayName("Not Başlığı")]
        [RuleRequiredField("RuleRequired_Not_Baslik", DefaultContexts.Save, "Not başlığı boş bırakılamaz.")]
        public string Baslik
        {
            get => _baslik;
            set => SetPropertyValue(nameof(Baslik), ref _baslik, value);
        }

        private string _icerik;
        [XafDisplayName("Not İçeriği")]
        [RuleRequiredField("RuleRequired_Not_Icerik", DefaultContexts.Save, "Not içeriği boş bırakılamaz.")]
        [FieldSize(FieldSizeAttribute.Unlimited)]
        public string Icerik
        {
            get => _icerik;
            set => SetPropertyValue(nameof(Icerik), ref _icerik, value);
        }

        private NotDerecesi _derece;
        [XafDisplayName("Önem Derecesi")]
        public NotDerecesi Derece
        {
            get => _derece;
            set => SetPropertyValue(nameof(Derece), ref _derece, value);
        }

        private Musteri _musteri;
        [XafDisplayName("Müşteri")]
        [ImmediatePostData]
        [VisibleInDetailView(true)]
        [VisibleInListView(true)]
        [Index(0)]
        [Association("Musteri-Notlar")]
        public Musteri Musteri
        {
            get => _musteri;
            set
            {
                if (SetPropertyValue(nameof(Musteri), ref _musteri, value))
                {
                    OnChanged(nameof(MusteriKisiler));
                    if (!IsLoading && !IsSaving && _kisi != null && _kisi.Musteri != _musteri)
                    {
                        Kisi = null;
                    }
                }
            }
        }

        [Browsable(false)]
        public XPCollection<Kisi> MusteriKisiler => Musteri?.Kisiler ?? new XPCollection<Kisi>(Session);

        private Kisi _kisi;
        [XafDisplayName("İlgili Kişi")]
        [DataSourceProperty(nameof(MusteriKisiler))]
        [ImmediatePostData]
        [RuleRequiredField("RuleRequired_Not_Kisi", DefaultContexts.Save, "Not bir kişiye bağlı olmak zorundadır.")]
        [VisibleInDetailView(true)]
        [VisibleInListView(true)]
        [Index(1)]
        [Association("Kisi-Notlar")]
        public Kisi Kisi
        {
            get => _kisi;
            set
            {
                if (SetPropertyValue(nameof(Kisi), ref _kisi, value))
                {
                    if (!IsLoading && !IsSaving && _kisi != null && _kisi.Musteri != null && _musteri == null)
                    {
                        _musteri = _kisi.Musteri;
                        OnChanged(nameof(Musteri));
                    }
                }
            }
        }

        private MailDurumu _mailDurumu = MailDurumu.Gonderilmedi;
        [XafDisplayName("E-posta Durumu")]
        [VisibleInListView(true)]
        [VisibleInDetailView(false)]
        [ReadOnly(true)]
        public MailDurumu MailDurumu
        {
            get => _mailDurumu;
            set => SetPropertyValue(nameof(MailDurumu), ref _mailDurumu, value);
        }

        private DateTime? _mailGonderilmeTarihi;
        [XafDisplayName("E-posta Gönderilme Tarihi")]
        [ModelDefault("DisplayFormat", "{0:dd.MM.yyyy HH:mm}")]
        [ModelDefault("EditMask", "dd.MM.yyyy HH:mm")]
        [VisibleInListView(true)]
        [VisibleInDetailView(false)]
        [ReadOnly(true)]
        public DateTime? MailGonderilmeTarihi
        {
            get => _mailGonderilmeTarihi;
            set => SetPropertyValue(nameof(MailGonderilmeTarihi), ref _mailGonderilmeTarihi, value);
        }

        private DateTime? _mailIletilmeTarihi;
        [XafDisplayName("E-posta İletilme Tarihi")]
        [ModelDefault("DisplayFormat", "{0:dd.MM.yyyy HH:mm}")]
        [ModelDefault("EditMask", "dd.MM.yyyy HH:mm")]
        [VisibleInListView(true)]
        [VisibleInDetailView(false)]
        [ReadOnly(true)]
        public DateTime? MailIletilmeTarihi
        {
            get => _mailIletilmeTarihi;
            set => SetPropertyValue(nameof(MailIletilmeTarihi), ref _mailIletilmeTarihi, value);
        }

        private DateTime? _mailOkunmaTarihi;
        [XafDisplayName("E-posta Okunma Tarihi")]
        [ModelDefault("DisplayFormat", "{0:dd.MM.yyyy HH:mm}")]
        [ModelDefault("EditMask", "dd.MM.yyyy HH:mm")]
        [VisibleInListView(true)]
        [VisibleInDetailView(false)]
        [ReadOnly(true)]
        public DateTime? MailOkunmaTarihi
        {
            get => _mailOkunmaTarihi;
            set => SetPropertyValue(nameof(MailOkunmaTarihi), ref _mailOkunmaTarihi, value);
        }

        private string _mailHataMesaji;
        [XafDisplayName("E-posta Hata Mesajı")]
        [VisibleInListView(false)]
        [VisibleInDetailView(false)]
        [ReadOnly(true)]
        public string MailHataMesaji
        {
            get => _mailHataMesaji;
            set => SetPropertyValue(nameof(MailHataMesaji), ref _mailHataMesaji, value);
        }

        private bool _isEmailSent;
        [Browsable(false)]
        public bool IsEmailSent
        {
            get => _isEmailSent;
            set => SetPropertyValue(nameof(IsEmailSent), ref _isEmailSent, value);
        }

        [Browsable(false)]
        public bool EmailGonderilebilir => !IsEmailSent && Kisi != null && !string.IsNullOrWhiteSpace(Kisi.Email);

        [Browsable(false)]
        [NonPersistent]
        public bool IsMusteriHidden { get; set; }

        [Browsable(false)]
        [NonPersistent]
        public bool IsKisiHidden { get; set; }

        private bool _project2IlePaylas;
        [XafDisplayName("Project2 ile Paylaş")]
        [VisibleInListView(true)]
        [VisibleInDetailView(true)]
        public bool Project2IlePaylas
        {
            get => _project2IlePaylas;
            set => SetPropertyValue(nameof(Project2IlePaylas), ref _project2IlePaylas, value);
        }

        private FileData _dosya;
        [DevExpress.Xpo.Aggregated]
        [ExpandObjectMembers(ExpandObjectMembers.Never)]
        [FileTypeFilter("PDF ve Görseller", "*.pdf;*.png;*.jpg;*.jpeg;*.gif;*.webp;*.bmp")]
        [XafDisplayName("Dosya")]
        public FileData Dosya
        {
            get => _dosya;
            set => SetPropertyValue(nameof(Dosya), ref _dosya, value);
        }

        [XafDisplayName("Dosya Adı")]
        [VisibleInDetailView(false)]
        [VisibleInListView(true)]
        public string DosyaAdi => Dosya?.FileName ?? string.Empty;

        [Browsable(false)]
        public long BoyutBytes => Dosya?.Size ?? 0;

        [Browsable(false)]
        public string ContentType => GetContentType(Dosya?.FileName);

        [Browsable(false)]
        public bool IsImage => ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

        [Browsable(false)]
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

        private DateTime _createdDate;
        [XafDisplayName("Oluşturma Tarihi")]
        [ModelDefault("DisplayFormat", "{0:dd.MM.yyyy HH:mm}")]
        [ModelDefault("EditMask", "dd.MM.yyyy HH:mm")]
        [VisibleInListView(true)]
        [VisibleInDetailView(false)]
        [ReadOnly(true)]
        public DateTime CreatedDate
        {
            get => _createdDate;
            set => SetPropertyValue(nameof(CreatedDate), ref _createdDate, value);
        }
    }
}
