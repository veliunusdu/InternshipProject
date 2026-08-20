#nullable enable
using System.Threading;
using System.Threading.Tasks;

namespace Project1.Core.Services.Interfaces
{
    /// <summary>
    /// E-posta bildirim isteği için parametre nesnesi.
    /// </summary>
    public record SendNoteNotificationRequest(
        Guid NoteId,
        string ToEmail,
        string RecipientName,
        string Title,
        string Content,
        string Severity,
        string CustomerName
    );

    /// <summary>
    /// Müşteri e-posta aktivasyon bildirimi için parametre nesnesi.
    /// </summary>
    public record SendEmailConfirmationRequest(
        string ToEmail,
        string CustomerName,
        string ConfirmationUrl,
        DateTime ExpiryDate
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

        /// <summary>
        /// Asenkron olarak yeni müşteri hesap aktivasyon e-postası gönderir.
        /// </summary>
        Task<EmailResult> SendConfirmationEmailAsync(
            SendEmailConfirmationRequest request,
            CancellationToken cancellationToken = default);
    }
}
