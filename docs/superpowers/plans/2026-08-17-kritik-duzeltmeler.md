# 🔴 Kritik Düzeltmeler Uygulama Planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `EmailService` üzerinde `CancellationToken` ve `ILogger<EmailService>` structured logging entegrasyonu sağlamak, `NoteService.CreateNoteAsync` metodunda `Musteri` ve `Kisi` nesne grafiği bağlantılarını kurmak ve `SystemStatusControllers.cs` dosyasını API ve XAF Window controller olarak iki bağımsız dosyaya ayırmak.

**Architecture:** ASP.NET Core Dependency Injection & `ILogger<T>`, Cooperative CancellationToken Pattern, DevExpress XAF ObjectSpace & XPO Navigation Properties, ASP.NET Core Web API Separation.

**Tech Stack:** .NET 8.0, C#, DevExpress XAF / XPO, Microsoft.Extensions.Logging, xUnit, Moq, FluentAssertions.

## Global Constraints
- `dotnet build InternshipProject.sln` sıfır derleme hatası ve uyarısıyla tamamlanmalıdır.
- `dotnet test InternshipProject.sln` tüm birim ve entegrasyon testlerini başarıyla geçirmelidir.
- Hassas bilgiler (şifreler, kimlik bilgileri vb.) loglara yazılmamalıdır.
- Tüm commit mesajları Conventional Commits (`feat:`, `fix:`, `refactor:`, `test:`) formatına uygun olmalıdır.

---

### Task 1: `EmailService` CancellationToken & Asenkron İptal Desteği

**Files:**
- Modify: `Project1/Project1.Module/Services/Implementations/EmailService.cs`
- Test: `Project1/Project1.Module.Tests/Services/EmailServiceTests.cs`

**Interfaces:**
- Consumes: `IEmailService.SendNoteNotificationEmailAsync(SendNoteNotificationRequest, CancellationToken)`
- Produces: `EmailResult(bool Success, string? ErrorMessage)`

- [ ] **Step 1: Write test for cancelled CancellationToken**

In `Project1/Project1.Module.Tests/Services/EmailServiceTests.cs`:
```csharp
[Fact]
public async Task SendNoteNotificationEmailAsync_ShouldReturnFailure_WhenCancellationTokenIsCancelled()
{
    // Arrange
    var settings = new EmailSettings
    {
        SmtpHost = "smtp.test.com",
        SmtpPort = 587,
        SenderEmail = "sender@example.com",
        SenderPassword = "password"
    };
    var emailService = new EmailService(settings);
    var request = new SendNoteNotificationRequest(
        ToEmail: "test@example.com",
        RecipientName: "Test Alıcı",
        Title: "Başlık",
        Content: "İçerik",
        Severity: "Normal",
        CustomerName: "Müşteri"
    );
    using var cts = new CancellationTokenSource();
    cts.Cancel(); // Önceden iptal edilmiş token

    // Act
    var result = await emailService.SendNoteNotificationEmailAsync(request, cts.Token);

    // Assert
    result.Success.Should().BeFalse();
    result.ErrorMessage.Should().Contain("iptal");
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter FullyQualifiedName~SendNoteNotificationEmailAsync_ShouldReturnFailure_WhenCancellationTokenIsCancelled`

- [ ] **Step 3: Update `EmailService.cs` with CancellationToken handling**

```csharp
cancellationToken.ThrowIfCancellationRequested();

try
{
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
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test --filter FullyQualifiedName~EmailServiceTests`

- [ ] **Step 5: Commit**

```bash
git add Project1/Project1.Module/Services/Implementations/EmailService.cs Project1/Project1.Module.Tests/Services/EmailServiceTests.cs
git commit -m "feat(email): add CancellationToken and cooperative cancellation support to EmailService"
```

---

### Task 2: `EmailService` `ILogger<EmailService>` Entegrasyonu ve Structured Logging

**Files:**
- Modify: `Project1/Project1.Module/Services/Implementations/EmailService.cs`
- Modify: `Project1/Project1.Blazor.Server/Startup.cs`
- Test: `Project1/Project1.Module.Tests/Services/EmailServiceTests.cs`

**Interfaces:**
- Consumes: `Microsoft.Extensions.Logging.ILogger<EmailService>`
- Produces: `EmailService(EmailSettings settings, ILogger<EmailService>? logger = null)`

- [ ] **Step 1: Write test for logger invocation**

In `Project1/Project1.Module.Tests/Services/EmailServiceTests.cs`:
```csharp
[Fact]
public async Task SendNoteNotificationEmailAsync_ShouldLogWarning_WhenCancelled()
{
    var settings = new EmailSettings
    {
        SmtpHost = "smtp.test.com",
        SmtpPort = 587,
        SenderEmail = "sender@example.com",
        SenderPassword = "password"
    };
    var mockLogger = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<EmailService>>();
    var emailService = new EmailService(settings, mockLogger.Object);
    var request = new SendNoteNotificationRequest(
        ToEmail: "test@example.com",
        RecipientName: "Test Alıcı",
        Title: "Başlık",
        Content: "İçerik",
        Severity: "Normal",
        CustomerName: "Müşteri"
    );
    using var cts = new CancellationTokenSource();
    cts.Cancel();

    var result = await emailService.SendNoteNotificationEmailAsync(request, cts.Token);

    result.Success.Should().BeFalse();
    mockLogger.Verify(
        x => x.Log(
            Microsoft.Extensions.Logging.LogLevel.Warning,
            Moq.It.IsAny<Microsoft.Extensions.Logging.EventId>(),
            Moq.It.Is<Moq.It.IsAnyType>((v, t) => v.ToString()!.Contains("iptal")),
            Moq.It.IsAny<Exception>(),
            Moq.It.IsAny<Func<Moq.It.IsAnyType, Exception?, string>>()),
        Moq.Times.AtLeastOnce);
}
```

- [ ] **Step 2: Update `EmailService.cs` constructor and logging calls**

```csharp
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
```
Remove any `Tracing.Tracer` calls completely and use `_logger.LogInformation`, `_logger.LogWarning`, `_logger.LogError`.

- [ ] **Step 3: Run test to verify it passes**

Run: `dotnet test --filter FullyQualifiedName~EmailServiceTests`

- [ ] **Step 4: Commit**

```bash
git add Project1/Project1.Module/Services/Implementations/EmailService.cs Project1/Project1.Blazor.Server/Startup.cs Project1/Project1.Module.Tests/Services/EmailServiceTests.cs
git commit -m "feat(email): inject ILogger and implement structured logging in EmailService"
```

---

### Task 3: `NoteService.CreateNoteAsync` Müşteri/Kişi Nesne Grafiği Bağlantısı

**Files:**
- Modify: `Project1/Project1.DTOs/Notes/NoteDto.cs`
- Modify: `Project1/Project1.Module/Services/Implementations/NoteService.cs`
- Test: `Project1/Project1.Module.Tests/Services/NoteServiceTests.cs` (New test file)

**Interfaces:**
- Consumes: `CreateNoteRequestDto(string Baslik, string Icerik, int Derece, Guid? MusteriOid = null, Guid? KisiOid = null)`
- Produces: `NoteDto MapToDto(Not n)` with Customer and Person names resolved

- [ ] **Step 1: Update `CreateNoteRequestDto` in `Project1.DTOs/Notes/NoteDto.cs`**

```csharp
namespace Project1.DTOs.Notes
{
    public record NoteDto(
        Guid Oid,
        string Baslik,
        string Icerik,
        string Derece,
        string Musteri,
        string Kisi,
        bool IsEmailSent
    );

    public record CreateNoteRequestDto(
        string Baslik,
        string Icerik,
        int Derece,
        Guid? MusteriOid = null,
        Guid? KisiOid = null
    );
}
```

- [ ] **Step 2: Update `NoteService.cs` to set `Musteri` and `Kisi`**

```csharp
public Task<NoteDto> CreateNoteAsync(CreateNoteRequestDto request, CancellationToken cancellationToken = default)
{
    cancellationToken.ThrowIfCancellationRequested();

    using IObjectSpace objectSpace = CreateObjectSpace();
    var not = objectSpace.CreateObject<Not>();
    not.Baslik = request.Baslik;
    not.Icerik = request.Icerik;
    not.Derece = (BusinessObjects.Enums.NotDerecesi)request.Derece;

    if (request.MusteriOid.HasValue && request.MusteriOid.Value != Guid.Empty)
    {
        not.Musteri = objectSpace.GetObjectByKey<Musteri>(request.MusteriOid.Value);
    }

    if (request.KisiOid.HasValue && request.KisiOid.Value != Guid.Empty)
    {
        not.Kisi = objectSpace.GetObjectByKey<Kisi>(request.KisiOid.Value);
    }

    objectSpace.CommitChanges();

    return Task.FromResult(MapToDto(not));
}
```

- [ ] **Step 3: Add `NoteServiceTests.cs`**

Create `Project1/Project1.Module.Tests/Services/NoteServiceTests.cs` verifying that `CreateNoteAsync` assigns `Musteri` and `Kisi` properties properly.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test --filter FullyQualifiedName~NoteServiceTests`

- [ ] **Step 5: Commit**

```bash
git add Project1/Project1.DTOs/Notes/NoteDto.cs Project1/Project1.Module/Services/Implementations/NoteService.cs Project1/Project1.Module.Tests/Services/NoteServiceTests.cs
git commit -m "feat(notes): link customer and contact navigation properties in NoteService.CreateNoteAsync"
```

---

### Task 4: `SystemStatusControllers.cs` Sorumluluk ve Dosya Ayrımı

**Files:**
- Create: `Project1/Project1.Blazor.Server/Controllers/SystemStatusApiController.cs`
- Create: `Project1/Project1.Blazor.Server/Controllers/SystemStatusWindowController.cs`
- Delete: `Project1/Project1.Blazor.Server/Controllers/SystemStatusControllers.cs`
- Modify: `Project1/Project1.Module.Tests/Api/ApiEndpointAttributeTests.cs`

**Interfaces:**
- Produces: `SystemStatusApiController` (REST Controller), `SystemStatusWindowController` (XAF WindowController)

- [ ] **Step 1: Create `SystemStatusApiController.cs`**

In `Project1/Project1.Blazor.Server/Controllers/SystemStatusApiController.cs`:
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Project1.Core.Services.Interfaces;

namespace Project1.Blazor.Server.Controllers
{
    /// <summary>
    /// Project2'nin sorguladığı REST API uç noktasını yönetir.
    /// </summary>
    [ApiController]
    [Route("api/systemstatus")]
    [AllowAnonymous]
    [EnableCors("AllowAll")]
    public class SystemStatusApiController : ControllerBase
    {
        private readonly ISystemStatusService _statusService;

        public SystemStatusApiController(ISystemStatusService statusService)
        {
            _statusService = statusService;
        }

        [HttpGet]
        public IActionResult GetStatus()
        {
            return Ok(new 
            { 
                isActive = _statusService.IsActive, 
                status = _statusService.IsActive ? "ACTIVE" : "PASSIVE" 
            });
        }

        [HttpPost("toggle")]
        public IActionResult ToggleStatus()
        {
            bool newState = _statusService.Toggle();
            return Ok(new 
            { 
                isActive = newState, 
                status = newState ? "ACTIVE" : "PASSIVE" 
            });
        }
    }
}
```

- [ ] **Step 2: Create `SystemStatusWindowController.cs`**

In `Project1/Project1.Blazor.Server/Controllers/SystemStatusWindowController.cs`:
```csharp
using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using Project1.Core.Services.Interfaces;

namespace Project1.Blazor.Server.Controllers
{
    /// <summary>
    /// Admin Paneli ana pencere üst çubuğunda API Durumunu (Aktif/Pasif) değiştiren butonu yönetir.
    /// </summary>
    public class SystemStatusWindowController : WindowController
    {
        private readonly SimpleAction _toggleStatusAction;
        private ISystemStatusService _statusService;

        public SystemStatusWindowController()
        {
            TargetWindowType = WindowType.Main;

            _toggleStatusAction = new SimpleAction(this, "ToggleSystemStatusAction", PredefinedCategory.Tools)
            {
                Caption = "API Durumu",
                ImageName = "State_ItemVisibility_Show",
                ToolTip = "Project2 API durumunu Aktif veya Pasif yap."
            };
            _toggleStatusAction.Execute += ToggleStatusAction_Execute;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            _statusService = Application.ServiceProvider?.GetService(typeof(ISystemStatusService)) as ISystemStatusService;
            bool isAdmin = string.Equals(Application?.Security?.UserName, "Admin", StringComparison.OrdinalIgnoreCase);
            _toggleStatusAction.Active["AdminOnly"] = isAdmin;
            UpdateCaption();
        }

        private void ToggleStatusAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (_statusService != null)
            {
                bool newState = _statusService.Toggle();
                UpdateCaption();

                string statusText = newState ? "AKTİF (ACTIVE)" : "PASİF (PASSIVE)";
                Application?.ShowViewStrategy?.ShowMessage(new MessageOptions
                {
                    Message = $"Project2 API Durumu Güncellendi: {statusText}",
                    Type = newState ? InformationType.Success : InformationType.Warning,
                    Duration = 4000
                });
            }
        }

        private void UpdateCaption()
        {
            if (_statusService != null)
            {
                _toggleStatusAction.Caption = _statusService.IsActive 
                    ? "🟢 API Durumu: AKTİF (Pasif Yap)" 
                    : "🔴 API Durumu: PASİF (Aktif Yap)";
            }
        }
    }
}
```

- [ ] **Step 3: Delete `SystemStatusControllers.cs` and update `ApiEndpointAttributeTests.cs`**

Delete `Project1/Project1.Blazor.Server/Controllers/SystemStatusControllers.cs`.
In `Project1/Project1.Module.Tests/Api/ApiEndpointAttributeTests.cs`:
Change `FindControllerFile("SystemStatusControllers.cs")` to `FindControllerFile("SystemStatusApiController.cs")`.

- [ ] **Step 4: Run all test suite to verify everything passes**

Run: `dotnet test InternshipProject.sln`

- [ ] **Step 5: Commit**

```bash
git add Project1/Project1.Blazor.Server/Controllers/SystemStatusApiController.cs Project1/Project1.Blazor.Server/Controllers/SystemStatusWindowController.cs Project1/Project1.Module.Tests/Api/ApiEndpointAttributeTests.cs
git rm Project1/Project1.Blazor.Server/Controllers/SystemStatusControllers.cs
git commit -m "refactor(controllers): separate SystemStatusApiController and SystemStatusWindowController into individual files"
```
