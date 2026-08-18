#nullable enable
using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Project1.Module.Models.Enums;
using Project1.Module.Models.Notes;

namespace Project1.Blazor.Server.Controllers
{
    [ApiController]
    [Route("api/mail-tracking")]
    [AllowAnonymous]
    [EnableCors("AllowAll")]
    public class MailTrackingController : ControllerBase
    {
        private readonly IObjectSpaceFactory _objectSpaceFactory;
        private readonly INonSecuredObjectSpaceFactory? _nonSecuredObjectSpaceFactory;
        private readonly ILogger<MailTrackingController>? _logger;

        // 1x1 şeffaf GIF byte dizisi
        private static readonly byte[] TransparentGifBytes = new byte[] {
            0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x01, 0x00,
            0x01, 0x00, 0x80, 0x00, 0x00, 0xFF, 0xFF, 0xFF,
            0x00, 0x00, 0x00, 0x21, 0xF9, 0x04, 0x01, 0x00,
            0x00, 0x00, 0x00, 0x2C, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x01, 0x00, 0x00, 0x02, 0x02, 0x44,
            0x01, 0x00, 0x3B
        };

        public MailTrackingController(
            IObjectSpaceFactory objectSpaceFactory,
            INonSecuredObjectSpaceFactory? nonSecuredObjectSpaceFactory = null,
            ILogger<MailTrackingController>? logger = null)
        {
            _objectSpaceFactory = objectSpaceFactory ?? throw new ArgumentNullException(nameof(objectSpaceFactory));
            _nonSecuredObjectSpaceFactory = nonSecuredObjectSpaceFactory;
            _logger = logger;
        }

        private IObjectSpace CreateObjectSpace()
        {
            if (_nonSecuredObjectSpaceFactory != null)
            {
                return _nonSecuredObjectSpaceFactory.CreateNonSecuredObjectSpace(typeof(Not));
            }
            return _objectSpaceFactory.CreateObjectSpace(typeof(Not));
        }

        [HttpGet("delivered/{noteId:guid}")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult TrackDelivered(Guid noteId)
        {
            try
            {
                using IObjectSpace objectSpace = CreateObjectSpace();
                var note = objectSpace.GetObjectByKey<Not>(noteId);

                if (note != null && note.MailDurumu != MailDurumu.Okundu && note.MailDurumu != MailDurumu.Iletildi)
                {
                    note.MailDurumu = MailDurumu.Iletildi;
                    note.MailIletilmeTarihi = DateTime.Now;
                    objectSpace.CommitChanges();
                    _logger?.LogInformation("Not bildirimi iletildi olarak işaretlendi. NoteId: {NoteId}", noteId);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Mail delivered işlenirken hata oluştu. NoteId: {NoteId}", noteId);
            }

            Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.Headers.Append("Pragma", "no-cache");
            Response.Headers.Append("Expires", "0");

            return File(TransparentGifBytes, "image/gif");
        }

        [HttpGet("read/{noteId:guid}")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult TrackRead(Guid noteId)
        {
            try
            {
                using IObjectSpace objectSpace = CreateObjectSpace();
                var note = objectSpace.GetObjectByKey<Not>(noteId);

                if (note != null && note.MailDurumu != MailDurumu.Okundu)
                {
                    note.MailDurumu = MailDurumu.Okundu;
                    if (!note.MailIletilmeTarihi.HasValue)
                    {
                        note.MailIletilmeTarihi = DateTime.Now;
                    }
                    note.MailOkunmaTarihi = DateTime.Now;
                    objectSpace.CommitChanges();
                    _logger?.LogInformation("Not bildirimi okundu olarak işaretlendi. NoteId: {NoteId}", noteId);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Mail tracking pikseli işlenirken hata oluştu. NoteId: {NoteId}", noteId);
            }

            Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.Headers.Append("Pragma", "no-cache");
            Response.Headers.Append("Expires", "0");

            return File(TransparentGifBytes, "image/gif");
        }
    }
}
