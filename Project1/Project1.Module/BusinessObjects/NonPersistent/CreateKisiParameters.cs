#nullable enable

using System;
using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using Project1.Module.BusinessObjects.Customers;

namespace Project1.Module.BusinessObjects.NonPersistent
{
    [DomainComponent]
    public class CreateKisiParameters
    {
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;

        // Gizli tutulacak veya arka planda atanacak alanlar
        [Browsable(false)]
        public Musteri? Musteri { get; set; }
    }
}
