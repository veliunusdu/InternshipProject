# Authentication & Authorization Implementation Plan

**Status:** Ready for implementation
**Scope:** Seeded `Admin` and `User` accounts, with least-privilege access to customers, contacts and notes.

## Decisions

1. Retain XAF password authentication and cookies; no custom login page or identity provider.
2. Keep built-in `Administrators` with `IsAdministrative = true`.
3. Replace `Default User` / `AllowAllByDefault` with an explicit `Standard User` role.
4. Store initial passwords outside source control: User Secrets for development and environment variables or a secret store for deployment.
5. Standard users have CRUD rights for `Musteri`, `Kisi`, and `Not`; no user or role management.

## Planned Files

| File | Change |
| --- | --- |
| `Project1/Project1.Module/Security/SecurityConstants.cs` | Role names, seed usernames, configuration keys. |
| `Project1/Project1.Module/Security/StandardUserRoleConfigurator.cs` | Explicit CRUD permissions for business entities. |
| `Project1/Project1.Module/DatabaseUpdate/Updater.cs` | Idempotent and secure role/user seed and migration. |
| `Project1/Project1.Blazor.Server/appsettings.json` | Password-free `InitialUsers` configuration shape. |
| `Project1/Project1.Blazor.Server/Startup.cs` | Pass configuration safely to the update path. |
| `Project1/Project1.Module.Tests/Security/AuthorizationTests.cs` | Automated role and authorization tests. |

## Steps

### 1. Shared security code

Create `Project1.Module/Security` with:

- `SecurityConstants`: `Administrators`, `Standard User`, `Admin`, `User`, `InitialUsers:AdminPassword`, and `InitialUsers:UserPassword` constants.
- `StandardUserRoleConfigurator`: assigns only `Read`, `Write`, `Create`, and `Delete` permissions for `Musteri`, `Kisi`, and `Not`.
- No permissions are granted for `PermissionPolicyUser` or `PermissionPolicyRole`.

### 2. Refactor startup data

Refactor `DatabaseUpdate/Updater.cs` into private methods:

```text
EnsureAdministratorRole()
EnsureStandardUserRole()
EnsureUser(username, role, password)
ValidateInitialPassword(password, accountName)
```

- Create a user only when it is missing.
- Read its first password from protected configuration and reject empty values.
- Do not reset an existing password at every startup.
- Migrate existing `User` membership from `Default User` to `Standard User`.
- Retain the legacy role during the first release; do not delete it automatically.

### 3. Configuration

Add this shape to `appsettings.json`, with no real passwords:

```json
"InitialUsers": {
  "AdminPassword": "",
  "UserPassword": ""
}
```

In development, set the values with .NET User Secrets. In deployment, use the `InitialUsers__AdminPassword` and `InitialUsers__UserPassword` environment variables or a secret manager. Do not commit password values.

### 4. Client behavior

Verify XAF navigation suppresses unauthorized Users and Roles views for `Standard User`. If necessary, use a Model Difference or security-aware controller only as a UI complement; database permissions remain the enforcement mechanism.

### 5. Tests and acceptance checks

| Scenario | Expected result |
| --- | --- |
| Admin login | Full administration access. |
| User login | Müşteri, Kişi, and Not views available. |
| User adds customer → contact → note | All records save with their relationships. |
| User opens a security view/action | Access denied. |
| Password is missing on fresh database | Clear setup error; no blank-password account. |
| Existing database update | Existing passwords persist; User receives Standard User role. |

Build with `dotnet build Project1/Project1.sln`; then run the new authorization test project and manually verify both Blazor and Windows clients against the same database.

## Rollout

1. Back up the target database.
2. Configure both non-empty initial passwords in the secret store.
3. Deploy and run the database updater once.
4. Verify Admin and User acceptance scenarios.
5. Delete the legacy `Default User` role only after confirming no other account uses it.

## Out of Scope

Self-registration, password reset email, MFA, external identity providers, per-user record ownership, and role-change auditing are separate future iterations.
