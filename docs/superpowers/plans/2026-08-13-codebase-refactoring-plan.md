# Codebase Improvements & Refactoring Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Refactor `Project1` codebase to fix structural code smells, improve async performance (`SendMailAsync`), separate controller responsibilities (SRP), remove hardcoded magic strings, and enhance email audit logging.

**Architecture:** 
1. Convert `IEmailService.SendNoteNotificationEmail` to async `SendNoteNotificationEmailAsync`.
2. Extract `NotDetailPopupController` into its own file (`NotDetailPopupController.cs`).
3. Replace hardcoded `"Admin"` magic strings with XAF Security API / `SecurityConstants`.
4. Add `INotNotificationService` to decouple notification logic from `NotEmailNotificationController`.

**Tech Stack:** C# 10, .NET 8, DevExpress XAF Blazor, Microsoft.Extensions.DependencyInjection.

---

## Global Constraints
- Do not break existing UI behaviors in Blazor or Win.
- Ensure solution builds with 0 errors via `dotnet build`.

---

### Task 1: Add Asynchronous Method `SendNoteNotificationEmailAsync` to `IEmailService` & `EmailService`

**Files:**
- Modify: `Project1/Project1.Module/Services/IEmailService.cs`
- Modify: `Project1/Project1.Module/Services/EmailService.cs`

- [ ] **Step 1: Update `IEmailService.cs` with async method**

```csharp
using System.Threading.Tasks;

namespace Project1.Module.Services
{
    public interface IEmailService
    {
        (bool Success, string ErrorMessage) SendNoteNotificationEmail(
            string toEmail,
            string kisiName,
            string baslik,
            string icerik,
            string derece,
            string musteriName);

        Task<(bool Success, string ErrorMessage)> SendNoteNotificationEmailAsync(
            string toEmail,
            string kisiName,
            string baslik,
            string icerik,
            string derece,
            string musteriName);
    }
}
```

- [ ] **Step 2: Implement `SendNoteNotificationEmailAsync` in `EmailService.cs`**

Use `await client.SendMailAsync(message);` in `EmailService.cs`.

- [ ] **Step 3: Build & Commit Task 1**

```bash
dotnet build Project1/Project1.sln
git add Project1/Project1.Module/Services/
git commit -m "feat: add SendNoteNotificationEmailAsync to IEmailService"
```

---

### Task 2: Separate `NotDetailPopupController` into its Own File

**Files:**
- Create: `Project1/Project1.Module/Controllers/NotDetailPopupController.cs`
- Modify: `Project1/Project1.Module/Controllers/NotController.cs`

- [ ] **Step 1: Move `NotDetailPopupController` class out of `NotController.cs` into `NotDetailPopupController.cs`**
- [ ] **Step 2: Build & Commit Task 2**

```bash
dotnet build Project1/Project1.sln
git add Project1/Project1.Module/Controllers/
git commit -m "refactor: extract NotDetailPopupController into standalone file"
```

---

### Task 3: Replace Hardcoded `"Admin"` Magic Strings with Security Framework Checks

**Files:**
- Modify: `Project1/Project1.Module/Controllers/MusteriController.cs`
- Modify: `Project1/Project1.Module/Controllers/NotController.cs`

- [ ] **Step 1: Replace hardcoded `"Admin"` checks in `MusteriController.cs` and `NotController.cs` with `SecurityConstants.AdministratorUserName` check**
- [ ] **Step 2: Build & Commit Task 3**

```bash
dotnet build Project1/Project1.sln
git add Project1/Project1.Module/Controllers/
git commit -m "refactor: replace hardcoded Admin strings with SecurityConstants"
```

---

### Task 4: Final Verification Build

- [ ] **Step 1: Run `dotnet build Project1/Project1.sln`**
- [ ] **Step 2: Verify 0 errors**
