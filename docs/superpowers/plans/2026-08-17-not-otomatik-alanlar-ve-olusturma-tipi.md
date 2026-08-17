# Not Otomatik Alanlar ve Oluşturma Tipi Uygulama Planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `Not` nesnesinde Referans No, Nesne Tipi/ID ve Not Oluşturma Tipi alanlarının kullanıcı müdahalesine gerek kalmadan sistem tarafından otomatik atanmasını ve arayüzde salt-okunur/gizli olmasını sağlamak.

**Architecture:** XPO `AfterConstruction` / `OnSaving` yaşam döngüsüyle `ReferenceNo` ve `ReferenceBaseObjectType`/`ReferenceBaseObjectId` senkronizasyonu; MediatR CQRS ve Service katmanlarında `NotOlusturmaTipi` enum'ı üzerinden kanal tespiti.

**Tech Stack:** .NET 8, DevExpress XAF / XPO, MediatR, xUnit.

---

### Task 1: `NotOlusturmaTipi` Enum Tanımı

**Files:**
- Create: `Project1/Project1.Module/BusinessObjects/Enums/NotOlusturmaTipi.cs`

**Interfaces:**
- Produces: `enum NotOlusturmaTipi { Manuel = 0, MusteriPopup = 1, KisiPopup = 2, ApiSistem = 3 }`

- [ ] **Step 1: Write Enum definition**

```csharp
using DevExpress.ExpressApp.DC;

namespace Project1.Module.BusinessObjects.Enums
{
    public enum NotOlusturmaTipi
    {
        [XafDisplayName("Manuel")]
        Manuel = 0,

        [XafDisplayName("Müşteri Not Ekle")]
        MusteriPopup = 1,

        [XafDisplayName("Kişi Not Ekle")]
        KisiPopup = 2,

        [XafDisplayName("API / Sistem")]
        ApiSistem = 3
    }
}
```

- [ ] **Step 2: Build to verify enum compiles**

Run: `dotnet build Project1/Project1.Module/Project1.Module.csproj`
Expected: Build succeeded with 0 errors.

---

### Task 2: `Not` İş Nesnesinin Güncellenmesi (Referans No, Oluşturma Tipi ve Nesne Senkronizasyonu)

**Files:**
- Modify: `Project1/Project1.Module/BusinessObjects/Notes/Not.cs`

**Interfaces:**
- Consumes: `NotOlusturmaTipi`
- Produces: `Not.ReferenceNo` (auto-generated, read-only in UI), `Not.OlusturmaTipi` (read-only in UI), `SyncReferenceBaseObject()` (auto-sync `ReferenceBaseObjectType` and `ReferenceBaseObjectId`)

- [ ] **Step 1: Update `Not.cs` implementation**

Add `OlusturmaTipi`, auto-generation of `ReferenceNo` in `AfterConstruction`, `SyncReferenceBaseObject` helper, and attributes `[ReadOnly(true)]` and `[VisibleInDetailView(false)]`:

```csharp
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            CreatedDate = DateTime.Now;
            if (string.IsNullOrWhiteSpace(ReferenceNo))
            {
                ReferenceNo = $"NOT-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}";
            }
            OlusturmaTipi = NotOlusturmaTipi.Manuel;
        }

        protected override void OnSaving()
        {
            base.OnSaving();
            if (string.IsNullOrWhiteSpace(ReferenceNo))
            {
                ReferenceNo = $"NOT-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}";
            }
            SyncReferenceBaseObject();
        }

        public void SyncReferenceBaseObject()
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

        private NotOlusturmaTipi _olusturmaTipi;
        [XafDisplayName("Oluşturma Tipi")]
        [VisibleInListView(true)]
        [VisibleInDetailView(true)]
        [ReadOnly(true)]
        public NotOlusturmaTipi OlusturmaTipi
        {
            get => _olusturmaTipi;
            set => SetPropertyValue(nameof(OlusturmaTipi), ref _olusturmaTipi, value);
        }

        private string _referenceNo;
        [XafDisplayName("Referans No")]
        [VisibleInListView(true)]
        [VisibleInDetailView(true)]
        [ReadOnly(true)]
        public string ReferenceNo
        {
            get => _referenceNo;
            set => SetPropertyValue(nameof(ReferenceNo), ref _referenceNo, value);
        }

        private string _referenceBaseObjectType;
        [XafDisplayName("Referans Nesne Tipi")]
        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [Browsable(false)]
        public string ReferenceBaseObjectType
        {
            get => _referenceBaseObjectType;
            set => SetPropertyValue(nameof(ReferenceBaseObjectType), ref _referenceBaseObjectType, value);
        }

        private Guid? _referenceBaseObjectId;
        [XafDisplayName("Referans Nesne ID")]
        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [Browsable(false)]
        public Guid? ReferenceBaseObjectId
        {
            get => _referenceBaseObjectId;
            set => SetPropertyValue(nameof(ReferenceBaseObjectId), ref _referenceBaseObjectId, value);
        }
```

- [ ] **Step 2: Build to verify `Not.cs` compiles**

Run: `dotnet build Project1/Project1.Module/Project1.Module.csproj`
Expected: Build succeeded with 0 errors.

---

### Task 3: Core & DTO Katmanı Güncellemeleri

**Files:**
- Modify: `Project1/Project1.Core/Commands/CreateNoteCommand.cs`
- Modify: `Project1/Project1.DTOs/Notes/NoteDto.cs`

**Interfaces:**
- Produces: `CreateNoteCommand(..., int olusturmaTipi = 0)`, `CreateNoteRequestDto(..., int OlusturmaTipi = 3)`, `NoteDto(..., string OlusturmaTipi, string ReferenceNo)`

- [ ] **Step 1: Update `CreateNoteCommand.cs`**

```csharp
namespace Project1.Core.Commands
{
    public class CreateNoteCommand : IRequest<Guid>
    {
        public string Baslik { get; set; } = string.Empty;
        public string Icerik { get; set; } = string.Empty;
        public int Derece { get; set; }
        public Guid? MusteriId { get; set; }
        public Guid? KisiId { get; set; }
        public int OlusturmaTipi { get; set; }

        public CreateNoteCommand(string baslik, string icerik, int derece, Guid? musteriId = null, Guid? kisiId = null, int olusturmaTipi = 0)
        {
            Baslik = baslik;
            Icerik = icerik;
            Derece = derece;
            MusteriId = musteriId;
            KisiId = kisiId;
            OlusturmaTipi = olusturmaTipi;
        }
    }
}
```

- [ ] **Step 2: Update `NoteDto.cs`**

```csharp
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
        string OlusturmaTipi = "",
        string ReferenceNo = ""
    );

    public record CreateNoteRequestDto(
        string Baslik,
        string Icerik,
        int Derece,
        Guid? MusteriOid = null,
        Guid? KisiOid = null,
        int OlusturmaTipi = 3
    );
}
```

- [ ] **Step 3: Build to verify Core and DTOs**

Run: `dotnet build Project1/Project1.Core/Project1.Core.csproj; dotnet build Project1/Project1.DTOs/Project1.DTOs.csproj`
Expected: Build succeeded with 0 errors.

---

### Task 4: Handler ve Service Katmanı Güncellemeleri

**Files:**
- Modify: `Project1/Project1.Module/Handlers/CreateNoteCommandHandler.cs`
- Modify: `Project1/Project1.Module/Services/Implementations/NoteService.cs`

**Interfaces:**
- Consumes: `CreateNoteCommand`, `CreateNoteRequestDto`, `NotOlusturmaTipi`

- [ ] **Step 1: Update `CreateNoteCommandHandler.cs`**

```csharp
            Not yeniNot = objectSpace.CreateObject<Not>();
            yeniNot.Baslik = request.Baslik;
            yeniNot.Icerik = request.Icerik;
            yeniNot.Derece = (NotDerecesi)request.Derece;
            yeniNot.OlusturmaTipi = (NotOlusturmaTipi)request.OlusturmaTipi;

            if (request.MusteriId.HasValue)
            {
                yeniNot.Musteri = objectSpace.GetObjectByKey<Musteri>(request.MusteriId.Value);
            }

            if (request.KisiId.HasValue)
            {
                yeniNot.Kisi = objectSpace.GetObjectByKey<Kisi>(request.KisiId.Value);
            }

            yeniNot.SyncReferenceBaseObject();
```

- [ ] **Step 2: Update `NoteService.cs`**

Update `MapToDto` and `CreateNoteAsync`:
```csharp
        private NoteDto MapToDto(Not n)
        {
            return new NoteDto(
                n.Oid,
                n.Baslik ?? string.Empty,
                n.Icerik ?? string.Empty,
                n.Derece.ToString(),
                n.Musteri != null ? n.Musteri.Ad : string.Empty,
                n.Kisi != null ? (n.Kisi.Ad + " " + n.Kisi.Soyad).Trim() : string.Empty,
                n.IsEmailSent,
                n.OlusturmaTipi.ToString(),
                n.ReferenceNo ?? string.Empty
            );
        }

        public Task<NoteDto> CreateNoteAsync(CreateNoteRequestDto request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using IObjectSpace objectSpace = CreateObjectSpace();
            var not = objectSpace.CreateObject<Not>();
            not.Baslik = request.Baslik;
            not.Icerik = request.Icerik;
            not.Derece = (BusinessObjects.Enums.NotDerecesi)request.Derece;
            not.OlusturmaTipi = (BusinessObjects.Enums.NotOlusturmaTipi)request.OlusturmaTipi;

            if (request.MusteriOid.HasValue && request.MusteriOid.Value != Guid.Empty)
            {
                not.Musteri = objectSpace.GetObjectByKey<Musteri>(request.MusteriOid.Value);
            }

            if (request.KisiOid.HasValue && request.KisiOid.Value != Guid.Empty)
            {
                not.Kisi = objectSpace.GetObjectByKey<Kisi>(request.KisiOid.Value);
            }

            not.SyncReferenceBaseObject();
            objectSpace.CommitChanges();

            return Task.FromResult(MapToDto(not));
        }
```

- [ ] **Step 3: Build to verify Handlers and Service**

Run: `dotnet build Project1/Project1.Module/Project1.Module.csproj`
Expected: Build succeeded with 0 errors.

---

### Task 5: UI Controller Güncellemeleri

**Files:**
- Modify: `Project1/Project1.Module/Controllers/Customers/KisiNotePopupController.cs`
- Modify: `Project1/Project1.Module/Controllers/Customers/MusteriPopupController.cs`
- Modify: `Project1/Project1.Module/Controllers/Customers/NestedListPopupController.cs`

- [ ] **Step 1: Update `KisiNotePopupController.cs`**

```csharp
            var command = new CreateNoteCommand(
                parameters.Baslik,
                parameters.Icerik,
                (int)parameters.Derece,
                parameters.Musteri?.Oid,
                parameters.Kisi?.Oid,
                olusturmaTipi: (int)NotOlusturmaTipi.KisiPopup);
```

- [ ] **Step 2: Update `MusteriPopupController.cs`**

```csharp
            var command = new CreateNoteCommand(
                parameters.Baslik, 
                parameters.Icerik, 
                (int)parameters.Derece, 
                parameters.Musteri?.Oid, 
                parameters.Kisi?.Oid,
                olusturmaTipi: (int)NotOlusturmaTipi.MusteriPopup);
```

- [ ] **Step 3: Update `NestedListPopupController.cs`**

```csharp
            else if (e.PopupWindowView.CurrentObject is CreateNoteParameters noteParams)
            {
                PropertyCollectionSource collectionSource = View.CollectionSource as PropertyCollectionSource;
                int olusturmaTipi = collectionSource?.MasterObject is Kisi 
                    ? (int)NotOlusturmaTipi.KisiPopup 
                    : (int)NotOlusturmaTipi.MusteriPopup;

                var command = new CreateNoteCommand(
                    noteParams.Baslik, 
                    noteParams.Icerik, 
                    (int)noteParams.Derece, 
                    noteParams.Musteri?.Oid, 
                    noteParams.Kisi?.Oid,
                    olusturmaTipi: olusturmaTipi);
                await mediator.Send(command);
            }
```

- [ ] **Step 4: Build to verify controllers**

Run: `dotnet build Project1/Project1.Module/Project1.Module.csproj`
Expected: Build succeeded with 0 errors.

---

### Task 6: Birim Testleri

**Files:**
- Create: `Project1/Project1.Module.Tests/BusinessObjects/NotAutomaticFieldsTests.cs`
- Modify: `Project1/Project1.Module.Tests/Services/NoteServiceTests.cs`

- [ ] **Step 1: Write `NotAutomaticFieldsTests.cs`**

```csharp
using System;
using System.Text.RegularExpressions;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using Project1.Module.BusinessObjects.Customers;
using Project1.Module.BusinessObjects.Enums;
using Project1.Module.BusinessObjects.Notes;
using Xunit;

namespace Project1.Module.Tests.BusinessObjects
{
    public class NotAutomaticFieldsTests
    {
        private readonly IObjectSpace _objectSpace;

        public NotAutomaticFieldsTests()
        {
            var dataStore = new DevExpress.Xpo.DB.InMemoryDataStore();
            var dataLayer = new SimpleDataLayer(dataStore);
            _objectSpace = new XPObjectSpace(new TypesInfo(), new XpoTypeInfoSource((TypesInfo)XafTypesInfo.Instance), () => new UnitOfWork(dataLayer));
        }

        [Fact]
        public void Not_AfterConstruction_ShouldGenerateReferenceNo_WithExpectedFormat()
        {
            var not = _objectSpace.CreateObject<Not>();

            Assert.NotNull(not.ReferenceNo);
            Assert.Matches(@"^NOT-\d{8}-[A-Z0-9]{4}$", not.ReferenceNo);
            Assert.Equal(NotOlusturmaTipi.Manuel, not.OlusturmaTipi);
        }

        [Fact]
        public void Not_SyncReferenceBaseObject_ShouldSetMusteri_WhenMusteriIsAssigned()
        {
            var not = _objectSpace.CreateObject<Not>();
            var musteri = _objectSpace.CreateObject<Musteri>();
            musteri.Ad = "Test Müşteri";

            not.Musteri = musteri;
            not.SyncReferenceBaseObject();

            Assert.Equal("Musteri", not.ReferenceBaseObjectType);
            Assert.Equal(musteri.Oid, not.ReferenceBaseObjectId);
        }

        [Fact]
        public void Not_SyncReferenceBaseObject_ShouldSetKisi_WhenOnlyKisiIsAssigned()
        {
            var not = _objectSpace.CreateObject<Not>();
            var kisi = _objectSpace.CreateObject<Kisi>();
            kisi.Ad = "Ahmet";
            kisi.Soyad = "Yılmaz";

            not.Kisi = kisi;
            not.SyncReferenceBaseObject();

            Assert.Equal("Kisi", not.ReferenceBaseObjectType);
            Assert.Equal(kisi.Oid, not.ReferenceBaseObjectId);
        }
    }
}
```

- [ ] **Step 2: Update `NoteServiceTests.cs` to verify `OlusturmaTipi` and `ReferenceNo`**

- [ ] **Step 3: Run unit tests**

Run: `dotnet test Project1/Project1.Module.Tests/Project1.Module.Tests.csproj`
Expected: All tests pass.

---

### Task 7: Çözümün Derlenmesi ve Uçtan Uca Doğrulama

- [ ] **Step 1: Solution build**

Run: `dotnet build InternshipProject.sln`
Expected: Build succeeded with 0 errors, 0 warnings.

- [ ] **Step 2: Run all tests**

Run: `dotnet test InternshipProject.sln`
Expected: All test suites pass.
