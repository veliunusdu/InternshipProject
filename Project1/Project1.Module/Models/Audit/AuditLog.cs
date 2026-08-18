using System;
using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace Project1.Module.Models.Audit
{
    [DefaultClassOptions]
    [DefaultProperty(nameof(Aciklama))]
    [ImageName("BO_Audit_ChangeHistory")]
    [XafDisplayName("Sistem İşlem Günlüğü (Audit Log)")]
    [NavigationItem("Sistem Yönetimi")]
    public class AuditLog : BaseObject
    {
        public AuditLog(Session session) : base(session)
        {
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            Tarih = DateTime.Now;
        }

        private DateTime _tarih;
        [XafDisplayName("Tarih / Saat")]
        [ModelDefault("DisplayFormat", "{0:dd.MM.yyyy HH:mm:ss}")]
        [VisibleInListView(true)]
        [VisibleInDetailView(true)]
        [ReadOnly(true)]
        public DateTime Tarih
        {
            get => _tarih;
            set => SetPropertyValue(nameof(Tarih), ref _tarih, value);
        }

        private string _kullanici;
        [XafDisplayName("İşlemi Yapan")]
        [VisibleInListView(true)]
        [VisibleInDetailView(true)]
        [ReadOnly(true)]
        public string Kullanici
        {
            get => _kullanici;
            set => SetPropertyValue(nameof(Kullanici), ref _kullanici, value);
        }

        private string _islemTuru;
        [XafDisplayName("İşlem Türü")]
        [VisibleInListView(true)]
        [VisibleInDetailView(true)]
        [ReadOnly(true)]
        public string IslemTuru
        {
            get => _islemTuru;
            set => SetPropertyValue(nameof(IslemTuru), ref _islemTuru, value);
        }

        private string _varlikTipi;
        [XafDisplayName("İlgili Modül / Varlık")]
        [VisibleInListView(true)]
        [VisibleInDetailView(true)]
        [ReadOnly(true)]
        public string VarlikTipi
        {
            get => _varlikTipi;
            set => SetPropertyValue(nameof(VarlikTipi), ref _varlikTipi, value);
        }

        private Guid _varlikId;
        [XafDisplayName("Kayıt ID (Oid)")]
        [VisibleInListView(false)]
        [VisibleInDetailView(true)]
        [ReadOnly(true)]
        public Guid VarlikId
        {
            get => _varlikId;
            set => SetPropertyValue(nameof(VarlikId), ref _varlikId, value);
        }

        private string _aciklama;
        [XafDisplayName("Açıklama / Detay")]
        [FieldSize(FieldSizeAttribute.Unlimited)]
        [VisibleInListView(true)]
        [VisibleInDetailView(true)]
        [ReadOnly(true)]
        public string Aciklama
        {
            get => _aciklama;
            set => SetPropertyValue(nameof(Aciklama), ref _aciklama, value);
        }
    }
}
