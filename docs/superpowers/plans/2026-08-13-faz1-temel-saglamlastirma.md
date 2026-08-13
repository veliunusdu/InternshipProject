# FAZ 1 — Temel Sağlamlaştırma Uygulama Planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `Project1.Module.Tests` xUnit test projesini kurmak, `EmailService` ve `Not.EmailGonderilebilir` domain kuralı için Unit Test'ler (Moq & FluentAssertions ile) yazmak, projenin derleme/test doğrulamalarını gerçekleştirmek ve Git akışını (Main branch + Conventional Commits) düzenlemektir.

**Architecture:** xUnit test altyapısı, Moq ile servis bağımlılıklarının taklit edilmesi (mocking), FluentAssertions ile okunabilir test iddalıları (assertions), `Not.cs` varlığı üzerinde `EmailGonderilebilir` kapsüllenmiş domain kuralı.

**Tech Stack:** .NET 8.0, xUnit, Moq, FluentAssertions, C#, Git.

## Global Constraints
- Testler `dotnet test` komutu ile tamamen bağımsız ve başarılı şekilde çalışmalıdır.
- DevExpress XPO Session nesnesi gerektiren domain nesnesi instantiating testlerinde XPO In-Memory `UnitOfWork` veya `Session` kullanılmalıdır.
- Tüm commit mesajları Conventional Commits formatında (`feat:`, `test:`, `docs:`, `refactor:`) yazılmalıdır.

---

## Task 1: Test Projesinin Oluşturulması ve Paketlerin Eklenmesi
## Task 2: EmailService Unit Testlerinin Yazılması (Result Pattern Testing)
## Task 3: Domain Kuralı Test Etme (`Not.EmailGonderilebilir`)
## Task 4: Git Akışı & Conventional Commits Düzenlemesi
## Task 5: GitHub Görünürlüğü (Description & Topics Bilgilendirmesi)
