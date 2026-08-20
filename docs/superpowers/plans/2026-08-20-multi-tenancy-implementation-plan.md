# Multi-Tenancy Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement row-level multi-tenancy on a shared database across all layers of the `InternshipProject` solution (`Core`, `DTOs`, `Module`, `Mapping`, `Business`, `Blazor.Server`, `Win`, and `Tests`).

**Architecture:** Shared database with row-level security. Entities inherit from `AuditedBaseObject` which implements `ITenantScoped` with an indexed `TenantId`. Users (`ApplicationUser`) belong to a `Tenant` (or `null` for Super Admin). Query filtering and object creation are automatically scoped via XAF Security criteria and `ICurrentTenantProvider`.

**Tech Stack:** .NET 8, DevExpress XAF (26.1.4) / XPO ORM, C# 10 records, xUnit, Moq, FluentAssertions.

## Global Constraints
- Target Framework: `net8.0` across all projects.
- Preserve existing Clean Architecture separation: `Core` (contracts) -> `DTOs` -> `Module` (entities) -> `Mapping` -> `Business` (services) -> `Hosts`.
- Do not break existing API contracts or unit tests.
- Every task must include compile and test verification.

---

### Task 1: Core Contracts & DTOs

**Files:**
- Create: `Project1/Project1.Core/MultiTenancy/ITenantScoped.cs`
- Create: `Project1/Project1.Core/MultiTenancy/ICurrentTenantProvider.cs`
- Create: `Project1/Project1.DTOs/Tenants/TenantDto.cs`
- Create: `Project1/Project1.DTOs/Tenants/TenantRequests.cs`
- Test: `Project1/Project1.Module.Tests/DTOs/TenantDtoTests.cs`

**Interfaces:**
- Produces:
  - `Project1.Core.MultiTenancy.ITenantScoped` { `Guid? TenantId { get; set; }` }
  - `Project1.Core.MultiTenancy.ICurrentTenantProvider` { `Guid? CurrentTenantId { get; }`, `string? CurrentTenantCode { get; }`, `bool IsSuperAdmin { get; }` }
  - `Project1.DTOs.Tenants.TenantDto(Guid Oid, string Name, string Code, bool IsActive, DateTime CreatedAt)`
  - `Project1.DTOs.Tenants.CreateTenantRequest(string Name, string Code)`
  - `Project1.DTOs.Tenants.UpdateTenantRequest(string Name, bool IsActive)`

- [ ] **Step 1: Write unit tests for Tenant DTOs and contracts**
Create `Project1/Project1.Module.Tests/DTOs/TenantDtoTests.cs` validating instantiation, record equality, and contract properties.

- [ ] **Step 2: Run test to verify it fails**
Run: `dotnet test Project1/Project1.Module.Tests/Project1.Module.Tests.csproj --filter "FullyQualifiedName~TenantDtoTests"`
Expected: FAIL (types not defined)

- [ ] **Step 3: Create `ITenantScoped` and `ICurrentTenantProvider` in `Project1.Core`**
Create `Project1/Project1.Core/MultiTenancy/ITenantScoped.cs` and `Project1/Project1.Core/MultiTenancy/ICurrentTenantProvider.cs`.

- [ ] **Step 4: Create `TenantDto` and request records in `Project1.DTOs`**
Create `Project1/Project1.DTOs/Tenants/TenantDto.cs` and `TenantRequests.cs`.

- [ ] **Step 5: Run tests and verify build**
Run: `dotnet test Project1/Project1.Module.Tests/Project1.Module.Tests.csproj --filter "FullyQualifiedName~TenantDtoTests"`
Expected: PASS

---

### Task 2: Domain Entity `Tenant`, `AuditedBaseObject` & `ApplicationUser` Multi-Tenant Updates

**Files:**
- Create: `Project1/Project1.Module/Models/Tenants/Tenant.cs`
- Modify: `Project1/Project1.Module/Models/Base/AuditedBaseObject.cs`
- Modify: `Project1/Project1.Module/Models/Security/ApplicationUser.cs`
- Test: `Project1/Project1.Module.Tests/Domain/TenantDomainTests.cs`

**Interfaces:**
- Consumes: `Project1.Core.MultiTenancy.ITenantScoped`
- Produces:
  - `Project1.Module.Models.Tenants.Tenant : BaseObject`
  - `AuditedBaseObject : BaseObject, ITenantScoped` (with `TenantId` property and automatic resolution on save)
  - `ApplicationUser` (with `Tenant Tenant` and `bool IsSuperAdmin`)

- [ ] **Step 1: Write domain tests for Tenant entity and AuditedBaseObject TenantId**
Create `Project1/Project1.Module.Tests/Domain/TenantDomainTests.cs` testing `Tenant` creation, `AuditedBaseObject.TenantId` assignment and inheritance on `Musteri`, `Kisi`, `Not`.

- [ ] **Step 2: Run test to verify it fails**
Run: `dotnet test Project1/Project1.Module.Tests/Project1.Module.Tests.csproj --filter "FullyQualifiedName~TenantDomainTests"`
Expected: FAIL

- [ ] **Step 3: Create `Tenant.cs` in `Project1.Module/Models/Tenants/`**
Implement `Tenant` class with `[Indexed(Unique = true)] string Code`, `string Name`, `bool IsActive`, `DateTime CreatedAt`.

- [ ] **Step 4: Update `AuditedBaseObject.cs`**
Implement `ITenantScoped`. Add `[Indexed] public Guid? TenantId` property.

- [ ] **Step 5: Update `ApplicationUser.cs`**
Add `Tenant Tenant` association/property and `bool IsSuperAdmin` property.

- [ ] **Step 6: Run tests and verify PASS**
Run: `dotnet test Project1/Project1.Module.Tests/Project1.Module.Tests.csproj --filter "FullyQualifiedName~TenantDomainTests"`
Expected: PASS

---

### Task 3: Mapping Layer (`TenantMapper`)

**Files:**
- Create: `Project1/Project1.Mapping/Tenants/TenantMapper.cs`
- Test: `Project1/Project1.Module.Tests/Mapping/TenantMapperTests.cs`

**Interfaces:**
- Consumes: `IMapper<Tenant, TenantDto>` from `Project1.Core.Mapping`, `Tenant` from `Project1.Module`, `TenantDto` from `Project1.DTOs`
- Produces: `Project1.Mapping.Tenants.TenantMapper : IMapper<Tenant, TenantDto>`

- [ ] **Step 1: Write test for `TenantMapper`**
Create `Project1/Project1.Module.Tests/Mapping/TenantMapperTests.cs` verifying full mapping and null-argument checks.

- [ ] **Step 2: Run test to verify it fails**
Run: `dotnet test Project1/Project1.Module.Tests/Project1.Module.Tests.csproj --filter "FullyQualifiedName~TenantMapperTests"`
Expected: FAIL

- [ ] **Step 3: Implement `TenantMapper.cs`**
Create `Project1/Project1.Mapping/Tenants/TenantMapper.cs`.

- [ ] **Step 4: Run test and verify PASS**
Run: `dotnet test Project1/Project1.Module.Tests/Project1.Module.Tests.csproj --filter "FullyQualifiedName~TenantMapperTests"`
Expected: PASS

---

### Task 4: Business Services & CurrentTenantProvider

**Files:**
- Create: `Project1/Project1.Business/Services/Implementations/CurrentTenantProvider.cs`
- Modify: `Project1/Project1.Business/Services/Implementations/NoteService.cs`
- Test: `Project1/Project1.Module.Tests/Services/CurrentTenantProviderTests.cs`

**Interfaces:**
- Consumes: `ICurrentTenantProvider` from `Project1.Core.MultiTenancy`
- Produces:
  - `Project1.Business.Services.Implementations.CurrentTenantProvider : ICurrentTenantProvider`
  - Scoped queries in `NoteService` respecting `CurrentTenantProvider.CurrentTenantId`

- [ ] **Step 1: Write test for `CurrentTenantProvider` and `NoteService` multi-tenant filtering**
Create `Project1/Project1.Module.Tests/Services/CurrentTenantProviderTests.cs`.

- [ ] **Step 2: Implement `CurrentTenantProvider.cs` in `Project1.Business`**
Resolve current user's `Tenant.Oid` and `IsSuperAdmin` via `ISecurityStrategyBase` / `IPrincipal`.

- [ ] **Step 3: Update `NoteService.cs` to filter by `TenantId` for non-superadmin users**

- [ ] **Step 4: Run tests and verify PASS**
Run: `dotnet test Project1/Project1.Module.Tests/Project1.Module.Tests.csproj --filter "FullyQualifiedName~CurrentTenantProviderTests"`
Expected: PASS

---

### Task 5: Security Roles, Permissions & Seed Data

**Files:**
- Modify: `Project1/Project1.Module/Security/SecurityConfig.cs`
- Modify: `Project1/Project1.Module/DatabaseUpdate/Updater.cs`
- Test: `Project1/Project1.Module.Tests/Security/MultiTenantSecurityTests.cs`

**Interfaces:**
- Produces:
  - Super Administrator role & user (`superadmin`)
  - Tenant Administrator role (`Tenant Administrators`)
  - Standard User role (`Standard Users` with `[TenantId] = CurrentUser.Tenant.Oid` criteria)
  - Sample seed tenants (`ACME / Tenant A` and `GLOBAL / Tenant B`)

- [ ] **Step 1: Write security tests for multi-tenant role permissions**
Create `Project1/Project1.Module.Tests/Security/MultiTenantSecurityTests.cs`.

- [ ] **Step 2: Update `SecurityConfig.cs` and `Updater.cs`**
Configure roles and create default tenants and test users (`admin_a`, `user_a`, `admin_b`, `user_b`, `superadmin`).

- [ ] **Step 3: Run security tests and verify PASS**
Run: `dotnet test Project1/Project1.Module.Tests/Project1.Module.Tests.csproj --filter "FullyQualifiedName~MultiTenantSecurityTests"`
Expected: PASS

---

### Task 6: Presentation Host & DI Registration

**Files:**
- Modify: `Project1/Project1.Blazor.Server/Startup.cs`
- Modify: `Project1/Project1.Win/Startup.cs`

**Interfaces:**
- Registers: `ICurrentTenantProvider`, `TenantMapper`

- [ ] **Step 1: Register `ICurrentTenantProvider` and `TenantMapper` in `Project1.Blazor.Server/Startup.cs`**
- [ ] **Step 2: Register in `Project1.Win/Startup.cs`**
- [ ] **Step 3: Build entire solution**
Run: `dotnet build InternshipProject.sln`
Expected: 0 Errors

---

### Task 7: End-to-End Multi-Tenant Isolation Verification

**Files:**
- Create: `Project1/Project1.Module.Tests/Integration/MultiTenantIsolationTests.cs`

- [ ] **Step 1: Write end-to-end isolation tests**
Verify that user from Tenant A cannot see, read, or modify Tenant B's notes or customers, and Super Admin can see all.
- [ ] **Step 2: Run all solution tests**
Run: `dotnet test InternshipProject.sln`
Expected: 100% PASS across all test suites.
