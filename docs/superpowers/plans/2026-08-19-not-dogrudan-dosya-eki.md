# Not Formunda Doğrudan Dosya Eki (PDF / Görsel) ve Project2 Entegrasyonu Uygulama Planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Not ekranındaki karmaşık "Ekler" alt tablosunu kaldırıp yerine doğrudan form alanları arasına tek tıkla işletim sisteminin dosya seçici penceresini açan `FileData Dosya` özelliğini eklemek, REST API üzerinden Project2'ye tekil ek bilgisiyle aktarmak ve Project2'de PDF açma / görsel modalı ile görüntülenmesini sağlamak.

**Architecture:** Project1 DevExpress XAF katmanında `Not` sınıfına doğrudan `FileData` (`Dosya`) entegrasyonu ve `NotEk` alt tablosunun temizlenmesi; `Project1.DTOs` ve `Project2.DTOs` katmanında güncellenmiş `NoteDto` (`Ek: NoteAttachmentDto?`); `Project1.Blazor.Server` katmanında `/api/notes` ve `/api/attachments/{noteId}/download` stream endpoint'leri; `Project2` katmanında sadeleştirilmiş ve modern tekil dosya eki arayüzü.

**Tech Stack:** C# .NET 8, DevExpress XAF / XPO (`FileData`, `FileTypeFilter`), ASP.NET Core Web API, Blazor Server, xUnit, FluentAssertions, Moq.

## Global Constraints
- Target Framework: .NET 8 (`net8.0`)
- DevExpress XPO `FileData` file management
- Allowed file types: `*.pdf;*.png;*.jpg;*.jpeg;*.gif;*.webp;*.bmp`
- REST API endpoint: `/api/attachments/{noteId}/download` (Stream binary file with `[AllowAnonymous]` and `[EnableCors("AllowAll")]`)
- TDD with passing unit tests before marking tasks complete

---

### Task 1: Update DTOs in `Project1.DTOs` and `Project2`

**Files:**
- Modify: `Project1/Project1.DTOs/Notes/NoteDto.cs`
- Modify: `Project2/DTOs/NoteDto.cs`
- Modify: `Project1/Project1.Module.Tests/DTOs/DtoValidationTests.cs`

**Interfaces:**
- Consumes: `NoteAttachmentDto` record definition
- Produces: `NoteDto` containing single optional `NoteAttachmentDto? Ek = null` (and backwards-compatible property `IsSharedWithProject2`)

- [ ] **Step 1: Write/Update the DTO unit test**

In `Project1/Project1.Module.Tests/DTOs/DtoValidationTests.cs`:
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

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter "FullyQualifiedName~DtoValidationTests"`
Expected: FAIL (Compilation error: `NoteDto` parameter mismatch with `ek`).

- [ ] **Step 3: Update `NoteDto.cs` in `Project1.DTOs` and `Project2/DTOs`**

In `Project1/Project1.DTOs/Notes/NoteDto.cs` and `Project2/DTOs/NoteDto.cs`:
```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Project1.DTOs.Notes
{
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

    public record CreateNoteRequestDto(
        [property: Required(ErrorMessage = "Not başlığı zorunludur.")]
        string Baslik,
        [property: Required(ErrorMessage = "Not içeriği zorunludur.")]
        string Icerik,
        [property: Range(0, 2, ErrorMessage = "Not derecesi geçerli bir değer olmalıdır.")]
        int Derece,
        Guid? MusteriOid = null,
        Guid? KisiOid = null,
        bool Project2IlePaylas = false
    );
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test --filter "FullyQualifiedName~DtoValidationTests"`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add Project1/Project1.DTOs/Notes/NoteDto.cs Project2/DTOs/NoteDto.cs Project1/Project1.Module.Tests/DTOs/DtoValidationTests.cs
git commit -m "feat(dto): update NoteDto to hold single NoteAttachmentDto"
```

---

### Task 2: Update Domain Model in `Project1.Module` and Clean Up `NotEk`

**Files:**
- Modify: `Project1/Project1.Module/Models/Notes/Not.cs`
- Delete: `Project1/Project1.Module/Models/Notes/NotEk.cs`
- Modify: `Project1/Project1.Module.Tests/Domain/NotDomainTests.cs`
- Delete: `Project1/Project1.Module.Tests/Domain/NotEkDomainTests.cs`

**Interfaces:**
- Consumes: DevExpress `FileData`, `FileTypeFilter`
- Produces: `Not` entity with direct property `FileData Dosya`, `DosyaAdi`, `BoyutBytes`, `ContentType`, `IsImage`, `IsPdf`

- [ ] **Step 1: Write Domain Tests for `Not.Dosya`**

In `Project1/Project1.Module.Tests/Domain/NotDomainTests.cs` (or create new facts):
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

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter "FullyQualifiedName~NotDomainTests"`
Expected: FAIL (Compilation error: `Dosya` / `GetContentType` does not exist on `Not`).

- [ ] **Step 3: Update `Not.cs` and remove `NotEk.cs`**

Delete `Project1/Project1.Module/Models/Notes/NotEk.cs` and `Project1/Project1.Module.Tests/Domain/NotEkDomainTests.cs`.
In `Project1/Project1.Module/Models/Notes/Not.cs`:
Replace `Ekler` collection with:
```csharp
        private FileData _dosya;
        [Aggregated]
        [FileTypeFilter("PDF ve Görseller", "*.pdf;*.png;*.jpg;*.jpeg;*.gif;*.webp;*.bmp")]
        [XafDisplayName("Dosya Eki (PDF / Görsel)")]
        public FileData Dosya
        {
            get => _dosya;
            set => SetPropertyValue(nameof(Dosya), ref _dosya, value);
        }

        [XafDisplayName("Dosya Adı")]
        [VisibleInDetailView(false)]
        [VisibleInListView(true)]
        public string DosyaAdi => Dosya?.FileName ?? string.Empty;

        [Browsable(false)]
        public long BoyutBytes => Dosya?.Size ?? 0;

        [Browsable(false)]
        public string ContentType => GetContentType(Dosya?.FileName);

        [Browsable(false)]
        public bool IsImage => ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

        [Browsable(false)]
        public bool IsPdf => ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase);

        public static string GetContentType(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return "application/octet-stream";
            string ext = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream"
            };
        }
```

- [ ] **Step 4: Run domain tests to verify they pass**

Run: `dotnet test --filter "FullyQualifiedName~NotDomainTests"`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add Project1/Project1.Module/Models/Notes/Not.cs Project1/Project1.Module.Tests/Domain/NotDomainTests.cs
git rm Project1/Project1.Module/Models/Notes/NotEk.cs Project1/Project1.Module.Tests/Domain/NotEkDomainTests.cs
git commit -m "feat(domain): add direct FileData property to Not and remove NotEk"
```

---

### Task 3: Update `INoteService` & `NoteService` and Service Tests

**Files:**
- Modify: `Project1/Project1.Core/Services/Interfaces/INoteService.cs`
- Modify: `Project1/Project1.Module/Services/Implementations/NoteService.cs`
- Modify: `Project1/Project1.Module.Tests/Services/NoteServiceTests.cs`

**Interfaces:**
- Consumes: `Not.Dosya`, `NoteDto`, `NoteAttachmentDto`
- Produces: `NoteService.GetNotesAsync` (maps `Dosya` to `NoteAttachmentDto`), `NoteService.GetAttachmentFileAsync(Guid noteId)`

- [ ] **Step 1: Update unit tests in `NoteServiceTests.cs`**

In `Project1/Project1.Module.Tests/Services/NoteServiceTests.cs`:
```csharp
[Fact]
public async Task GetNotesAsync_ShouldIncludeAttachmentInfo_WhenNoteHasDosya()
{
    var dataStore = new InMemoryDataStore(AutoCreateOption.DatabaseAndSchema);
    var dataLayer = new SimpleDataLayer(dataStore);
    var typesInfoSource = XpoTypesInfoHelper.GetXpoTypeInfoSource();
    var typesInfo = XpoTypesInfoHelper.GetTypesInfo();
    typesInfo.RegisterEntity(typeof(Not));
    typesInfo.RegisterEntity(typeof(Musteri));
    typesInfo.RegisterEntity(typeof(Kisi));

    Guid noteId;
    using (var uow = new UnitOfWork(dataLayer))
    {
        var fileData = new FileData(uow);
        fileData.LoadFromStream("rapor.pdf", new System.IO.MemoryStream(new byte[] { 10, 20, 30 }));

        var n = new Not(uow)
        {
            Baslik = "Rapor Notu",
            Icerik = "Ekli dosya testi",
            Project2IlePaylas = true,
            Dosya = fileData
        };
        n.Save();
        uow.CommitChanges();
        noteId = n.Oid;
    }

    var mockFactory = new Mock<IObjectSpaceFactory>();
    mockFactory
        .Setup(f => f.CreateObjectSpace(It.IsAny<Type>()))
        .Returns(() => new XPObjectSpace(typesInfo, typesInfoSource, () => new UnitOfWork(dataLayer)));

    var noteService = new NoteService(mockFactory.Object);

    var notes = (await noteService.GetNotesAsync(onlyShared: true)).ToList();

    notes.Should().HaveCount(1);
    notes[0].Ek.Should().NotBeNull();
    notes[0].Ek!.DosyaAdi.Should().Be("rapor.pdf");
    notes[0].Ek.DownloadUrl.Should().Be($"/api/attachments/{noteId}/download");
    notes[0].Ek.IsPdf.Should().BeTrue();
}

[Fact]
public async Task GetAttachmentFileAsync_ShouldReturnFileBytes_WhenNoteHasDosya()
{
    var dataStore = new InMemoryDataStore(AutoCreateOption.DatabaseAndSchema);
    var dataLayer = new SimpleDataLayer(dataStore);
    var typesInfoSource = XpoTypesInfoHelper.GetXpoTypeInfoSource();
    var typesInfo = XpoTypesInfoHelper.GetTypesInfo();
    typesInfo.RegisterEntity(typeof(Not));
    typesInfo.RegisterEntity(typeof(Musteri));
    typesInfo.RegisterEntity(typeof(Kisi));

    Guid noteId;
    var expectedBytes = new byte[] { 1, 2, 3, 4, 5 };
    using (var uow = new UnitOfWork(dataLayer))
    {
        var fileData = new FileData(uow);
        fileData.LoadFromStream("resim.png", new System.IO.MemoryStream(expectedBytes));

        var n = new Not(uow)
        {
            Baslik = "Resimli Not",
            Icerik = "Görsel testi",
            Dosya = fileData
        };
        n.Save();
        uow.CommitChanges();
        noteId = n.Oid;
    }

    var mockFactory = new Mock<IObjectSpaceFactory>();
    mockFactory
        .Setup(f => f.CreateObjectSpace(It.IsAny<Type>()))
        .Returns(() => new XPObjectSpace(typesInfo, typesInfoSource, () => new UnitOfWork(dataLayer)));

    var noteService = new NoteService(mockFactory.Object);

    var file = await noteService.GetAttachmentFileAsync(noteId);

    file.Should().NotBeNull();
    file!.Value.FileName.Should().Be("resim.png");
    file.Value.ContentType.Should().Be("image/png");
    file.Value.Bytes.Should().BeEquivalentTo(expectedBytes);
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter "FullyQualifiedName~NoteServiceTests"`
Expected: FAIL (Compilation/runtime error due to old `NotEk` mapping in `NoteService`).

- [ ] **Step 3: Update `NoteService.cs` and `INoteService.cs`**

In `Project1/Project1.Core/Services/Interfaces/INoteService.cs`:
`Task<(byte[] Bytes, string FileName, string ContentType)?> GetAttachmentFileAsync(Guid noteId, CancellationToken cancellationToken = default);`

In `Project1/Project1.Module/Services/Implementations/NoteService.cs`:
```csharp
private NoteDto MapToDto(Not n)
{
    NoteAttachmentDto? ekDto = null;
    if (n.Dosya != null && !string.IsNullOrEmpty(n.Dosya.FileName))
    {
        ekDto = new NoteAttachmentDto(
            n.Oid,
            n.DosyaAdi,
            n.ContentType,
            n.BoyutBytes,
            n.CreatedDate,
            $"/api/attachments/{n.Oid}/download",
            n.IsImage,
            n.IsPdf
        );
    }

    return new NoteDto(
        n.Oid,
        n.Baslik ?? string.Empty,
        n.Icerik ?? string.Empty,
        n.Derece.ToString(),
        n.Musteri != null ? n.Musteri.Ad : string.Empty,
        n.Kisi != null ? (n.Kisi.Ad + " " + n.Kisi.Soyad).Trim() : string.Empty,
        n.IsEmailSent,
        n.CreatedDate,
        n.MailDurumu.ToString(),
        n.MailGonderilmeTarihi,
        n.MailIletilmeTarihi,
        n.MailOkunmaTarihi,
        n.Project2IlePaylas,
        ekDto
    );
}

public Task<(byte[] Bytes, string FileName, string ContentType)?> GetAttachmentFileAsync(Guid noteId, CancellationToken cancellationToken = default)
{
    using IObjectSpace objectSpace = CreateObjectSpace();
    var not = objectSpace.GetObjectByKey<Not>(noteId);
    if (not?.Dosya?.Content == null || not.Dosya.Content.Length == 0)
    {
        return Task.FromResult<(byte[] Bytes, string FileName, string ContentType)?>(null);
    }

    return Task.FromResult<(byte[] Bytes, string FileName, string ContentType)?>((
        not.Dosya.Content,
        not.DosyaAdi,
        not.ContentType
    ));
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test --filter "FullyQualifiedName~NoteServiceTests"`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add Project1/Project1.Core/Services/Interfaces/INoteService.cs Project1/Project1.Module/Services/Implementations/NoteService.cs Project1/Project1.Module.Tests/Services/NoteServiceTests.cs
git commit -m "feat(service): update NoteService to map direct Dosya attachment and stream by noteId"
```

---

### Task 4: Update `AttachmentsApiController` and Controller Tests

**Files:**
- Modify: `Project1/Project1.Blazor.Server/Controllers/AttachmentsApiController.cs`
- Modify: `Project1/Project1.Module.Tests/Api/AttachmentsApiControllerTests.cs`

**Interfaces:**
- Consumes: `INoteService.GetAttachmentFileAsync(Guid noteId)`
- Produces: `GET /api/attachments/{noteId:guid}/download` -> `FileResult`

- [ ] **Step 1: Update `AttachmentsApiControllerTests.cs`**

Verify `AttachmentsApiControllerTests` has tests for successful download and 404 Not Found.

- [ ] **Step 2: Run test to verify it passes**

Run: `dotnet test --filter "FullyQualifiedName~AttachmentsApiControllerTests"`
Expected: PASS

- [ ] **Step 3: Commit**

```bash
git add Project1/Project1.Blazor.Server/Controllers/AttachmentsApiController.cs Project1/Project1.Module.Tests/Api/AttachmentsApiControllerTests.cs
git commit -m "feat(api): verify and update AttachmentsApiController for direct noteId download"
```

---

### Task 5: Update `Project2/Pages/Notes.razor` UI

**Files:**
- Modify: `Project2/Pages/Notes.razor`

**Interfaces:**
- Consumes: `NoteDto.Ek` (`NoteAttachmentDto?`)
- Produces: Responsive UI with 🖼️ Modal preview button for images, 📄 PDF view/download button for PDFs, 📎 Download for other files, or `Ek yok` if empty.

- [ ] **Step 1: Update `Project2/Pages/Notes.razor`**

Update the "Ek (PDF / Görsel)" table header and cell rendering:
```razor
<th style="width: 20%;">Ek (PDF / Görsel)</th>
...
<td>
    @if (note.Ek == null || string.IsNullOrWhiteSpace(note.Ek.DownloadUrl))
    {
        <span class="text-muted small"><em>Ek yok</em></span>
    }
    else
    {
        var ek = note.Ek;
        var downloadUrl = GetFullDownloadUrl(ek.DownloadUrl);
        if (ek.IsImage)
        {
            <button type="button" class="btn btn-sm btn-outline-info text-dark d-inline-flex align-items-center gap-1 py-1 px-2"
                    @onclick="() => OpenImageModal(downloadUrl, ek.DosyaAdi)"
                    title="Görseli Büyüt">
                <span>🖼️</span>
                <span class="text-truncate" style="max-width: 140px;">@ek.DosyaAdi</span>
                <span class="badge bg-light text-muted">@FormatBytes(ek.BoyutBytes)</span>
            </button>
        }
        else if (ek.IsPdf)
        {
            <a href="@downloadUrl" target="_blank" class="btn btn-sm btn-outline-danger d-inline-flex align-items-center gap-1 py-1 px-2 text-decoration-none" title="PDF Aç / İndir">
                <span>📄</span>
                <span class="text-truncate" style="max-width: 140px;">@ek.DosyaAdi</span>
                <span class="badge bg-light text-muted">@FormatBytes(ek.BoyutBytes)</span>
            </a>
        }
        else
        {
            <a href="@downloadUrl" target="_blank" class="btn btn-sm btn-outline-secondary d-inline-flex align-items-center gap-1 py-1 px-2 text-decoration-none" title="Dosyayı İndir">
                <span>📎</span>
                <span class="text-truncate" style="max-width: 140px;">@ek.DosyaAdi</span>
                <span class="badge bg-light text-muted">@FormatBytes(ek.BoyutBytes)</span>
            </a>
        }
    }
</td>
```

- [ ] **Step 2: Build Project2 to verify compilation**

Run: `dotnet build Project2/Project2.csproj`
Expected: Build succeeded with 0 errors.

- [ ] **Step 3: Commit**

```bash
git add Project2/Pages/Notes.razor
git commit -m "feat(ui): update Project2 notes page for single direct file attachment"
```

---

### Task 6: Full Solution Build, Test Suite Run & Service Verification

**Files:**
- Solution: `InternshipProject.sln`

- [ ] **Step 1: Run complete test suite**

Run: `dotnet test InternshipProject.sln`
Expected: All unit tests pass with 0 errors.

- [ ] **Step 2: Restart background running services (Project1 and Project2)**

Restart Project1 Blazor Server and Project2 services.

- [ ] **Step 3: Verify end-to-end functionality**

Verify XAF Not DetailView renders direct FileData browse button without subgrid, and Project2 renders shared notes with attachments correctly.
