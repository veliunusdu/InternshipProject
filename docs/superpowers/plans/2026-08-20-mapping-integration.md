# Project1 Mapping Integration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Keep `Project1.Mapping` separate and make the live note service use strongly typed entity-to-DTO mappers without circular dependencies.

**Architecture:** The generic mapper contract lives in `Project1.Core`, which Module already references. Concrete XAF entity mappers live in `Project1.Mapping`; Blazor.Server is the composition root that binds contracts to implementations.

**Tech Stack:** .NET 8, C#, DevExpress XAF/XPO 26.1.4, Microsoft dependency injection, xUnit, Moq, FluentAssertions

**Spec:** `docs/superpowers/specs/2026-08-20-mapping-integration-design.md`

## Global Constraints

- `Project1.Mapping` remains a separate solution project.
- Never add a Module → Mapping project reference.
- Mapping only performs entity → DTO conversion and never persists XPO entities.
- Do not add AutoMapper, Mapster, Infrastructure, Application, or Domain projects.
- Preserve existing XAF models, API behavior, attachment URLs, and DTO values.
- Use tests first for mapper behavior and service integration.

---

### Task 1: Add the typed contract and entity mappers

**Files:**
- Create: `Project1/Project1.Core/Mapping/IMapper.cs`
- Create: `Project1/Project1.Mapping/Customers/MusteriMapper.cs`
- Create: `Project1/Project1.Mapping/Customers/KisiMapper.cs`
- Create: `Project1/Project1.Mapping/Notes/NoteMapper.cs`
- Create: `Project1/Project1.Module.Tests/Mapping/EntityMapperTests.cs`
- Modify: `Project1/Project1.Module.Tests/Project1.Module.Tests.csproj`
- Delete: `Project1/Project1.Mapping/Common/IObjectMapper.cs`
- Delete: `Project1/Project1.Mapping/Common/ObjectMapper.cs`
- Delete: `Project1/Project1.Mapping/Customers/CustomerMappingExtensions.cs`
- Delete: `Project1/Project1.Mapping/Customers/ContactMappingExtensions.cs`
- Delete: `Project1/Project1.Mapping/Notes/NoteMappingExtensions.cs`

**Interfaces:**
- Produces: `IMapper<in TSource, out TDestination>.Map(TSource source)`
- Produces: `NoteMapper : IMapper<Not, NoteDto>`
- Produces: `MusteriMapper : IMapper<Musteri, MusteriDto>`
- Produces: `KisiMapper : IMapper<Kisi, KisiDto>`

- [ ] **Step 1: Add the Mapping test reference and write failing mapper tests**

Add:

```xml
<ProjectReference Include="..\Project1.Mapping\Project1.Mapping.csproj" />
```

Create in-memory XPO entities and assert exact mapping behavior:

```csharp
mapper.Map(note).Baslik.Should().Be("Plan");
mapper.Map(note).Ek.Should().BeNull();
mapper.Map(musteri).Ad.Should().Be("Acme");
mapper.Map(kisi).MusteriOid.Should().Be(musteri.Oid);
Action act = () => mapper.Map(null!);
act.Should().Throw<ArgumentNullException>();
```

- [ ] **Step 2: Run the mapper tests and verify red**

Run: `dotnet test Project1\Project1.Module.Tests\Project1.Module.Tests.csproj --filter FullyQualifiedName~EntityMapperTests`

Expected: compilation fails because the typed contract and mapper classes do not exist.

- [ ] **Step 3: Add the typed mapper contract**

```csharp
namespace Project1.Core.Mapping;

public interface IMapper<in TSource, out TDestination>
{
    TDestination Map(TSource source);
}
```

- [ ] **Step 4: Implement the concrete mappers**

Every mapper begins with `ArgumentNullException.ThrowIfNull(source);`. `NoteMapper` reproduces every current `NoteService.MapToDto` value, including:

```csharp
var attachment = note.Dosya is null || string.IsNullOrEmpty(note.Dosya.FileName)
    ? null
    : new NoteAttachmentDto(
        note.Oid, note.DosyaAdi, note.ContentType, note.BoyutBytes,
        note.CreatedDate, $"/api/attachments/{note.Oid}/download",
        note.IsImage, note.IsPdf);
```

Customer and contact mappers use the existing DTO constructors and preserve empty-string defaults.

- [ ] **Step 5: Remove obsolete mapper files**

Delete the old generic mapper and extension files listed above. Do not replace `ToEntity`; XPO creation and relationship lookup remain in services or handlers.

- [ ] **Step 6: Run mapper tests and verify green**

Run: `dotnet test Project1\Project1.Module.Tests\Project1.Module.Tests.csproj --filter FullyQualifiedName~EntityMapperTests`

Expected: all mapper tests pass.

- [ ] **Step 7: Commit**

```powershell
git add Project1/Project1.Core/Mapping Project1/Project1.Mapping Project1/Project1.Module.Tests/Mapping Project1/Project1.Module.Tests/Project1.Module.Tests.csproj
git commit -m "refactor: add typed entity mappers"
```

---

### Task 2: Integrate NoteService with NoteMapper

**Files:**
- Modify: `Project1/Project1.Module/Services/Implementations/NoteService.cs`
- Modify: `Project1/Project1.Module.Tests/Services/NoteServiceTests.cs`

**Interfaces:**
- Consumes: `IMapper<Not, NoteDto>.Map(Not source)`
- Produces: `NoteService(IObjectSpaceFactory, IMapper<Not, NoteDto>, INonSecuredObjectSpaceFactory? = null)`

- [ ] **Step 1: Write a failing service integration test**

```csharp
var expected = new NoteDto { Baslik = "Mapper sonucu" };
var mapper = new Mock<IMapper<Not, NoteDto>>();
mapper.Setup(x => x.Map(It.IsAny<Not>())).Returns(expected);
var service = new NoteService(factory.Object, mapper.Object);
var result = await service.GetNoteByIdAsync(note.Oid);
result.Should().BeSameAs(expected);
mapper.Verify(x => x.Map(It.IsAny<Not>()), Times.Once);
```

- [ ] **Step 2: Run NoteService tests and verify red**

Run: `dotnet test Project1\Project1.Module.Tests\Project1.Module.Tests.csproj --filter FullyQualifiedName~NoteServiceTests`

Expected: compilation fails because NoteService does not yet accept a mapper.

- [ ] **Step 3: Inject and use the typed mapper**

Add `_noteMapper`, require it in the constructor, replace all `MapToDto` calls with `_noteMapper.Map`, and delete the private `MapToDto` method. Keep request-to-entity assignments and `CommitChanges()` unchanged.

- [ ] **Step 4: Update existing NoteService tests**

Pass `new NoteMapper()` to every normal service construction:

```csharp
var noteService = new NoteService(mockFactory.Object, new NoteMapper());
```

Preserve all existing assertions.

- [ ] **Step 5: Run all NoteService tests**

Run: `dotnet test Project1\Project1.Module.Tests\Project1.Module.Tests.csproj --filter FullyQualifiedName~NoteServiceTests`

Expected: all NoteService tests pass and the mocked mapper is called once.

- [ ] **Step 6: Commit**

```powershell
git add Project1/Project1.Module/Services/Implementations/NoteService.cs Project1/Project1.Module.Tests/Services/NoteServiceTests.cs
git commit -m "refactor: use typed mapper in note service"
```

---

### Task 3: Configure composition-root registrations

**Files:**
- Modify: `Project1/Project1.Mapping/Project1.Mapping.csproj`
- Modify: `Project1/Project1.Blazor.Server/Startup.cs`
- Delete: `Project1/Project1.Mapping/DependencyInjection.cs`

**Interfaces:**
- Consumes: mapper implementations from Task 1
- Produces: singleton DI registrations for all entity-to-DTO mappers

- [ ] **Step 1: Replace the old DI extension**

Remove `services.AddCrmMappingServices()` and explicitly register:

```csharp
services.AddSingleton<IMapper<Not, NoteDto>, NoteMapper>();
services.AddSingleton<IMapper<Musteri, MusteriDto>, MusteriMapper>();
services.AddSingleton<IMapper<Kisi, KisiDto>, KisiMapper>();
```

- [ ] **Step 2: Remove Mapping's ASP.NET Core dependency**

Delete `DependencyInjection.cs` and remove `<FrameworkReference Include="Microsoft.AspNetCore.App" />` from Mapping. Keep Core, DTOs, and Module project references.

- [ ] **Step 3: Restore**

Run: `dotnet restore Project1\Project1.sln`

Expected: restore completes and creates Mapping's assets file.

- [ ] **Step 4: Build and test**

Run:

```powershell
dotnet build Project1\Project1.sln --no-restore
dotnet test Project1\Project1.Module.Tests\Project1.Module.Tests.csproj --no-restore
```

Expected: zero build errors and all tests pass. Report existing DevExpress, XAF analyzer, or package-version warnings.

- [ ] **Step 5: Verify dependency direction and cleanup**

```powershell
rg -n "AddCrmMappingServices|IObjectMapper|MapToDto|ToEntity" Project1 --glob '!**/bin/**' --glob '!**/obj/**'
dotnet list Project1\Project1.Module\Project1.Module.csproj reference
```

Expected: no obsolete mapper APIs and no Module → Mapping reference.

- [ ] **Step 6: Commit**

```powershell
git add Project1/Project1.Mapping Project1/Project1.Blazor.Server/Startup.cs
git commit -m "refactor: integrate mapping composition root"
```

---

### Task 4: Verify and push

**Files:**
- Verify: all Mapping integration source, project, test, spec, and plan files

**Interfaces:**
- Consumes: Tasks 1–3
- Produces: a verified `master` pushed to `origin`

- [ ] **Step 1: Inspect the final diff**

```powershell
git status --short
git diff origin/master...HEAD --stat
git diff --check origin/master...HEAD
```

Expected: only Mapping integration and documentation changes; no whitespace errors.

- [ ] **Step 2: Run fresh verification**

```powershell
dotnet build Project1\Project1.sln --no-restore
dotnet test Project1\Project1.Module.Tests\Project1.Module.Tests.csproj --no-restore
```

Expected: zero build errors and zero failed tests.

- [ ] **Step 3: Push and verify the remote hash**

```powershell
git push origin master
git fetch origin
git rev-parse HEAD
git rev-parse origin/master
```

Expected: local and remote hashes are identical.
