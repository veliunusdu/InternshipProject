using DevExpress.ExpressApp.DC;

namespace Project1.Module.Models.Enums
{
    public enum MailDurumu
    {
        [XafDisplayName("Gönderilmedi / Bekliyor")]
        Gonderilmedi = 0,

        [XafDisplayName("Gönderildi")]
        Gonderildi = 1,

        [XafDisplayName("İletildi")]
        Iletildi = 2,

        [XafDisplayName("Okundu")]
        Okundu = 3,

        [XafDisplayName("İletilemedi / Hata")]
        Basarisiz = 4
    }
}
