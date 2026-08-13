#nullable enable
using System.Threading;
using System.Threading.Tasks;

namespace Project1.Module.Services.Interfaces
{
    /// <summary>
    /// E-posta bildirim isteği için parametre nesnesi.
    /// </summary>
    public record SendNoteNotificationRequest(
        string ToEmail,
        string RecipientName,
        string Title,
        string Content,
        string Severity,
        string CustomerName
    );

    /// <summary>
    /// E-posta bildirim sonucu nesnesi.
    /// </summary>
    public record EmailResult(bool Success, string? ErrorMessage);

    /// <summary>
    /// E-posta gönderim servisi sözleşmesi.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Asenkron olarak not bildirimi e-postası gönderir.
        /// </summary>
        Task<EmailResult> SendNoteNotificationEmailAsync(
            SendNoteNotificationRequest request,
            CancellationToken cancellationToken = default);
    }
}
