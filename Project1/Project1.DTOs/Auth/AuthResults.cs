#nullable enable
using System;

namespace Project1.DTOs.Auth
{
    /// <summary>
    /// Müşteri kayıt operasyonu sonucu.
    /// </summary>
    public record RegisterResult(
        bool Success,
        string? ErrorMessage = null,
        Guid? UserId = null
    )
    {
        public static RegisterResult Ok(Guid userId) => new(true, null, userId);
        public static RegisterResult Fail(string error) => new(false, error, null);
    }

    /// <summary>
    /// E-posta onay operasyonu sonucu.
    /// </summary>
    public record ConfirmEmailResult(
        bool Success,
        string? Message = null,
        string? ErrorMessage = null
    )
    {
        public static ConfirmEmailResult Ok(string message = "E-posta adresiniz başarıyla doğrulandı.") => new(true, message, null);
        public static ConfirmEmailResult Fail(string error) => new(false, null, error);
    }
}
