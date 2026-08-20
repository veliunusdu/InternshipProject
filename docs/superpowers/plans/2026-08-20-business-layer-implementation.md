# Project1.Business Layer Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create a dedicated `.NET 8 Class Library` project (`Project1.Business`), move business services from `Project1.Module` into it, configure solution and project references, and ensure all tests build and pass cleanly.

**Architecture:** Separation of concerns following Clean Architecture principles. `Project1.Business` encapsulates business logic, service orchestrations, and validations by consuming `Project1.Core` (contracts), `Project1.DTOs` (models), `Project1.Mapping` (mappers), and `Project1.Module` (XPO entities & `IObjectSpace`).

**Tech Stack:** .NET 8, C#, DevExpress XAF (26.1.4), xUnit, Moq, FluentAssertions.

## Global Constraints
- Target Framework: `net8.0`
- ImplicitUsings: `enable`, Nullable: `enable`
- Zero circular dependencies
- All solution builds and tests must pass with 0 errors

---

### Task 1: Create `Project1.Business` Class Library

**Files:**
- Create: `Project1/Project1.Business/Project1.Business.csproj`

- [ ] **Step 1: Write Project1.Business.csproj**

Create `Project1/Project1.Business/Project1.Business.csproj` with references to `Project1.Core`, `Project1.DTOs`, `Project1.Mapping`, `Project1.Module`, and `DevExpress.ExpressApp`.

- [ ] **Step 2: Verify project restore**

Run: `dotnet restore Project1/Project1.Business/Project1.Business.csproj`
Expected: Restore succeeds.

---

### Task 2: Add Project to Solutions (`InternshipProject.sln` and `Project1.sln`)

**Files:**
- Modify: `InternshipProject.sln`
- Modify: `Project1/Project1.sln`

- [ ] **Step 1: Add project to InternshipProject.sln**

Run: `dotnet sln InternshipProject.sln add Project1/Project1.Business/Project1.Business.csproj`

- [ ] **Step 2: Add project to Project1.sln**

Run: `dotnet sln Project1/Project1.sln add Project1/Project1.Business/Project1.Business.csproj`

---

### Task 3: Migrate Services to `Project1.Business`

**Files:**
- Create: `Project1/Project1.Business/Services/Implementations/NoteService.cs`
- Create: `Project1/Project1.Business/Services/Implementations/EmailService.cs`
- Create: `Project1/Project1.Business/Services/Implementations/CrmNotificationService.cs`
- Create: `Project1/Project1.Business/Services/Implementations/SystemStatusService.cs`
- Delete: `Project1/Project1.Module/Services/Implementations/NoteService.cs`
- Delete: `Project1/Project1.Module/Services/Implementations/EmailService.cs`
- Delete: `Project1/Project1.Module/Services/Implementations/CrmNotificationService.cs`
- Delete: `Project1/Project1.Module/Services/Implementations/SystemStatusService.cs`

- [ ] **Step 1: Write services in Project1.Business**

Namespace will be `Project1.Business.Services.Implementations`.

- [ ] **Step 2: Remove old implementations from Project1.Module**

Delete the migrated service files from `Project1.Module` to prevent duplicate type declarations.

---

### Task 4: Update Consumer Projects (`Blazor.Server` and `Module.Tests`)

**Files:**
- Modify: `Project1/Project1.Blazor.Server/Project1.Blazor.Server.csproj`
- Modify: `Project1/Project1.Blazor.Server/Startup.cs`
- Modify: `Project1/Project1.Module.Tests/Project1.Module.Tests.csproj`
- Modify: `Project1/Project1.Module.Tests/Services/NoteServiceTests.cs`
- Modify: `Project1/Project1.Module.Tests/Services/EmailServiceTests.cs`

- [ ] **Step 1: Add Project1.Business reference to Blazor.Server and Module.Tests**

Update csproj files to reference `..\Project1.Business\Project1.Business.csproj`.

- [ ] **Step 2: Update usings in Startup.cs and test files**

Add `using Project1.Business.Services.Implementations;` where needed.

---

### Task 5: Build and Verification

- [ ] **Step 1: Build entire solution**

Run: `dotnet build InternshipProject.sln`
Expected: Build succeeded with 0 errors.

- [ ] **Step 2: Run all unit tests**

Run: `dotnet test InternshipProject.sln`
Expected: All tests pass.

- [ ] **Step 3: Commit all changes**

Commit with descriptive message.
