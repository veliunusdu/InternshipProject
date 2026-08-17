using System;
using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Project1.Module.BusinessObjects.Notes;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Editors;

namespace Project1.Module.BusinessObjects.Customers
{
    [DefaultClassOptions]
    [DefaultProperty(nameof(AdSoyad))]
    [ImageName("BO_Person")]
    [XafDisplayName("Kişi")]
    [Appearance("HideMusteriInKisiPopup", TargetItems = "Musteri", Criteria = "[IsMusteriHidden] = True", Context = "DetailView", Visibility = ViewItemVisibility.Hide)]
    public class Kisi : BaseObject
    {
        public Kisi(Session session) : base(session)
        {
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            CreatedDate = DateTime.Now;
        }

        private string _ad;
        [XafDisplayName("Ad")]
        [RuleRequiredField("RuleRequired_Kisi_Ad", DefaultContexts.Save, "Kişi adı boş bırakılamaz.")]
        public string Ad
        {
            get => _ad;
            set => SetPropertyValue(nameof(Ad), ref _ad, value);
        }

        private string _soyad;
        [XafDisplayName("Soyad")]
        [RuleRequiredField("RuleRequired_Kisi_Soyad", DefaultContexts.Save, "Kişi soyadı boş bırakılamaz.")]
        public string Soyad
        {
            get => _soyad;
            set => SetPropertyValue(nameof(Soyad), ref _soyad, value);
        }

        [XafDisplayName("Ad Soyad")]
        [VisibleInListView(true)]
        [VisibleInDetailView(false)]
        public string AdSoyad => $"{Ad} {Soyad}".Trim();

        private string _email;
        [XafDisplayName("E-Posta Adresi")]
        [RuleRegularExpression("RuleRegex_Kisi_Email", DefaultContexts.Save,
            @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$",
            CustomMessageTemplate = "Geçerli bir e-posta adresi giriniz.")]
        public string Email
        {
            get => _email;
            set => SetPropertyValue(nameof(Email), ref _email, value);
        }

        private string _telefon;
        [XafDisplayName("Telefon")]
        public string Telefon
        {
            get => _telefon;
            set => SetPropertyValue(nameof(Telefon), ref _telefon, value);
        }

        private Musteri _musteri;
        [XafDisplayName("Bağlı Müşteri")]
        [Association("Musteri-Kisiler")]
        public Musteri Musteri
        {
            get => _musteri;
            set => SetPropertyValue(nameof(Musteri), ref _musteri, value);
        }

        [Association("Kisi-Notlar")]
        [XafDisplayName("Notlar")]
        public XPCollection<Not> Notlar => GetCollection<Not>(nameof(Notlar));

        [Browsable(false)]
        [NonPersistent]
        public bool IsMusteriHidden { get; set; }

        private DateTime _createdDate;
        [XafDisplayName("Oluşturma Tarihi")]
        [VisibleInListView(true)]
        [VisibleInDetailView(true)]
        public DateTime CreatedDate
        {
            get => _createdDate;
            set => SetPropertyValue(nameof(CreatedDate), ref _createdDate, value);
        }

        private string _referenceNo;
        [XafDisplayName("Referans No")]
        [VisibleInListView(true)]
        [VisibleInDetailView(true)]
        public string ReferenceNo
        {
            get => _referenceNo;
            set => SetPropertyValue(nameof(ReferenceNo), ref _referenceNo, value);
        }

        private string _referenceBaseObjectType;
        [XafDisplayName("Referans Nesne Tipi")]
        [VisibleInDetailView(true)]
        public string ReferenceBaseObjectType
        {
            get => _referenceBaseObjectType;
            set => SetPropertyValue(nameof(ReferenceBaseObjectType), ref _referenceBaseObjectType, value);
        }

        private Guid? _referenceBaseObjectId;
        [XafDisplayName("Referans Nesne ID")]
        [VisibleInDetailView(true)]
        public Guid? ReferenceBaseObjectId
        {
            get => _referenceBaseObjectId;
            set => SetPropertyValue(nameof(ReferenceBaseObjectId), ref _referenceBaseObjectId, value);
        }
    }
}
