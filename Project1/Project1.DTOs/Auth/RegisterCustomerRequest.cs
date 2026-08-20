#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Project1.DTOs.Auth
{
    /// <summary>
    /// Müşteri portali üzerinden yeni müşteri kayıt isteği modeli.
    /// </summary>
    public class RegisterCustomerRequest
    {
        public RegisterCustomerRequest() { }

        public RegisterCustomerRequest(string musteriAdi, string userName, string email, string password, string telefon, string? adres = null)
        {
            MusteriAdi = musteriAdi;
            UserName = userName;
            Email = email;
            Password = password;
            Telefon = telefon;
            Adres = adres;
        }

        [Required(ErrorMessage = "Firma/Müşteri adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Firma adı en fazla 100 karakter olabilir.")]
        public string MusteriAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3 ile 50 karakter arasında olmalıdır.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        public string Telefon { get; set; } = string.Empty;

        public string? Adres { get; set; }
    }
}
