# Tüm Not Ekleme Ekranlarında Doğrudan Dosya Eki (PDF / Görsel) ve Project2 Entegrasyonu Uygulama Planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Not ekranlarındaki eski/karmaşık yapıları temizleyip tüm not ekleme noktalarına (Notlar ana menüsü, Kişi detayı popup'ı ve Müşteri detayı popup'ı) tek tıkla PDF/görsel yükleme butonu ve "Project2 ile Paylaş" seçeneği entegre etmek; paylaşılan notları ve eklerini Project2'de zengin önizleme (PDF açma & görsel modalı) ile sunmak.

**Architecture:** Project1 DevExpress XAF katmanında `Not` sınıfına `FileData Dosya` ve `bool Project2IlePaylas` entegrasyonu; `Not_DetailView` XAFML düzeni ve denetleyicilerin (`KisiNotePopupController`, `MusteriPopupController`, `NestedListPopupController`) doğrudan `Not` DetailView'ı açacak şekilde yapılandırılması; `Project1.DTOs` ve `Project2.DTOs` katmanında güncel `NoteDto` / `NoteAttachmentDto` modelleri; `Project1.Blazor.Server` katmanında `/api/notes?onlyShared=true` ve `/api/attachments/{noteId}/download` binary stream endpoint'leri; `Project2` Blazor Server arayüzünde modern ek görüntüleme.

**Tech Stack:** C# .NET 8, DevExpress XAF / XPO (`FileData`, `FileTypeFilter`), ASP.NET Core Web API, Blazor Server, xUnit, FluentAssertions, Moq.

## Global Constraints
- Target Framework: .NET 8 (`net8.0`)
- DevExpress XPO `FileData` dosya yönetimi
- İzin verilen dosya uzantıları: `*.pdf;*.png;*.jpg;*.jpeg;*.gif;*.webp;*.bmp`
- REST API endpoint: `/api/attachments/{noteId}/download` (`[AllowAnonymous]`, `[EnableCors("AllowAll")]`)
- TDD yaklaşımı ile her görevde birim testleri yazılarak doğrulanmalıdır.

---

### Task 1: DTO Modelleri ve Doğrulama Testleri

**Files:**
- Modify: `Project1/Project1.DTOs/Notes/NoteDto.cs`
- Modify: `Project2/DTOs/NoteDto.cs`
- Modify: `Project1/Project1.Module.Tests/DTOs/DtoValidationTests.cs`

**Interfaces:**
- Consumes: `NoteAttachmentDto`
- Produces: `NoteDto` (tekil `NoteAttachmentDto? Ek = null`, `bool IsSharedWithProject2 = false`)

- [ ] **Step 1: Write/Update unit test in `DtoValidationTests.cs`**

```csharp
[Fact]
public void NoteDto_ShouldHoldSingleAttachmentDto()
{
    var ek = new NoteAttachmentDto(
        Guid.NewGuid(),
        "belge.pdf",
        "application/pdf",
        1024,
        DateTime.Now,
        "/api/attachments/123/download",
        false,
        true
    );

    var noteDto = new NoteDto(
        Guid.NewGuid(),
        "Başlık",
        "İçerik",
        "Normal",
        "Müşteri A",
        "Kişi B",
        false,
        DateTime.Now,
        "Gonderilmedi",
        null,
        null,
        null,
        true,
        ek
    );

    noteDto.Ek.Should().NotBeNull();
    noteDto.Ek!.DosyaAdi.Should().Be("belge.pdf");
    noteDto.Ek.IsPdf.Should().BeTrue();
    noteDto.IsSharedWithProject2.Should().BeTrue();
}
```

- [ ] **Step 2: Run test to verify it passes**

Run: `dotnet test --filter "FullyQualifiedName~DtoValidationTests"`
Expected: PASS

- [ ] **Step 3: Commit DTO changes**

```bash
git add Project1/Project1.DTOs/Notes/NoteDto.cs Project2/DTOs/NoteDto.cs Project1/Project1.Module.Tests/DTOs/DtoValidationTests.cs
git commit -m "feat(dto): ensure NoteDto holds single NoteAttachmentDto and Project2 sharing flag"
```

---

### Task 2: Domain Modeli `Not.cs` ve Domain Birim Testleri

**Files:**
- Modify: `Project1/Project1.Module/Models/Notes/Not.cs`
- Modify: `Project1/Project1.Module.Tests/Domain/NotDomainTests.cs`

**Interfaces:**
- Consumes: `DevExpress.Persistent.BaseImpl.FileData`, `FileTypeFilterAttribute`
- Produces: `Not` varlığı üzerinde `FileData Dosya`, `bool Project2IlePaylas`, `DosyaAdi`, `BoyutBytes`, `ContentType`, `IsImage`, `IsPdf`

- [ ] **Step 1: Write/Update Domain tests in `NotDomainTests.cs`**

```csharp
[Fact]
public void Not_ShouldComputeContentTypeAndFlags_ForPdfAndImages()
{
    Not.GetContentType("dokuman.pdf").Should().Be("application/pdf");
    Not.GetContentType("resim.png").Should().Be("image/png");
    Not.GetContentType("foto.jpg").Should().Be("image/jpeg");
    Not.GetContentType("foto.jpeg").Should().Be("image/jpeg");
    Not.GetContentType("animasyon.gif").Should().Be("image/gif");
    Not.GetContentType("vektor.svg").Should().Be("image/svg+xml");
    Not.GetContentType("diger.xyz").Should().Be("application/octet-stream");
}

[Fact]
public void Not_ShouldSetDosyaProperty_AndExtractProperties()
{
    using var session = CreateInMemorySession();
    var not = new Not(session)
    {
        Baslik = "Dosyalı Not",
        Icerik = "İçerik",
        Project2IlePaylas = true
    };

    var fileData = new FileData(session);
    fileData.LoadFromStream("sample.pdf", new System.IO.MemoryStream(new byte[] { 1, 2, 3 }));
    not.Dosya = fileData;
    not.Save();

    not.DosyaAdi.Should().Be("sample.pdf");
    not.BoyutBytes.Should().Be(3);
    not.IsPdf.Should().BeTrue();
    not.IsImage.Should().BeFalse();
    not.Project2IlePaylas.Should().BeTrue();
}
```

- [ ] **Step 2: Run test to verify it passes**

Run: `dotnet test --filter "FullyQualifiedName~NotDomainTests"`
Expected: PASS

- [ ] **Step 3: Commit Domain Model**

```bash
git add Project1/Project1.Module/Models/Notes/Not.cs Project1/Project1.Module.Tests/Domain/NotDomainTests.cs
git commit -m "feat(domain): verify FileData property and helper flags on Not model"
```

---

### Task 3: `Model.DesignedDiffs.xafml` Not_DetailView Layout Düzenlemesi

**Files:**
- Modify: `Project1/Project1.Module/Model.DesignedDiffs.xafml`

**Interfaces:**
- Consumes: `Not.Dosya`, `Not.Project2IlePaylas`
- Produces: `Not_DetailView` içinde `Dosya` (Index 5) ve `Project2IlePaylas` (Index 6) alanlarının layout öğeleri

- [ ] **Step 1: Update `Model.DesignedDiffs.xafml`**

In `Project1/Project1.Module/Model.DesignedDiffs.xafml`:
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

- [ ] **Step 2: Build project to verify xafml schema validity**

Run: `dotnet build Project1/Project1.Module/Project1.Module.csproj`
Expected: Build succeeded with 0 errors.

- [ ] **Step 3: Commit xafml layout**

```bash
git add Project1/Project1.Module/Model.DesignedDiffs.xafml
git commit -m "feat(xafml): add Dosya and Project2IlePaylas layout items to Not_DetailView"
```

---

### Task 4: Tüm Not Ekleme Noktaları İçin Denetleyicilerin Güncellenmesi

**Files:**
- Modify: `Project1/Project1.Module/Controllers/Customers/KisiNotePopupController.cs`
- Modify: `Project1/Project1.Module/Controllers/Customers/MusteriPopupController.cs`
- Modify: `Project1/Project1.Module/Controllers/Customers/NestedListPopupController.cs`

**Interfaces:**
- Consumes: `Application.CreateObjectSpace(typeof(Not))`, `Application.CreateDetailView(objectSpace, yeniNot)`
- Produces: Kişi ve Müşteri ekranlarındaki "Not Ekle" popup'larında doğrudan `Dosya` (dosya yükleme butonu) ve `Project2IlePaylas` içeren `Not` formu.

- [ ] **Step 1: Update `KisiNotePopupController.cs`**

```csharp
using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using Project1.Module.Models.Customers;
using Project1.Module.Models.Notes;
using Project1.Module.BusinessObjects.Enums;

namespace Project1.Module.Controllers.Customers
{
    public sealed class KisiNotePopupController : ObjectViewController<ObjectView, Kisi>
    {
        private readonly PopupWindowShowAction notEkleAction;

        public KisiNotePopupController()
        {
            notEkleAction = new PopupWindowShowAction(this, "KisiNotEkleAction", PredefinedCategory.RecordEdit)
            {
                Caption = "Not Ekle",
                ImageName = "Crm_Not",
                TargetObjectType = typeof(Kisi),
                TargetViewType = ViewType.Any,
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            notEkleAction.CustomizePopupWindowParams += NotEkleAction_CustomizePopupWindowParams;
            notEkleAction.Execute += NotEkleAction_Execute;
        }

        private void NotEkleAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            IObjectSpace objectSpace = Application.CreateObjectSpace(typeof(Not));
            Not yeniNot = objectSpace.CreateObject<Not>();
            yeniNot.Derece = NotDerecesi.Normal;

            if (View.CurrentObject is Kisi seciliKisi)
            {
                yeniNot.Kisi = objectSpace.GetObject(seciliKisi);
                if (seciliKisi.Musteri != null)
                {
                    yeniNot.Musteri = objectSpace.GetObject(seciliKisi.Musteri);
                }
                yeniNot.IsMusteriHidden = true;
                yeniNot.IsKisiHidden = true;
            }

            e.View = Application.CreateDetailView(objectSpace, yeniNot);
        }

        private void NotEkleAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowView?.CurrentObject is Not yeniNot)
            {
                e.PopupWindowView.ObjectSpace.CommitChanges();
                View.ObjectSpace.Refresh();
            }
        }
    }
}
```

- [ ] **Step 2: Update `MusteriPopupController.cs`**

```csharp
private void NotEkleAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
{
    IObjectSpace objectSpace = Application.CreateObjectSpace(typeof(Not));
    Not yeniNot = objectSpace.CreateObject<Not>();
    yeniNot.Derece = NotDerecesi.Normal;

    if (View.CurrentObject is Musteri seciliMusteri)
    {
        yeniNot.Musteri = objectSpace.GetObject(seciliMusteri);
        yeniNot.IsMusteriHidden = true;
    }

    e.View = Application.CreateDetailView(objectSpace, yeniNot);
}

private void NotEkleAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
{
    if (e.PopupWindowView?.CurrentObject is Not yeniNot)
    {
        e.PopupWindowView.ObjectSpace.CommitChanges();
        View.ObjectSpace.Refresh();
    }
}
```

- [ ] **Step 3: Update `NestedListPopupController.cs`**

```csharp
private void PopupEkleAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
{
    if (View.ObjectTypeInfo.Type == typeof(Kisi))
    {
        Type paramType = typeof(CreateKisiParameters);
        IObjectSpace popupObjectSpace = Application.CreateObjectSpace(paramType);
        var kisiParams = popupObjectSpace.CreateObject<CreateKisiParameters>();

        PropertyCollectionSource collectionSource = View.CollectionSource as PropertyCollectionSource;
        if (collectionSource?.MasterObject is Musteri musteri)
        {
            kisiParams.Musteri = popupObjectSpace.GetObject(musteri);
        }

        e.View = Application.CreateDetailView(popupObjectSpace, kisiParams);
    }
    else if (View.ObjectTypeInfo.Type == typeof(Not))
    {
        IObjectSpace popupObjectSpace = Application.CreateObjectSpace(typeof(Not));
        Not yeniNot = popupObjectSpace.CreateObject<Not>();
        yeniNot.Derece = NotDerecesi.Normal;

        PropertyCollectionSource collectionSource = View.CollectionSource as PropertyCollectionSource;
        object masterObject = collectionSource?.MasterObject;

        if (masterObject is Musteri musteri)
        {
            yeniNot.Musteri = popupObjectSpace.GetObject(musteri);
            yeniNot.IsMusteriHidden = true;
        }
        else if (masterObject is Kisi kisi)
        {
            yeniNot.Kisi = popupObjectSpace.GetObject(kisi);
            if (kisi.Musteri != null)
            {
                yeniNot.Musteri = popupObjectSpace.GetObject(kisi.Musteri);
            }
            yeniNot.IsMusteriHidden = true;
            yeniNot.IsKisiHidden = true;
        }

        e.View = Application.CreateDetailView(popupObjectSpace, yeniNot);
    }
}

private async void PopupEkleAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
{
    if (e.PopupWindowView.CurrentObject is CreateKisiParameters kisiParams)
    {
        var mediator = Application.ServiceProvider.GetRequiredService<IMediator>();
        var command = new CreateKisiCommand(
            kisiParams.Ad, 
            kisiParams.Soyad, 
            kisiParams.Email,
            kisiParams.Telefon,
            kisiParams.Musteri?.Oid);
        await mediator.Send(command);
    }
    else if (e.PopupWindowView.CurrentObject is Not)
    {
        e.PopupWindowView.ObjectSpace.CommitChanges();
    }

    View.ObjectSpace.Refresh();
}
```

- [ ] **Step 4: Build and test**

Run: `dotnet test InternshipProject.sln`
Expected: PASS

- [ ] **Step 5: Commit Controllers**

```bash
git add Project1/Project1.Module/Controllers/Customers/
git commit -m "feat(controllers): enable direct FileData and Project2 sharing in all note creation popups"
```

---

### Task 5: Servis Katmanı (`INoteService` & `NoteService`) ve Birim Testleri

**Files:**
- Modify: `Project1/Project1.Core/Services/Interfaces/INoteService.cs`
- Modify: `Project1/Project1.Module/Services/Implementations/NoteService.cs`
- Modify: `Project1/Project1.Module.Tests/Services/NoteServiceTests.cs`

**Interfaces:**
- Consumes: `Not.Dosya`, `NoteDto`, `NoteAttachmentDto`
- Produces: `NoteService.GetNotesAsync`, `NoteService.GetAttachmentFileAsync`

- [ ] **Step 1: Verify service unit tests in `NoteServiceTests.cs`**

```csharp
[Fact]
public async Task GetNotesAsync_ShouldIncludeAttachmentInfo_WhenNoteHasDosya()
{
    // InMemory session setup with Not and FileData
    // Asserts note.Ek is populated with /api/attachments/{noteId}/download
}

[Fact]
public async Task GetAttachmentFileAsync_ShouldReturnFileBytes_WhenNoteHasDosya()
{
    // Verifies binary content, filename, and content-type stream
}
```

- [ ] **Step 2: Run test to verify it passes**

Run: `dotnet test --filter "FullyQualifiedName~NoteServiceTests"`
Expected: PASS

- [ ] **Step 3: Commit Service changes**

```bash
git add Project1/Project1.Core/Services/Interfaces/INoteService.cs Project1/Project1.Module/Services/Implementations/NoteService.cs Project1/Project1.Module.Tests/Services/NoteServiceTests.cs
git commit -m "feat(service): ensure NoteService accurately maps direct FileData and serves attachment stream"
```

---

### Task 6: REST API (`AttachmentsApiController` & `NotesApiController`)

**Files:**
- Modify: `Project1/Project1.Blazor.Server/Controllers/AttachmentsApiController.cs`
- Modify: `Project1/Project1.Module.Tests/Api/AttachmentsApiControllerTests.cs`

**Interfaces:**
- Consumes: `INoteService.GetAttachmentFileAsync(Guid noteId)`
- Produces: `GET /api/attachments/{noteId:guid}/download` binary response

- [ ] **Step 1: Run Controller Tests**

Run: `dotnet test --filter "FullyQualifiedName~AttachmentsApiControllerTests"`
Expected: PASS

- [ ] **Step 2: Commit API Controller**

```bash
git add Project1/Project1.Blazor.Server/Controllers/AttachmentsApiController.cs Project1/Project1.Module.Tests/Api/AttachmentsApiControllerTests.cs
git commit -m "feat(api): verify AttachmentsApiController binary file streaming"
```

---

### Task 7: Project2 Blazor Web Arayüzü (`Project2/Pages/Notes.razor`)

**Files:**
- Modify: `Project2/Pages/Notes.razor`

**Interfaces:**
- Consumes: `NoteDto.Ek` (`NoteAttachmentDto?`)
- Produces: Tablo üzerinde 🖼️ Görsel Önizleme Modalı, 📄 PDF Aç/İndir butonu, 📎 Dosya İndir butonu veya `Ek yok` rozeti.

- [ ] **Step 1: Verify `Project2/Pages/Notes.razor` Implementation**

In `Project2/Pages/Notes.razor`:
- Check cell rendering for `note.Ek`.
- Check `OpenImageModal` and `CloseImageModal` functions.
- Check `GetFullDownloadUrl` and `FormatBytes`.

- [ ] **Step 2: Build Project2 to verify compilation**

Run: `dotnet build Project2/Project2.csproj`
Expected: Build succeeded with 0 errors.

- [ ] **Step 3: Commit Project2 UI**

```bash
git add Project2/Pages/Notes.razor
git commit -m "feat(ui): update Project2 notes view with PDF and image modal previews"
```

---

### Task 8: Çözüm Genelinde Test Koşumu ve Uçtan Uca Doğrulama

**Files:**
- Solution: `InternshipProject.sln`

- [ ] **Step 1: Run complete solution tests**

Run: `dotnet test InternshipProject.sln`
Expected: Total tests passed, 0 failures.

- [ ] **Step 2: Restart and verify background services**

Restart Project1 Blazor Server and Project2 services to verify manual workflow.
