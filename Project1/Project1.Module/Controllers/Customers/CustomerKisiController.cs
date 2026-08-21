using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.SystemModule;
using Project1.Module.BusinessObjects.Security;
using Project1.Module.Models.Customers;

namespace Project1.Module.Controllers.Customers
{
    /// <summary>
    /// Müşteri kullanıcısı yeni bir Kişi (kontak) oluşturduğunda,
    /// Kişinin Müşteri alanını otomatik olarak giriş yapan müşteriye kilitler ve atar.
    /// </summary>
    public sealed class CustomerKisiController : ObjectViewController<ObjectView, Kisi>
    {
        private NewObjectViewController newObjectViewController;

        protected override void OnActivated()
        {
            base.OnActivated();

            newObjectViewController = Frame.GetController<NewObjectViewController>();
            if (newObjectViewController != null)
            {
                newObjectViewController.ObjectCreated += NewObjectViewController_ObjectCreated;
            }

            View.ObjectSpace.Committing += ObjectSpace_Committing;
            AssignCurrentUserMusteri(View.CurrentObject as Kisi);
        }

        private void ObjectSpace_Committing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AssignCurrentUserMusteri(View.CurrentObject as Kisi);
        }

        protected override void OnDeactivated()
        {
            if (newObjectViewController != null)
            {
                newObjectViewController.ObjectCreated -= NewObjectViewController_ObjectCreated;
                newObjectViewController = null;
            }

            View.ObjectSpace.Committing -= ObjectSpace_Committing;
            base.OnDeactivated();
        }

        private void NewObjectViewController_ObjectCreated(object sender, ObjectCreatedEventArgs e)
        {
            if (e.CreatedObject is Kisi kisi)
            {
                AssignCurrentUserMusteri(kisi);
            }
        }

        private void AssignCurrentUserMusteri(Kisi kisi)
        {
            if (kisi == null) return;

            ApplicationUser currentUser = Application?.Security?.User as ApplicationUser;
            if (currentUser == null && Application?.Security?.UserId is Guid userGuid)
            {
                currentUser = View.ObjectSpace.GetObjectByKey<ApplicationUser>(userGuid);
            }

            if (currentUser == null && !string.IsNullOrEmpty(Application?.Security?.UserName))
            {
                currentUser = View.ObjectSpace.FirstOrDefault<ApplicationUser>(u => u.UserName == Application.Security.UserName);
            }

            if (currentUser == null && SecuritySystem.CurrentUser is ApplicationUser su)
            {
                currentUser = su;
            }

            if (currentUser?.Musteri != null && kisi.Musteri == null)
            {
                kisi.Musteri = View.ObjectSpace.GetObjectByKey<Musteri>(currentUser.Musteri.Oid);
            }

            if (currentUser?.Firma != null && kisi.Firma == null)
            {
                kisi.Firma = View.ObjectSpace.GetObjectByKey<Project1.Module.Models.Tenants.Firma>(currentUser.Firma.Oid);
            }
        }
    }
}
