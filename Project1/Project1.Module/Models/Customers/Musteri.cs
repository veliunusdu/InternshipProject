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

namespace Project1.Module.Models.Customers
{
    [DefaultClassOptions]
    [DefaultProperty(nameof(Ad))]
    [DeferredDeletion(true)]
    [ImageName("BO_Customer")]
    [XafDisplayName("Müşteri")]
    public class Musteri : AuditedBaseObject
    {
        public Musteri(Session session) : base(session)
        {
        }

        [Browsable(false)]
        public override string EntityDisplayName => "Müşteri";

        [Browsable(false)]
        public override string RecordTitle => Ad ?? "-";

        private string _ad;
        [XafDisplayName("Müşteri Adı")]
        [RuleRequiredField("RuleRequired_Musteri_Ad", DefaultContexts.Save, "Müşteri adı boş bırakılamaz.")]
        public string Ad
        {
            get => _ad;
            set => SetPropertyValue(nameof(Ad), ref _ad, value);
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

        [Association("Musteri-Kisiler")]
        [XafDisplayName("İlgili Kişiler")]
        public XPCollection<Kisi> Kisiler => GetCollection<Kisi>(nameof(Kisiler));

        [Association("Musteri-Notlar")]
        [XafDisplayName("Müşteri Notları")]
        public XPCollection<Not> Notlar => GetCollection<Not>(nameof(Notlar));
    }
}
