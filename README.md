# StajProject / InternshipProject

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![DevExpress XAF](https://img.shields.io/badge/DevExpress-XAF-FF7200?style=flat-square)
![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-blue?style=flat-square)

DevExpress eXpressApp Framework (XAF) tabanlı, modern **Blazor Server (Web UI)** ve **WinForms (Desktop UI)** destekli müşteri ve not yönetim sistemi.

---

## 🏗️ Proje Mimarisi ve Klasör Yapısı

Proje, resmi **DevExpress XAF Standartlarına** ve **Temiz Mimari (Clean Architecture)** ilkelerine göre modüler olarak yapılandırılmıştır:

```text
Project1/
├── Project1.Module/                     # 🧠 Çekirdek Modül (Core & Domain)
│   ├── BusinessObjects/                 # 📦 DevExpress XAF Entity Nesneleri
│   │   ├── Notes/                       # Not.cs (Müşteri & Kişi Bağlantılı)
│   │   ├── Customers/                   # Musteri.cs & Kisi.cs
│   │   ├── Security/                    # ApplicationUser.cs & UserEmailPermission.cs
│   │   └── Enums/                       # NotDerecesi.cs (Normal, Önemli, Acil)
│   │
│   ├── Controllers/                     # ⚙️ DevExpress XAF Denetleyicileri
│   │   ├── Notes/
│   │   │   ├── NoteNotificationController.cs # (Asenkron e-posta bildirimi)
│   │   │   └── NotePopupController.cs        # (Müşteri detayından "Not Ekle" popup'ı)
│   │   └── Navigation/
│   │       └── MenuSecurityController.cs     # (Admin harici menü kısıtlaması)
│   │
│   ├── Services/                        # 🔌 İş Mantığı & Servis Katmanı
│   │   ├── Interfaces/                  # IEmailService.cs & ISystemStatusService.cs
│   │   └── Implementations/             # EmailService.cs & SystemStatusService.cs
│   │
│   └── DatabaseUpdate/                  # 🛠️ Veritabanı Updater & Seed verileri
│
├── Project1.Blazor.Server/              # 🌐 Blazor Server Web Kullanıcı Arayüzü
│   ├── Pages/                           # Dashboard & AdminDashboard sayfaları
│   ├── Controllers/                     # REST API Endpoint'leri (/api/systemstatus)
│   └── Startup.cs                       # DI Container kayıtları
│
└── Project1.Win/                        # 🖥️ WinForms Masaüstü Kullanıcı Arayüzü
```

---

## ✨ Öne Çıkan Özellikler

* **Müşteri & İlgili Kişi Not Sistemi:** Notlar bir müşteriye ve ilgili kişiye bağlanır.
* **Asenkron E-posta Bildirim Servisi (`IEmailService`):**
  * Parameter Object (`SendNoteNotificationRequest`) kullanımı.
  * Hata yönetimi (`EmailResult`) ve non-blocking I/O (`SendMailAsync`).
  * Çifte gönderim önleme (Deduplication queue) mekanizması.
* **Detaylı Yetkilendirme & Menü Güvenliği:**
  * Yönetici (`Admin`) ve Standart Kullanıcı rolleri.
  * Kullanıcı bazlı "E-posta Gönderebilir" yetki kontrolü.
  * Standart kullanıcılar için yönetim menülerinin otomatik gizlenmesi.
* **Sistem Durumu REST API:** Diğer uygulamaların sorgulayabileceği `/api/systemstatus` endpoint'i.

---

## 🛠️ Kurulum ve Çalıştırma

### Gereksinimler
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* DevExpress Universal License (XAF Blazor & WinForms paketleri için)

### 1. Projeyi Klonlayın
```bash
git clone https://github.com/veliunusdu/InternshipProject.git
cd InternshipProject
```

### 2. Projeyi Derleyin
```bash
dotnet build Project1/Project1.sln
```

### 3. Blazor Server Web Uygulamasını Çalıştırın
```bash
dotnet run --project Project1/Project1.Blazor.Server/Project1.Blazor.Server.csproj
```
Uygulama varsayılan olarak `https://localhost:44318` adresinde açılacaktır.

### 4. E-posta Yapılandırması (Opsiyonel)
`appsettings.json` veya ortam değişkenleri (Environment Variables) üzerinden SMTP bilgilerinizi tanımlayabilirsiniz:
```json
"Email": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": 587,
  "EnableSsl": true,
  "SenderName": "Project1 Bildirim",
  "SenderEmail": "ornek@gmail.com",
  "SenderPassword": "app_password"
}
```

---

## 📄 Lisans
Bu proje staj eğitimi kapsamında geliştirilmiş bir referans uygulamadır.
