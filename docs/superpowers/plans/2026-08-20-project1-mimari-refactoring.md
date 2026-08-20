# Project1 Mimari İyileştirme ve Refactoring Uygulama Planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `Project1` mimarisini Clean Architecture standartlarına kavuşturmak, katmanlar arası ters bağımlılıkları gidermek, entity'lerdeki denetim (audit) ve yaşam döngüsü kod tekrarlarını ortaklaştırmak, API iş mantığını servis katmanına ayırmak ve WinForms DI eksiklikleri ile test çalışma kilitlenmelerini çözmek. (Project2 DTO'ları bağımsız tutulacaktır).

**Architecture:** Domain enum'ları ve sözleşmeler `Project1.Core` katmanında toplanır; entity'lerdeki ortak save/audit operasyonları `AuditedBaseObject` temel sınıfına taşınır; Web API controller'ındaki iş kuralları `IMailTrackingService`'e devredilir; API ve XAF controller'ları ayrıştırılır.

**Tech Stack:** .NET 8, DevExpress XAF (XPO, Blazor Server, WinForms), MediatR, xUnit, FluentAssertions, Moq.

## Global Constraints

- `Project2` DTO'ları bağımsız istemci modelleri (Client DTOs) olarak korunacak, `Project1.DTOs`'a bağlanmayacaktır.
- Mevcut tüm xUnit testleri refactoring sonrası eksiksiz ve yeşil (`PASS`) çalışmalıdır.
- DevExpress XPO model ilişkileri (`Association`, `Aggregated`) ve XAF kuralları bozulmamalıdır.
- Windows ortamında test koştururken shadow copy aktif olmalıdır.

---

### Task 1: xUnit Test Koşucusu Kilitlenmesini Düzeltme (Shadow Copy)

**Files:**
- Modify: `Project1/Project1.Module.Tests/xunit.runner.json`

**Interfaces:**
- Consumes: xUnit runner config
- Produces: `shadowCopy: true` ayarı ile test DLL kilitlenmelerini engelleme

- [ ] **Step 1: `xunit.runner.json` dosyasını güncelle**

```json
{
  "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
  "shadowCopy": true,
  "parallelizeAssembly": false,
  "parallelizeTestCollections": false
}
```

- [ ] **Step 2: Testleri çalıştırarak dosya kilitlenme hatasının çözüldüğünü doğrula**

Run: `dotnet test Project1/Project1.Module.Tests/Project1.Module.Tests.csproj`
Expected: `FileLoadException (Erişim engellendi)` hatası vermeden testlerin derlenip çalışması.

- [ ] **Step 3: Commit**

```bash
git add Project1/Project1.Module.Tests/xunit.runner.json
git commit -m "fix(tests): enable shadowCopy in xunit runner to prevent assembly file locks"
```

---

### Task 2: Domain Enum'larını `Project1.Core` Katmanına Taşıma & Ters Bağımlılığı Giderme

**Files:**
- Create: `Project1/Project1.Core/Enums/NotDerecesi.cs`
- Create: `Project1/Project1.Core/Enums/MailDurumu.cs`
- Modify: `Project1/Project1.Core/Commands/CreateNoteCommand.cs`
- Delete / Redirect: `Project1/Project1.Module/Models/Enums/NotDerecesi.cs`, `MailDurumu.cs`
- Modify: `Project1/Project1.Module/Models/Notes/Not.cs`
- Modify: `Project1/Project1.Business/Services/Implementations/NoteService.cs`
- Modify: `Project1/Project1.Mapping/Notes/NoteMapper.cs`

**Interfaces:**
- Produces: `Project1.Core.Enums.NotDerecesi`, `Project1.Core.Enums.MailDurumu`
- Produces: `CreateNoteCommand(string Baslik, string Icerik, NotDerecesi Derece, ...)`

- [ ] **Step 1: `Project1.Core/Enums/NotDerecesi.cs` oluştur**

```csharp
namespace Project1.Core.Enums
{
    public enum NotDerecesi
    {
        Normal = 0,
        Onemli = 1,
        Acil = 2
    }
}
```

- [ ] **Step 2: `Project1.Core/Enums/MailDurumu.cs` oluştur**

```csharp
namespace Project1.Core.Enums
{
    public enum MailDurumu
    {
        Gonderilmedi = 0,
        Gonderildi = 1,
        Iletildi = 2,
        Okundu = 3,
        Basarisiz = 4
    }
}
```

- [ ] **Step 3: `CreateNoteCommand.cs` içindeki primitive `int Derece`'yi güçlü tipli `NotDerecesi Derece` yap**

```csharp
using System;
using MediatR;
using Project1.Core.Enums;

namespace Project1.Core.Commands
{
    public class CreateNoteCommand : IRequest<Guid>
    {
        public string Baslik { get; set; } = string.Empty;
        public string Icerik { get; set; } = string.Empty;
        public NotDerecesi Derece { get; set; } = NotDerecesi.Normal;
        public Guid? MusteriId { get; set; }
        public Guid? KisiId { get; set; }

        public CreateNoteCommand(string baslik, string icerik, NotDerecesi derece, Guid? musteriId = null, Guid? kisiId = null)
        {
            Baslik = baslik;
            Icerik = icerik;
            Derece = derece;
            MusteriId = musteriId;
            KisiId = kisiId;
        }
    }
}
```

- [ ] **Step 4: `Project1.Module/Models/Notes/Not.cs`, `NoteService.cs`, `NoteMapper.cs` ve diğer dosyalardaki namespace referanslarını güncelle**

- [ ] **Step 5: Projeyi derle ve testleri koştur**

Run: `dotnet test Project1/Project1.Module.Tests/Project1.Module.Tests.csproj`
Expected: Enum taşınması sonrası tüm projelerin başarıyla derlenmesi ve testlerin geçmesi.

- [ ] **Step 6: Commit**

```bash
git add Project1/Project1.Core/Enums Project1/Project1.Core/Commands Project1/Project1.Module Project1/Project1.Business Project1/Project1.Mapping
git commit -m "refactor(core): move domain enums to Project1.Core and use strongly-typed NotDerecesi in commands"
```

---

### Task 3: Varlık Yaşam Döngüsü ve Denetim İzi İçin `AuditedBaseObject` Ortak Temel Sınıfı

**Files:**
- Create: `Project1/Project1.Module/Models/Base/AuditedBaseObject.cs`
- Modify: `Project1/Project1.Module/Models/Notes/Not.cs`
- Modify: `Project1/Project1.Module/Models/Customers/Musteri.cs`
- Modify: `Project1/Project1.Module/Models/Customers/Kisi.cs`

**Interfaces:**
- Produces: `public abstract class AuditedBaseObject : BaseObject`
- Implements: `CreatedDate`, `OnSaving` (AuditLog generation), `OnDeleting` (Soft Delete AuditLog generation), `GetCurrentUserName()`

- [ ] **Step 1: `AuditedBaseObject.cs` oluştur**

```csharp
using System;
using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using Project1.Module.Models.Audit;

namespace Project1.Module.Models.Base
{
    [NonPersistent]
    public abstract class AuditedBaseObject : BaseObject
    {
        protected AuditedBaseObject(Session session) : base(session) { }

        [Browsable(false)]
        public abstract string EntityDisplayName { get; }

        [Browsable(false)]
        public abstract string RecordTitle { get; }

        private DateTime _createdDate;
        [XafDisplayName("Oluşturma Tarihi")]
        [ModelDefault("DisplayFormat", "{0:dd.MM.yyyy HH:mm}")]
        [ModelDefault("EditMask", "dd.MM.yyyy HH:mm")]
        [VisibleInListView(true)]
        [VisibleInDetailView(false)]
        [ReadOnly(true)]
        public DateTime CreatedDate
        {
            get => _createdDate;
            set => SetPropertyValue(nameof(CreatedDate), ref _createdDate, value);
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            CreatedDate = DateTime.Now;
        }

        protected override void OnSaving()
        {
            base.OnSaving();
            if (CreatedDate == default)
            {
                CreatedDate = DateTime.Now;
            }

            if (!Session.IsObjectsLoading && !IsDeleted)
            {
                string user = GetCurrentUserName();
                bool isNew = Session.IsNewObject(this);
                new AuditLog(Session)
                {
                    Tarih = DateTime.Now,
                    Kullanici = user,
                    IslemTuru = isNew ? "Oluşturuldu" : "Güncellendi",
                    VarlikTipi = EntityDisplayName,
                    VarlikId = Oid,
                    Aciklama = isNew 
                        ? $"'{RecordTitle}' başlıklı yeni {EntityDisplayName.ToLowerInvariant()} oluşturuldu."
                        : $"'{RecordTitle}' başlıklı {EntityDisplayName.ToLowerInvariant()} güncellendi."
                };
            }
        }

        protected override void OnDeleting()
        {
            base.OnDeleting();
            string user = GetCurrentUserName();
            new AuditLog(Session)
            {
                Tarih = DateTime.Now,
                Kullanici = user,
                IslemTuru = "Silindi (Soft Delete)",
                VarlikTipi = EntityDisplayName,
                VarlikId = Oid,
                Aciklama = $"'{RecordTitle}' başlıklı {EntityDisplayName.ToLowerInvariant()} silindi."
            };
        }

        protected static string GetCurrentUserName()
        {
            try
            {
                return SecuritySystem.CurrentUserName ?? "Sistem";
            }
            catch
            {
                return "Sistem";
            }
        }
    }
}
```

- [ ] **Step 2: `Not.cs`, `Musteri.cs` ve `Kisi.cs` sınıflarını `AuditedBaseObject`'ten türet ve mükerrer metotları sil**

- [ ] **Step 3: Testleri çalıştırarak AuditLog ve Domain testlerinin geçtiğini doğrula**

Run: `dotnet test Project1/Project1.Module.Tests/Project1.Module.Tests.csproj --filter "FullyQualifiedName~Domain"`
Expected: `AuditLogDomainTests`, `NotDomainTests`, `SoftDeleteDomainTests` vb. tüm testlerin PASS olması.

- [ ] **Step 4: Commit**

```bash
git add Project1/Project1.Module/Models
git commit -m "refactor(models): extract AuditedBaseObject to eliminate audit log duplication in domain entities"
```

---

### Task 4: `MailTrackingController` İş Mantığının `IMailTrackingService`'e Taşınması

**Files:**
- Create: `Project1/Project1.Core/Services/Interfaces/IMailTrackingService.cs`
- Create: `Project1/Project1.Business/Services/Implementations/MailTrackingService.cs`
- Modify: `Project1/Project1.Blazor.Server/Controllers/MailTrackingController.cs`
- Modify: `Project1/Project1.Blazor.Server/Startup.cs`
- Modify: `Project1/Project1.Module.Tests/Api/MailTrackingControllerTests.cs`

**Interfaces:**
- Produces: `IMailTrackingService.ProcessDeliveredAsync(Guid noteId)`
- Produces: `IMailTrackingService.ProcessReadAsync(Guid noteId)`

- [ ] **Step 1: `IMailTrackingService.cs` arayüzünü oluştur**

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Project1.Core.Services.Interfaces
{
    public interface IMailTrackingService
    {
        Task<bool> ProcessDeliveredAsync(Guid noteId, CancellationToken cancellationToken = default);
        Task<bool> ProcessReadAsync(Guid noteId, CancellationToken cancellationToken = default);
    }
}
```

- [ ] **Step 2: `MailTrackingService.cs` implementasyonunu yaz**

- [ ] **Step 3: `MailTrackingController.cs` sınıfını `IMailTrackingService` çağıracak şekilde sadeleştir**

- [ ] **Step 4: `Startup.cs` dosyasına `services.AddScoped<IMailTrackingService, MailTrackingService>();` kaydını ekle**

- [ ] **Step 5: `MailTrackingControllerTests.cs` testlerini güncelle ve çalıştır**

Run: `dotnet test Project1/Project1.Module.Tests/Project1.Module.Tests.csproj --filter "FullyQualifiedName~MailTrackingControllerTests"`
Expected: Tüm tracking testlerinin PASS olması.

- [ ] **Step 6: Commit**

```bash
git add Project1/Project1.Core Project1/Project1.Business Project1/Project1.Blazor.Server Project1/Project1.Module.Tests
git commit -m "refactor(mail-tracking): delegate tracking business logic to IMailTrackingService"
```

---

### Task 5: Web API & XAF Controller Klasör Organizasyonu ve Namespace Düzeltmeleri

**Files:**
- Move: `Project1.Blazor.Server/Controllers/*ApiController.cs` ➔ `Project1.Blazor.Server/Controllers/Api/`
- Move: `Project1.Blazor.Server/Controllers/MailTrackingController.cs` ➔ `Project1.Blazor.Server/Controllers/Api/`
- Move: `Project1.Blazor.Server/Controllers/SystemStatusWindowController.cs` ➔ `Project1.Blazor.Server/Controllers/Xaf/`
- Modify: Namespace uyuşmazlığı olan `Models/Security/ApplicationUser.cs` ve `Models/Security/UserEmailPermission.cs`

- [ ] **Step 1: `Project1.Blazor.Server/Controllers/Api` ve `Controllers/Xaf` klasörlerini oluşturup dosyaları taşı**
- [ ] **Step 2: Namespace ve import referanslarını düzenle**
- [ ] **Step 3: Projeyi derle ve testleri koştur**

Run: `dotnet test InternshipProject.sln`
Expected: Build ve Test başarıyla tamamlanmalı.

- [ ] **Step 4: Commit**

```bash
git add Project1/Project1.Blazor.Server/Controllers Project1/Project1.Module/Models
git commit -m "refactor(controllers): separate Web API controllers from XAF controllers and fix namespaces"
```

---

### Task 6: `Project1.Win` DI Servis Kayıtlarının Tamamlanması

**Files:**
- Modify: `Project1/Project1.Win/Startup.cs`

**Interfaces:**
- Consumes: `EmailSettings`, `NoteService`, `IMapper`
- Produces: WinForms uygulamasında eksik DI bağımlılıklarının tescil edilmesi

- [ ] **Step 1: `Project1.Win/Startup.cs` içine servis kayıtlarını ekle**

```csharp
builder.Services.AddScoped<IEmailService>(sp =>
{
    return new EmailService(new EmailSettings());
});
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Project1.Module.Project1Module).Assembly));
```

- [ ] **Step 2: WinForms projesini derle**

Run: `dotnet build Project1/Project1.Win/Project1.Win.csproj`
Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add Project1/Project1.Win/Startup.cs
git commit -m "feat(win): register required DI services in WinForms application builder"
```

---

### Task 7: Bütüncül Doğrulama ve Tam Test Koşusu

**Files:**
- Solution genelinde doğrulama

- [ ] **Step 1: Tüm çözümü temizle ve derle**

Run: `dotnet clean InternshipProject.sln && dotnet build InternshipProject.sln`
Expected: 0 Error, 0 Warning (DevExpress lisans uyarısı hariç).

- [ ] **Step 2: Tüm test süitini çalıştır**

Run: `dotnet test InternshipProject.sln`
Expected: 62/62 test PASS.

- [ ] **Step 3: Son durum commit'i**

```bash
git commit --allow-empty -m "chore: complete Project1 architectural refactoring successfully"
```
