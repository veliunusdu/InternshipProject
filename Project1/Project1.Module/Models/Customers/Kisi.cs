using System;
using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Project1.Module.Models.Notes;
using Project1.Module.Models.Audit;
using Project1.Module.Models.Base;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Editors;
using Project1.Module.BusinessObjects;
using Project1.Module.BusinessObjects.Security;
using Project1.Module.Models.Tenants;

namespace Project1.Module.Models.Customers
{
    [DefaultClassOptions]
    [DefaultProperty(nameof(AdSoyad))]
    [DeferredDeletion(true)]
    [ImageName("BO_Person")]
    [XafDisplayName("Kişi")]
    [Appearance("HideMusteriInKisiPopup", TargetItems = "Musteri", Criteria = "[IsMusteriHidden] = True", Context = "DetailView", Visibility = ViewItemVisibility.Hide)]
    public class Kisi : AuditedBaseObject, IFirmaAware
    {
        public Kisi(Session session) : base(session)
        {
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            try
            {
                if (SecuritySystem.CurrentUser is ApplicationUser appUser)
                {
                    if (appUser.Musteri != null && Musteri == null)
                    {
                        Musteri = Session.GetObjectByKey<Musteri>(appUser.Musteri.Oid);
                    }
                    if (appUser.Firma != null && Firma == null)
                    {
                        Firma = Session.GetObjectByKey<Firma>(appUser.Firma.Oid);
                    }
                }
            }
            catch
            {
                // Fallback for non-security initialization
            }
        }

        [Browsable(false)]
        public override string EntityDisplayName => "Kişi";

        [Browsable(false)]
        public override string RecordTitle => AdSoyad;

        private Firma _firma;
        [XafDisplayName("Firma")]
        [Association("Firma-Kisiler"), ExplicitLoading]
        public Firma Firma
        {
            get => _firma;
            set => SetPropertyValue(nameof(Firma), ref _firma, value);
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
    }
}
