#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Project1.DTOs.Auth;

namespace Project1.Core.Services.Interfaces
{
    /// <summary>
    /// Müşteri portali kayıt ve kimlik doğrulama servisi sözleşmesi.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Yeni bir müşteri ve bağlı kullanıcı hesabı oluşturup aktivasyon e-postası tetikler.
        /// </summary>
        Task<RegisterResult> RegisterCustomerAsync(
            RegisterCustomerRequest request, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// E-posta aktivasyon linkinden gelen token'ı doğrular ve kullanıcı hesabını aktifleştirir.
        /// </summary>
        Task<ConfirmEmailResult> ConfirmEmailAsync(
            Guid userId, 
            string token, 
            CancellationToken cancellationToken = default);
    }
}
