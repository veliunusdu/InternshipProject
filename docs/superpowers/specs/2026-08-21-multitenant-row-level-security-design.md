# Multi-Tenant Row-Level Security (Firma Bazlı Satır Seviyesi İzolasyon) Tasarımı

## 1. Amaç ve Kapsam
DevExpress XAF (.NET 8) mimarisinde (Blazor Server & WinForms), veritabanı sorgusu seviyesinde (Data Layer) ve XAF Security Engine seviyesinde **Firma (Tenant)** bazlı katı satır düzeyinde güvenlik (Row-Level Security) sağlamak.

### Temel Kurallar
1. **Firma Kullanıcısı (Tenant User):** Yalnızca kendi firmasına (`ApplicationUser.Firma`) ait `Musteri`, `Kisi` ve `Not` kayıtlarını görebilmeli, ekleyebilmeli ve güncelleyebilmelidir.
2. **Başka Firmalara Erişim Yasağı:** Başka firmalara ait verilere erişim hem UI hem de veritabanı sorgusu / ORM seviyesinde (Data Layer) engellenmelidir.
3. **Admin (Süper Yönetici):** Tüm firmaların verilerini kısıtlamasız görebilmeli ve yönetebilmelidir.

---

## 2. 5 Adımlı Mimari Yol Haritası

### ADIM 1: Arayüz ve Domain Modellerini Güncelleme
* **`IFirmaAware.cs`**: `Project1.Module.BusinessObjects` altında `Firma Firma { get; set; }`.
* **`Firma.cs`**: `BaseObject` türevi, `Ad`, `VergiNo`, `Adres`, `Telefon` ve `Kullanicilar`, `Musteriler`, `Kisiler`, `Notlar` koleksiyonları (`[Association]`).
* **`ApplicationUser.cs`**: `[Association("Firma-Kullanicilar"), ExplicitLoading] public Firma Firma { get; set; }`.
* **`Musteri.cs`**: `IFirmaAware` implementasyonu, `[Association("Firma-Musteriler"), ExplicitLoading] public Firma Firma { get; set; }`.
* **`Kisi.cs`**: `IFirmaAware` implementasyonu, `[Association("Firma-Kisiler"), ExplicitLoading] public Firma Firma { get; set; }`.
* **`Not.cs`**: `IFirmaAware` implementasyonu, `[Association("Firma-Notlar"), ExplicitLoading] public Firma Firma { get; set; }`.
* **`AuditedBaseObject.cs`**: `AfterConstruction()` ve `OnSaving()` anında `IFirmaAware` nesnesi için aktif kullanıcının `Firma` değerinin otomatik atanması ve çapraz firma atamasının engellenmesi.

---

### ADIM 2: Özel Güvenlik Kriter Fonksiyon Operatörü (`CurrentFirmaOidOperator`)
* Criteria motoruna `CurrentFirmaOid()` özel fonksiyonunu kazandırır.
* `Evaluate()` metodunda `SecuritySystem.CurrentUser as ApplicationUser` üzerinden `Firma?.Oid` değerini döndürür.
* `Format()` metodunda SQL sorgusuna doğrudan Oid enjeksiyonunu formatlar.

---

### ADIM 3: Rol ve Güvenlik İzinleri (`SecurityConfig.cs`)
* **Admin Rolü (`Administrators`):**
  * `Firma`, `Musteri`, `Kisi`, `Not`, `ApplicationUser` üzerinde `FullAccess`.
* **Firma / Standart Kullanıcı Rolü (`Standard User`):**
  * `PermissionPolicy = DenyAllByDefault`.
  * `Firma`: Kendi firmasını okuma izni (`[Oid] = CurrentFirmaOid()`).
  * `Musteri`: `[Firma.Oid] = CurrentFirmaOid()` kriteri ile `Read`, `Write`, `Create`, `Delete`, `Navigate`.
  * `Kisi`: `[Firma.Oid] = CurrentFirmaOid()` kriteri ile `Read`, `Write`, `Create`, `Delete`, `Navigate`.
  * `Not`: `[Firma.Oid] = CurrentFirmaOid()` kriteri ile `Read`, `Write`, `Create`, `Delete`, `Navigate`.

---

### ADIM 4: Veritabanı Güncelleyici (`Updater.cs`) ve Başlangıç Verileri
* İki örnek firma oluşturulması: `Acme Teknoloji A.Ş.` ve `Beta Lojistik A.Ş.`.
* Kullanıcıların oluşturulması ve firmalara bağlanması:
  * `admin` -> Firma bağımsız (Tüm firmaları görür).
  * `user_acme` -> `Acme Teknoloji A.Ş.`
  * `user_beta` -> `Beta Lojistik A.Ş.`
* Her firma için izole `Musteri`, `Kisi` ve `Not` kayıtları.

---

### ADIM 5: Doğrulama ve Testler (`Project1.Module.Tests`)
* `FirmaRowLevelSecurityTests.cs`:
  * A firması kullanıcısının B firması müşteri/not kayıtlarını listeleyemediği ve erişemediği testi.
  * Yeni oluşturulan kayıtlara otomatik aktif firma atanmasının testi.
  * Admin kullanıcısının her iki firmanın kayıtlarını görebildiğinin testi.
