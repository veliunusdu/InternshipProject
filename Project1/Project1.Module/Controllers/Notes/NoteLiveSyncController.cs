#nullable enable
using System;
using System.Threading;
using DevExpress.ExpressApp;
using Project1.Core.Services.Interfaces;
using Project1.Module.Models.Notes;

namespace Project1.Module.Controllers.Notes
{
    /// <summary>
    /// E-posta okundu bildirimi (Webhook/Tracking pixel) tetiklendiğinde ekrandaki
    /// Not listesini veya detayını otomatik ve anlık olarak yeniler (F5/Refresh gerektirmez).
    /// </summary>
    public class NoteLiveSyncController : ObjectViewController<ObjectView, Not>
    {
        private ICrmNotificationService? _notificationService;
        private SynchronizationContext? _syncContext;

        protected override void OnActivated()
        {
            base.OnActivated();
            _syncContext = SynchronizationContext.Current;
            _notificationService = Application?.ServiceProvider?.GetService(typeof(ICrmNotificationService)) as ICrmNotificationService;
            if (_notificationService != null)
            {
                _notificationService.OnNoteRead += NotificationService_OnNoteRead;
            }
        }

        protected override void OnDeactivated()
        {
            if (_notificationService != null)
            {
                _notificationService.OnNoteRead -= NotificationService_OnNoteRead;
            }
            base.OnDeactivated();
        }

        private void NotificationService_OnNoteRead(NoteReadNotificationEvent evt)
        {
            Action refreshAction = () =>
            {
                try
                {
                    // XAF yerleşik bildirim mesajı
                    Application?.ShowViewStrategy?.ShowMessage(new MessageOptions
                    {
                        Message = $"✉️ {evt.ContactName} ({evt.CustomerName}) '{evt.Title}' başlıklı not bildirimini okudu!",
                        Type = InformationType.Success,
                        Duration = 6000
                    });

                    if (View != null && !View.IsDisposed && ObjectSpace != null && !ObjectSpace.IsDisposed)
                    {
                        if (View is ListView listView)
                        {
                            listView.CollectionSource?.Reload();
                            listView.ObjectSpace?.Refresh();
                            listView.Refresh();
                        }
                        else if (View is DetailView detailView && detailView.CurrentObject is Not currentNot)
                        {
                            if (currentNot.Oid == evt.NoteId)
                            {
                                detailView.ObjectSpace?.ReloadObject(currentNot);
                                detailView.Refresh();
                            }
                        }
                    }
                }
                catch
                {
                    // View kapandıysa veya eşzamanlı dispose olduysa yoksay
                }
            };

            if (_syncContext != null)
            {
                _syncContext.Post(_ => refreshAction(), null);
            }
            else
            {
                refreshAction();
            }
        }
    }
}
