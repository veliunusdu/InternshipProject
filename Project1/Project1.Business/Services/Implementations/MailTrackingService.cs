#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using Microsoft.Extensions.Logging;
using Project1.Core.Enums;
using Project1.Core.Services.Interfaces;
using Project1.Module.Models.Audit;
using Project1.Module.Models.Notes;

namespace Project1.Business.Services.Implementations
{
    public class MailTrackingService : IMailTrackingService
    {
        private readonly IObjectSpaceFactory _objectSpaceFactory;
        private readonly INonSecuredObjectSpaceFactory? _nonSecuredObjectSpaceFactory;
        private readonly ICrmNotificationService? _notificationService;
        private readonly ILogger<MailTrackingService>? _logger;

        public MailTrackingService(
            IObjectSpaceFactory objectSpaceFactory,
            INonSecuredObjectSpaceFactory? nonSecuredObjectSpaceFactory = null,
            ICrmNotificationService? notificationService = null,
            ILogger<MailTrackingService>? logger = null)
        {
            _objectSpaceFactory = objectSpaceFactory ?? throw new ArgumentNullException(nameof(objectSpaceFactory));
            _nonSecuredObjectSpaceFactory = nonSecuredObjectSpaceFactory;
            _notificationService = notificationService;
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

        public Task<bool> ProcessDeliveredAsync(Guid noteId, CancellationToken cancellationToken = default)
        {
            try
            {
                using IObjectSpace objectSpace = CreateObjectSpace();
                var note = objectSpace.GetObjectByKey<Not>(noteId);

                if (note != null && note.MailDurumu != MailDurumu.Okundu && note.MailDurumu != MailDurumu.Iletildi)
                {
                    note.MailDurumu = MailDurumu.Iletildi;
                    note.MailIletilmeTarihi = DateTime.Now;

                    var auditLog = objectSpace.CreateObject<AuditLog>();
                    auditLog.Tarih = DateTime.Now;
                    auditLog.Kullanici = "Posta Dağıtım Sunucusu (Webhook)";
                    auditLog.IslemTuru = "E-posta İletildi";
                    auditLog.VarlikTipi = "Not";
                    auditLog.VarlikId = note.Oid;
                    auditLog.Aciklama = $"'{note.Baslik}' başlıklı notun e-posta teslimatı teyit edildi.";

                    objectSpace.CommitChanges();
                    _logger?.LogInformation("Not bildirimi iletildi olarak işaretlendi. NoteId: {NoteId}", noteId);
                    return Task.FromResult(true);
                }

                return Task.FromResult(note != null);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Mail delivered işlenirken hata oluştu. NoteId: {NoteId}", noteId);
                return Task.FromResult(false);
            }
        }

        public Task<bool> ProcessReadAsync(Guid noteId, CancellationToken cancellationToken = default)
        {
            try
            {
                using IObjectSpace objectSpace = CreateObjectSpace();
                var note = objectSpace.GetObjectByKey<Not>(noteId);

                if (note != null)
                {
                    bool wasNotReadYet = note.MailDurumu != MailDurumu.Okundu;
                    if (wasNotReadYet)
                    {
                        note.MailDurumu = MailDurumu.Okundu;
                        if (!note.MailIletilmeTarihi.HasValue)
                        {
                            note.MailIletilmeTarihi = DateTime.Now;
                        }
                        note.MailOkunmaTarihi = DateTime.Now;

                        var auditLog = objectSpace.CreateObject<AuditLog>();
                        auditLog.Tarih = DateTime.Now;
                        auditLog.Kullanici = "Alıcı (E-posta İstemcisi)";
                        auditLog.IslemTuru = "E-posta Okundu";
                        auditLog.VarlikTipi = "Not";
                        auditLog.VarlikId = note.Oid;
                        auditLog.Aciklama = $"'{note.Baslik}' başlıklı not bildirim e-postası alıcı tarafından açıldı/okundu.";

                        objectSpace.CommitChanges();
                        _logger?.LogInformation("Not bildirimi okundu olarak işaretlendi. NoteId: {NoteId}", noteId);
                    }

                    _notificationService?.PublishNoteRead(new NoteReadNotificationEvent(
                        note.Oid,
                        "Okundu",
                        note.MailOkunmaTarihi ?? DateTime.Now,
                        note.Baslik ?? "-",
                        note.Musteri?.Ad ?? "-",
                        note.Kisi?.AdSoyad ?? "-"
                    ));

                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Mail tracking pikseli işlenirken hata oluştu. NoteId: {NoteId}", noteId);
                return Task.FromResult(false);
            }
        }
    }
}
