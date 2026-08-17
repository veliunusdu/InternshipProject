using System;
using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Project1.Module.BusinessObjects.Notes;

namespace Project1.Module.BusinessObjects.Customers
{
    [DefaultClassOptions]
    [DefaultProperty(nameof(Ad))]
    [ImageName("BO_Customer")]
    [XafDisplayName("Müşteri")]
    public class Musteri : BaseObject
    {
        public Musteri(Session session) : base(session)
        {
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            CreatedDate = DateTime.Now;
        }

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

        [Association("Musteri-Kisiler")]
        [XafDisplayName("İlgili Kişiler")]
        public XPCollection<Kisi> Kisiler => GetCollection<Kisi>(nameof(Kisiler));

        [Association("Musteri-Notlar")]
        [XafDisplayName("Müşteri Notları")]
        public XPCollection<Not> Notlar => GetCollection<Not>(nameof(Notlar));
    }
}
