using System.ComponentModel;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace Project1.Module.Models.Entities
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
    }

    [DefaultClassOptions]
    [DefaultProperty(nameof(User))]
    [NavigationItem("Yönetim")]
    [ImageName("BO_Security_Permission")]
    [RuleCombinationOfPropertiesIsUnique(
        "RuleUnique_UserEmailPermission_User",
        DefaultContexts.Save,
        nameof(User),
        CustomMessageTemplate = "Bu kullanıcı için e-posta yetkisi zaten tanımlı.")]
    public class UserEmailPermission : BaseObject
    {
        public UserEmailPermission(Session session) : base(session)
        {
        }

        private PermissionPolicyUser _user;

        [XafDisplayName("Kullanıcı")]
        [RuleRequiredField("RuleRequired_UserEmailPermission_User", DefaultContexts.Save)]
        [DataSourceCriteria("UserName <> 'Admin'")]
        public PermissionPolicyUser User
        {
            get => _user;
            set => SetPropertyValue(nameof(User), ref _user, value);
        }

        private bool _canSendEmail;

        [XafDisplayName("E-posta Gönderebilir")]
        public bool CanSendEmail
        {
            get => _canSendEmail;
            set => SetPropertyValue(nameof(CanSendEmail), ref _canSendEmail, value);
        }
    }
}
