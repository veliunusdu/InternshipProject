using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Project1.Module.Services;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace Project1.Blazor.Server.Controllers
{
    /// <summary>
    /// Project2'nin sorguladığı REST API uç noktasını yönetir.
    /// </summary>
    [ApiController]
    [Route("api/systemstatus")]
    [EnableCors("AllowAll")]
    public class SystemStatusApiController : ControllerBase
    {
        private readonly ISystemStatusService _statusService;

        public SystemStatusApiController(ISystemStatusService statusService)
        {
            _statusService = statusService;
        }

        [HttpGet]
        public IActionResult GetStatus()
        {
            return Ok(new 
            { 
                isActive = _statusService.IsActive, 
                status = _statusService.IsActive ? "ACTIVE" : "PASSIVE" 
            });
        }

        [HttpPost("toggle")]
        public IActionResult ToggleStatus()
        {
            bool newState = _statusService.Toggle();
            return Ok(new 
            { 
                isActive = newState, 
                status = newState ? "ACTIVE" : "PASSIVE" 
            });
        }
    }

    /// <summary>
    /// Admin Paneli ana pencere üst çubuğunda API Durumunu (Aktif/Pasif) değiştiren butonu yönetir.
    /// </summary>
    public class SystemStatusWindowController : WindowController
    {
        private readonly SimpleAction _toggleStatusAction;
        private ISystemStatusService _statusService;

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
            _statusService = Application.ServiceProvider?.GetService(typeof(ISystemStatusService)) as ISystemStatusService;
            bool isAdmin = string.Equals(Application?.
            Security?.UserName, "Admin", StringComparison.
            OrdinalIgnoreCase);
            _toggleStatusAction.Active["AdminOnly"] = isAdmin;
            UpdateCaption();
        }

        private void ToggleStatusAction_Execute(object sender, SimpleActionExecuteEventArgs e)
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
