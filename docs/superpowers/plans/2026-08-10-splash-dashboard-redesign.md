# Splash Screen & Home Dashboard Redesign Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Redesign the Splash Screen and Home Dashboard in `Project1.Blazor.Server` and `Project1.Win` with a modern dark glassmorphic UI system.

**Architecture:** Update `site.css` and `_Host.cshtml` for Blazor splash screen and glassmorphic base styles. Upgrade `Dashboard.razor` to show dynamic stats, recent activity tables (Notes & Contacts), and quick actions. Update `XafSplashScreen.cs` and `XafSplashScreen.Designer.cs` in `Project1.Win` for modern dark palette.

**Tech Stack:** C#, Blazor Server, DevExpress XAF .NET 8, WinForms, CSS3 (Glassmorphism, CSS Gradients, CSS Animations).

## Global Constraints

- Target Framework: net8.0
- Color System: Dark Slate (`#0f172a`), Card Translucent (`rgba(30, 41, 59, 0.65)`), Glass Blur (`20px`).
- Solution File: `C:\Users\veli.unusdu\source\repos\InternshipProject\Project1\Project1.sln`

---

### Task 1: Blazor CSS & Splash Screen Glassmorphism Enhancements

**Files:**
- Modify: `Project1.Blazor.Server/wwwroot/css/site.css`
- Modify: `Project1.Blazor.Server/Pages/_Host.cshtml`

- [ ] **Step 1: Enhance `site.css` with Glassmorphism and dark dashboard theme rules**

Update `.dx-splash-screen`, `.dx-splash-screen-content`, `.stat-card`, and add dynamic table and badge styles in `Project1/Project1.Blazor.Server/wwwroot/css/site.css`.

- [ ] **Step 2: Update `_Host.cshtml` branding meta tags and splash container parameters**

Verify `_Host.cshtml` links `site.css` properly and contains clean splash parameters.

- [ ] **Step 3: Test compilation of `Project1.Blazor.Server`**

Run: `dotnet build Project1.Blazor.Server/Project1.Blazor.Server.csproj`
Expected: 0 Errors.

---

### Task 2: Blazor Home Dashboard (`Dashboard.razor`) Redesign

**Files:**
- Modify: `Project1.Blazor.Server/Pages/Dashboard.razor`

- [ ] **Step 1: Implement dynamic data fetching and modern UI layout in `Dashboard.razor`**

Update `Dashboard.razor` with:
- Dynamic time-based greeting header banner with refresh button.
- 3 Stat cards (Kayıtlı Müşteri, Toplam Notlar, İletişim Kişileri) with hover animations and links.
- Recent Activity section querying top 5 `Not` entries and top 5 `Kisi` entries from `IObjectSpace`.
- Quick action grid.

- [ ] **Step 2: Test compilation of `Project1.Blazor.Server`**

Run: `dotnet build Project1.Blazor.Server/Project1.Blazor.Server.csproj`
Expected: 0 Errors.

---

### Task 3: WinForms Desktop Splash Screen (`XafSplashScreen`) Redesign

**Files:**
- Modify: `Project1.Win/XafSplashScreen.cs`
- Modify: `Project1.Win/XafSplashScreen.Designer.cs`

- [ ] **Step 1: Update Designer properties in `XafSplashScreen.Designer.cs`**

Change background colors from white/orange to dark slate (`#0f172a`) and deep indigo header (`#1e1b4b`). Update progress bar colors to indigo/violet (`#6366f1` / `#a855f7`).

- [ ] **Step 2: Update `XafSplashScreen.cs` code-behind styling**

Update `DrawContent` border styling and label typography colors.

- [ ] **Step 3: Test build of entire solution `Project1.sln`**

Run: `dotnet build Project1.sln`
Expected: 0 Errors.
