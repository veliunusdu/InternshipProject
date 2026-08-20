using MediatR;
using System;
using Project1.Core.Enums;

namespace Project1.Core.Commands
{
    public class CreateNoteCommand : IRequest<Guid>
    {
        public string Baslik { get; set; } = string.Empty;
        public string Icerik { get; set; } = string.Empty;
        public NotDerecesi Derece { get; set; } = NotDerecesi.Normal;

        public Guid? MusteriId { get; set; }
        public Guid? KisiId { get; set; }

        public CreateNoteCommand(string baslik, string icerik, NotDerecesi derece, Guid? musteriId = null, Guid? kisiId = null)
        {
            Baslik = baslik;
            Icerik = icerik;
            Derece = derece;
            MusteriId = musteriId;
            KisiId = kisiId;
        }
    }
}
