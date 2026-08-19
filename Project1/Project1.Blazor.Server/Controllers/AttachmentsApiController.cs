using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Project1.Core.Services.Interfaces;

namespace Project1.Blazor.Server.Controllers
{
    [ApiController]
    [Route("api/attachments")]
    [AllowAnonymous]
    [EnableCors("AllowAll")]
    public class AttachmentsApiController : ControllerBase
    {
        private readonly INoteService _noteService;

        public AttachmentsApiController(INoteService noteService)
        {
            _noteService = noteService;
        }

        [HttpGet("{id:guid}/download")]
        public async Task<IActionResult> DownloadAttachment(Guid id)
        {
            var file = await _noteService.GetAttachmentFileAsync(id);
            if (file == null || file.Value.Bytes == null || file.Value.Bytes.Length == 0)
            {
                return NotFound("Dosya eki bulunamadı.");
            }

            var (bytes, fileName, contentType) = file.Value;
            return File(bytes, contentType, fileName);
        }
    }
}
