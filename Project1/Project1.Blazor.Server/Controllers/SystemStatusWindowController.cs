#nullable enable
using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using Project1.Core.Services.Interfaces;

namespace Project1.Blazor.Server.Controllers
{
    /// <summary>
    /// Admin Paneli ana pencere üst çubuğunda API Durumunu (Aktif/Pasif) değiştiren butonu yönetir.
    /// </summary>
    public class SystemStatusWindowController : WindowController
    {
        private readonly SimpleAction _toggleStatusAction;
        private ISystemStatusService? _statusService;

        public SystemStatusWindowController()
        {
            TargetWindowType = WindowType.Main;

            _toggleStatusAction = new SimpleAction(this, "ToggleSystemStatusAction", PredefinedCategory.Tools)
            {
                Caption = "API Durumu",
                ImageName = "State_ItemVisibility_Show",
                ToolTip = "Project2 API durumunu Aktif veya Pasif yap."
            };
            _toggleStatusAction.Execute += ToggleStatusAction_Execute;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            _statusService = Application?.ServiceProvider?.GetService(typeof(ISystemStatusService)) as ISystemStatusService;
            bool isAdmin = string.Equals(Application?.Security?.UserName, "Admin", StringComparison.OrdinalIgnoreCase);
            _toggleStatusAction.Active["AdminOnly"] = isAdmin;
            UpdateCaption();
        }

        private void ToggleStatusAction_Execute(object? sender, SimpleActionExecuteEventArgs e)
        {
            if (_statusService != null)
            {
                bool newState = _statusService.Toggle();
                UpdateCaption();

                string statusText = newState ? "AKTİF (ACTIVE)" : "PASİF (PASSIVE)";
                Application?.ShowViewStrategy?.ShowMessage(new MessageOptions
                {
                    Message = $"Project2 API Durumu Güncellendi: {statusText}",
                    Type = newState ? InformationType.Success : InformationType.Warning,
                    Duration = 4000
                });
            }
        }

        private void UpdateCaption()
        {
            if (_statusService != null)
            {
                _toggleStatusAction.Caption = _statusService.IsActive 
                    ? "🟢 API Durumu: AKTİF (Pasif Yap)" 
                    : "🔴 API Durumu: PASİF (Aktif Yap)";
            }
        }
    }
}
