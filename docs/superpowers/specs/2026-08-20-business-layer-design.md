# Project1.Business Layer Architecture & Integration Design

## Amaç
Bu tasarım, `Project1` mimarisine iş mantığını, doğrulama kurallarını ve servis uygulamalarını barındıran bağımsız bir **`Project1.Business`** (.NET 8 Class Library) katmanının eklenmesini ve mevcut servislerin (`NoteService`, `EmailService`, `CrmNotificationService`, `SystemStatusService`) bu yeni katmana taşınmasını tanımlar.

## Mimari Gerekçe
- **Single Responsibility:** `Project1.Module` projesi yalnızca XAF Business Objects (XPO Entity modelleri: `Musteri`, `Kisi`, `Not`), XAF View Controller'ları ve XAF konfigürasyonlarını barındıracaktır.
- **İş Mantığı İzolasyonu:** İş kuralları ve servis implementasyonları (`Project1.Business`) bağımsız bir kütüphane olarak geliştirilip test edilebilir olacaktır.
- **DataAccess Ayrımı:** XAF ortamında DevExpress `IObjectSpace` mekanizması doğrudan DataAccess/UnitOfWork rolünü üstlendiği için ayrıca bir `Project1.DataAccess` projesine ihtiyaç duyulmaz. `Project1.Business` doğrudan `IObjectSpace` ve `IMapper` sözleşmelerini kullanır.

## Proje ve Bağımlılık Yapısı

```text
Project1.Core (Arayüzler: INoteService, IEmailService, IMapper)
    ▲
    │
Project1.DTOs (Veri Transfer Modelleri)
    ▲
    │
Project1.Mapping (IMapper Implementasyonları) ───┐
    ▲                                           │
    │                                           ▼
Project1.Module (XPO Modelleri & IObjectSpace) ──┼───► Project1.Business (Servisler / İş Mantığı)
                                                │            ▲
                                                │            │
Project1.Blazor.Server (API & Web UI Host) ─────┴────────────┘
```

### Proje Referansları
`Project1.Business.csproj`:
- `Project1.Core`
- `Project1.DTOs`
- `Project1.Mapping`
- `Project1.Module`
- Package: `DevExpress.ExpressApp` (26.1.4)
- Package: `Microsoft.Extensions.Logging.Abstractions` (8.0.0)

## Taşınacak Bileşenler
`Project1.Module/Services/Implementations/` altındaki şu sınıflar `Project1.Business/Services/` altına taşınacaktır:
1. `NoteService.cs` (ve bağımlı mapping/iş kuralları)
2. `EmailService.cs` (ve `EmailSettings`)
3. `CrmNotificationService.cs`
4. `SystemStatusService.cs`

## Dependency Injection (Startup.cs)
`Project1.Blazor.Server/Startup.cs` içindeki servis kayıtları `Project1.Business` namespace'leri ile güncellenecektir:
```csharp
services.AddScoped<INoteService, NoteService>();
services.AddScoped<IEmailService, EmailService>();
services.AddScoped<ICrmNotificationService, CrmNotificationService>();
services.AddScoped<ISystemStatusService, SystemStatusService>();
```

## Başarı Kriterleri
1. `Project1.Business` projesi derlenir ve çözüm dosyalarına (`InternshipProject.sln`, `Project1.sln`) eklenir.
2. Tüm iş servisleri `Project1.Business` projesinde yer alır.
3. `Project1.Module` içinde servis implementasyonları kaldırılır ve sadece entity/controller odaklı kalır.
4. Tüm testler (`dotnet test InternshipProject.sln`) ve tüm çözüm build'i (`dotnet build InternshipProject.sln`) sıfır hata ile başarıyla geçer.
