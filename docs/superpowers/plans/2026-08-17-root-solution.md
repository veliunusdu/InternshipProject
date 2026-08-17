# Root Solution (InternshipProject.sln) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create a unified root solution `InternshipProject.sln` containing all 7 projects organized in Solution Folders, and update documentation and guides for seamless Visual Studio / IDE usage.

**Architecture:** A .NET Solution (.sln) file in the root workspace folder referencing all 7 .csproj files, structured using Solution Folders (`Project1/Core & Domain`, `Project1/UI & Presentation`, `Project1/Tests`, `Project2`).

**Tech Stack:** .NET 8.0 / 10.0 SDK (`dotnet sln` CLI), Visual Studio Solution format.

## Global Constraints
- Target root: `E:\users\internshipProject\`
- All 7 projects must build cleanly via `dotnet build InternshipProject.sln`.
- Preserve existing `Project1/Project1.sln` for backward compatibility.

---

### Task 1: Create Root Solution and Add All Projects

**Files:**
- Create: `InternshipProject.sln`

**Interfaces:**
- Consumes: `Project1/**/*.csproj`, `Project2/Project2.csproj`
- Produces: `InternshipProject.sln` with nested solution folders

- [ ] **Step 1: Create the empty solution file**

Run:
```powershell
dotnet new sln -n InternshipProject
```

- [ ] **Step 2: Add all 7 projects to the solution**

Run:
```powershell
dotnet sln InternshipProject.sln add Project1\Project1.Core\Project1.Core.csproj --solution-folder "Project1/Core & Domain"
dotnet sln InternshipProject.sln add Project1\Project1.DTOs\Project1.DTOs.csproj --solution-folder "Project1/Core & Domain"
dotnet sln InternshipProject.sln add Project1\Project1.Module\Project1.Module.csproj --solution-folder "Project1/Core & Domain"
dotnet sln InternshipProject.sln add Project1\Project1.Blazor.Server\Project1.Blazor.Server.csproj --solution-folder "Project1/UI & Presentation"
dotnet sln InternshipProject.sln add Project1\Project1.Win\Project1.Win.csproj --solution-folder "Project1/UI & Presentation"
dotnet sln InternshipProject.sln add Project1\Project1.Module.Tests\Project1.Module.Tests.csproj --solution-folder "Project1/Tests"
dotnet sln InternshipProject.sln add Project2\Project2.csproj --solution-folder "Project2"
```

- [ ] **Step 3: Verify the solution builds cleanly**

Run:
```powershell
dotnet build InternshipProject.sln
```
Expected: Build succeeded with 0 errors.

- [ ] **Step 4: Commit solution creation**

```bash
git add InternshipProject.sln
git commit -m "feat: add root InternshipProject.sln with categorized solution folders"
```

---

### Task 2: Update README.md with Solution and IDE Instructions

**Files:**
- Modify: `README.md`

**Interfaces:**
- Consumes: `InternshipProject.sln`
- Produces: Updated `README.md` containing build instructions and Visual Studio Solution View instructions

- [ ] **Step 1: Update README.md build instructions and add Visual Studio Solution guide**

Update `README.md` to reference `InternshipProject.sln` in `dotnet build` and provide guidance on how to open the project in Visual Studio.

- [ ] **Step 2: Commit README update**

```bash
git add README.md docs/
git commit -m "docs: update README with root solution build instructions and VS guide"
```
