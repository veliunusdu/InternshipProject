using System;
using System.ComponentModel;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo;
using Project1.Module.Models.Customers;
using Project1.Module.Models.Tenants;

namespace Project1.Module.BusinessObjects.Security
{
    [MapInheritance(MapInheritanceType.ParentTable)]
    [DefaultProperty(nameof(UserName))]
    [Appearance("HideAdminToggles", TargetItems = "IsActive; CanSendEmailOnNoteCreation", Criteria = "UserName = 'Admin'", Visibility = ViewItemVisibility.Hide)]
    public class ApplicationUser : PermissionPolicyUser, ISecurityUserWithRoles
    {
        public ApplicationUser(Session session) : base(session) { }

        private bool _canSendEmailOnNoteCreation = true;
        [XafDisplayName("Not Oluşturulduğunda Mail Gönder")]
        public bool CanSendEmailOnNoteCreation
        {
            get => _canSendEmailOnNoteCreation;
            set => SetPropertyValue(nameof(CanSendEmailOnNoteCreation), ref _canSendEmailOnNoteCreation, value);
        }

        private Firma _firma;
        [XafDisplayName("Firma")]
        [Association("Firma-Kullanicilar"), ExplicitLoading]
        public Firma Firma
        {
            get => _firma;
            set => SetPropertyValue(nameof(Firma), ref _firma, value);
        }

        private Musteri _musteri;
        [XafDisplayName("Bağlı Olduğu Müşteri")]
        public Musteri Musteri
        {
            get => _musteri;
            set => SetPropertyValue(nameof(Musteri), ref _musteri, value);
        }

        private string _email;
        [XafDisplayName("E-Posta")]
        public string Email
        {
            get => _email;
            set => SetPropertyValue(nameof(Email), ref _email, value);
        }

        private bool _emailConfirmed;
        [XafDisplayName("E-Posta Doğrulandı")]
        public bool EmailConfirmed
        {
            get => _emailConfirmed;
            set => SetPropertyValue(nameof(EmailConfirmed), ref _emailConfirmed, value);
        }

        private string _emailConfirmationToken;
        [Browsable(false)]
        [Size(128)]
        public string EmailConfirmationToken
        {
            get => _emailConfirmationToken;
            set => SetPropertyValue(nameof(EmailConfirmationToken), ref _emailConfirmationToken, value);
        }

        private DateTime? _confirmationTokenExpiry;
        [Browsable(false)]
        public DateTime? ConfirmationTokenExpiry
        {
            get => _confirmationTokenExpiry;
            set => SetPropertyValue(nameof(ConfirmationTokenExpiry), ref _confirmationTokenExpiry, value);
        }
    }
}
