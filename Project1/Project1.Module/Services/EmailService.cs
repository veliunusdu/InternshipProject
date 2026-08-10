using System;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using DevExpress.Persistent.Base;

namespace Project1.Module.Services
{
    public static class EmailService
    {
        // Ücretsiz Gmail / Outlook SMTP Yapılandırması
        public static string SmtpHost { get; set; } = "smtp.gmail.com";
        public static int SmtpPort { get; set; } = 587;
        public static string SenderEmail { get; set; } = "skolobarba@gmail.com";
        public static string SenderPassword { get; set; } = "tevd tjui zxbe igjn";

        public static (bool Success, string ErrorMessage) SendNoteNotificationEmail(
            string toEmail,
            string kisiName,
            string baslik,
            string icerik,
            string derece,
            string musteriName)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                return (false, "E-posta adresi belirtilmemiş.");
            }

            try
            {
                string cleanPassword = SenderPassword?.Replace(" ", "") ?? string.Empty;

                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(SenderEmail, "Project1 Sistem Bildirimi");
                    message.To.Add(new MailAddress(toEmail, kisiName));
                    message.Subject = $"[Yeni Not Bildirimi] {musteriName} - {baslik}";
                    message.IsBodyHtml = true;
                    message.Body = $@"
                        <div style='font-family: Arial, sans-serif; padding: 24px; background-color: #f4f6f9; border-radius: 8px;'>
                            <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 30px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
                                <h2 style='color: #2c3e50; margin-top: 0;'>Merhaba {kisiName},</h2>
                                <p style='color: #34495e; font-size: 15px;'>Tarafınıza <strong>{musteriName}</strong> müşterisi ile ilgili yeni bir not eklenmiştir.</p>
                                <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                                <table style='width: 100%; border-collapse: collapse; font-size: 14px;'>
                                    <tr>
                                        <td style='padding: 8px 0; color: #7f8c8d; width: 120px;'><strong>Müşteri:</strong></td>
                                        <td style='padding: 8px 0; color: #2c3e50;'>{musteriName}</td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 8px 0; color: #7f8c8d;'><strong>Not Başlığı:</strong></td>
                                        <td style='padding: 8px 0; color: #2c3e50;'><strong>{baslik}</strong></td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 8px 0; color: #7f8c8d;'><strong>Önem Derecesi:</strong></td>
                                        <td style='padding: 8px 0; color: #e74c3c;'><strong>{derece}</strong></td>
                                    </tr>
                                </table>
                                <div style='margin-top: 20px; padding: 16px; background-color: #ebf5fb; border-left: 4px solid #3498db; border-radius: 4px;'>
                                    <strong style='color: #2980b9;'>Not İçeriği:</strong>
                                    <p style='margin: 8px 0 0 0; color: #2c3e50; white-space: pre-wrap;'>{icerik}</p>
                                </div>
                                <br/>
                                <p style='font-size: 12px; color: #95a5a6; text-align: center; margin-bottom: 0;'>Bu e-posta otomatik olarak gönderilmiştir. Lütfen yanıtlamayınız.</p>
                            </div>
                        </div>";

                    using (var client = new SmtpClient(SmtpHost, SmtpPort))
                    {
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential(SenderEmail, cleanPassword);
                        client.EnableSsl = true;
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;

                        client.Send(message);
                        Tracing.Tracer.LogText($"[Email Success] Mail sent to {toEmail} successfully.");
                        return (true, null);
                    }
                }
            }
            catch (Exception ex)
            {
                string err = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Tracing.Tracer.LogError($"[Email Error] Mail gönderilemedi ({toEmail}): {ex}");
                Debug.WriteLine($"[Email Error] Mail gönderilemedi: {ex}");
                return (false, err);
            }
        }
    }
}

