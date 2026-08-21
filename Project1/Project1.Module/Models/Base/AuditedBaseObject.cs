using System;
using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using Project1.Module.Models.Audit;
using Project1.Module.BusinessObjects;
using Project1.Module.BusinessObjects.Security;
using Project1.Module.Models.Tenants;

namespace Project1.Module.Models.Base
{
    [NonPersistent]
    public abstract class AuditedBaseObject : BaseObject
    {
        protected AuditedBaseObject(Session session) : base(session) { }

        [Browsable(false)]
        public abstract string EntityDisplayName { get; }

        [Browsable(false)]
        public abstract string RecordTitle { get; }

        private DateTime _createdDate;
        [XafDisplayName("Oluşturma Tarihi")]
        [ModelDefault("DisplayFormat", "{0:dd.MM.yyyy HH:mm}")]
        [ModelDefault("EditMask", "dd.MM.yyyy HH:mm")]
        [VisibleInListView(true)]
        [VisibleInDetailView(false)]
        [ReadOnly(true)]
        public DateTime CreatedDate
        {
            get => _createdDate;
            set => SetPropertyValue(nameof(CreatedDate), ref _createdDate, value);
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            CreatedDate = DateTime.Now;
            AssignCurrentFirma();
        }

        protected override void OnSaving()
        {
            base.OnSaving();
            if (CreatedDate == default)
            {
                CreatedDate = DateTime.Now;
            }

            AssignCurrentFirma();

            if (!Session.IsObjectsLoading && !IsDeleted)
            {
                string user = GetCurrentUserName();
                bool isNew = Session.IsNewObject(this);
                new AuditLog(Session)
                {
                    Tarih = DateTime.Now,
                    Kullanici = user,
                    IslemTuru = isNew ? "Oluşturuldu" : "Güncellendi",
                    VarlikTipi = EntityDisplayName,
                    VarlikId = Oid,
                    Aciklama = isNew 
                        ? $"'{RecordTitle}' başlıklı/adlı yeni {EntityDisplayName.ToLowerInvariant()} oluşturuldu."
                        : $"'{RecordTitle}' başlıklı/adlı {EntityDisplayName.ToLowerInvariant()} güncellendi."
                };
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
                VarlikTipi = EntityDisplayName,
                VarlikId = Oid,
                Aciklama = $"'{RecordTitle}' başlıklı/adlı {EntityDisplayName.ToLowerInvariant()} silindi."
            };
        }

        protected static string GetCurrentUserName()
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

        protected virtual void AssignCurrentFirma()
        {
            try
            {
                if (this is IFirmaAware firmaAware && firmaAware.Firma == null)
                {
                    if (SecuritySystem.CurrentUser is ApplicationUser appUser && appUser.Firma != null)
                    {
                        firmaAware.Firma = Session.GetObjectByKey<Firma>(appUser.Firma.Oid);
                    }
                }
            }
            catch
            {
                // Güvenlik nesnesi henüz yüklenmediyse veya session bağlamı dışındaysa yutulur
            }
        }
    }
}
