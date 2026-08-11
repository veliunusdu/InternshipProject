using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Security;
using Project1.Module.Models.Entities;
using Project1.Module.Services;

namespace Project1.Module.Controllers
{
    public class NotController : ViewController
    {
        private static readonly Dictionary<string, DateTime> _recentlySentNoteKeys = new Dictionary<string, DateTime>();
        private bool _showToastNotification = false;

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

        private static List<Kisi> GetRecipientsForNote(Not note)
        {
            var result = new List<Kisi>();
            if (note.Kisi != null && !string.IsNullOrWhiteSpace(note.Kisi.Email))
            {
                result.Add(note.Kisi);
            }
            return result;
        }

        private void ObjectSpace_Committing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _showToastNotification = false;

            // Check if current user is allowed to send emails
            bool canSendEmail = true;
            if (SecuritySystem.CurrentUser is ApplicationUser currentUser)
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

            foreach (var note in pendingNotes)
            {
                var recipients = GetRecipientsForNote(note);
                if (recipients.Count == 0)
                    continue;

                bool anyEmailSentSuccessfully = false;

                foreach (var kisi in recipients)
                {
                    string deduplicationKey = $"{note.Oid}_{kisi.Email.ToLowerInvariant()}";

                    lock (_recentlySentNoteKeys)
                    {
                        var expiredKeys = _recentlySentNoteKeys
                            .Where(kvp => (DateTime.Now - kvp.Value).TotalSeconds > 15)
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

                        _recentlySentNoteKeys[deduplicationKey] = DateTime.Now;
                    }

                    var (success, errorMsg) = EmailService.SendNoteNotificationEmail(
                        kisi.Email,
                        kisi.AdSoyad,
                        note.Baslik,
                        note.Icerik,
                        note.Derece.ToString(),
                        note.Musteri?.Ad);

                    if (success)
                    {
                        anyEmailSentSuccessfully = true;
                    }
                    else
                    {
                        try
                        {
                            Application?.ShowViewStrategy?.ShowMessage(new MessageOptions
                            {
                                Message = $"E-posta gönderilemedi ({kisi.Email}): {errorMsg}",
                                Type = InformationType.Warning,
                                Duration = 8000
                            });
                        }
                        catch
                        {
                            // Fallback
                        }
                    }
                }

                if (anyEmailSentSuccessfully)
                {
                    note.IsEmailSent = true;
                    _showToastNotification = true;
                }
            }
        }

        private void ObjectSpace_Committed(object sender, EventArgs e)
        {
            if (_showToastNotification)
            {
                _showToastNotification = false;
                try
                {
                    Application?.ShowViewStrategy?.ShowMessage(new MessageOptions
                    {
                        Message = "Not bu kişiye mail olarak gitti",
                        Type = InformationType.Success,
                        Duration = 2000
                    });
                }
                catch
                {
                    // Fallback
                }
            }

            try
            {
                if (View is ListView listView && listView.CollectionSource != null)
                {
                    listView.CollectionSource.Reload();
                }
                else if (View != null && View.ObjectSpace != null)
                {
                    View.ObjectSpace.Refresh();
                }
            }
            catch
            {
                // Fallback
            }
        }
    }
}
