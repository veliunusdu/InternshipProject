using MediatR;
using System;

namespace Project1.Core.Commands
{
    public class CreateKisiCommand : IRequest<Guid>
    {
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        
        public Guid? MusteriId { get; set; }

        public CreateKisiCommand(string ad, string soyad, string email, string telefon, Guid? musteriId = null)
        {
            Ad = ad;
            Soyad = soyad;
            Email = email;
            Telefon = telefon;
            MusteriId = musteriId;
        }
    }
}
