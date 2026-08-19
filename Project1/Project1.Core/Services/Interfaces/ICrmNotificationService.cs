using System;

namespace Project1.Core.Services.Interfaces
{
    public record NoteReadNotificationEvent(
        Guid NoteId,
        string Status,
        DateTime ReadDate,
        string Title,
        string CustomerName,
        string ContactName
    );

    public interface ICrmNotificationService
    {
        event Action<NoteReadNotificationEvent> OnNoteRead;
        void PublishNoteRead(NoteReadNotificationEvent notification);
    }
}
