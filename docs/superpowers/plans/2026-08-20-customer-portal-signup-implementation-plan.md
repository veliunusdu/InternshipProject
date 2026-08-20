# Customer Portal, Sign-Up & Email Confirmation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Enable self-service Customer Portal registration with email confirmation via `IEmailService`, row-level security isolation (Customer role), and customer contact (`Kisi`) management.

**Architecture:** 
- `ApplicationUser` links to `Musteri` with email confirmation flags.
- `IAuthService` in `Business` handles customer registration, token generation, and account activation.
- `IEmailService` sends branded activation emails.
- XAF Security enforces row-level permissions for the `Customer` role (`[Musteri.Oid] = CurrentUser.Musteri.Oid`).
- Public Blazor pages (`/signup`, `/confirm-email`) provide clean self-service UI.

**Tech Stack:** .NET 8, DevExpress XAF Blazor Server, XPO ORM, C# 10 records, xUnit, Moq, FluentAssertions.

## Global Constraints
- Target Framework: `net8.0`.
- Preserve Clean Architecture: `Core` (contracts) -> `DTOs` -> `Module` (entities/security) -> `Mapping` -> `Business` (services) -> `Blazor.Server`.
- Zero compiler warnings or broken tests.

---

### Task 1: Core Contracts & DTOs for Auth and Email Confirmation

**Files:**
- Create: `Project1/Project1.Core/Services/Interfaces/IAuthService.cs`
- Modify: `Project1/Project1.Core/Services/Interfaces/IEmailService.cs`
- Create: `Project1/Project1.DTOs/Auth/RegisterCustomerRequest.cs`
- Create: `Project1/Project1.DTOs/Auth/AuthResults.cs`
- Test: `Project1/Project1.Module.Tests/DTOs/AuthDtoTests.cs`

**Interfaces:**
- Produces:
  - `IAuthService` { `RegisterCustomerAsync`, `ConfirmEmailAsync` }
  - `SendEmailConfirmationRequest(string ToEmail, string CustomerName, string ConfirmationUrl, DateTime ExpiryDate)`
  - `RegisterCustomerRequest(string MusteriAdi, string Email, string Password, string Telefon, string? Adres)`
  - `RegisterResult(bool Success, string? ErrorMessage, Guid? UserId)`
  - `ConfirmEmailResult(bool Success, string? ErrorMessage)`

- [ ] **Step 1: Write DTO unit tests**
- [ ] **Step 2: Update `IEmailService.cs` with `SendConfirmationEmailAsync`**
- [ ] **Step 3: Create `IAuthService.cs` in `Project1.Core`**
- [ ] **Step 4: Create DTOs in `Project1.DTOs/Auth/`**
- [ ] **Step 5: Verify build & tests pass**

---

### Task 2: Domain Entity Extensions & Security Roles

**Files:**
- Modify: `Project1/Project1.Module/Models/Security/ApplicationUser.cs`
- Modify: `Project1/Project1.Module/Security/SecurityConfig.cs`
- Modify: `Project1/Project1.Module/DatabaseUpdate/Updater.cs`
- Test: `Project1/Project1.Module.Tests/Security/CustomerRoleSecurityTests.cs`

**Interfaces:**
- Produces:
  - `ApplicationUser` properties: `Musteri Musteri`, `bool EmailConfirmed`, `string EmailConfirmationToken`, `DateTime? ConfirmationTokenExpiry`
  - `CustomerRoleConfigurator` (Row-level permissions for Customer role)
  - `Updater.cs` seeds `Customer` role and default customer user `customer_acme`

- [ ] **Step 1: Write security unit tests for Customer role isolation**
- [ ] **Step 2: Update `ApplicationUser.cs` with `Musteri` link and confirmation fields**
- [ ] **Step 3: Create `CustomerRoleConfigurator` in `SecurityConfig.cs`**
- [ ] **Step 4: Update `Updater.cs` to seed Customer role and permissions**
- [ ] **Step 5: Verify tests pass**

---

### Task 3: Email Service Confirmation Template & Logic

**Files:**
- Modify: `Project1/Project1.Business/Services/Implementations/EmailService.cs`
- Test: `Project1/Project1.Module.Tests/Services/EmailServiceConfirmationTests.cs`

**Interfaces:**
- Implements: `IEmailService.SendConfirmationEmailAsync`

- [ ] **Step 1: Write unit tests for `SendConfirmationEmailAsync`**
- [ ] **Step 2: Implement HTML confirmation email template in `EmailService.cs`**
- [ ] **Step 3: Verify tests pass**

---

### Task 4: Auth Service Implementation (Sign-Up & Token Verification)

**Files:**
- Create: `Project1/Project1.Business/Services/Implementations/AuthService.cs`
- Test: `Project1/Project1.Module.Tests/Services/AuthServiceTests.cs`

**Interfaces:**
- Implements: `IAuthService`

- [ ] **Step 1: Write unit tests for customer registration and token confirmation**
- [ ] **Step 2: Implement `AuthService.cs` with password hashing, user/musteri creation, token generation, and email trigger**
- [ ] **Step 3: Implement `ConfirmEmailAsync` with token validation, expiry check, and user activation**
- [ ] **Step 4: Verify tests pass**

---

### Task 5: Blazor UI (Sign-Up & Email Confirmation Pages)

**Files:**
- Create: `Project1/Project1.Blazor.Server/Pages/Auth/SignUp.razor`
- Create: `Project1/Project1.Blazor.Server/Pages/Auth/ConfirmEmail.razor`
- Create: `Project1/Project1.Module/Controllers/Customers/CustomerKisiController.cs`
- Modify: `Project1/Project1.Blazor.Server/Startup.cs`

**Interfaces:**
- Public endpoints: `/signup`, `/confirm-email`
- `CustomerKisiController`: Automatically assigns `Kisi.Musteri = CurrentUser.Musteri` when customer creates a new contact.

- [ ] **Step 1: Create `CustomerKisiController.cs` in `Project1.Module`**
- [ ] **Step 2: Create `SignUp.razor` in `Project1.Blazor.Server/Pages/Auth/`**
- [ ] **Step 3: Create `ConfirmEmail.razor` in `Project1.Blazor.Server/Pages/Auth/`**
- [ ] **Step 4: Register `IAuthService` in `Project1.Blazor.Server/Startup.cs`**
- [ ] **Step 5: Verify build succeeds**

---

### Task 6: End-to-End Verification & Test Suite

**Files:**
- Create: `Project1/Project1.Module.Tests/Integration/CustomerPortalIntegrationTests.cs`

- [ ] **Step 1: Write E2E integration tests**
  - Sign-up -> Confirm Email -> Login as Customer -> Add Contact -> Verify other customer cannot see data.
- [ ] **Step 2: Run all solution tests (`dotnet test InternshipProject.sln`)**
- [ ] **Step 3: Verify 100% PASS**
