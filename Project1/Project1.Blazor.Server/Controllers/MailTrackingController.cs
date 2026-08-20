#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project1.Core.Services.Interfaces;

namespace Project1.Blazor.Server.Controllers
{
    [ApiController]
    [Route("api/mail-tracking")]
    [AllowAnonymous]
    [EnableCors("AllowAll")]
    public class MailTrackingController : ControllerBase
    {
        private readonly IMailTrackingService _mailTrackingService;

        // 1x1 şeffaf GIF byte dizisi
        private static readonly byte[] TransparentGifBytes = new byte[] {
            0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x01, 0x00,
            0x01, 0x00, 0x80, 0x00, 0x00, 0xFF, 0xFF, 0xFF,
            0x00, 0x00, 0x00, 0x21, 0xF9, 0x04, 0x01, 0x00,
            0x00, 0x00, 0x00, 0x2C, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x01, 0x00, 0x00, 0x02, 0x02, 0x44,
            0x01, 0x00, 0x3B
        };

        public MailTrackingController(IMailTrackingService mailTrackingService)
        {
            _mailTrackingService = mailTrackingService ?? throw new ArgumentNullException(nameof(mailTrackingService));
        }

        [HttpGet("delivered/{noteId:guid}")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> TrackDelivered(Guid noteId, CancellationToken cancellationToken = default)
        {
            await _mailTrackingService.ProcessDeliveredAsync(noteId, cancellationToken).ConfigureAwait(false);

            Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.Headers.Append("Pragma", "no-cache");
            Response.Headers.Append("Expires", "0");

            return File(TransparentGifBytes, "image/gif");
        }

        [HttpGet("read/{noteId:guid}")]
        [HttpHead("read/{noteId:guid}")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> TrackRead(Guid noteId, [FromQuery] bool redirect = false, CancellationToken cancellationToken = default)
        {
            await _mailTrackingService.ProcessReadAsync(noteId, cancellationToken).ConfigureAwait(false);

            Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate, max-age=0");
            Response.Headers.Append("Pragma", "no-cache");
            Response.Headers.Append("Expires", "0");

            if (redirect)
            {
                return Redirect("/#Not_ListView");
            }

            return File(TransparentGifBytes, "image/gif");
        }
    }
}
