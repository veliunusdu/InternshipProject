# EmailService Refactoring to Interface and Dependency Injection

## Overview
Refactor `EmailService` in `Project1.Module` from a static class (`public static class EmailService`) to an instance-based class (`public class EmailService : IEmailService`) using constructor injection for configuration (`EmailSettings`). Register `IEmailService` as a singleton in the Dependency Injection container (`Startup.cs`) and update consumer controllers (`NotController.cs`).

## Design Details

### 1. Interface Definition (`IEmailService.cs`)
Create `Project1/Project1.Module/Services/IEmailService.cs`:
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

### 2. Service Implementation (`EmailService.cs`)
Modify `Project1/Project1.Module/Services/EmailService.cs`:
- Remove `static` modifier from class and methods.
- Store `EmailSettings` as `private readonly EmailSettings _settings;` injected via constructor.
- Fallback environment variable check during configuration validation / constructor initialization.

```csharp
namespace Project1.Module.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(EmailSettings settings)
        {
            _settings = settings ?? new EmailSettings();
            
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
            // Email sending logic using _settings
        }
    }
}
```

### 3. DI Container Registration (`Startup.cs`)
In `Project1.Blazor.Server/Startup.cs`:
```csharp
EmailSettings emailSettings = Configuration.GetSection("Email").Get<EmailSettings>() ?? new EmailSettings();
services.AddSingleton(emailSettings);
services.AddSingleton<IEmailService, EmailService>();
```

### 4. Controller Refactoring (`NotController.cs`)
In `Project1/Project1.Module/Controllers/NotController.cs` (`NotEmailNotificationController`):
- Obtain `IEmailService` using `Application.ServiceProvider?.GetService(typeof(IEmailService)) as IEmailService;` (or `GetRequiredService<IEmailService>()`).
- Call `emailService.SendNoteNotificationEmail(...)`.

## Verification Strategy
- Build solution using `dotnet build`.
- Verify `IEmailService` is resolved successfully without compile or runtime errors.
