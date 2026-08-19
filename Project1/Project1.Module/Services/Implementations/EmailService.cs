#nullable enable
using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Project1.Core.Services.Interfaces;

namespace Project1.Module.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService>? _logger;

        public EmailService(EmailSettings settings, ILogger<EmailService>? logger = null)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger;

            if (string.IsNullOrWhiteSpace(_settings.SenderEmail))
            {
                _settings.SenderEmail = Environment.GetEnvironmentVariable("Email__SenderEmail") ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(_settings.SenderPassword))
            {
                _settings.SenderPassword = Environment.GetEnvironmentVariable("Email__SenderPassword") ?? string.Empty;
            }
        }

        public async Task<EmailResult> SendNoteNotificationEmailAsync(
            SendNoteNotificationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                string? configurationError = ValidateConfiguration();
                if (configurationError != null)
                {
                    _logger?.LogWarning("E-posta yapılandırması geçersiz: {Error}", configurationError);
                    return new EmailResult(false, configurationError);
                }

                if (string.IsNullOrWhiteSpace(request.ToEmail))
                {
                    _logger?.LogWarning("E-posta adresi belirtilmemiş.");
                    return new EmailResult(false, "E-posta adresi belirtilmemiş.");
                }

                _logger?.LogInformation("Not bildirim e-postası gönderiliyor. Alıcı: {ToEmail}, Başlık: {Title}", request.ToEmail, request.Title);

                using var message = CreateMailMessage(request);
                using var client = CreateSmtpClient();

                await client.SendMailAsync(message, cancellationToken).ConfigureAwait(false);
                _logger?.LogInformation("Not bildirim e-postası başarıyla gönderildi. Alıcı: {ToEmail}", request.ToEmail);
                return new EmailResult(true, null);
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("E-posta gönderim işlemi iptal edildi. Alıcı: {ToEmail}", request.ToEmail);
                return new EmailResult(false, "E-posta gönderim işlemi iptal edildi.");
            }
            catch (Exception exception)
            {
                _logger?.LogError(exception, "E-posta gönderilemedi ({ToEmail})", request.ToEmail);
                return new EmailResult(false, exception.Message);
            }
        }

        private MailMessage CreateMailMessage(SendNoteNotificationRequest request)
        {
            string safeKisiName = WebUtility.HtmlEncode(request.RecipientName);
            string safeMusteriName = WebUtility.HtmlEncode(request.CustomerName);
            string safeBaslik = WebUtility.HtmlEncode(request.Title);
            string safeIcerik = WebUtility.HtmlEncode(request.Content);
            string safeDerece = WebUtility.HtmlEncode(request.Severity);
            string trackingUrl = $"{_settings.BaseUrl.TrimEnd('/')}/api/mail-tracking/read/{request.NoteId}?ngrok-skip-browser-warning=true";

            var message = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = $"[Yeni Not Bildirimi] {request.CustomerName} - {request.Title}",
                IsBodyHtml = true,
                Body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 24px; background-color: #f4f6f9; border-radius: 8px;'>
                        <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 30px; border-radius: 8px;'>
                            <h2 style='color: #2c3e50; margin-top: 0;'>Merhaba {safeKisiName},</h2>
                            <p style='color: #34495e; font-size: 15px;'>Tarafınıza <strong>{safeMusteriName}</strong> müşterisi ile ilgili yeni bir not eklenmiştir.</p>
                            <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                            <table style='width: 100%; border-collapse: collapse; font-size: 14px;'>
                                <tr><td style='padding: 8px 0; color: #7f8c8d; width: 120px;'><strong>Müşteri:</strong></td><td style='padding: 8px 0; color: #2c3e50;'>{safeMusteriName}</td></tr>
                                <tr><td style='padding: 8px 0; color: #7f8c8d;'><strong>Not Başlığı:</strong></td><td style='padding: 8px 0; color: #2c3e50;'><strong>{safeBaslik}</strong></td></tr>
                                <tr><td style='padding: 8px 0; color: #7f8c8d;'><strong>Önem Derecesi:</strong></td><td style='padding: 8px 0; color: #e74c3c;'><strong>{safeDerece}</strong></td></tr>
                                <tr><td style='padding: 8px 0; color: #7f8c8d;'><strong>Tarih / Saat:</strong></td><td style='padding: 8px 0; color: #2c3e50;'>{DateTime.Now:dd.MM.yyyy HH:mm}</td></tr>
                            </table>
                            <div style='margin-top: 20px; padding: 16px; background-color: #ebf5fb; border-left: 4px solid #3498db; border-radius: 4px;'>
                                <strong style='color: #2980b9;'>Not İçeriği:</strong>
                                <p style='margin: 8px 0 0 0; color: #2c3e50; white-space: pre-wrap;'>{safeIcerik}</p>
                            </div>
                            <p style='font-size: 12px; color: #95a5a6; text-align: center;'>Bu e-posta otomatik olarak gönderilmiştir. Lütfen yanıtlamayınız.</p>
                            <img src='{trackingUrl}' alt='' width='1' height='1' border='0' style='display:block; width:1px; height:1px; max-width:1px; max-height:1px; opacity:0.01; border:0; outline:none; text-decoration:none;' />
                        </div>
                    </div>"
            };
            message.To.Add(new MailAddress(request.ToEmail, request.RecipientName));
            return message;
        }

        private SmtpClient CreateSmtpClient()
        {
            string cleanPassword = _settings.SenderPassword.Replace(" ", string.Empty);
            return new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_settings.SenderEmail, cleanPassword),
                EnableSsl = _settings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 10000
            };
        }

        private string? ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(_settings.SmtpHost))
            {
                return "SMTP sunucusu yapılandırılmamış.";
            }

            if (_settings.SmtpPort <= 0)
            {
                return "SMTP portu geçersiz.";
            }

            if (string.IsNullOrWhiteSpace(_settings.SenderEmail) ||
                string.IsNullOrWhiteSpace(_settings.SenderPassword))
            {
                return "Gönderici e-posta hesabı veya uygulama şifresi tanımlanmamış (appsettings.json veya Email__SenderEmail / Email__SenderPassword ortam değişkenini doldurun).";
            }

            return null;
        }
    }

    public sealed class EmailSettings
    {
        public string SmtpHost { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string SenderName { get; set; } = "Project1 Sistem Bildirimi";
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderPassword { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://localhost:5001";
    }
}
