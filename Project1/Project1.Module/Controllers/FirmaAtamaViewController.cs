#nullable enable
using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using Project1.Module.BusinessObjects;
using Project1.Module.BusinessObjects.Security;
using Project1.Module.Models.Tenants;

namespace Project1.Module.Controllers
{
    public class FirmaAtamaViewController : ObjectViewController<DetailView, IFirmaAware>
    {
        protected override void OnActivated()
        {
            base.OnActivated();
            View.ObjectSpace.CustomCommitChanges += ObjectSpace_CustomCommitChanges;
            View.ObjectSpace.Committing += ObjectSpace_Committing;
        }

        private void ObjectSpace_Committing(object? sender, CancelEventArgs e)
        {
            AssignFirmaIfEmpty();
        }

        private void ObjectSpace_CustomCommitChanges(object? sender, HandledEventArgs e)
        {
            AssignFirmaIfEmpty();
        }

        private void AssignFirmaIfEmpty()
        {
            var obj = View.CurrentObject as IFirmaAware;
            if (obj != null && obj.Firma == null)
            {
                var currentUser = SecuritySystem.CurrentUser as ApplicationUser;
                if (currentUser?.Firma != null)
                {
                    obj.Firma = View.ObjectSpace.GetObjectByKey<Firma>(currentUser.Firma.Oid);
                }
            }
        }

        protected override void OnDeactivated()
        {
            View.ObjectSpace.CustomCommitChanges -= ObjectSpace_CustomCommitChanges;
            View.ObjectSpace.Committing -= ObjectSpace_Committing;
            base.OnDeactivated();
        }
    }
}
