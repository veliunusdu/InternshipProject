using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo;
using System.ComponentModel;

namespace Project1.Module.Models.Entities
{
    [MapInheritance(MapInheritanceType.ParentTable)]
    [DefaultProperty(nameof(UserName))]
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
}
