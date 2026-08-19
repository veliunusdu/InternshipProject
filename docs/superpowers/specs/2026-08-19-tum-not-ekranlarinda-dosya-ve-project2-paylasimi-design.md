# Tüm Not Ekleme Ekranlarında Doğrudan Dosya Eki (PDF / Görsel) ve Project2 Paylaşımı Tasarım Dokümanı

## 1. Genel Bakış ve Amaç
Kullanıcıların not oluştururken karmaşık alt gridler (Ekler tablosu) yerine, tek tıkla işletim sisteminin dosya seçici penceresini açan bir dosya yükleme butonu üzerinden PDF ve görsel yükleyebilmesi ve bu notları Project2 ile paylaşabilmesi hedeflenmektedir.

Bu yetenek sistemdeki **tüm not ekleme noktalarında** geçerli olacaktır:
1. **Notlar Ana Menüsü (`Not_ListView` / `Not_DetailView`)**
2. **Kişi Detayı ve Kişi Listesinden Not Ekleme (`KisiNotePopupController` & `NestedListPopupController`)**
3. **Müşteri Detayı ve Müşteri Listesinden Not Ekleme (`MusteriPopupController` & `NestedListPopupController`)**

---

## 2. Mimari ve Bileşen Tasarımı

### 2.1. Project1 XAF Domain & Model Katmanı (`Project1.Module`)

#### `Not.cs` Varlık Modeli
- **`Dosya` Alanı:**
  - Tür: `DevExpress.Persistent.BaseImpl.FileData`
  - Nitelikler: `[Aggregated]`, `[FileTypeFilter("PDF ve Görseller", "*.pdf;*.png;*.jpg;*.jpeg;*.gif;*.webp;*.bmp")]`, `[XafDisplayName("Dosya Eki (PDF / Görsel)")]`
- **`Project2IlePaylas` Alanı:**
  - Tür: `bool`
  - Nitelik: `[XafDisplayName("Project2 ile Paylaş")]`
- **Hesaplanan Özellikler:**
  - `string DosyaAdi => Dosya?.FileName ?? string.Empty;`
  - `long BoyutBytes => Dosya?.Size ?? 0;`
  - `string ContentType => GetContentType(Dosya?.FileName);`
  - `bool IsImage => ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);`
  - `bool IsPdf => ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase);`
  - `static string GetContentType(string? fileName)`

#### `Model.DesignedDiffs.xafml` Görünüm Yapılandırması
`Not_DetailView` layout grubuna `Dosya` ve `Project2IlePaylas` alanları açıkça eklenir:
```xml
<DetailView Id="Not_DetailView">
  <Items>
    <PropertyEditor Id="CreatedDate" Removed="True" />
    <PropertyEditor Id="MailDurumu" Removed="True" />
    <PropertyEditor Id="MailGonderilmeTarihi" Removed="True" />
    <PropertyEditor Id="MailIletilmeTarihi" Removed="True" />
    <PropertyEditor Id="MailOkunmaTarihi" Removed="True" />
    <PropertyEditor Id="MailHataMesaji" Removed="True" />
  </Items>
  <Layout>
    <LayoutGroup Id="Main" RelativeSize="100" Direction="Vertical">
      <LayoutGroup Id="SimpleEditors" RelativeSize="100" Direction="Vertical">
        <LayoutGroup Id="Not" RelativeSize="100" Direction="Vertical">
          <LayoutItem Id="Musteri" Index="0" />
          <LayoutItem Id="Kisi" Index="1" />
          <LayoutItem Id="Baslik" Index="2" />
          <LayoutItem Id="Icerik" Index="3" />
          <LayoutItem Id="Derece" Index="4" />
          <LayoutItem Id="Dosya" Index="5" />
          <LayoutItem Id="Project2IlePaylas" Index="6" />
        </LayoutGroup>
      </LayoutGroup>
    </LayoutGroup>
  </Layout>
</DetailView>
```

#### Denetleyiciler (Controllers)
1. **`KisiNotePopupController` (Kişi ekranından "Not Ekle" aksiyonu):**
   - Kişi seçildiğinde doğrudan `Not` nesnesi oluşturulur (`Application.CreateObjectSpace(typeof(Not))`).
   - `yeniNot.Kisi = seciliKisi`, `yeniNot.Musteri = seciliKisi.Musteri`, `yeniNot.IsMusteriHidden = true`, `yeniNot.IsKisiHidden = true` atanır.
   - Açılan popup'ta Başlık, İçerik, Derece, Dosya Yükleme Butonu ve Project2 ile Paylaş onay kutusu yer alır.
2. **`MusteriPopupController` (Müşteri ekranından "Not Ekle" aksiyonu):**
   - Müşteri seçildiğinde doğrudan `Not` nesnesi oluşturulur.
   - `yeniNot.Musteri = seciliMusteri`, `yeniNot.IsMusteriHidden = true` atanır.
   - Açılan popup'ta Kişi seçimi, Başlık, İçerik, Derece, Dosya Yükleme Butonu ve Project2 ile Paylaş yer alır.
3. **`NestedListPopupController` (Müşteri veya Kişi detayındaki "Notlar" alt sekmesinden "Ekle" aksiyonu):**
   - Üst nesneye göre (Müşteri veya Kişi) bağlam atanarak `Not` nesnesi DetailView olarak açılır.

---

### 2.2. DTO & REST API Servis Katmanı (`Project1.DTOs` & `Project1.Blazor.Server`)

#### `NoteDto` ve `NoteAttachmentDto`
```csharp
public record NoteDto(
    Guid Oid,
    string Baslik,
    string Icerik,
    string Derece,
    string Musteri,
    string Kisi,
    bool IsEmailSent,
    DateTime CreatedDate = default,
    string MailDurumu = "Gonderilmedi",
    DateTime? MailGonderilmeTarihi = null,
    DateTime? MailIletilmeTarihi = null,
    DateTime? MailOkunmaTarihi = null,
    bool IsSharedWithProject2 = false,
    NoteAttachmentDto? Ek = null
);

public record NoteAttachmentDto(
    Guid Oid,
    string DosyaAdi,
    string ContentType,
    long BoyutBytes,
    DateTime YuklemeTarihi,
    string DownloadUrl,
    bool IsImage,
    bool IsPdf
);
```

#### `INoteService` & `NoteService`
- `GetNotesAsync(bool? onlyShared = null)`: `onlyShared == true` olduğunda yalnızca `Project2IlePaylas == true` olan notları ve tekil `Ek` bilgilerini döner.
- `GetAttachmentFileAsync(Guid noteId)`: Nota ait dosya baytlarını, dosya adını ve MIME türünü döner.

#### REST API Endpoint'leri
- `GET /api/notes?onlyShared=true`: Project2 için paylaşılan notları döner.
- `GET /api/attachments/{noteId}/download`: Belirtilen nota ait dosyayı doğrudan binary stream olarak indirir (`[AllowAnonymous]`, `[EnableCors("AllowAll")]`).

---

### 2.3. Project2 Blazor Web Arayüzü (`Project2/Pages/Notes.razor`)
- Tablo sütunu: "Ek (PDF / Görsel)"
- Eğer `note.Ek != null`:
  - **Görsel ise (`IsImage == true`):** Önizleme butonu (`🖼️`), tıklandığında büyütülen görsel modalı (`Image Preview Modal`).
  - **PDF ise (`IsPdf == true`):** Yeni sekmede açma / indirme butonu (`📄`).
  - **Diğer ise:** Dosyayı indir butonu (`📎`).
- Eğer ek yoksa: `Ek yok` metni.

---

## 3. Doğrulama ve Test Planı

### 3.1. Otomatik Birim Testleri (`Project1.Module.Tests`)
1. **`NotDomainTests`:**
   - `Not.Dosya` ataması yapıldığında `DosyaAdi`, `BoyutBytes`, `ContentType`, `IsImage`, `IsPdf` değerlerinin doğru hesaplandığının doğrulanması.
2. **`DtoValidationTests`:**
   - `NoteDto` ve `NoteAttachmentDto` veri bütünlüğünün test edilmesi.
3. **`NoteServiceTests`:**
   - `GetNotesAsync(onlyShared: true)` filtresinin çalıştığı ve `Ek.DownloadUrl` değerinin `$"/api/attachments/{note.Oid}/download"` olarak üretildiğinin test edilmesi.
   - `GetAttachmentFileAsync` ile dosya baytlarının başarıyla okunduğunun test edilmesi.
4. **`AttachmentsApiControllerTests`:**
   - `/api/attachments/{noteId}/download` endpoint'inin 200 OK ile doğru dosya içeriğini ve MIME türünü döndüğünün test edilmesi.

### 3.2. Manuel Uçtan Uca Doğrulama
1. **Notlar Menüsü:** Yeni not oluşturup PDF veya resim seçilir, `Project2 ile Paylaş` işaretlenip kaydedilir.
2. **Kişi Detayı:** Kişi sayfasından "Not Ekle" popup'ı açılır, dosya seçilip kaydedilir.
3. **Müşteri Detayı:** Müşteri sayfasından "Not Ekle" popup'ı açılır, kişi seçilir, dosya seçilip kaydedilir.
4. **Project2:** `https://localhost:5001/notes` adresine gidilir, paylaşılan tüm notların ve eklerinin (PDF açma & resim modalı) eksiksiz çalıştığı doğrulanır.
