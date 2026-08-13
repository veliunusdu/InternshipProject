using MediatR;
using System;

namespace Project1.Core.Commands
{
    public class CreateNoteCommand : IRequest<Guid>
    {
        public string Baslik { get; set; } = string.Empty;
        public string Icerik { get; set; } = string.Empty;
        
        // Using int for enum mapping since Core doesn't know about NotDerecesi
        public int Derece { get; set; } 

        public Guid? MusteriId { get; set; }
        public Guid? KisiId { get; set; }

        public CreateNoteCommand(string baslik, string icerik, int derece, Guid? musteriId = null, Guid? kisiId = null)
        {
            Baslik = baslik;
            Icerik = icerik;
            Derece = derece;
            MusteriId = musteriId;
            KisiId = kisiId;
        }
    }
}
