using System.Net;
using System.Net.Mail;
using DevExpress.Persistent.Base;

namespace Project1.Module.Services
{
    public static class EmailService
    {
        private static EmailSettings settings = new();

        public static void Configure(EmailSettings emailSettings)
        {
            settings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
        }

        public static (bool Success, string ErrorMessage) SendNoteNotificationEmail(
            string toEmail,
            string kisiName,
            string baslik,
            string icerik,
            string derece,
            string musteriName)
        {
            string configurationError = ValidateConfiguration();
            if (configurationError != null)
            {
                return (false, configurationError);
            }

            if (string.IsNullOrWhiteSpace(toEmail))
            {
                return (false, "E-posta adresi belirtilmemiş.");
            }

            try
            {
                string cleanPassword = settings.SenderPassword.Replace(" ", string.Empty);
                string safeKisiName = WebUtility.HtmlEncode(kisiName);
                string safeMusteriName = WebUtility.HtmlEncode(musteriName);
                string safeBaslik = WebUtility.HtmlEncode(baslik);
                string safeIcerik = WebUtility.HtmlEncode(icerik);
                string safeDerece = WebUtility.HtmlEncode(derece);

                using var message = new MailMessage
                {
                    From = new MailAddress(settings.SenderEmail, settings.SenderName),
                    Subject = $"[Yeni Not Bildirimi] {musteriName} - {baslik}",
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
                message.To.Add(new MailAddress(toEmail, kisiName));

                using var client = new SmtpClient(settings.SmtpHost, settings.SmtpPort)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(settings.SenderEmail, cleanPassword),
                    EnableSsl = settings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                client.Send(message);
                Tracing.Tracer.LogText($"[Email Success] Mail sent to {toEmail} successfully.");
                return (true, null);
            }
            catch (Exception exception)
            {
                string errorMessage = exception.InnerException?.Message ?? exception.Message;
                Tracing.Tracer.LogError($"[Email Error] Mail gönderilemedi ({toEmail}): {exception}");
                return (false, errorMessage);
            }
        }

        private static string ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(settings.SmtpHost))
            {
                return "SMTP sunucusu yapılandırılmamış.";
            }

            if (settings.SmtpPort <= 0)
            {
                return "SMTP portu geçersiz.";
            }

            if (string.IsNullOrWhiteSpace(settings.SenderEmail) ||
                string.IsNullOrWhiteSpace(settings.SenderPassword))
            {
                return "Gönderici e-posta hesabı yapılandırılmamış.";
            }

            return null;
        }
    }
}
