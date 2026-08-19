using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Project1.DTOs.Notes
{
    public record NoteDto(
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
        NoteAttachmentDto? Ek = null
    );

    public record CreateNoteRequestDto(
        [property: Required(ErrorMessage = "Not başlığı zorunludur.")]
        string Baslik,
        [property: Required(ErrorMessage = "Not içeriği zorunludur.")]
        string Icerik,
        [property: Range(0, 2, ErrorMessage = "Not derecesi geçerli bir değer olmalıdır.")]
        int Derece,
        Guid? MusteriOid = null,
        Guid? KisiOid = null,
        bool Project2IlePaylas = false
    );
}
