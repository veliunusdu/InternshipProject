# EmailService Refactoring Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Refactor `EmailService` from a static class to an instance-based class implementing `IEmailService`, registered as a Singleton in the DI container, and consume it via DI in `NotEmailNotificationController`.

**Architecture:** Create `IEmailService` interface in `Project1.Module/Services`, convert `EmailService` to a non-static class implementing `IEmailService` with `EmailSettings` constructor parameter, register in `Startup.cs` DI container, and update `NotEmailNotificationController` to resolve `IEmailService`.

**Tech Stack:** C# 10, .NET 6 / DevExpress XAF Blazor, Microsoft.Extensions.DependencyInjection.

## Global Constraints
- Do not introduce breaking API changes outside of `EmailService` and `NotEmailNotificationController`.
- Maintain HTML formatting and logic for `SendNoteNotificationEmail`.

---

### Task 1: Create `IEmailService` Interface

**Files:**
- Create: `Project1/Project1.Module/Services/IEmailService.cs`

**Interfaces:**
- Produces: `IEmailService` interface with `(bool Success, string ErrorMessage) SendNoteNotificationEmail(string toEmail, string kisiName, string baslik, string icerik, string derece, string musteriName)`

- [ ] **Step 1: Create `IEmailService.cs` interface file**

```csharp
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
```

- [ ] **Step 2: Commit Task 1**

```bash
git add Project1/Project1.Module/Services/IEmailService.cs
git commit -m "feat: add IEmailService interface"
```

---

### Task 2: Refactor `EmailService` to Implement `IEmailService`

**Files:**
- Modify: `Project1/Project1.Module/Services/EmailService.cs`

**Interfaces:**
- Consumes: `IEmailService`
- Produces: `EmailService` class implementing `IEmailService` with constructor `EmailService(EmailSettings emailSettings)`

- [ ] **Step 1: Update `EmailService.cs`**

Remove `static` keywords from `EmailService` class and methods. Inject `EmailSettings` via constructor.

```csharp
using System;
using System.Net;
using System.Net.Mail;
using DevExpress.Persistent.Base;

namespace Project1.Module.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(EmailSettings emailSettings)
        {
            _settings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));

            if (string.IsNullOrWhiteSpace(_settings.SenderEmail))
            {
                _settings.SenderEmail = Environment.GetEnvironmentVariable("Email__SenderEmail") ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(_settings.SenderPassword))
            {
                _settings.SenderPassword = Environment.GetEnvironmentVariable("Email__SenderPassword") ?? string.Empty;
            }
        }

        public (bool Success, string ErrorMessage) SendNoteNotificationEmail(
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
                string cleanPassword = _settings.SenderPassword.Replace(" ", string.Empty);
                string safeKisiName = WebUtility.HtmlEncode(kisiName);
                string safeMusteriName = WebUtility.HtmlEncode(musteriName);
                string safeBaslik = WebUtility.HtmlEncode(baslik);
                string safeIcerik = WebUtility.HtmlEncode(icerik);
                string safeDerece = WebUtility.HtmlEncode(derece);

                using var message = new MailMessage
                {
                    From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
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

                using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_settings.SenderEmail, cleanPassword),
                    EnableSsl = _settings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                client.Send(message);
                Tracing.Tracer.LogText($"[Email Success] Mail sent to {toEmail} successfully.");
                return (true, null);
            }
            catch (Exception exception)
            {
                Tracing.Tracer.LogError($"[Email Error] Mail gönderilemedi ({toEmail}): {exception}");
                return (false, exception.Message);
            }
        }

        private string ValidateConfiguration()
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
```

- [ ] **Step 2: Commit Task 2**

```bash
git add Project1/Project1.Module/Services/EmailService.cs
git commit -m "refactor: convert EmailService to non-static class implementing IEmailService"
```

---

### Task 3: Register `IEmailService` in `Startup.cs`

**Files:**
- Modify: `Project1/Project1.Blazor.Server/Startup.cs:25-30`

**Interfaces:**
- Consumes: `EmailSettings`, `EmailService`, `IEmailService`

- [ ] **Step 1: Update `Startup.cs` ConfigureServices method**

Replace `EmailService.Configure(emailSettings);` with DI singleton registration:

```csharp
EmailSettings emailSettings = Configuration.GetSection("Email").Get<EmailSettings>() ?? new EmailSettings();
services.AddSingleton(emailSettings);
services.AddSingleton<IEmailService, EmailService>();
```

- [ ] **Step 2: Commit Task 3**

```bash
git add Project1/Project1.Blazor.Server/Startup.cs
git commit -m "feat: register IEmailService in Startup.cs DI container"
```

---

### Task 4: Update `NotEmailNotificationController` to use `IEmailService`

**Files:**
- Modify: `Project1/Project1.Module/Controllers/NotController.cs:103-110`

**Interfaces:**
- Consumes: `IEmailService`

- [ ] **Step 1: Update `NotController.cs` to resolve `IEmailService`**

Replace static call `EmailService.SendNoteNotificationEmail(...)` with service resolution from `Application.ServiceProvider`:

```csharp
var emailService = Application.ServiceProvider?.GetService(typeof(IEmailService)) as IEmailService;
var (success, errorMessage) = emailService?.SendNoteNotificationEmail(
    recipient.Email,
    recipient.AdSoyad,
    note.Baslik,
    note.Icerik,
    note.Derece.ToString(),
    note.Musteri?.Ad) ?? (false, "E-posta servisi bulunamadı.");
```

- [ ] **Step 2: Commit Task 4**

```bash
git add Project1/Project1.Module/Controllers/NotController.cs
git commit -m "refactor: update NotEmailNotificationController to resolve IEmailService from DI"
```

---

### Task 5: Build and Verify Solution

- [ ] **Step 1: Run `dotnet build`**

Run: `dotnet build Project1/Project1.sln`
Expected: Build succeeds with 0 errors.

- [ ] **Step 2: Final Commit**

```bash
git add .
git commit -m "chore: complete EmailService DI refactoring"
```
