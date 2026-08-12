using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace Project1.Module.Models.Entities
{
    [DefaultClassOptions]
    [DefaultProperty(nameof(AdSoyad))]
    [DeferredDeletion(false)]
    [ImageName("Crm_Kisi")]
    [XafDisplayName("Kişi")]
    public class Kisi : BaseObject
    {
        public Kisi(Session session) : base(session)
        {
        }

        private string _ad;
        [XafDisplayName("Ad")]
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
        [XafDisplayName("Soyad")]
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

        private string _email;
        [XafDisplayName("E-posta Adresi")]
        [RuleRegularExpression("RuleRegularExpression_Kisi_Email", DefaultContexts.Save, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", CustomMessageTemplate = "Lütfen geçerli bir e-posta adresi giriniz.")]
        public string Email
        {
            get => _email;
            set => SetPropertyValue(nameof(Email), ref _email, value);
        }

        [XafDisplayName("Ad Soyad")]
        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        public string AdSoyad => $"{Ad} {Soyad}".Trim();

        private Musteri _musteri;
        [XafDisplayName("Bağlı Olduğu Müşteri")]
        [Association("Musteri-Kisiler")]
        [RuleRequiredField("RuleRequired_Kisi_Musteri", DefaultContexts.Save, "Müşteri seçimi zorunludur. Lütfen bir müşteri seçiniz.")]
        [ImmediatePostData]
        public Musteri Musteri
        {
            get => _musteri;
            set => SetPropertyValue(nameof(Musteri), ref _musteri, value);
        }

        [XafDisplayName("Notlar")]
        [Association("Kisi-Notlar"), DevExpress.Xpo.Aggregated]
        public XPCollection<Not> Notlar => GetCollection<Not>(nameof(Notlar));
    }
}
