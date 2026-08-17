# Not Otomatik Alanlar ve Oluşturma Tipi Tasarım Dokümanı

## 1. Genel Bakış ve Amaç
Bu doküman, `Not` iş nesnesi (`BusinessObjects/Notes/Not.cs`) üzerindeki alanların kullanıcı girişinden arındırılması, sistem tarafından otomatik yönetilmesi ve notun nereden/nasıl oluşturulduğunu (`NotOlusturmaTipi`) takip eden kuralların mimari tasarımını içerir.

---

## 2. Gereksinimler ve Kararlar

1. **Not Oluşturma Tipi (`NotOlusturmaTipi`):**
   - Notun hangi kanaldan oluşturulduğunu belirten bir `enum` tanımlanır (`Manuel`, `MusteriPopup`, `KisiPopup`, `ApiSistem`).
   - Kullanıcı bu alanı elle düzenleyemez (`[ReadOnly(true)]`).
   - Oluşturma kaynağına göre ilgili controller veya servis tarafından otomatik olarak atanır.

2. **Referans No (`ReferenceNo`):**
   - Format: `NOT-yyyyMMdd-XXXX` (Örn: `NOT-20260817-A1B2`).
   - `AfterConstruction()` veya `OnSaving()` aşamasında otomatik olarak üretilir.
   - Kullanıcı tarafından değiştirilemez (`[ReadOnly(true)]`). Liste ve detay ekranlarında bilgi amaçlı görünür.

3. **Nesne Tipi & Nesne ID (`ReferenceBaseObjectType` / `ReferenceBaseObjectId`):**
   - Detay ekranında kullanıcıya gösterilmez (`[VisibleInDetailView(false)]`).
   - `Musteri` veya `Kisi` atandığında ya da kaydedilirken arka planda otomatik senkronize edilir:
     - `Musteri` varsa: `ReferenceBaseObjectType = "Musteri"`, `ReferenceBaseObjectId = Musteri.Oid`
     - Sadece `Kisi` varsa: `ReferenceBaseObjectType = "Kisi"`, `ReferenceBaseObjectId = Kisi.Oid`

---

## 3. Mimari ve Bileşen Değişiklikleri

### 3.1. Enum Tanımı (`Project1.Module/BusinessObjects/Enums/NotOlusturmaTipi.cs`)
```csharp
namespace Project1.Module.BusinessObjects.Enums
{
    public enum NotOlusturmaTipi
    {
        Manuel = 0,
        MusteriPopup = 1,
        KisiPopup = 2,
        ApiSistem = 3
    }
}
```

### 3.2. `Not` İş Nesnesi (`Project1.Module/BusinessObjects/Notes/Not.cs`)
- `ReferenceNo`: `AfterConstruction`'da otomatik doldurulur. `[ReadOnly(true)]`, `[VisibleInListView(true)]`, `[VisibleInDetailView(true)]`.
- `OlusturmaTipi`: `NotOlusturmaTipi` tipinde property, varsayılan `Manuel`. `[ReadOnly(true)]`, `[VisibleInListView(true)]`, `[VisibleInDetailView(true)]`.
- `ReferenceBaseObjectType` & `ReferenceBaseObjectId`: `[VisibleInDetailView(false)]`, `[VisibleInListView(false)]`.
- `Musteri` ve `Kisi` property setter'larında ve `OnSaving()` metodunda `SyncReferenceBaseObject()` çağrılarak nesne tipi ve ID senkronizasyonu sağlanır.

```csharp
private void SyncReferenceBaseObject()
{
    if (Musteri != null)
    {
        ReferenceBaseObjectType = nameof(Customers.Musteri);
        ReferenceBaseObjectId = Musteri.Oid;
    }
    else if (Kisi != null)
    {
        ReferenceBaseObjectType = nameof(Customers.Kisi);
        ReferenceBaseObjectId = Kisi.Oid;
    }
    else
    {
        ReferenceBaseObjectType = null;
        ReferenceBaseObjectId = null;
    }
}
```

### 3.3. Command, Handler ve DTO Katmanı
- **`CreateNoteCommand` (`Project1.Core/Commands/CreateNoteCommand.cs`):**
  - `int OlusturmaTipi` parametresi eklenir (varsayılan `0` / `Manuel`).
- **`CreateNoteCommandHandler` (`Project1.Module/Handlers/CreateNoteCommandHandler.cs`):**
  - `yeniNot.OlusturmaTipi = (NotOlusturmaTipi)request.OlusturmaTipi;`
  - `SyncReferenceBaseObject` mekanizması işletilir.
- **`CreateNoteRequestDto` & `NoteDto` (`Project1.DTOs/Notes/NoteDto.cs`):**
  - `CreateNoteRequestDto`: `int OlusturmaTipi = 3` (`ApiSistem`).
  - `NoteDto`: `string OlusturmaTipi` ve `string ReferenceNo` eklenir.
- **`NoteService` (`Project1.Module/Services/Implementations/NoteService.cs`):**
  - `CreateNoteAsync` içinde `not.OlusturmaTipi = (NotOlusturmaTipi)request.OlusturmaTipi;` atanır.

### 3.4. Controller Güncellemeleri
- **`KisiNotePopupController`:** `CreateNoteCommand` çağrısına `olusturmaTipi: (int)NotOlusturmaTipi.KisiPopup` iletilir.
- **`MusteriPopupController`:** `CreateNoteCommand` çağrısına `olusturmaTipi: (int)NotOlusturmaTipi.MusteriPopup` iletilir.
- **`NestedListPopupController`:** Master nesneye göre `MusteriPopup` veya `KisiPopup` olarak atanır.

---

## 4. Test ve Doğrulama Planı

### 4.1. Birim Testleri (`Project1.Module.Tests`)
- `Not_AfterConstruction_ShouldGenerateReferenceNoWithExpectedFormat`: Formatın `NOT-yyyyMMdd-XXXX` olduğunu doğrulama.
- `Not_SyncReferenceBaseObject_ShouldSetMusteriInfo_WhenMusteriIsSet`: `Musteri` atandığında `ReferenceBaseObjectType` ve `ReferenceBaseObjectId` değerlerinin doğrulanması.
- `Not_SyncReferenceBaseObject_ShouldSetKisiInfo_WhenOnlyKisiIsSet`: Yalnızca `Kisi` atandığında `ReferenceBaseObjectType` ve `ReferenceBaseObjectId` değerlerinin doğrulanması.
- `CreateNoteCommandHandler_ShouldSetOlusturmaTipi`: Handler üzerinden oluşturulan notta tipin doğrulanması.
- `NoteServiceTests`: `CreateNoteAsync` ile `ApiSistem` tipinin doğru aktarıldığının doğrulanması.

### 4.2. Manuel Doğrulama
- UI üzerinde Notlar, Müşteri -> Not Ekle ve Kişi -> Not Ekle akışlarının test edilmesi.
- DetailView ekranında Referans No ve Oluşturma Tipinin salt-okunur (değiştirilemez) olduğunun, Referans Nesne Tipi ve ID alanlarının gizlendiğinin doğrulanması.
