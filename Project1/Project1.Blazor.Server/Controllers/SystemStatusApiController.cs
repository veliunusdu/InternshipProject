using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Project1.Core.Services.Interfaces;

namespace Project1.Blazor.Server.Controllers
{
    /// <summary>
    /// Project2'nin sorguladığı REST API uç noktasını yönetir.
    /// </summary>
    [ApiController]
    [Route("api/systemstatus")]
    [AllowAnonymous]
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
}
