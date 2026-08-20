#nullable enable

using System;
using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using Project1.Core.Enums;
using Project1.Module.Models.Customers;

namespace Project1.Module.BusinessObjects.NonPersistent
{
    [DomainComponent]
    public class CreateNoteParameters
    {
        public string Baslik { get; set; } = string.Empty;
        public string Icerik { get; set; } = string.Empty;
        public NotDerecesi Derece { get; set; } = NotDerecesi.Normal;

        // Gizli tutulacak veya arka planda atanacak alanlar
        [Browsable(false)]
        public Musteri? Musteri { get; set; }
        [Browsable(false)]
        public Kisi? Kisi { get; set; }
    }
}
