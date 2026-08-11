using System;
using System.Linq;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Security;
using Project1.Module.Models.Entities;
using Project1.Module.Services;

namespace Project1.Module.Controllers
{
    public sealed class NotController : ObjectViewController<ObjectView, Not>
    {
        private static readonly Dictionary<string, DateTime> _recentlySentNoteKeys = new Dictionary<string, DateTime>();
        private bool _showToastNotification = false;
        private bool _emailPermissionDenied = false;

        protected override void OnActivated()
        {
            base.OnActivated();
            ObjectSpace.Committing += ObjectSpace_Committing;
            ObjectSpace.Committed += ObjectSpace_Committed;
        }

        protected override void OnDeactivated()
        {
            ObjectSpace.Committing -= ObjectSpace_Committing;
            ObjectSpace.Committed -= ObjectSpace_Committed;
            base.OnDeactivated();
        }

        private void ObjectSpace_Committing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _showToastNotification = false;
            _emailPermissionDenied = false;

            // Check if current user is allowed to send emails
            bool canSendEmail = true;
            if (SecuritySystem.CurrentUserId != null && ObjectSpace.GetObjectByKey<ApplicationUser>(SecuritySystem.CurrentUserId) is ApplicationUser currentUser)
            {
                if (currentUser.UserName == "Admin")
                {
                    canSendEmail = true; // Admin always sends emails
                }
                else
                {
                    canSendEmail = currentUser.CanSendEmailOnNoteCreation;
                }
            }

            if (!canSendEmail)
            {
                return; // User is not allowed to send email, skip sending. (Note is still created)
            }

            var pendingNotes = ObjectSpace.ModifiedObjects
                .OfType<Not>()
                .Where(n => !n.IsEmailSent)
                .ToList();

            if (pendingNotes.Count > 0 && !CanCurrentUserSendEmail())
            {
                _emailPermissionDenied = true;
                return;
            }

            foreach (var note in pendingNotes)
            {
                Kisi recipient = note.Kisi;
                if (recipient == null || string.IsNullOrWhiteSpace(recipient.Email))
                    continue;

                string deduplicationKey = $"{note.Oid}_{recipient.Email.ToLowerInvariant()}";

                lock (_recentlySentNoteKeys)
                {
                    var expiredKeys = _recentlySentNoteKeys
                        .Where(kvp => (DateTime.UtcNow - kvp.Value).TotalSeconds > 15)
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (var key in expiredKeys)
                    {
                        _recentlySentNoteKeys.Remove(key);
                    }

                    if (_recentlySentNoteKeys.ContainsKey(deduplicationKey))
                    {
                        continue;
                    }

                    _recentlySentNoteKeys[deduplicationKey] = DateTime.UtcNow;
                }

                var (success, errorMessage) = EmailService.SendNoteNotificationEmail(
                    recipient.Email,
                    recipient.AdSoyad,
                    note.Baslik,
                    note.Icerik,
                    note.Derece.ToString(),
                    note.Musteri?.Ad);

                if (success)
                {
                    note.IsEmailSent = true;
                    _showToastNotification = true;
                }
                else
                {
                    Application?.ShowViewStrategy?.ShowMessage(new MessageOptions
                    {
                        Message = $"E-posta gönderilemedi ({recipient.Email}): {errorMessage}",
                        Type = InformationType.Warning,
                        Duration = 8000
                    });
                }
            }
        }

        private void ObjectSpace_Committed(object sender, EventArgs e)
        {
            if (_emailPermissionDenied)
            {
                _emailPermissionDenied = false;
                Application?.ShowViewStrategy?.ShowMessage(new MessageOptions
                {
                    Message = "Not kaydedildi; e-posta gönderme yetkiniz kapalı olduğu için bildirim gönderilmedi.",
                    Type = InformationType.Warning,
                    Duration = 5000
                });
            }

            if (_showToastNotification)
            {
                _showToastNotification = false;
                Application?.ShowViewStrategy?.ShowMessage(new MessageOptions
                {
                    Message = "Not bu kişiye e-posta olarak gönderildi.",
                    Type = InformationType.Success,
                    Duration = 2000
                });
            }
        }

        private bool CanCurrentUserSendEmail()
        {
            ISecurityStrategyBase security = Application?.Security;
            if (security?.IsAuthenticated != true || security.UserId == null)
            {
                return false;
            }

            if (string.Equals(
                security.UserName,
                Security.SecurityConstants.AdministratorUserName,
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (Application == null)
            {
                return false;
            }

            INonSecuredObjectSpaceFactory objectSpaceFactory = Application.ServiceProvider?
                .GetService(typeof(INonSecuredObjectSpaceFactory)) as INonSecuredObjectSpaceFactory;

            if (objectSpaceFactory != null)
            {
                using IObjectSpace permissionObjectSpace =
                    objectSpaceFactory.CreateNonSecuredObjectSpace(typeof(UserEmailPermission));
                return GetEmailPermission(permissionObjectSpace, security.UserId);
            }

            // Normalde Blazor uygulamasında non-secured factory her zaman bulunur.
            // Yedek yol, uygulamanın farklı bir host ile çalıştırılması durumundadır.
            using IObjectSpace fallbackObjectSpace =
                Application.CreateObjectSpace(typeof(UserEmailPermission));
            return GetEmailPermission(fallbackObjectSpace, security.UserId);
        }

        private static bool GetEmailPermission(IObjectSpace objectSpace, object userId)
        {
            UserEmailPermission permission = objectSpace.FindObject<UserEmailPermission>(
                CriteriaOperator.Parse("User.Oid = ?", userId));
            return permission?.CanSendEmail == true;
        }
    }
}
