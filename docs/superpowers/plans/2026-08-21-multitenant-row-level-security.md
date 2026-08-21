# Multi-Tenant Row-Level Security Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** DevExpress XAF (.NET 8) projesinde (Blazor Server & WinForms), Firma (Tenant) bazlı katı satır düzeyinde güvenlik (Row-Level Security) uygulayarak multitenant mimariyi kurmak.

**Architecture:** XPO Domain modellerinde `IFirmaAware` arayüzü ve `Firma` varlığı, `CurrentFirmaOidOperator` özel kriter operatörü, XAF Security Engine nesne seviyesi izinleri (`SecurityConfig`), `AuditedBaseObject` üzerinden otomatik firma bağlama ve `Updater.cs` seed verileri.

**Tech Stack:** .NET 8, DevExpress XAF (26.1.4), DevExpress XPO, xUnit, FluentAssertions.

---

### Task 1: Domain Modelleri ve `IFirmaAware` Arayüzünün Oluşturulması
**Files:**
- Create: `Project1/Project1.Module/BusinessObjects/IFirmaAware.cs`
- Create: `Project1/Project1.Module/Models/Tenants/Firma.cs`
- Modify: `Project1/Project1.Module/Models/Security/ApplicationUser.cs`
- Modify: `Project1/Project1.Module/Models/Customers/Musteri.cs`
- Modify: `Project1/Project1.Module/Models/Customers/Kisi.cs`
- Modify: `Project1/Project1.Module/Models/Notes/Not.cs`
- Modify: `Project1/Project1.Module/Models/Base/AuditedBaseObject.cs`

- [ ] **Step 1: `IFirmaAware.cs` arayüzünü oluştur**
- [ ] **Step 2: `Firma.cs` XPO sınıfını oluştur**
- [ ] **Step 3: `ApplicationUser.cs` sınıfına `Firma` ilişkisini ekle**
- [ ] **Step 4: `Musteri.cs`, `Kisi.cs`, `Not.cs` sınıflarına `IFirmaAware` ve `Firma` ilişkisini ekle**
- [ ] **Step 5: `AuditedBaseObject.cs` içinde `IFirmaAware` için otomatik `Firma` atamasını yapılandır**

---

### Task 2: Özel Güvenlik Kriter Fonksiyon Operatörü (`CurrentFirmaOidOperator`)
**Files:**
- Create: `Project1/Project1.Module/Security/CurrentFirmaOidOperator.cs`
- Modify: `Project1/Project1.Module/Module.cs` (Operatörün kaydı)

- [ ] **Step 1: `CurrentFirmaOidOperator.cs` sınıfını `ICustomFunctionOperatorBrowsable` ve `ICustomFunctionOperatorFormattable` ile oluştur**
- [ ] **Step 2: Modül başlangıcında operatörün `CriteriaOperator.RegisterCustomFunction` ile kaydedilmesini sağla**

---

### Task 3: Rol ve Güvenlik İzinlerinin Yapılandırılması (`SecurityConfig.cs`)
**Files:**
- Modify: `Project1/Project1.Module/Security/SecurityConfig.cs`

- [ ] **Step 1: `AdminRoleConfigurator` içine `Firma` için FullAccess ve navigasyon izni ekle**
- [ ] **Step 2: `StandardUserRoleConfigurator` ve `CustomerRoleConfigurator` içine `Musteri`, `Kisi`, `Not` için `[Firma.Oid] = CurrentFirmaOid()` nesne izinlerini tanımla**

---

### Task 4: Platform Entegrasyonu (Blazor & WinForms Startup)
**Files:**
- Modify: `Project1/Project1.Win/Startup.cs`

- [ ] **Step 1: WinForms `Startup.cs` içinde `UseIntegratedMode` ve `AddSecuredXpo` yapılandırmasını tamamla**

---

### Task 5: Veritabanı Güncelleyici (`Updater.cs`) ve Başlangıç Verileri
**Files:**
- Modify: `Project1/Project1.Module/DatabaseUpdate/Updater.cs`

- [ ] **Step 1: Örnek firmalar (`Acme Teknoloji A.Ş.`, `Beta Lojistik A.Ş.`) oluştur**
- [ ] **Step 2: Kullanıcıları (`user_acme`, `user_beta`) firmalarına bağla ve rolleri ata**
- [ ] **Step 3: Her iki firma için izole örnek Müşteri ve Not kayıtları üret**

---

### Task 6: Birim Testleri ve İzolasyon Doğrulaması
**Files:**
- Create: `Project1/Project1.Module.Tests/Security/FirmaRowLevelSecurityTests.cs`
- Modify: `Project1/Project1.Module.Tests/Security/CustomerRoleSecurityTests.cs`
- Modify: `Project1/Project1.Module.Tests/Security/AuthorizationTests.cs`

- [ ] **Step 1: `FirmaRowLevelSecurityTests.cs` testlerini yaz**
- [ ] **Step 2: Tüm testleri `dotnet test` ile çalıştırıp başarıyla geçtiğini doğrula**
