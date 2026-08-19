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

namespace Project1.Module.Models.Customers
{
    [DefaultClassOptions]
    [DefaultProperty(nameof(Ad))]
    [DeferredDeletion(true)]
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
                        VarlikTipi = "Müşteri",
                        VarlikId = Oid,
                        Aciklama = $"'{Ad}' adlı müşteri kaydı oluşturuldu."
                    };
                }
                else
                {
                    new AuditLog(Session)
                    {
                        Tarih = DateTime.Now,
                        Kullanici = user,
                        IslemTuru = "Güncellendi",
                        VarlikTipi = "Müşteri",
                        VarlikId = Oid,
                        Aciklama = $"'{Ad}' adlı müşteri kaydı güncellendi."
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
                VarlikTipi = "Müşteri",
                VarlikId = Oid,
                Aciklama = $"'{Ad}' adlı müşteri silindi."
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
        [VisibleInDetailView(false)]
        [ReadOnly(true)]
        public DateTime CreatedDate
        {
            get => _createdDate;
            set => SetPropertyValue(nameof(CreatedDate), ref _createdDate, value);
        }

        [Association("Musteri-Kisiler")]
        [XafDisplayName("İlgili Kişiler")]
        public XPCollection<Kisi> Kisiler => GetCollection<Kisi>(nameof(Kisiler));

        [Association("Musteri-Notlar")]
        [XafDisplayName("Müşteri Notları")]
        public XPCollection<Not> Notlar => GetCollection<Not>(nameof(Notlar));
    }
}
