using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Project1.Module.BusinessObjects.Security;
using Project1.Module.Models.Customers;
using Project1.Module.Models.Notes;

namespace Project1.Module.Models.Tenants
{
    [DefaultClassOptions]
    [DefaultProperty(nameof(Ad))]
    [DeferredDeletion(true)]
    [ImageName("BO_Organization")]
    [XafDisplayName("Firma")]
    public class Firma : BaseObject
    {
        public Firma(Session session) : base(session) { }

        private string _ad;
        [XafDisplayName("Firma Adı")]
        [RuleRequiredField("RuleRequired_Firma_Ad", DefaultContexts.Save, "Firma adı boş bırakılamaz.")]
        public string Ad
        {
            get => _ad;
            set => SetPropertyValue(nameof(Ad), ref _ad, value);
        }

        private string _unvan;
        [XafDisplayName("Ticari Ünvan")]
        public string Unvan
        {
            get => _unvan;
            set => SetPropertyValue(nameof(Unvan), ref _unvan, value);
        }

        private string _vergiNo;
        [XafDisplayName("Vergi Numarası")]
        public string VergiNo
        {
            get => _vergiNo;
            set => SetPropertyValue(nameof(VergiNo), ref _vergiNo, value);
        }

        private string _telefon;
        [XafDisplayName("Telefon")]
        public string Telefon
        {
            get => _telefon;
            set => SetPropertyValue(nameof(Telefon), ref _telefon, value);
        }

        private string _adres;
        [XafDisplayName("Adres")]
        [FieldSize(FieldSizeAttribute.Unlimited)]
        public string Adres
        {
            get => _adres;
            set => SetPropertyValue(nameof(Adres), ref _adres, value);
        }

        [Association("Firma-Kullanicilar")]
        [XafDisplayName("Kullanıcılar")]
        public XPCollection<ApplicationUser> Kullanicilar => GetCollection<ApplicationUser>(nameof(Kullanicilar));

        [Association("Firma-Musteriler")]
        [XafDisplayName("Müşteriler")]
        public XPCollection<Musteri> Musteriler => GetCollection<Musteri>(nameof(Musteriler));

        [Association("Firma-Kisiler")]
        [XafDisplayName("İlgili Kişiler")]
        public XPCollection<Kisi> Kisiler => GetCollection<Kisi>(nameof(Kisiler));

        [Association("Firma-Notlar")]
        [XafDisplayName("Notlar")]
        public XPCollection<Not> Notlar => GetCollection<Not>(nameof(Notlar));
    }
}
