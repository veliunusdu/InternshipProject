# Kişi Detay Not Popup'ı Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Kişi detayından eklenen notların kişi ve müşteriye otomatik bağlandığı, yalnızca not alanlarını gösteren bir popup sağlamak.

**Architecture:** `RelatedRecordPopupController`, `Kisi_Notlar_ListView` için açılan Not popup'ını oluşturur. Koleksiyonun ana kişisi ve bağlı müşterisi yeni `Not` nesnesine atanır; popup oluşturulduktan sonra ilişki editörleri gizlenir. Kaydetme mevcut popup kaydetme akışını ve e-posta bildirimini kullanır.

**Tech Stack:** .NET 8, DevExpress XAF Blazor 26.1, XPO, C#.

## Global Constraints

- Değişiklik yalnızca `Kisi_Notlar_ListView` içindeki Not Ekle akışını etkiler.
- Müşteri detayından açılan not popup'ı kişi seçimini göstermeye devam eder.
- Popup'ta yalnızca `Baslik`, `Icerik` ve `Derece` görünür.
- Notun `Kisi` ve `Musteri` alanları açık olan kişinin ilişkilerinden atanır.

---

### Task 1: Kişi-bağlamlı not popup'ını düzelt

**Files:**
- Modify: `Project1/Project1.Module/Controllers/RelatedRecordPopupController.cs`
- Test: `Project1/Project1.Blazor.Server/Project1.Blazor.Server.csproj` build

**Interfaces:**
- Consumes: `PropertyCollectionSource.MasterObject`, `Kisi.Musteri`, `Not.Kisi`, `Not.Musteri`.
- Produces: `KisiNotEkleAction_CustomizePopupWindowParams` içinde kişi ve müşteri atanmış, ilişki editörleri gizlenmiş `DetailView`.

- [ ] **Step 1: Mevcut kişi-not popup davranışını kontrol et**

`KisiNotEkleAction_CustomizePopupWindowParams` metodunun boş bir `Not` oluşturduğunu ve ilişki alanlarını ayarlamadığını doğrula.

- [ ] **Step 2: Kişi ve müşteriyi popup nesnesine ata**

```csharp
if (View.CollectionSource is PropertyCollectionSource collectionSource &&
    collectionSource.MasterObject is Kisi kisi)
{
    yeniNot.Kisi = objectSpace.GetObject(kisi);
    yeniNot.Musteri = objectSpace.GetObject(kisi.Musteri);
}
```

- [ ] **Step 3: İlişki editörlerini gizle**

```csharp
DetailView popup = CreateEditableDetailView(objectSpace, yeniNot);
HideEditor(popup, nameof(Not.Musteri));
HideEditor(popup, nameof(Not.Kisi));
e.View = popup;
```

`HideEditor` yardımcı metodu `IAppearanceVisibility.Visibility = ViewItemVisibility.Hide` atamalıdır.

- [ ] **Step 4: Derleme doğrulaması yap**

Run: `dotnet build Project1.Blazor.Server.csproj --no-restore`

Expected: Build succeeds with no errors.

### Task 2: Çalışma zamanı davranışını doğrula

**Files:**
- Test: Yerel Blazor uygulaması

**Interfaces:**
- Consumes: Task 1 popup davranışı.
- Produces: Kişi detayından kaydedilebilen bağlı not.

- [ ] **Step 1: Uygulamayı başlat**

Run: `dotnet run`

- [ ] **Step 2: Kişi detayından popup'ı aç**

Bir kişiyi aç, Notlar bölümündeki Not Ekle eylemini seç ve popup'ta yalnızca Başlık, İçerik ve Derece alanlarının bulunduğunu doğrula.

- [ ] **Step 3: Notu kaydet ve ilişkiyi doğrula**

Notu kaydet; notun kişi detayındaki Notlar listesine geldiğini ve ilişkili müşterinin notlarında da göründüğünü doğrula.
