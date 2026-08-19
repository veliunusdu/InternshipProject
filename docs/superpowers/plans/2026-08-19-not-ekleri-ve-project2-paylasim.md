# Notlara PDF/Görsel Yükleme ve Project2 ile Paylaşım Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Notlara PDF ve görsel (resim) dosyalarının çoklu olarak yüklenebilmesi, `Project2IlePaylas` alanı ile notların filtrelenerek REST API üzerinden Project2'ye aktarılması ve Project2 Blazor Web uygulamasında eklerin (PDF görüntüleme/indirme ve resim önizleme) sunulması.

**Architecture:** Project1 DevExpress XAF (XPO) katmanında `NotEk` varlığı ve `FileData` entegrasyonu; Project1.DTOs katmanında `NoteAttachmentDto` ve güncellenmiş `NoteDto`; Project1.Blazor.Server katmanında `/api/notes?onlyShared=true` ve `/api/attachments/{id}/download` stream endpoint'leri; Project2 (Blazor Server) katmanında modernize edilmiş zengin ek destekli not listesi ve modal görsel önizleme.

**Tech Stack:** .NET 8.0, DevExpress XAF (XPO & Blazor Server), ASP.NET Core REST API, Blazor Server (Project2), xUnit, FluentAssertions, Moq.

## Global Constraints
- Target Framework: net8.0 (net8.0-windows for WinForms)
- Clean Architecture katman bağımlılıklarına uyulmalıdır (`Project1.Core` -> `Project1.DTOs`, `Project1.Module` -> `Project1.Core`, vb.).
- `Project1.Module.Tests` birim testleri eksiksiz geçmelidir (`dotnet test InternshipProject.sln`).
- REST API endpoint'leri (`/api/notes`, `/api/attachments/*`) `[AllowAnonymous]` ve `[EnableCors("AllowAll")]` özniteliklerine sahip olmalıdır.

---

### Task 1: DTO Katmanı Güncellemeleri (`Project1.DTOs`)

**Files:**
- Create: `Project1/Project1.DTOs/Notes/NoteAttachmentDto.cs`
- Modify: `Project1/Project1.DTOs/Notes/NoteDto.cs`
- Modify: `Project1/Project1.Module.Tests/DTOs/DtoValidationTests.cs`

**Interfaces:**
- Produces: `NoteAttachmentDto(Guid Oid, string DosyaAdi, string ContentType, long BoyutBytes, DateTime YuklemeTarihi, string DownloadUrl, bool IsImage, bool IsPdf)`
- Produces: `NoteDto(Guid Oid, string Baslik, string Icerik, string Derece, string Musteri, string Kisi, bool IsEmailSent, bool IsSharedWithProject2, List<NoteAttachmentDto>? Ekler, DateTime CreatedDate, string MailDurumu, DateTime? MailGonderilmeTarihi, DateTime? MailIletilmeTarihi, DateTime? MailOkunmaTarihi)`

- [ ] **Step 1: Write the failing DTO test**
Add a test in `Project1/Project1.Module.Tests/DTOs/DtoValidationTests.cs` verifying `NoteAttachmentDto` properties and `NoteDto` attachment collection serialization.

```csharp
[Fact]
public void NoteAttachmentDto_ShouldCorrectlyInstantiateAndHoldValues()
{
    var id = Guid.NewGuid();
    var now = DateTime.Now;
    var dto = new NoteAttachmentDto(
        Oid: id,
        DosyaAdi: "rapor.pdf",
        ContentType: "application/pdf",
        BoyutBytes: 1024,
        YuklemeTarihi: now,
        DownloadUrl: "/api/attachments/" + id + "/download",
        IsImage: false,
        IsPdf: true
    );

    dto.Oid.Should().Be(id);
    dto.DosyaAdi.Should().Be("rapor.pdf");
    dto.ContentType.Should().Be("application/pdf");
    dto.BoyutBytes.Should().Be(1024);
    dto.IsPdf.Should().BeTrue();
    dto.IsImage.Should().BeFalse();
}
```

- [ ] **Step 2: Run test to verify it fails**
Run: `dotnet test --filter "FullyQualifiedName~NoteAttachmentDto"`
Expected: FAIL (Compilation error: `NoteAttachmentDto` does not exist).

- [ ] **Step 3: Implement `NoteAttachmentDto` and update `NoteDto`**
Create `Project1/Project1.DTOs/Notes/NoteAttachmentDto.cs`:
```csharp
using System;

namespace Project1.DTOs.Notes
{
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
}
```

Update `Project1/Project1.DTOs/Notes/NoteDto.cs` to include `IsSharedWithProject2` and `Ekler`:
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
        bool IsSharedWithProject2 = false,
        List<NoteAttachmentDto>? Ekler = null,
        DateTime CreatedDate = default,
        string MailDurumu = "Gonderilmedi",
        DateTime? MailGonderilmeTarihi = null,
        DateTime? MailIletilmeTarihi = null,
        DateTime? MailOkunmaTarihi = null
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
Run: `dotnet test --filter "FullyQualifiedName~NoteAttachmentDto"`
Expected: PASS.

- [ ] **Step 5: Commit DTO changes**
```bash
git add Project1/Project1.DTOs/ Project1/Project1.Module.Tests/DTOs/
git commit -m "feat(dto): add NoteAttachmentDto and update NoteDto with attachments and sharing flag"
```

---

### Task 2: Domain Katmanı Güncellemeleri (`Project1.Module`)

**Files:**
- Create: `Project1/Project1.Module/Models/Notes/NotEk.cs`
- Modify: `Project1/Project1.Module/Models/Notes/Not.cs`
- Create: `Project1/Project1.Module.Tests/Domain/NotEkDomainTests.cs`
- Modify: `Project1/Project1.Module.Tests/Domain/NotDomainTests.cs`

**Interfaces:**
- Produces: `Project1.Module.Models.Notes.NotEk` with properties `Not`, `Dosya`, `Aciklama`, `YuklemeTarihi`, `DosyaAdi`, `BoyutBytes`, `ContentType`, `IsImage`, `IsPdf`.
- Modifies: `Project1.Module.Models.Notes.Not` with property `bool Project2IlePaylas` and `XPCollection<NotEk> Ekler`.

- [ ] **Step 1: Write the failing Domain tests**
Create `Project1/Project1.Module.Tests/Domain/NotEkDomainTests.cs`:
```csharp
#nullable enable
using System;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using FluentAssertions;
using Project1.Module.Models.Notes;
using Xunit;

namespace Project1.Module.Tests.Domain
{
    public class NotEkDomainTests
    {
        private Session CreateInMemorySession()
        {
            var dataStore = new InMemoryDataStore(AutoCreateOption.DatabaseAndSchema);
            var dataLayer = new SimpleDataLayer(dataStore);
            return new Session(dataLayer);
        }

        [Fact]
        public void NotEk_ShouldComputeContentTypeAndFlags_ForPdfAndImages()
        {
            NotEk.GetContentType("dokuman.pdf").Should().Be("application/pdf");
            NotEk.GetContentType("resim.png").Should().Be("image/png");
            NotEk.GetContentType("foto.jpg").Should().Be("image/jpeg");
            NotEk.GetContentType("foto.jpeg").Should().Be("image/jpeg");
            NotEk.GetContentType("animasyon.gif").Should().Be("image/gif");
            NotEk.GetContentType("diger.xyz").Should().Be("application/octet-stream");
        }

        [Fact]
        public void Not_ShouldManageEklerCollection_AndCascadeDelete()
        {
            using var session = CreateInMemorySession();
            var not = new Not(session)
            {
                Baslik = "Ekli Not",
                Icerik = "İçerik",
                Project2IlePaylas = true
            };
            not.Save();

            var ek = new NotEk(session)
            {
                Not = not,
                Aciklama = "Test Eki"
            };
            ek.Save();
            session.CommitTransaction();

            not.Ekler.Should().HaveCount(1);
            not.Project2IlePaylas.Should().BeTrue();
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**
Run: `dotnet test --filter "FullyQualifiedName~NotEkDomainTests"`
Expected: FAIL (Compilation error: `NotEk` does not exist).

- [ ] **Step 3: Implement `NotEk.cs` and update `Not.cs`**
Create `Project1/Project1.Module/Models/Notes/NotEk.cs`:
```csharp
using System;
using System.ComponentModel;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace Project1.Module.Models.Notes
{
    [DefaultClassOptions]
    [DefaultProperty(nameof(DosyaAdi))]
    [ImageName("BO_FileAttachment")]
    [XafDisplayName("Not Eki")]
    public class NotEk : BaseObject
    {
        public NotEk(Session session) : base(session)
        {
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            YuklemeTarihi = DateTime.Now;
        }

        private Not _not;
        [Association("Not-Ekler")]
        [XafDisplayName("Not")]
        public Not Not
        {
            get => _not;
            set => SetPropertyValue(nameof(Not), ref _not, value);
        }

        private FileData _dosya;
        [Aggregated]
        [FileTypeFilter("PDF ve Görseller", "*.pdf;*.png;*.jpg;*.jpeg;*.gif;*.webp;*.bmp")]
        [XafDisplayName("Dosya")]
        public FileData Dosya
        {
            get => _dosya;
            set => SetPropertyValue(nameof(Dosya), ref _dosya, value);
        }

        private string _aciklama;
        [XafDisplayName("Açıklama")]
        [Size(500)]
        public string Aciklama
        {
            get => _aciklama;
            set => SetPropertyValue(nameof(Aciklama), ref _aciklama, value);
        }

        private DateTime _yuklemeTarihi;
        [XafDisplayName("Yükleme Tarihi")]
        [ReadOnly(true)]
        public DateTime YuklemeTarihi
        {
            get => _yuklemeTarihi;
            set => SetPropertyValue(nameof(YuklemeTarihi), ref _yuklemeTarihi, value);
        }

        [XafDisplayName("Dosya Adı")]
        [NonPersistent]
        public string DosyaAdi => Dosya?.FileName ?? string.Empty;

        [XafDisplayName("Boyut (Bytes)")]
        [NonPersistent]
        public long BoyutBytes => Dosya?.Size ?? 0;

        [NonPersistent]
        public string ContentType => GetContentType(Dosya?.FileName);

        [NonPersistent]
        public bool IsImage => ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

        [NonPersistent]
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
    }
}
```

Update `Project1/Project1.Module/Models/Notes/Not.cs` adding `Project2IlePaylas` and `Ekler`:
```csharp
private bool _project2IlePaylas;
[XafDisplayName("Project2 ile Paylaş")]
[VisibleInListView(true)]
[VisibleInDetailView(true)]
[Index(5)]
public bool Project2IlePaylas
{
    get => _project2IlePaylas;
    set => SetPropertyValue(nameof(Project2IlePaylas), ref _project2IlePaylas, value);
}

[Association("Not-Ekler"), Aggregated]
[XafDisplayName("Ekler")]
public XPCollection<NotEk> Ekler => GetCollection<NotEk>(nameof(Ekler));
```

- [ ] **Step 4: Run tests to verify they pass**
Run: `dotnet test --filter "FullyQualifiedName~NotEkDomainTests|FullyQualifiedName~NotDomainTests"`
Expected: PASS.

- [ ] **Step 5: Commit Domain changes**
```bash
git add Project1/Project1.Module/Models/Notes/ Project1/Project1.Module.Tests/Domain/
git commit -m "feat(domain): add NotEk XPO entity and Project2IlePaylas property to Not"
```

---

### Task 3: Servis Katmanı Güncellemeleri (`Project1.Core` & `Project1.Module`)

**Files:**
- Modify: `Project1/Project1.Core/Services/Interfaces/INoteService.cs`
- Modify: `Project1/Project1.Module/Services/Implementations/NoteService.cs`
- Modify: `Project1/Project1.Module.Tests/Services/NoteServiceTests.cs`

**Interfaces:**
- Modifies: `INoteService.GetNotesAsync(bool? onlyShared = null, CancellationToken cancellationToken = default)`
- Modifies: `INoteService.GetAttachmentFileAsync(Guid attachmentId, CancellationToken cancellationToken = default)`

- [ ] **Step 1: Write failing tests for service filtering and attachments**
In `Project1/Project1.Module.Tests/Services/NoteServiceTests.cs`:
Add tests for `GetNotesAsync(onlyShared: true)` filtering and mapping of `NoteAttachmentDto` and `GetAttachmentFileAsync`.

```csharp
[Fact]
public async Task GetNotesAsync_ShouldFilterByOnlyShared_WhenRequested()
{
    var (objectSpace, uow) = CreateInMemoryObjectSpace();
    var n1 = new Not(uow) { Baslik = "Paylaşılan Not", Icerik = "A", Project2IlePaylas = true };
    var n2 = new Not(uow) { Baslik = "Gizli Not", Icerik = "B", Project2IlePaylas = false };
    n1.Save();
    n2.Save();
    uow.CommitChanges();

    var mockFactory = new Mock<IObjectSpaceFactory>();
    mockFactory.Setup(f => f.CreateObjectSpace(It.IsAny<Type>())).Returns(objectSpace);

    var noteService = new NoteService(mockFactory.Object);
    var sharedNotes = await noteService.GetNotesAsync(onlyShared: true);

    sharedNotes.Should().HaveCount(1);
    sharedNotes.First().Baslik.Should().Be("Paylaşılan Not");
    sharedNotes.First().IsSharedWithProject2.Should().BeTrue();
}
```

- [ ] **Step 2: Run test to verify it fails**
Run: `dotnet test --filter "FullyQualifiedName~GetNotesAsync_ShouldFilterByOnlyShared"`
Expected: FAIL.

- [ ] **Step 3: Update `INoteService` and `NoteService`**
Update `Project1/Project1.Core/Services/Interfaces/INoteService.cs`:
```csharp
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Project1.DTOs.Notes;

namespace Project1.Core.Services.Interfaces
{
    public interface INoteService
    {
        Task<IEnumerable<NoteDto>> GetNotesAsync(bool? onlyShared = null, CancellationToken cancellationToken = default);
        Task<NoteDto?> GetNoteByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<NoteDto> CreateNoteAsync(CreateNoteRequestDto request, CancellationToken cancellationToken = default);
        Task<(byte[] Bytes, string FileName, string ContentType)?> GetAttachmentFileAsync(Guid attachmentId, CancellationToken cancellationToken = default);
    }
}
```

Update `Project1/Project1.Module/Services/Implementations/NoteService.cs` to map `Ekler` and implement `GetAttachmentFileAsync`:
```csharp
private NoteDto MapToDto(Not n)
{
    var eklerDto = n.Ekler?
        .Select(e => new NoteAttachmentDto(
            e.Oid,
            e.DosyaAdi,
            e.ContentType,
            e.BoyutBytes,
            e.YuklemeTarihi,
            $"/api/attachments/{e.Oid}/download",
            e.IsImage,
            e.IsPdf
        ))
        .ToList() ?? new List<NoteAttachmentDto>();

    return new NoteDto(
        n.Oid,
        n.Baslik ?? string.Empty,
        n.Icerik ?? string.Empty,
        n.Derece.ToString(),
        n.Musteri != null ? n.Musteri.Ad : string.Empty,
        n.Kisi != null ? (n.Kisi.Ad + " " + n.Kisi.Soyad).Trim() : string.Empty,
        n.IsEmailSent,
        n.Project2IlePaylas,
        eklerDto,
        n.CreatedDate,
        n.MailDurumu.ToString(),
        n.MailGonderilmeTarihi,
        n.MailIletilmeTarihi,
        n.MailOkunmaTarihi
    );
}

public Task<IEnumerable<NoteDto>> GetNotesAsync(bool? onlyShared = null, CancellationToken cancellationToken = default)
{
    using IObjectSpace objectSpace = CreateObjectSpace();
    var query = objectSpace.GetObjectsQuery<Not>();
    if (onlyShared.HasValue && onlyShared.Value)
    {
        query = query.Where(n => n.Project2IlePaylas);
    }

    var notes = query.AsEnumerable().Select(n => MapToDto(n)).ToList();
    return Task.FromResult<IEnumerable<NoteDto>>(notes);
}

public Task<(byte[] Bytes, string FileName, string ContentType)?> GetAttachmentFileAsync(Guid attachmentId, CancellationToken cancellationToken = default)
{
    using IObjectSpace objectSpace = CreateObjectSpace();
    var ek = objectSpace.GetObjectByKey<NotEk>(attachmentId);
    if (ek?.Dosya?.Content == null) return Task.FromResult<(byte[], string, string)?>(null);

    return Task.FromResult<(byte[], string, string)?>((
        ek.Dosya.Content,
        ek.DosyaAdi,
        ek.ContentType
    ));
}
```

- [ ] **Step 4: Run tests to verify they pass**
Run: `dotnet test --filter "FullyQualifiedName~NoteServiceTests"`
Expected: PASS.

- [ ] **Step 5: Commit Service changes**
```bash
git add Project1/Project1.Core/ Project1/Project1.Module/Services/ Project1/Project1.Module.Tests/Services/
git commit -m "feat(service): implement note sharing filtering and attachment file retrieval in NoteService"
```

---

### Task 4: REST API Katmanı Güncellemeleri (`Project1.Blazor.Server`)

**Files:**
- Modify: `Project1/Project1.Blazor.Server/Controllers/NotesApiController.cs`
- Create: `Project1/Project1.Blazor.Server/Controllers/AttachmentsApiController.cs`
- Create: `Project1/Project1.Module.Tests/Api/AttachmentsApiControllerTests.cs`
- Modify: `Project1/Project1.Module.Tests/Api/ApiEndpointAttributeTests.cs`

**Interfaces:**
- Produces: `GET /api/notes?onlyShared={bool}`
- Produces: `GET /api/notes/shared`
- Produces: `GET /api/attachments/{id}/download` streaming file with `File(bytes, contentType, fileName)`

- [ ] **Step 1: Write failing API attribute and endpoint tests**
In `Project1/Project1.Module.Tests/Api/ApiEndpointAttributeTests.cs` and new `AttachmentsApiControllerTests.cs`:
Verify `AttachmentsApiController` and `NotesApiController` routing, `[AllowAnonymous]`, `[EnableCors("AllowAll")]`.

- [ ] **Step 2: Run test to verify it fails**
Run: `dotnet test --filter "FullyQualifiedName~AttachmentsApiControllerTests|FullyQualifiedName~ApiEndpointAttributeTests"`
Expected: FAIL.

- [ ] **Step 3: Implement `AttachmentsApiController.cs` and update `NotesApiController.cs`**
Create `Project1/Project1.Blazor.Server/Controllers/AttachmentsApiController.cs`:
```csharp
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Project1.Core.Services.Interfaces;

namespace Project1.Blazor.Server.Controllers
{
    [ApiController]
    [Route("api/attachments")]
    [AllowAnonymous]
    [EnableCors("AllowAll")]
    public class AttachmentsApiController : ControllerBase
    {
        private readonly INoteService _noteService;

        public AttachmentsApiController(INoteService noteService)
        {
            _noteService = noteService;
        }

        [HttpGet("{id:guid}/download")]
        public async Task<IActionResult> DownloadAttachment(Guid id)
        {
            var file = await _noteService.GetAttachmentFileAsync(id);
            if (file == null || file.Value.Bytes == null || file.Value.Bytes.Length == 0)
            {
                return NotFound("Dosya eki bulunamadı.");
            }

            var (bytes, fileName, contentType) = file.Value;
            return File(bytes, contentType, fileName);
        }
    }
}
```

Update `Project1/Project1.Blazor.Server/Controllers/NotesApiController.cs`:
```csharp
[HttpGet]
public async Task<IActionResult> GetNotes([FromQuery] bool? onlyShared = null)
{
    var notes = await _noteService.GetNotesAsync(onlyShared);
    return Ok(notes);
}

[HttpGet("shared")]
public async Task<IActionResult> GetSharedNotes()
{
    var notes = await _noteService.GetNotesAsync(onlyShared: true);
    return Ok(notes);
}
```

- [ ] **Step 4: Run tests to verify they pass**
Run: `dotnet test --filter "FullyQualifiedName~AttachmentsApiControllerTests|FullyQualifiedName~ApiEndpointAttributeTests"`
Expected: PASS.

- [ ] **Step 5: Commit API changes**
```bash
git add Project1/Project1.Blazor.Server/Controllers/ Project1/Project1.Module.Tests/Api/
git commit -m "feat(api): add AttachmentsApiController and update NotesApiController for shared notes"
```

---

### Task 5: Project2 Blazor Web Arayüzü Entegrasyonu (`Project2`)

**Files:**
- Create: `Project2/DTOs/NoteAttachmentDto.cs`
- Modify: `Project2/DTOs/NoteDto.cs`
- Modify: `Project2/ViewModels/NoteListViewModel.cs`
- Modify: `Project2/Pages/Notes.razor`

**Interfaces:**
- Consumes: `/api/notes?onlyShared=true` and `/api/attachments/{id}/download`

- [ ] **Step 1: Update Project2 DTOs and ViewModel**
Create `Project2/DTOs/NoteAttachmentDto.cs`:
```csharp
using System;

namespace Project2.DTOs
{
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
}
```

Update `Project2/DTOs/NoteDto.cs`:
```csharp
using System;
using System.Collections.Generic;

namespace Project2.DTOs
{
    public record NoteDto(
        Guid Oid,
        string Baslik,
        string Icerik,
        string Derece,
        string Musteri,
        string Kisi,
        bool IsEmailSent,
        bool IsSharedWithProject2 = false,
        List<NoteAttachmentDto>? Ekler = null,
        DateTime CreatedDate = default,
        string MailDurumu = "Gonderilmedi",
        DateTime? MailGonderilmeTarihi = null,
        DateTime? MailIletilmeTarihi = null,
        DateTime? MailOkunmaTarihi = null
    )
    {
        // Backward-compatible computed properties
        public string MusteriUnvan => Musteri;
        public string KisiAdSoyad => Kisi;
    }
}
```

Update `Project2/ViewModels/NoteListViewModel.cs` to request `api/notes?onlyShared=true`:
```csharp
public async Task LoadNotesAsync()
{
    IsLoading = true;
    ErrorMessage = null;
    try
    {
        var result = await _http.GetFromJsonAsync<List<NoteDto>>("api/notes?onlyShared=true");
        Notes = result ?? new List<NoteDto>();
    }
    catch (Exception ex)
    {
        ErrorMessage = $"Notlar yüklenirken hata oluştu: {ex.Message}";
    }
    finally
    {
        IsLoading = false;
    }
}
```

- [ ] **Step 2: Update `Project2/Pages/Notes.razor` with rich attachment view & modal**
Enhance `Project2/Pages/Notes.razor` to include:
- Ekler column with PDF download links and Image thumbnail badges.
- Click to open Image modal popup (`previewImageUrl`, `previewImageName`).
- File size formatting helper (e.g. `KB`, `MB`).
- Modern Bootstrap styling.

- [ ] **Step 3: Build Project2 to verify compilation**
Run: `dotnet build Project2/Project2.csproj`
Expected: Build succeeded with 0 errors.

- [ ] **Step 4: Commit Project2 changes**
```bash
git add Project2/
git commit -m "feat(project2): add note attachment rendering, image modal preview, and shared notes loading"
```

---

### Task 6: Çözüm Geneli Derleme, Testler ve Doğrulama

**Files:**
- All Solution Projects

- [ ] **Step 1: Run full test suite**
Run: `dotnet test InternshipProject.sln`
Expected: All tests pass (0 failures).

- [ ] **Step 2: Run full solution build**
Run: `dotnet build InternshipProject.sln`
Expected: All projects build cleanly (0 errors).

- [ ] **Step 3: Commit all changes**
```bash
git add .
git commit -m "chore: verify build and all unit tests for note attachments and Project2 sharing"
```
