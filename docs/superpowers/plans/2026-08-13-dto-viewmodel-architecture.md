# DTO, ViewModel ve Interface Mimarisi Uygulama Planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Projeye Clean Architecture ve MVVM (Model-View-ViewModel) ilkelerine uygun olarak Interface (Arayüz), DTO (Data Transfer Object) ve ViewModel katmanlarını entegre etmek.

**Architecture:** 
1. `Project1.Module` katmanında `INoteService` sözleşmesi, `NoteDto` transfer nesnesi ve DevExpress `IObjectSpaceFactory` tabanlı `NoteService` gerçekleştirimi tanımlanacak.
2. `Project1.Blazor.Server` katmanında dış sistemlerin (Project2 gibi) Not verilerini DTO formatında alıp gönderebileceği `/api/notes` REST API Controller'ı oluşturulacak ve DI kaydı yapılacak.
3. `Project2` katmanında MVVM desenini uygulamak adına `NoteListViewModel` ve `Notes.razor` Blazor UI sayfası tanımlanacak.

**Tech Stack:** .NET 8.0, C#, DevExpress XAF (XPO ORM), ASP.NET Core Web API, Blazor Server (MVVM).

## Global Constraints
- C# `record` veya `class` veri transfer yapıları kullanılacaktır.
- Veritabanı domain entity'leri (`Not`, `Musteri`, `Kisi`) REST API katmanında dışarı açılmayacak, sadece DTO nesneleri döndürülecektir.
- Her adım sonrası `dotnet build` çalıştırılarak sıfır derleme hatası doğrulanacaktır.

---

## Task 1: Module Katmanına DTO, Interface ve Service Tanımlanması

**Files:**
- Create: `Project1/Project1.Module/DTOs/NoteDto.cs`
- Create: `Project1/Project1.Module/Services/Interfaces/INoteService.cs`
- Create: `Project1/Project1.Module/Services/Implementations/NoteService.cs`

**Interfaces:**
- Consumes: DevExpress `IObjectSpaceFactory`, `Not` entity
- Produces: `INoteService`, `NoteDto`, `CreateNoteRequestDto`

- [ ] **Step 1: `NoteDto.cs` dosyasını oluştur**
- [ ] **Step 2: `INoteService.cs` arayüz dosyasını oluştur**
- [ ] **Step 3: `NoteService.cs` servis sınıfını oluştur**
- [ ] **Step 4: Project1 çözümünü derle ve doğrula**
- [ ] **Step 5: Git Commit**

---

## Task 2: Project1.Blazor.Server Katmanında REST API Controller ve DI Kaydı

**Files:**
- Create: `Project1/Project1.Blazor.Server/Controllers/NotesApiController.cs`
- Modify: `Project1/Project1.Blazor.Server/Startup.cs`

**Interfaces:**
- Consumes: `INoteService`
- Produces: REST API Endpoint `/api/notes`

- [ ] **Step 1: `NotesApiController.cs` oluştur**
- [ ] **Step 2: `Startup.cs` dosyasına `INoteService` DI kaydı ekle**
- [ ] **Step 3: Project1 çözümünü derle ve doğrula**
- [ ] **Step 4: Git Commit**

---

## Task 3: Project2 İstemcisinde DTO, ViewModel ve Blazor UI Entegrasyonu

**Files:**
- Create: `Project2/DTOs/NoteDto.cs`
- Create: `Project2/ViewModels/NoteListViewModel.cs`
- Create: `Project2/Pages/Notes.razor`
- Modify: `Project2/Program.cs`
- Modify: `Project2/Shared/NavMenu.razor`

**Interfaces:**
- Consumes: `HttpClient`
- Produces: MVVM tabanlı `Notes.razor` UI sayfası

- [ ] **Step 1: `Project2/DTOs/NoteDto.cs` dosyasını oluştur**
- [ ] **Step 2: `Project2/ViewModels/NoteListViewModel.cs` ViewModel sınıfını oluştur**
- [ ] **Step 3: `Project2/Program.cs` içerisine ViewModel DI kaydı ekle**
- [ ] **Step 4: `Project2/Pages/Notes.razor` Blazor sayfasını oluştur**
- [ ] **Step 5: `Project2/Shared/NavMenu.razor` navigasyon menüsünü güncelle**
- [ ] **Step 6: Project2 projesini derle ve doğrula**
- [ ] **Step 7: Git Commit**
