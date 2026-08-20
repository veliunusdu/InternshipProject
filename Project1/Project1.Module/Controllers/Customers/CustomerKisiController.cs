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
        }

        protected override void OnDeactivated()
        {
            if (newObjectViewController != null)
            {
                newObjectViewController.ObjectCreated -= NewObjectViewController_ObjectCreated;
                newObjectViewController = null;
            }

            base.OnDeactivated();
        }

        private void NewObjectViewController_ObjectCreated(object sender, ObjectCreatedEventArgs e)
        {
            if (e.CreatedObject is Kisi kisi)
            {
                ApplicationUser? currentUser = Application?.Security?.User as ApplicationUser;
                if (currentUser == null && Application?.Security?.UserId is Guid userGuid)
                {
                    currentUser = e.ObjectSpace.GetObjectByKey<ApplicationUser>(userGuid);
                }

                if (currentUser == null && !string.IsNullOrEmpty(Application?.Security?.UserName))
                {
                    currentUser = e.ObjectSpace.FirstOrDefault<ApplicationUser>(u => u.UserName == Application.Security.UserName);
                }

                if (currentUser == null && SecuritySystem.CurrentUser is ApplicationUser su)
                {
                    currentUser = su;
                }

                if (currentUser?.Musteri != null)
                {
                    // Giriş yapan müşterinin şirketini yeni kişiye otomatik ata
                    kisi.Musteri = e.ObjectSpace.GetObject(currentUser.Musteri);
                }
            }
        }
    }
}
