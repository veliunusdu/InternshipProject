using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;

namespace Project1.Module.Models.Entities
{
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

        [RuleRequiredField("RuleRequired_UserEmailPermission_User", DefaultContexts.Save)]
        [DataSourceCriteria("UserName <> 'Admin'")]
        public PermissionPolicyUser User
        {
            get => _user;
            set => SetPropertyValue(nameof(User), ref _user, value);
        }

        private bool _canSendEmail;

        [System.ComponentModel.DisplayName("E-posta Gönderebilir")]
        public bool CanSendEmail
        {
            get => _canSendEmail;
            set => SetPropertyValue(nameof(CanSendEmail), ref _canSendEmail, value);
        }
    }
}
