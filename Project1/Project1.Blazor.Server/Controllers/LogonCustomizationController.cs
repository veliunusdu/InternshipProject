using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Templates;
using DevExpress.Persistent.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Project1.Blazor.Server.Controllers
{
    public class LogonCustomizationController : ObjectViewController<DetailView, AuthenticationStandardLogonParameters>
    {
        public SimpleAction SignUpAction { get; private set; }

        public LogonCustomizationController()
        {
            TargetViewId = "AuthenticationStandardLogonParameters_DetailView";

            SignUpAction = new SimpleAction(this, "LogonSignUpAction", "PopupActions")
            {
                Caption = "🏢 Yeni Müşteri Kaydı (Kayıt Ol)",
                ToolTip = "Yeni müşteri firması ve kullanıcı hesabı oluştur",
                ImageName = "Action_Add",
                PaintStyle = ActionItemPaintStyle.CaptionAndImage
            };
            SignUpAction.Execute += SignUpAction_Execute;
        }

        private void SignUpAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var nav = Application.ServiceProvider?.GetService<NavigationManager>();
            if (nav != null)
            {
                nav.NavigateTo("/signup", forceLoad: true);
            }
        }
    }
}
