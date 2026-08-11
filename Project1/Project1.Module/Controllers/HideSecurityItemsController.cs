using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;

namespace Project1.Module.Controllers
{
    public class HideSecurityItemsController : WindowController
    {
        public HideSecurityItemsController()
        {
            TargetWindowType = WindowType.Main;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            
            MyDetailsController myDetailsController = Frame.GetController<MyDetailsController>();
            if (myDetailsController != null)
            {
                myDetailsController.Active["HiddenByRequirement"] = false;
            }
        }
    }
}
