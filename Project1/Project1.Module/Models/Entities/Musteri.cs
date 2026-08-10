using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;

namespace Project1.Module.Models.Entities
{
    [DefaultClassOptions]
    [DefaultProperty(nameof(AdSoyad))]
    [DeferredDeletion(false)]
    public class Musteri : BaseObject
    {
        public Musteri(Session session) : base(session)
        {
        }

        private string _ad;
        [RuleRequiredField("RuleRequired_Musteri_Ad", DefaultContexts.Save, "Müşteri adı boş bırakılamaz.")]
        [ImmediatePostData]
        public string Ad
        {
            get => _ad;
            set
            {
                if (SetPropertyValue(nameof(Ad), ref _ad, value))
                {
                    OnChanged(nameof(AdSoyad));
                }
            }
        }

        private string _soyad;
        [RuleRequiredField("RuleRequired_Musteri_Soyad", DefaultContexts.Save, "Müşteri soyadı boş bırakılamaz.")]
        [ImmediatePostData]
        public string Soyad
        {
            get => _soyad;
            set
            {
                if (SetPropertyValue(nameof(Soyad), ref _soyad, value))
                {
                    OnChanged(nameof(AdSoyad));
                }
            }
        }

        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        public string AdSoyad => $"{Ad} {Soyad}".Trim();

        [Association("Musteri-Kisiler"), DevExpress.Xpo.Aggregated]
        public XPCollection<Kisi> Kisiler => GetCollection<Kisi>(nameof(Kisiler));

        [VisibleInDetailView(true)]
        [VisibleInListView(false)]
        public XPCollection<Not> Notlar => new XPCollection<Not>(Session, DevExpress.Data.Filtering.CriteriaOperator.Parse("Kisi.Musteri.Oid = ?", Oid));
    }
}
