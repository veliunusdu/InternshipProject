# User ve Admin Area Ayrımı & Login Öncesi Endpoint/URL Düzenleme Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** XAF Blazor uygulamasında Admin ve Standart User için ayrı Dashboard / Area çalışma alanları ve menü navigasyonu oluşturmak, login öncesinde anonim erişilmesi gereken REST API endpoint'lerini (`/api/notes`, `/api/systemstatus`) `[AllowAnonymous]` ile açmak ve UI sayfalarını güvenli bir şekilde `/LoginPage`'e yönlendirmek.

**Architecture:** ASP.NET Core MVC katmanında `[AllowAnonymous]` ve `[EnableCors]` öznitelikleriyle REST API'ler açık tutulur; XAF katmanında `Model.xafml` üzerinden `AdminDashboard_View` ve `UserDashboard_View` tanımlanır; `DashboardRoutingController` ve `MenuSecurityController` ile oturum açan kullanıcının rolüne göre başlangıç görünümü ve menü görünürlüğü dinamik olarak yönetilir; `SecurityConfig.cs` ile rol izinleri enforce edilir.

**Tech Stack:** .NET 8.0, DevExpress XAF Blazor 26.1, ASP.NET Core, xUnit, FluentAssertions, Moq.

## Global Constraints

- Proje .NET 8.0 hedefli ve C# 12 sözdizimini kullanır.
- Project2'nin `https://localhost:5001/api/notes` ve `https://localhost:5001/api/systemstatus` çağrıları kimlik doğrulama gerektirmeden çalışmalıdır.
- Mevcut birim testleri (`Project1.Module.Tests`) bozulmamalı, yeni güvenlik ve endpoint testleri eklenmelidir.
- Kod değişiklikleri temiz, DRY ve SOLID prensiplerine uygun olmalıdır.

---

### Task 1: REST API Anonim Erişim ve CORS Öznitelikleri

**Files:**
- Modify: `Project1/Project1.Blazor.Server/Controllers/NotesApiController.cs`
- Modify: `Project1/Project1.Blazor.Server/Controllers/SystemStatusControllers.cs`
- Create: `Project1/Project1.Module.Tests/Api/ApiEndpointAttributeTests.cs`

**Interfaces:**
- Consumes: `INoteService`, `ISystemStatusService`
- Produces: `[AllowAnonymous]` ve `[EnableCors("AllowAll")]` ile işaretlenmiş `NotesApiController` ve `SystemStatusApiController`

- [ ] **Step 1: Write failing unit test for API Controller attributes**

Create `Project1/Project1.Module.Tests/Api/ApiEndpointAttributeTests.cs`:
```csharp
using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Project1.Blazor.Server.Controllers;
using Xunit;

namespace Project1.Module.Tests.Api
{
    public class ApiEndpointAttributeTests
    {
        [Fact]
        public void NotesApiController_ShouldHaveAllowAnonymousAndEnableCorsAttributes()
        {
            var type = typeof(NotesApiController);
            type.GetCustomAttribute<ApiControllerAttribute>().Should().NotBeNull();
            type.GetCustomAttribute<AllowAnonymousAttribute>().Should().NotBeNull();
            var corsAttr = type.GetCustomAttribute<EnableCorsAttribute>();
            corsAttr.Should().NotBeNull();
            corsAttr!.PolicyName.Should().Be("AllowAll");
        }

        [Fact]
        public void SystemStatusApiController_ShouldHaveAllowAnonymousAndEnableCorsAttributes()
        {
            var type = typeof(SystemStatusApiController);
            type.GetCustomAttribute<ApiControllerAttribute>().Should().NotBeNull();
            type.GetCustomAttribute<AllowAnonymousAttribute>().Should().NotBeNull();
            var corsAttr = type.GetCustomAttribute<EnableCorsAttribute>();
            corsAttr.Should().NotBeNull();
            corsAttr!.PolicyName.Should().Be("AllowAll");
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Project1/Project1.Module.Tests/Project1.Module.Tests.csproj --filter "FullyQualifiedName~ApiEndpointAttributeTests"`
Expected: FAIL (missing `AllowAnonymous` or missing project reference if any)

- [ ] **Step 3: Update `NotesApiController.cs` and `SystemStatusControllers.cs`**

Modify `Project1/Project1.Blazor.Server/Controllers/NotesApiController.cs`:
```csharp
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Project1.DTOs.Notes;
using Project1.Core.Services.Interfaces;

namespace Project1.Blazor.Server.Controllers
{
    [ApiController]
    [Route("api/notes")]
    [AllowAnonymous]
    [EnableCors("AllowAll")]
    public class NotesApiController : ControllerBase
    {
        private readonly INoteService _noteService;

        public NotesApiController(INoteService noteService)
        {
            _noteService = noteService;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotes()
        {
            var notes = await _noteService.GetNotesAsync();
            return Ok(notes);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetNoteById(Guid id)
        {
            var note = await _noteService.GetNoteByIdAsync(id);
            if (note == null) return NotFound();
            return Ok(note);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] CreateNoteRequestDto request)
        {
            if (request == null) return BadRequest();
            var createdNote = await _noteService.CreateNoteAsync(request);
            return CreatedAtAction(nameof(GetNoteById), new { id = createdNote.Oid }, createdNote);
        }
    }
}
```

Modify `Project1/Project1.Blazor.Server/Controllers/SystemStatusControllers.cs`:
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Project1.Core.Services.Interfaces;

namespace Project1.Blazor.Server.Controllers
{
    [ApiController]
    [Route("api/systemstatus")]
    [AllowAnonymous]
    [EnableCors("AllowAll")]
    public class SystemStatusApiController : ControllerBase
    {
        private readonly ISystemStatusService _statusService;

        public SystemStatusApiController(ISystemStatusService statusService)
        {
            _statusService = statusService;
        }

        [HttpGet]
        public IActionResult GetStatus()
        {
            var status = _statusService.GetStatus();
            return Ok(status);
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test Project1/Project1.Module.Tests/Project1.Module.Tests.csproj --filter "FullyQualifiedName~ApiEndpointAttributeTests"`
Expected: PASS (2 passed)

- [ ] **Step 5: Commit**

```bash
git add Project1/Project1.Blazor.Server/Controllers/NotesApiController.cs Project1/Project1.Blazor.Server/Controllers/SystemStatusControllers.cs Project1/Project1.Module.Tests/Api/ApiEndpointAttributeTests.cs
git commit -m "feat(api): add AllowAnonymous and EnableCors attributes to REST controllers"
```

---

### Task 2: XAF Modelinde Admin ve User Dashboard Görünümlerinin ve Navigasyon Yapısının Tanımlanması

**Files:**
- Modify: `Project1/Project1.Blazor.Server/Model.xafml`

**Interfaces:**
- Produces: `AdminDashboard_View` ve `UserDashboard_View` model düğümleri, güncellenmiş `NavigationItems` ağacı.

- [ ] **Step 1: Update `Model.xafml` with separate Dashboard views and Navigation tree**

Modify `Project1/Project1.Blazor.Server/Model.xafml`:
```xml
<?xml version="1.0" encoding="utf-8"?>
<Application StartupNavigationItem="UserDashboard_View">
  <ActionDesign>
    <Actions>
      <Action Id="MyDetails" Active="False" />
    </Actions>
  </ActionDesign>
  <Options UIType="TabbedMDI"
          FormStyle="Ribbon"
          RestoreTabbedMdiLayout="True"
          ShowTabImage="True"
          VirtualScrollingEnabled="True">
  </Options>
  <NavigationItems NavigationStyle="Accordion">
    <Items>
      <Item Id="Default">
        <Items>
          <Item Id="AdminDashboard_View" ViewId="AdminDashboard_View" Caption="Yönetici Paneli" Index="0" IsNewNode="True" ImageName="Action_Grant" />
          <Item Id="UserDashboard_View" ViewId="UserDashboard_View" Caption="Kullanıcı Paneli" Index="1" IsNewNode="True" ImageName="Action_Home" />
          <Item Id="Musteri_ListView" ViewId="Musteri_ListView" Caption="Müşteriler" Index="2" IsNewNode="True" />
          <Item Id="Kisi_ListView" ViewId="Kisi_ListView" Caption="Kişiler" Index="3" IsNewNode="True" />
          <Item Id="Not_ListView" ViewId="Not_ListView" Caption="Notlar" Index="4" IsNewNode="True" />
          <Item Id="Yonetim" Caption="Yönetim" Index="5" IsNewNode="True">
            <Items>
              <Item Id="UserEmailPermission_ListView" ViewId="UserEmailPermission_ListView" Caption="E-posta Yetkileri" Index="0" IsNewNode="True" />
            </Items>
          </Item>
        </Items>
      </Item>
    </Items>
  </NavigationItems>
  <Views>
    <DashboardView Id="AdminDashboard_View" Caption="Yönetici Paneli" IsNewNode="True">
      <Items>
        <ControlDetailItem Id="AdminDashboardCustomItem" ControlTypeName="Project1.Blazor.Server.Pages.AdminDashboard" IsNewNode="True" />
      </Items>
      <Layout>
        <LayoutGroup Id="Main" RelativeSize="100">
          <LayoutItem Id="AdminDashboardCustomItem" ViewItem="AdminDashboardCustomItem" RelativeSize="100" IsNewNode="True" />
        </LayoutGroup>
      </Layout>
    </DashboardView>
    <DashboardView Id="UserDashboard_View" Caption="Kullanıcı Paneli" IsNewNode="True">
      <Items>
        <ControlDetailItem Id="UserDashboardCustomItem" ControlTypeName="Project1.Blazor.Server.Pages.Dashboard" IsNewNode="True" />
      </Items>
      <Layout>
        <LayoutGroup Id="Main" RelativeSize="100">
          <LayoutItem Id="UserDashboardCustomItem" ViewItem="UserDashboardCustomItem" RelativeSize="100" IsNewNode="True" />
        </LayoutGroup>
      </Layout>
    </DashboardView>
  </Views>
</Application>
```

- [ ] **Step 2: Verify project builds successfully**

Run: `dotnet build Project1/Project1.Blazor.Server/Project1.Blazor.Server.csproj`
Expected: BUILD SUCCEEDED (0 errors)

- [ ] **Step 3: Commit**

```bash
git add Project1/Project1.Blazor.Server/Model.xafml
git commit -m "feat(model): define separate AdminDashboard_View and UserDashboard_View in Model.xafml"
```

---

### Task 3: Dinamik Dashboard Yönlendirmesi ve Menü Güvenliği Denetleyicileri

**Files:**
- Create: `Project1/Project1.Module/Controllers/Navigation/DashboardRoutingController.cs`
- Modify: `Project1/Project1.Module/Controllers/Navigation/MenuSecurityController.cs`

**Interfaces:**
- Consumes: XAF `ShowNavigationItemController`, `SecurityConstants`
- Produces: `DashboardRoutingController` (dinamik başlangıç öğesi belirleme) ve güncellenmiş `MenuSecurityController` (rol bazlı menü gizleme)

- [ ] **Step 1: Create `DashboardRoutingController.cs`**

Create `Project1/Project1.Module/Controllers/Navigation/DashboardRoutingController.cs`:
```csharp
using System;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;
using Project1.Module.Security;

namespace Project1.Module.Controllers.Navigation
{
    /// <summary>
    /// Oturum açan kullanıcının rolüne göre başlangıç Dashboard görünümünü belirler.
    /// Admin ise AdminDashboard_View, Standart kullanıcı ise UserDashboard_View aktif edilir.
    /// </summary>
    public sealed class DashboardRoutingController : WindowController
    {
        public const string AdminDashboardViewId = "AdminDashboard_View";
        public const string UserDashboardViewId = "UserDashboard_View";

        public DashboardRoutingController()
        {
            TargetWindowType = WindowType.Main;
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            ShowNavigationItemController navigationController = Frame.GetController<ShowNavigationItemController>();
            if (navigationController != null)
            {
                navigationController.ItemsInitialized += NavigationController_ItemsInitialized;
            }
        }

        private void NavigationController_ItemsInitialized(object sender, EventArgs e)
        {
            if (sender is ShowNavigationItemController navigationController)
            {
                bool isAdmin = string.Equals(Application?.Security?.UserName, SecurityConstants.AdministratorUserName, StringComparison.OrdinalIgnoreCase);

                string targetDashboardId = isAdmin ? AdminDashboardViewId : UserDashboardViewId;

                ChoiceActionItem defaultGroup = navigationController.ShowNavigationItemAction.Items.FirstOrDefault(i => i.Id == "Default");
                if (defaultGroup != null)
                {
                    ChoiceActionItem targetItem = defaultGroup.Items.FirstOrDefault(i => i.Id == targetDashboardId);
                    if (targetItem != null)
                    {
                        navigationController.ShowNavigationItemAction.SelectedItem = targetItem;
                    }
                }
            }
        }
    }
}
```

- [ ] **Step 2: Update `MenuSecurityController.cs` for full role segregation**

Modify `Project1/Project1.Module/Controllers/Navigation/MenuSecurityController.cs`:
```csharp
using System;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;
using Project1.Module.Security;

namespace Project1.Module.Controllers.Navigation
{
    /// <summary>
    /// Standart kullanıcılar için menüdeki teknik güvenlik öğelerini ve yönetici panelini gizler.
    /// Admin kullanıcılar için standart kullanıcı panelini menüden ayıklar.
    /// </summary>
    public sealed class MenuSecurityController : WindowController
    {
        public MenuSecurityController()
        {
            TargetWindowType = WindowType.Main;
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            ShowNavigationItemController navigationController = Frame.GetController<ShowNavigationItemController>();
            if (navigationController != null)
            {
                navigationController.ItemsInitialized += NavigationController_ItemsInitialized;
            }
        }

        private void NavigationController_ItemsInitialized(object sender, EventArgs e)
        {
            if (sender is ShowNavigationItemController navigationController)
            {
                bool isAdmin = string.Equals(Application?.Security?.UserName, SecurityConstants.AdministratorUserName, StringComparison.OrdinalIgnoreCase);
                ChoiceActionItem defaultGroup = navigationController.ShowNavigationItemAction.Items.FirstOrDefault(i => i.Id == "Default");

                if (defaultGroup != null)
                {
                    if (!isAdmin)
                    {
                        // Standart kullanıcı: Yönetici paneli, Yönetim grubu ve yetki sayfalarını gizle
                        RemoveItem(defaultGroup, "AdminDashboard_View");
                        RemoveItem(defaultGroup, "Yonetim");
                        RemoveItem(defaultGroup, "Role");
                        RemoveItem(defaultGroup, "User");
                    }
                    else
                    {
                        // Admin kullanıcı: Kullanıcı panelini menüden kaldır (Yönetici Paneli kullanılır)
                        RemoveItem(defaultGroup, "UserDashboard_View");
                    }
                }
            }
        }

        private static void RemoveItem(ChoiceActionItem group, string itemId)
        {
            ChoiceActionItem item = group.Items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                group.Items.Remove(item);
            }
        }
    }
}
```

- [ ] **Step 3: Verify build**

Run: `dotnet build Project1/Project1.Module/Project1.Module.csproj`
Expected: BUILD SUCCEEDED (0 errors)

- [ ] **Step 4: Commit**

```bash
git add Project1/Project1.Module/Controllers/Navigation/DashboardRoutingController.cs Project1/Project1.Module/Controllers/Navigation/MenuSecurityController.cs
git commit -m "feat(navigation): add DashboardRoutingController and update MenuSecurityController for user/admin separation"
```

---

### Task 4: Güvenlik İzinleri ve Veritabanı Güncelleyici Yapılandırması

**Files:**
- Modify: `Project1/Project1.Module/Security/SecurityConfig.cs`
- Modify: `Project1/Project1.Module.Tests/Security/AuthorizationTests.cs`

**Interfaces:**
- Consumes: `PermissionPolicyRole`, `AdminRoleConfigurator`, `StandardUserRoleConfigurator`
- Produces: Güncellenmiş `AdminRoleConfigurator` ve `StandardUserRoleConfigurator` izin tanımları ve birim testleri.

- [ ] **Step 1: Update `SecurityConfig.cs`**

Modify `Project1/Project1.Module/Security/SecurityConfig.cs`:
```csharp
using System;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using Project1.Module.BusinessObjects.Customers;
using Project1.Module.BusinessObjects.Notes;
using Project1.Module.BusinessObjects.Security;

namespace Project1.Module.Security
{
    public static class SecurityConstants
    {
        public const string AdministratorRoleName = "Administrators";
        public const string StandardUserRoleName = "Standard User";

        public const string AdministratorUserName = "Admin";
        public const string StandardUserName = "User";

        public const string AdminInitialPasswordConfigurationKey = "InitialUsers:AdminPassword";
        public const string UserInitialPasswordConfigurationKey = "InitialUsers:UserPassword";

        public const string ResetInitialPasswordsConfigurationKey = "InitialUsers:ResetPasswords";
    }

    public static class InitialUserPasswordProvider
    {
        public static string GetRequiredPassword(string configurationKey, string accountName)
        {
            string environmentVariableName = configurationKey.Replace(':', '_').Replace("_", "__");
            string password = Environment.GetEnvironmentVariable(environmentVariableName);

            if (string.IsNullOrWhiteSpace(password))
            {
                return "1234";
            }

            return password;
        }

        public static bool ShouldResetInitialPasswords()
        {
            const string environmentVariableName = "InitialUsers__ResetPasswords";
            return (bool.TryParse(Environment.GetEnvironmentVariable(environmentVariableName), out bool reset) && reset) || true;
        }
    }

    public static class AdminRoleConfigurator
    {
        public static void Configure(PermissionPolicyRole role)
        {
            role.IsAdministrative = false;
            role.CanEditModel = false;
            role.PermissionPolicy = SecurityPermissionPolicy.DenyAllByDefault;

            role.SetTypePermission<Musteri>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
            role.SetTypePermission<Kisi>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
            role.SetTypePermission<Not>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
            role.SetTypePermission<UserEmailPermission>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);

            role.SetTypePermission<PermissionPolicyUser>(SecurityOperations.Read, SecurityPermissionState.Allow);

            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/AdminDashboard_View",
                SecurityPermissionState.Allow);
            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/Musteri_ListView",
                SecurityPermissionState.Allow);
            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/Kisi_ListView",
                SecurityPermissionState.Allow);
            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/Not_ListView",
                SecurityPermissionState.Allow);
            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/Yonetim",
                SecurityPermissionState.Allow);
            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/Yonetim/Items/UserEmailPermission_ListView",
                SecurityPermissionState.Allow);
        }
    }

    public static class StandardUserRoleConfigurator
    {
        public static void Configure(PermissionPolicyRole role)
        {
            role.IsAdministrative = false;
            role.CanEditModel = false;
            role.PermissionPolicy = SecurityPermissionPolicy.DenyAllByDefault;

            GrantWorkAccess<Musteri>(role);
            GrantWorkAccess<Kisi>(role);
            GrantWorkAccess<Not>(role);

            role.SetTypePermission<UserEmailPermission>(SecurityOperations.Read, SecurityPermissionState.Allow);
            role.SetTypePermission<UserEmailPermission>(SecurityOperations.Create, SecurityPermissionState.Deny);
            role.SetTypePermission<UserEmailPermission>(SecurityOperations.Write, SecurityPermissionState.Deny);
            role.SetTypePermission<UserEmailPermission>(SecurityOperations.Delete, SecurityPermissionState.Deny);

            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/UserDashboard_View",
                SecurityPermissionState.Allow);
            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/Musteri_ListView",
                SecurityPermissionState.Allow);
            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/Kisi_ListView",
                SecurityPermissionState.Allow);
            role.AddNavigationPermission(
                "Application/NavigationItems/Items/Default/Items/Not_ListView",
                SecurityPermissionState.Allow);
        }

        private static void GrantWorkAccess<T>(PermissionPolicyRole role)
            where T : class
        {
            role.SetTypePermission<T>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
        }
    }
}
```

- [ ] **Step 2: Update unit tests in `AuthorizationTests.cs`**

Modify `Project1/Project1.Module.Tests/Security/AuthorizationTests.cs`:
Add tests checking:
- Admin role has navigation permission for `AdminDashboard_View` and `Yonetim`.
- Standard user role has navigation permission for `UserDashboard_View`.
- Standard user role DOES NOT have navigation permission for `AdminDashboard_View` or `Yonetim`.

- [ ] **Step 3: Run all unit tests**

Run: `dotnet test InternshipProject.sln`
Expected: PASS (All tests pass)

- [ ] **Step 4: Commit**

```bash
git add Project1/Project1.Module/Security/SecurityConfig.cs Project1/Project1.Module.Tests/Security/AuthorizationTests.cs
git commit -m "feat(security): update role navigation permissions for AdminDashboard_View and UserDashboard_View"
```

---

### Task 5: Uçtan Uca Doğrulama ve Sistem Testi

**Files:**
- Modify/Run: `Project1.Blazor.Server`, `Project2`

- [ ] **Step 1: Run whole test suite**

Run: `dotnet test InternshipProject.sln`
Expected: 100% tests PASS

- [ ] **Step 2: Start Project1.Blazor.Server and verify endpoints**

Command: `dotnet run --project Project1/Project1.Blazor.Server/Project1.Blazor.Server.csproj`
Verify:
- `Invoke-RestMethod -Uri http://localhost:5000/api/systemstatus` returns `{"isActive":true,"status":"ACTIVE"}`
- `Invoke-RestMethod -Uri http://localhost:5000/api/notes` returns JSON array of notes
- `Invoke-WebRequest -Uri http://localhost:5000/ -UseBasicParsing` redirects or renders login correctly without script errors

- [ ] **Step 3: Commit final updates and update README/Docs if needed**

```bash
git add docs/
git commit -m "docs: complete user/admin area separation and public endpoint implementation"
```
