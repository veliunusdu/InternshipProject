# Design Specification: Splash Screen & Home Dashboard Redesign

**Date:** 2026-08-10  
**Target Solution:** `Project1` (`Project1.Blazor.Server`, `Project1.Win`, `Project1.Module`)

---

## 1. Overview & Objectives
The goal of this redesign is to replace the generic and legacy splash screen and dashboard UI in `Project1` with a modern, high-aesthetic **Dark Glassmorphism UI design system**.

The update impacts both user frontends in `Project1`:
1. **Blazor Web Server** (`Project1.Blazor.Server`): Custom animated splash screen, responsive header greeting, real-time stat cards, dynamic recent activity feed (notes & contacts), and quick action tiles.
2. **WinForms Desktop** (`Project1.Win`): Redesigned dark-theme splash screen matching the solution branding.

---

## 2. Design System Tokens & Aesthetics

### Color Palette
- **Canvas / Background**: Slate Dark (`#0f172a`)
- **Card Background**: Semi-transparent dark slate (`rgba(30, 41, 59, 0.65)`) with `backdrop-filter: blur(20px)`
- **Card Borders**: Subtly visible glowing borders (`1px solid rgba(255, 255, 255, 0.1)`)
- **Typography Font**: `Inter`, system-ui, sans-serif

### Gradient Accents
- **Hero Header Banner**: `linear-gradient(135deg, #1e1b4b 0%, #312e81 50%, #4338ca 100%)`
- **Müşteriler Stat Card**: Emerald Gradient (`linear-gradient(135deg, #064e3b 0%, #059669 100%)`)
- **Notlar Stat Card**: Violet Gradient (`linear-gradient(135deg, #4c1d95 0%, #7c3aed 100%)`)
- **Kişiler Stat Card**: Rose Gradient (`linear-gradient(135deg, #831843 0%, #e11d48 100%)`)

---

## 3. Component Architecture & Specifications

### 3.1. Blazor Web Splash Screen & Host (`_Host.cshtml`, `site.css`)
- **Full Screen Overlay**: `.dx-splash-screen` styled with radial dark background (`#1e1b4b` to `#0f172a`).
- **Glass Floating Card**: `.dx-splash-screen-content` with rounded corners (`24px`), backdrop blur, ambient purple box-shadow, floating animation keyframes (`floatCard`).
- **Loading Ring**: Dual-colored continuous rotating spinner (`#6366f1` and `#a855f7`).

### 3.2. Blazor Web Home Dashboard (`Pages/Dashboard.razor`)
- **Header Banner**:
  - Greeting text ("İyi günler! / Hoş Geldiniz!") with date/time.
  - Active status pill ("Sistem Aktif") & Manual Refresh button.
- **Metrics Grid (3 Cards)**:
  - Kayıtlı Müşteri Count & shortcut.
  - Toplam Notlar Count & shortcut.
  - İletişim Kişileri Count & shortcut.
- **Recent Activity Feed**:
  - Queries `IObjectSpaceFactory` for latest 5 `Not` entries and latest 5 `Kisi` entries.
  - Renders responsive tables/cards with timestamp badges, titles, and author details.
- **Quick Shortcuts Tiles**:
  - Highlighting main navigation targets (`#Musteri_ListView`, `#Not_ListView`, `#Kisi_ListView`).

### 3.3. WinForms Desktop Splash Screen (`XafSplashScreen.cs`, `XafSplashScreen.Designer.cs`)
- **Background**: Replaced legacy `#FFFFFF` with Slate Dark (`#0f172a`).
- **Header Panel**: `#1e1b4b` background, white title typography.
- **Progress Bar**: Custom start and end colors (`#6366f1` to `#a855f7`).
- **Copyright & Status**: High contrast silver text (`#94a3b8`).

---

## 4. Verification & Testing Criteria
1. Build `Project1.sln` using `dotnet build` with zero errors.
2. Confirm `Project1.Blazor.Server` compiles and renders Dashboard with live entity data.
3. Confirm `Project1.Win` compiles with upgraded splash screen design properties.
