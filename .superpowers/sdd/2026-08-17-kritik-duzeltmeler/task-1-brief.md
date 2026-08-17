# Task 1: `EmailService` CancellationToken & Asenkron İptal Desteği

## Hedef
`EmailService.SendNoteNotificationEmailAsync` metoduna iletilen `CancellationToken`'ı kullanarak operasyon öncesi ve asenkron işlem sırasında kooperatif iptal (cooperative cancellation) mekanizmasını hayata geçirmek ve `OperationCanceledException` durumunu yönetmek.

## Dosyalar
- **Değiştirilecek Dosya:** `Project1/Project1.Module/Services/Implementations/EmailService.cs`
- **Test Dosyası:** `Project1/Project1.Module.Tests/Services/EmailServiceTests.cs`

## Gereksinimler & Adımlar
1. `EmailServiceTests.cs` içerisine `SendNoteNotificationEmailAsync_ShouldReturnFailure_WhenCancellationTokenIsCancelled` testini yazın:
   - Önceden iptal edilmiş bir `CancellationToken` (`cts.Cancel()`) ile çağrıldığında `EmailResult.Success` false olmalı ve `ErrorMessage` "iptal" içermelidir.
2. `EmailService.cs` içinde:
   - Metodun başında `cancellationToken.ThrowIfCancellationRequested()` çağrısı yapın.
   - `client.SendMailAsync(message, cancellationToken)` çağrısını kullanın.
   - `catch (OperationCanceledException)` bloğu ekleyerek `new EmailResult(false, "E-posta gönderim işlemi iptal edildi.")` döndürün.
3. Testleri çalıştırarak doğruluğu teyit edin: `dotnet test --filter FullyQualifiedName~EmailServiceTests`
4. Değişiklikleri commit edin:
   `git commit -m "feat(email): add CancellationToken and cooperative cancellation support to EmailService"`

## Rapor Dosyası
Raporunuzu `.superpowers/sdd/2026-08-17-kritik-duzeltmeler/task-1-report.md` dosyasına yazın.
