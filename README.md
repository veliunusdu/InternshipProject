# StajProject / InternshipProject

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![DevExpress XAF](https://img.shields.io/badge/DevExpress-XAF-FF7200?style=flat-square)
![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-blue?style=flat-square)
![Testing](https://img.shields.io/badge/Testing-xUnit%20%7C%20FluentAssertions%20%7C%20Moq-brightgreen?style=flat-square)

DevExpress eXpressApp Framework (XAF) tabanlı, modern **Blazor Server (Web UI)** ve **WinForms (Desktop UI)** destekli müşteri ve not yönetim sistemi.

---

## 🏗️ Çözüm (Solution) ve Proje Yapısı

Tüm projeler ana dizindeki **`InternshipProject.sln`** çözüm dosyası altında mantıksal Çözüm Klasörleri (Solution Folders) ile organize edilmiştir:

```text
InternshipProject.sln (Root Solution)
├── 📁 1. Project1 (DevExpress XAF & Core Architecture)
│   ├── 📁 Core & Domain
│   │   ├── Project1.Core              # Temel domain mantığı
│   │   ├── Project1.DTOs              # Veri transfer nesneleri & API modelleri
│   │   └── Project1.Module            # DevExpress XAF İş Nesneleri, Denetleyiciler, Servisler
│   ├── 📁 UI & Presentation
│   │   ├── Project1.Blazor.Server     # XAF Blazor Server Web UI
│   │   └── Project1.Win               # XAF WinForms Masaüstü UI
│   └── 📁 Tests
│       └── Project1.Module.Tests      # xUnit & Moq birim testleri
│
└── 📁 2. Project2 (Blazor Web UI)
    └── Project2                       # Standalone Blazor Web Arayüzü
```

---

## 💻 Visual Studio'da Çözüm (Solution) Olarak Açma

Visual Studio'da projeyi klasör görünümü yerine standart Çözüm/Proje ağacıyla açmak için:

1. **Doğrudan Solution Açma:** Visual Studio açılış ekranında **"Open a project or solution"** seçeneğini seçin ve ana dizindeki `InternshipProject.sln` dosyasını açın (veya dosya yöneticisinden `InternshipProject.sln` dosyasına çift tıklayın).
2. **Klasör Görünümünden Çözüme Geçiş:** Eğer proje klasör olarak açılmışsa, **Solution Explorer (Çözüm Gezgini)** penceresinin üst araç çubuğundaki **"Switch Views" (Görünümleri Değiştir)** butonuna tıklayıp `InternshipProject.sln` seçeneğini işaretleyin.

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

### 2. Tüm Çözümü Derleyin
```bash
dotnet build InternshipProject.sln
```

### 3. Testleri Çalıştırın
```bash
dotnet test InternshipProject.sln
```

### 4. Blazor Server Web Uygulamasını Çalıştırın
```bash
dotnet run --project Project1/Project1.Blazor.Server/Project1.Blazor.Server.csproj
```
Uygulama varsayılan olarak `https://localhost:44318` adresinde açılacaktır.

### 5. Project2 Blazor Web Uygulamasını Çalıştırın
```bash
dotnet run --project Project2/Project2.csproj
```

### 6. E-posta Yapılandırması (Opsiyonel)
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
