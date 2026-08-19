# Tüm Not Ekleme Ekranlarında Doğrudan Dosya Yükleme ve Project2 Paylaşımı Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Not oluştururken karmaşık alt gridler yerine tüm not ekleme noktalarında (Notlar ana menüsü, Müşteri ve Kişi ekranlarındaki "Not Ekle" popup'ları, alt liste "Ekle" butonu) doğrudan işletim sistemi dosya seçicisini açan tek bir dosya yükleme butonu (`Dosya`) ve `Project2 ile Paylaş` seçeneği sunmak; bu dosyaların Project2 üzerinde görüntülenip indirilmesini sağlamak.

**Architecture:** Project1 DevExpress XAF katmanında `Not` sınıfına `FileData Dosya` ve `bool Project2IlePaylas` entegrasyonu; `Not_DetailView` XAFML düzeni ve denetleyicilerin (`KisiNotePopupController`, `MusteriPopupController`, `NestedListPopupController`) doğrudan `Not` DetailView'ı açacak şekilde yapılandırılması; `Project1.DTOs` ve `Project2.DTOs` katmanında güncel `NoteDto` / `NoteAttachmentDto` modelleri; `Project1.Blazor.Server` katmanında `/api/notes?onlyShared=true` ve `/api/attachments/{noteId}/download` binary stream endpoint'leri; `Project2` Blazor Server arayüzünde modern ek görüntüleme.

**Tech Stack:** .NET 8, DevExpress XAF Blazor (XPO), MediatR, ASP.NET Core Web API, Blazor Server, xUnit, FluentAssertions.

## Global Constraints
- Not ekleme ekranlarında alt tablo / grid kullanılmayacaktır; doğrudan `FileData` kullanılacaktır.
- `Not` modelindeki `Dosya` alanı PDF ve Görsel dosya uzantılarını (`*.pdf;*.png;*.jpg;*.jpeg;*.gif;*.webp;*.bmp`) desteklemelidir.
- REST API endpoint: `/api/attachments/{noteId}/download` (`[AllowAnonymous]`, `[EnableCors("AllowAll")]`).
- Tüm testler `dotnet test` komutuyla hatasız geçmelidir.

---

### Task 1: XAF Model ve View Düzenlemeleri (`Model.DesignedDiffs.xafml`)

**Files:**
- Modify: `Project1/Project1.Module/Model.DesignedDiffs.xafml`
- Test: `Project1/Project1.Module.Tests/Domain/NotDomainTests.cs`

**Interfaces:**
- Consumes: `Not.Dosya`, `Not.Project2IlePaylas`
- Produces: `Not_DetailView` with layout containing `Dosya` and `Project2IlePaylas`

- [ ] **Step 1: Verify NotDomainTests passes**

Run: `dotnet test --filter "FullyQualifiedName~NotDomainTests"`
Expected: PASS

- [ ] **Step 2: Update Not_DetailView Layout in Model.DesignedDiffs.xafml**

Add `Dosya` and `Project2IlePaylas` layout items to `Not_DetailView`.

- [ ] **Step 3: Build Project1.Module**

Run: `dotnet build Project1/Project1.Module/Project1.Module.csproj`
Expected: Build succeeded with 0 errors.

- [ ] **Step 4: Commit Model changes**

```bash
git add Project1/Project1.Module/Model.DesignedDiffs.xafml
git commit -m "feat(xaf): add Dosya and Project2IlePaylas to Not_DetailView layout"
```

---

### Task 2: Denetleyicilerin (`KisiNotePopupController`, `MusteriPopupController`, `NestedListPopupController`) Doğrudan `Not_DetailView` Açacak Şekilde Güncellenmesi

**Files:**
- Modify: `Project1/Project1.Module/Controllers/Customers/KisiNotePopupController.cs`
- Modify: `Project1/Project1.Module/Controllers/Customers/MusteriPopupController.cs`
- Modify: `Project1/Project1.Module/Controllers/Customers/NestedListPopupController.cs`
- Test: `Project1/Project1.Module.Tests/Controllers/KisiNotePopupControllerTests.cs`

**Interfaces:**
- Consumes: `Application.CreateObjectSpace(typeof(Not))`, `Application.CreateDetailView(...)`
- Produces: Popup dialogs that open standard `Not_DetailView` with context properties prepopulated and hidden

- [ ] **Step 1: Update KisiNotePopupController to open Not_DetailView**

Configure `CustomizePopupWindowParams` to create `Not` object, set `Kisi`, `Musteri`, `IsKisiHidden = true`, `IsMusteriHidden = true`, and open `Not_DetailView`. In `Execute`, commit the object space.

- [ ] **Step 2: Update MusteriPopupController to open Not_DetailView for Not Ekle**

Configure `CustomizePopupWindowParams` to create `Not` object, set `Musteri`, `IsMusteriHidden = true`, and open `Not_DetailView`. In `Execute`, commit the object space.

- [ ] **Step 3: Update NestedListPopupController to open Not_DetailView for Not list**

When target object type is `Not`, create `Not` object space, set master object context (`Musteri` or `Kisi`), set hidden flags, and open `Not_DetailView`. In `Execute`, commit the object space.

- [ ] **Step 4: Build and Test Controllers**

Run: `dotnet test Project1/Project1.Module.Tests/Project1.Module.Tests.csproj`
Expected: All tests pass.

- [ ] **Step 5: Commit Controller updates**

```bash
git add Project1/Project1.Module/Controllers/Customers/
git commit -m "feat(controllers): configure all note popups to use Not_DetailView with file upload and sharing"
```

---

### Task 3: Solution Genelinde Derleme ve Tüm Testlerin Doğrulanması

**Files:**
- Test: All projects in solution

- [ ] **Step 1: Run full test suite**

Run: `dotnet test`
Expected: All tests PASS.

- [ ] **Step 2: Build complete solution**

Run: `dotnet build InternshipProject.sln`
Expected: Build succeeded with 0 errors.

- [ ] **Step 3: Commit all remaining changes**

```bash
git add .
git commit -m "chore: verify build and test suite for note attachments and Project2 sharing"
```
