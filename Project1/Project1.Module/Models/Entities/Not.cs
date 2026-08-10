using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;
using Project1.Module.Models.Enums;

namespace Project1.Module.Models.Entities
{
    [DefaultClassOptions]
    [DeferredDeletion(false)]
    public class Not : BaseObject
    {
        public Not(Session session) : base(session)
        {
        }

        private string _baslik;
        [RuleRequiredField("RuleRequired_Not_Baslik", DefaultContexts.Save, "Not başlığı boş bırakılamaz.")]
        public string Baslik
        {
            get => _baslik;
            set => SetPropertyValue(nameof(Baslik), ref _baslik, value);
        }

        private string _icerik;
        [RuleRequiredField("RuleRequired_Not_Icerik", DefaultContexts.Save, "Not içeriği boş bırakılamaz.")]
        [FieldSize(FieldSizeAttribute.Unlimited)]
        public string Icerik
        {
            get => _icerik;
            set => SetPropertyValue(nameof(Icerik), ref _icerik, value);
        }

        private NotDerecesi _derece;
        public NotDerecesi Derece
        {
            get => _derece;
            set => SetPropertyValue(nameof(Derece), ref _derece, value);
        }

        private Musteri _musteri;
        [ImmediatePostData]
        [Appearance("HideMusteriWhenMusteriIsSet", Criteria = "Musteri IS NOT NULL", Visibility = ViewItemVisibility.Hide, Context = "DetailView")]
        [VisibleInDetailView(true)]
        [VisibleInListView(true)]
        [Index(0)]
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
        [DataSourceProperty(nameof(MusteriKisiler))]
        [ImmediatePostData]
        [RuleRequiredField("RuleRequired_Not_Kisi", DefaultContexts.Save, "Not bir kişiye bağlı olmak zorundadır.")]
        [Appearance("HideKisiWhenBothSet", Criteria = "Kisi IS NOT NULL AND Musteri IS NOT NULL", Visibility = ViewItemVisibility.Hide, Context = "DetailView")]
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
    }
}
