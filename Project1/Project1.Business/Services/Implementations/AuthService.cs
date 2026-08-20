#nullable enable
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using Microsoft.Extensions.Logging;
using Project1.Core.Services.Interfaces;
using Project1.DTOs.Auth;
using Project1.Module.BusinessObjects.Security;
using Project1.Module.Models.Customers;
using Project1.Module.Security;

namespace Project1.Business.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IObjectSpaceFactory _objectSpaceFactory;
        private readonly INonSecuredObjectSpaceFactory? _nonSecuredObjectSpaceFactory;
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor? _httpContextAccessor;
        private readonly ILogger<AuthService>? _logger;

        public AuthService(
            IObjectSpaceFactory objectSpaceFactory,
            IEmailService emailService,
            EmailSettings emailSettings,
            INonSecuredObjectSpaceFactory? nonSecuredObjectSpaceFactory = null,
            Microsoft.AspNetCore.Http.IHttpContextAccessor? httpContextAccessor = null,
            ILogger<AuthService>? logger = null)
        {
            _objectSpaceFactory = objectSpaceFactory ?? throw new ArgumentNullException(nameof(objectSpaceFactory));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
            _nonSecuredObjectSpaceFactory = nonSecuredObjectSpaceFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private IObjectSpace CreateObjectSpace()
        {
            if (_nonSecuredObjectSpaceFactory != null)
            {
                return _nonSecuredObjectSpaceFactory.CreateNonSecuredObjectSpace(typeof(ApplicationUser));
            }

            return _objectSpaceFactory.CreateObjectSpace(typeof(ApplicationUser));
        }

        public async Task<RegisterResult> RegisterCustomerAsync(
            RegisterCustomerRequest request, 
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var objectSpace = CreateObjectSpace();

                // 1. Kullanıcı adı ve E-posta adresi kontrolü
                string trimmedUserName = request.UserName.Trim();
                string trimmedEmail = request.Email.Trim();

                var existingUserName = objectSpace.FirstOrDefault<ApplicationUser>(u => u.UserName == trimmedUserName);
                if (existingUserName != null)
                {
                    _logger?.LogWarning("Kayıt başarısız: {UserName} kullanıcı adı zaten kullanımda.", trimmedUserName);
                    return RegisterResult.Fail("Bu kullanıcı adı zaten kullanılmaktadır. Lütfen farklı bir kullanıcı adı seçiniz.");
                }

                var existingEmail = objectSpace.FirstOrDefault<ApplicationUser>(u => u.Email == trimmedEmail || u.UserName == trimmedEmail);
                if (existingEmail != null)
                {
                    _logger?.LogWarning("Kayıt başarısız: {Email} e-posta adresi zaten kayıtlı.", trimmedEmail);
                    return RegisterResult.Fail("Bu e-posta adresi ile zaten bir kullanıcı hesabı bulunmaktadır.");
                }

                // 2. Yeni Müşteri (Firma) Oluştur
                var musteri = objectSpace.CreateObject<Musteri>();
                musteri.Ad = request.MusteriAdi.Trim();
                musteri.Telefon = request.Telefon.Trim();
                musteri.Adres = request.Adres?.Trim() ?? string.Empty;

                // 3. Yeni ApplicationUser Oluştur
                var user = objectSpace.CreateObject<ApplicationUser>();
                user.UserName = trimmedUserName;
                user.Email = trimmedEmail;
                user.Musteri = musteri;
                user.IsActive = false; // E-posta onaylanana kadar XAF Login kapalı
                user.EmailConfirmed = false;
                user.SetPassword(request.Password);

                // 4. Müşteri Rolünü Ata
                var customerRole = objectSpace.FirstOrDefault<PermissionPolicyRole>(r => r.Name == SecurityConstants.CustomerRoleName);
                if (customerRole != null)
                {
                    user.Roles.Add(customerRole);
                }

                // 5. Güvenli Token Üret (32-byte kriptografik rastgele dize)
                byte[] tokenBytes = RandomNumberGenerator.GetBytes(32);
                string token = Convert.ToHexString(tokenBytes);
                user.EmailConfirmationToken = token;
                user.ConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24);

                objectSpace.CommitChanges();
                _logger?.LogInformation("Yeni müşteri ve kullanıcı başarıyla kaydedildi. User: {Email}, Musteri: {Musteri}", user.UserName, musteri.Ad);

                // 6. Aktivasyon E-postası Gönder (Dinamik Host Çözümleme)
                string baseUrl = _emailSettings.BaseUrl?.TrimEnd('/') ?? "http://localhost:5000";
                if (_httpContextAccessor?.HttpContext?.Request != null)
                {
                    var req = _httpContextAccessor.HttpContext.Request;
                    string scheme = req.Scheme;
                    string host = req.Host.Value;
                    if (!string.IsNullOrWhiteSpace(scheme) && !string.IsNullOrWhiteSpace(host))
                    {
                        baseUrl = $"{scheme}://{host}";
                    }
                }

                string confirmationUrl = $"{baseUrl}/confirm-email?userId={user.Oid}&token={token}";

                var emailResult = await _emailService.SendConfirmationEmailAsync(new SendEmailConfirmationRequest(
                    ToEmail: user.Email,
                    CustomerName: musteri.Ad,
                    ConfirmationUrl: confirmationUrl,
                    ExpiryDate: user.ConfirmationTokenExpiry.Value
                ), cancellationToken).ConfigureAwait(false);

                if (!emailResult.Success)
                {
                    _logger?.LogWarning("Kayıt oluşturuldu ancak e-posta gönderilemedi: {Error}", emailResult.ErrorMessage);
                }

                return RegisterResult.Ok(user.Oid);
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("Müşteri kayıt işlemi iptal edildi.");
                return RegisterResult.Fail("İşlem iptal edildi.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Müşteri kayıt sırasında beklenmedik hata oluştu.");
                return RegisterResult.Fail($"Kayıt oluşturulamadı: {ex.Message}");
            }
        }

        public async Task<ConfirmEmailResult> ConfirmEmailAsync(
            Guid userId, 
            string token, 
            CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty || string.IsNullOrWhiteSpace(token))
            {
                return ConfirmEmailResult.Fail("Geçersiz veya eksik doğrulama parametreleri.");
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();

                using var objectSpace = CreateObjectSpace();

                var user = objectSpace.GetObjectByKey<ApplicationUser>(userId);
                if (user == null)
                {
                    _logger?.LogWarning("E-posta onayı başarısız: Kullanıcı bulunamadı ({UserId}).", userId);
                    return ConfirmEmailResult.Fail("Kullanıcı hesabı bulunamadı.");
                }

                if (user.EmailConfirmed && user.IsActive)
                {
                    return ConfirmEmailResult.Ok("E-posta adresiniz zaten daha önce doğrulanmış. Giriş yapabilirsiniz.");
                }

                if (string.IsNullOrWhiteSpace(user.EmailConfirmationToken) || user.EmailConfirmationToken != token)
                {
                    _logger?.LogWarning("E-posta onayı başarısız: Geçersiz token ({UserId}).", userId);
                    return ConfirmEmailResult.Fail("Doğrulama anahtarı (token) geçersiz veya hatalı.");
                }

                if (user.ConfirmationTokenExpiry.HasValue && DateTime.UtcNow > user.ConfirmationTokenExpiry.Value)
                {
                    _logger?.LogWarning("E-posta onayı başarısız: Token süresi dolmuş ({UserId}).", userId);
                    return ConfirmEmailResult.Fail("Doğrulama linkinin süresi dolmuş (24 saat). Lütfen yeni bir kayıt veya aktivasyon talep edin.");
                }

                // Hesabı aktifleştir
                user.EmailConfirmed = true;
                user.IsActive = true;
                user.EmailConfirmationToken = null;
                user.ConfirmationTokenExpiry = null;

                objectSpace.CommitChanges();
                _logger?.LogInformation("Kullanıcı e-posta adresi doğrulandı ve hesap aktifleştirildi: {Email}", user.Email);

                return ConfirmEmailResult.Ok("E-posta adresiniz başarıyla doğrulandı! Artık Müşteri Portalına giriş yapabilirsiniz.");
            }
            catch (OperationCanceledException)
            {
                return ConfirmEmailResult.Fail("İşlem iptal edildi.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "E-posta doğrulama sırasında beklenmedik hata oluştu.");
                return ConfirmEmailResult.Fail($"Doğrulama gerçekleştirilemedi: {ex.Message}");
            }
        }
    }
}
