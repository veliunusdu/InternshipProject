#nullable enable
using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.Persistent.Base;
using Project1.Core.Services.Interfaces;

namespace Project1.Module.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(EmailSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

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

            string? configurationError = ValidateConfiguration();
            if (configurationError != null)
            {
                return new EmailResult(false, configurationError);
            }

            if (string.IsNullOrWhiteSpace(request.ToEmail))
            {
                return new EmailResult(false, "E-posta adresi belirtilmemiş.");
            }

            try
            {
                using var message = CreateMailMessage(request);
                using var client = CreateSmtpClient();

                await client.SendMailAsync(message);
                Tracing.Tracer.LogText($"[Email Success Async] Mail sent to {request.ToEmail} successfully.");
                return new EmailResult(true, null);
            }
            catch (Exception exception)
            {
                Tracing.Tracer.LogError($"[Email Error Async] Mail gönderilemedi ({request.ToEmail}): {exception}");
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
                            </table>
                            <div style='margin-top: 20px; padding: 16px; background-color: #ebf5fb; border-left: 4px solid #3498db; border-radius: 4px;'>
                                <strong style='color: #2980b9;'>Not İçeriği:</strong>
                                <p style='margin: 8px 0 0 0; color: #2c3e50; white-space: pre-wrap;'>{safeIcerik}</p>
                            </div>
                            <p style='font-size: 12px; color: #95a5a6; text-align: center;'>Bu e-posta otomatik olarak gönderilmiştir. Lütfen yanıtlamayınız.</p>
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
                DeliveryMethod = SmtpDeliveryMethod.Network
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
    }
}
