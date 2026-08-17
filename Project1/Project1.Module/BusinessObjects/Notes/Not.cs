using System;
using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Project1.Module.BusinessObjects.Customers;
using Project1.Module.BusinessObjects.Enums;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Editors;

namespace Project1.Module.BusinessObjects.Notes
{
    [DefaultClassOptions]
    [DefaultProperty(nameof(Baslik))]
    [DeferredDeletion(false)]
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

        private DateTime _createdDate;
        [XafDisplayName("Oluşturma Tarihi")]
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
