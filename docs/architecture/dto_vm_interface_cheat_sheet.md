# DTO / ViewModel / Interface İçin Hızlı Prompt & Cheat Sheet

İşte AI'a (bana veya başka bir AI'a) her seferinde uzun uzun açıklamadan direkt kod ürettirebileceğin hazır prompt'lar, hızlı referans tablosu, isimlendirme kuralları, XML dokümantasyon şablonları, hızlı snippet'ler ve kontrol listesi.

---

## 🎯 1. AI Prompt Template'leri (Kopyala-Yapıştır)

### 🟢 Prompt 1: DTO Üret
```text
Aşağıda verdiğim {EntityName} varlığı için C# 10 `record` yapısını kullanarak immutable Request ve Response DTO sınıflarını oluştur:
1. {Action}{EntityName}Request (Create/Update için)
2. {EntityName}Response (Detay gösterimi için)
3. {EntityName}ListItem (Liste gösterimi için sade DTO)

Dosya yerleşimi: Application/Dto/{Feature}/
Entity:
[Entity kopyala-yapıştır]
```

### 🟢 Prompt 2: ViewModel Üret
```text
Aşağıdaki {Feature} ekranı için C# ViewModel sınıfı ({PageName}ViewModel) üret:
1. Sadece UI projesinde kullanılacak (ViewModels/ klasörü).
2. DataAnnotations validation özniteliklerini ekle ([Required], [StringLength], vb.).
3. Ekran için gerekli UI state ve display formatlama özelliklerini içersin.

Ekran Gereksinimleri:
[Ekran alanları ve validation kurallarını yazın]
```

### 🟢 Prompt 3: Interface Üret
```text
Aşağıdaki {Feature} servisi için C# `I{Feature}Service` arayüzünü tanımla:
1. Metotlar asenkron olsun (Task, CancellationToken desteği).
2. Parametre ve geri dönüş tiplerinde Entity yerine DTO kullan.
3. Dosya yerleşimi: Application/Interfaces/ veya Domain/Interfaces/

Servis Metot Gereksinimleri:
[Örn: Customer oluşturma, Id ile detay getirme, listeleme]
```

### 🟢 Prompt 4: Full Feature (Hepsi Bir Arada)
```text
Aşağıdaki {EntityName} yapısı için aşağıdaki tüm katman bileşenlerini sırasıyla üret:
1. DTO'lar: {Action}{EntityName}Request, {EntityName}Response, {EntityName}ListItem (Application/Dto/{Feature}/)
2. Interface: I{EntityName}Service (Application/Interfaces/ veya Domain/Interfaces/)
3. Mapper: Static extension method tabanlı {EntityName}Mapper (Application/Mappers/)
4. ViewModel: {EntityName}FormViewModel (ViewModels/)

Entity / Gereksinim:
[Entity veya alan detaylarını yazın]
```

---

## 📊 2. Hızlı Referans Tablosu (Cheat Sheet)

### Dosya Yerleşim Tablosu

| Ne? | Nereye? | Örnek |
| :--- | :--- | :--- |
| **Interface** | `Domain/Interfaces/` veya `Application/Interfaces/` | `ICustomerService.cs` |
| **Entity** | `Domain/Entities/` | `Customer.cs` |
| **Request DTO** | `Application/Dto/{Feature}/` | `CreateCustomerRequest.cs` |
| **Response DTO** | `Application/Dto/{Feature}/` | `CustomerResponse.cs` |
| **List DTO** | `Application/Dto/{Feature}/` | `CustomerListItem.cs` |
| **Mapper** | `Application/Mappers/` | `CustomerMapper.cs` |
| **Service** | `Application/Services/` | `CustomerService.cs` |
| **ViewModel** | `ViewModels/` *(Sadece UI projesinde)* | `CustomerListViewModel.cs` |

---

### İsimlendirme Kuralları

| Tür | İsim Şablonu | Örnek |
| :--- | :--- | :--- |
| **Interface** | `I{Noun}{Verb/Service}` | `ICustomerService`, `IUserRepository` |
| **Entity** | `{Noun}` | `Customer`, `User`, `Order` |
| **Request DTO** | `{Action}{Noun}Request` | `CreateCustomerRequest` |
| **Response DTO** | `{Noun}Response` | `CustomerResponse` |
| **List DTO** | `{Noun}ListItem` | `CustomerListItem` |
| **Detail DTO** | `{Noun}Detail` | `CustomerDetail` |
| **ViewModel** | `{PageName}ViewModel` | `CustomerListViewModel` |
| **Mapper** | `{Noun}Mapper` | `CustomerMapper` |

---

### Akış Kuralı (Unidirectional Data Flow)

> 🛑 **Altın Kural:** Tek yönlü akış: `UI → ViewModel → DTO → Entity`. Asla tersi olmasın (Entity verisi asla doğrudan UI katmanına sızdırılmamalıdır).

```
1. UI ──> ViewModel alır (Form / Input State).
2. ViewModel ──> DTO'ya dönüştürür (Request DTO).
3. DTO ──> Service / Interface'e iletilir.
4. Service ──> Mapper kullanarak DTO'yu Entity'ye çevirir ve DB'ye yazar.
5. DB Response ──> Service, Entity'yi Response DTO'ya çevirir ve UI'a (ViewModel'e) döner.
```

---

## 💬 3. Kod İçine Eklenecek XML Comment Template'leri

### Interface İçin
```csharp
/// <summary>
/// {EntityName} nesneleri ile ilgili iş mantığı operasyonlarını yönetir.
/// </summary>
public interface I{EntityName}Service
{
    /// <summary>
    /// Yeni bir {EntityName} kaydı oluşturur.
    /// </summary>
    Task<Guid> CreateAsync({Action}{EntityName}Request request, CancellationToken cancellationToken = default);
}
```

### Request DTO İçin
```csharp
/// <summary>
/// Yeni bir {EntityName} oluşturma isteği için gereken veri transfer nesnesi.
/// </summary>
public record Create{EntityName}Request(string Name, string Description);
```

### Response DTO İçin
```csharp
/// <summary>
/// {EntityName} detaylarının istemciye sunumu için veri yanıt nesnesi.
/// </summary>
public record {EntityName}Response(Guid Id, string Name, DateTime CreatedAt);
```

### ViewModel İçin
```csharp
/// <summary>
/// {PageName} ekranındaki form alanlarını ve UI durumunu yöneten ViewModel sınıfı.
/// </summary>
public class {PageName}ViewModel
{
    // UI alanları ve validation öznitelikleri
}
```

---

## ⚡ 4. Hızlı Snippet'ler (VS Code / Rider / VS)

### DTO Template (Request & Response)
```csharp
namespace Application.Dto.{Feature};

/// <summary>
/// {Feature} oluşturma isteği DTO.
/// </summary>
public record Create{Feature}Request(
    string Title,
    string Description,
    Guid RelatedEntityId
);

/// <summary>
/// {Feature} detay yanıt DTO.
/// </summary>
public record {Feature}Response(
    Guid Id,
    string Title,
    string Description,
    DateTime CreatedAt
);
```

### Mapper Template (Static Extension Methods)
```csharp
namespace Application.Mappers;

using Application.Dto.{Feature};
using Domain.Entities;

public static class {Feature}Mapper
{
    public static {Feature}Response ToResponse(this {Feature} entity) =>
        new(
            entity.Id,
            entity.Title,
            entity.Description,
            entity.CreatedAt
        );

    public static {Feature} ToEntity(this Create{Feature}Request request) =>
        new()
        {
            Title = request.Title,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };
}
```

### ViewModel Template (With DataAnnotations)
```csharp
namespace ViewModels;

using System.ComponentModel.DataAnnotations;

public class {PageName}ViewModel
{
    [Required(ErrorMessage = "Bu alan zorunludur.")]
    [StringLength(100, ErrorMessage = "En fazla 100 karakter girebilirsiniz.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsBusy { get; set; }
}
```

---

## 🎯 5. Bir Bakışta Kontrol Listesi (Checklist)

Yeni bir özellik eklerken bu checklist'ten geçin:

- [ ] **`Domain/Interfaces/`** → Service + Repository interface'leri var mı?
- [ ] **`Application/Dto/{Feature}/`** → Request + Response + ListItem DTO'ları var mı?
- [ ] **`Application/Dto/` veya `ViewModels/`** → Validation attribute'ları eklendi mi?
- [ ] **`Application/Mappers/`** → Mapper extension'ları yazıldı mı?
- [ ] **`Application/Services/`** → Service implementasyonu var mı?
- [ ] **`ViewModels/`** → UI'a özel VM var mı?
- [ ] **`Program.cs` / `Startup.cs`** → DI kaydı yapıldı mı (`services.AddScoped<I..., ...>()`)?
- [ ] **UI İzolasyon Kontrolü:** UI → sadece ViewModel mi kullanıyor (Entity/DTO sızmıyor mu)?
