namespace Project1.Module.Services
{
    public interface IEmailService
    {
        (bool Success, string ErrorMessage) SendNoteNotificationEmail(
            string toEmail,
            string kisiName,
            string baslik,
            string icerik,
            string derece,
            string musteriName);
    }
}
