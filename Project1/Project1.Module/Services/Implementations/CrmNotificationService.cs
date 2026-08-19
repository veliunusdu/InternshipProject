using System;
using Project1.Core.Services.Interfaces;

namespace Project1.Module.Services.Implementations
{
    public class CrmNotificationService : ICrmNotificationService
    {
        public event Action<NoteReadNotificationEvent>? OnNoteRead;

        public void PublishNoteRead(NoteReadNotificationEvent notification)
        {
            OnNoteRead?.Invoke(notification);
        }
    }
}
