using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Project1.DTOs.Notes
{
    public class NoteDto
    {
        public Guid Oid { get; set; }
        public string Baslik { get; set; } = string.Empty;
        public string Icerik { get; set; } = string.Empty;
        public string Derece { get; set; } = string.Empty;
        public string Musteri { get; set; } = string.Empty;
        public string Kisi { get; set; } = string.Empty;
        public bool IsEmailSent { get; set; }
        public DateTime CreatedDate { get; set; }
        public string MailDurumu { get; set; } = "Gonderilmedi";
        public DateTime? MailGonderilmeTarihi { get; set; }
        public DateTime? MailIletilmeTarihi { get; set; }
        public DateTime? MailOkunmaTarihi { get; set; }
        public bool IsSharedWithProject2 { get; set; }
        public NoteAttachmentDto? Ek { get; set; }

        public NoteDto() { }

        public NoteDto(
            Guid Oid,
            string Baslik,
            string Icerik,
            string Derece,
            string Musteri,
            string Kisi,
            bool IsEmailSent,
            DateTime CreatedDate = default,
            string MailDurumu = "Gonderilmedi",
            DateTime? MailGonderilmeTarihi = null,
            DateTime? MailIletilmeTarihi = null,
            DateTime? MailOkunmaTarihi = null,
            bool IsSharedWithProject2 = false,
            NoteAttachmentDto? Ek = null)
        {
            this.Oid = Oid;
            this.Baslik = Baslik;
            this.Icerik = Icerik;
            this.Derece = Derece;
            this.Musteri = Musteri;
            this.Kisi = Kisi;
            this.IsEmailSent = IsEmailSent;
            this.CreatedDate = CreatedDate;
            this.MailDurumu = MailDurumu;
            this.MailGonderilmeTarihi = MailGonderilmeTarihi;
            this.MailIletilmeTarihi = MailIletilmeTarihi;
            this.MailOkunmaTarihi = MailOkunmaTarihi;
            this.IsSharedWithProject2 = IsSharedWithProject2;
            this.Ek = Ek;
        }
    }

    public class CreateNoteRequestDto
    {
        [Required(ErrorMessage = "Not başlığı zorunludur.")]
        public string Baslik { get; set; } = string.Empty;

        [Required(ErrorMessage = "Not içeriği zorunludur.")]
        public string Icerik { get; set; } = string.Empty;

        [Range(0, 2, ErrorMessage = "Not derecesi geçerli bir değer olmalıdır.")]
        public int Derece { get; set; }

        public Guid? MusteriOid { get; set; }
        public Guid? KisiOid { get; set; }
        public bool Project2IlePaylas { get; set; }

        public CreateNoteRequestDto() { }

        public CreateNoteRequestDto(
            string Baslik,
            string Icerik,
            int Derece,
            Guid? MusteriOid = null,
            Guid? KisiOid = null,
            bool Project2IlePaylas = false)
        {
            this.Baslik = Baslik;
            this.Icerik = Icerik;
            this.Derece = Derece;
            this.MusteriOid = MusteriOid;
            this.KisiOid = KisiOid;
            this.Project2IlePaylas = Project2IlePaylas;
        }
    }
}
