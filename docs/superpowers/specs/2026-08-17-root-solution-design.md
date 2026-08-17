# Design Spec: Root Solution Structure (InternshipProject.sln)

**Tarih:** 2026-08-17  
**Durum:** Onaylandı (Approved)  

## 1. Amaç ve Kapsam
Kullanıcının projeyi klasör görünümü (Folder View) yerine Visual Studio ve modern .NET geliştirme ortamlarında standart Çözüm (Solution) ve Proje hiyerarşisiyle açabilmesi için ana dizinde (`E:\users\internshipProject\`) birleşik bir `InternshipProject.sln` çözüm dosyası oluşturmak ve tüm alt projeleri mantıksal Çözüm Klasörleri (Solution Folders) altında düzenlemektir.

## 2. Mimari ve Çözüm Hiyerarşisi

### Çözüm Klasörleri Yapısı
`InternshipProject.sln` dosyası aşağıdaki 7 projeyi barındıracaktır:

```text
InternshipProject.sln
├── 📁 1. Project1
│   ├── 📁 Core & Domain
│   │   ├── Project1.Core (Project1/Project1.Core/Project1.Core.csproj)
│   │   ├── Project1.DTOs (Project1/Project1.DTOs/Project1.DTOs.csproj)
│   │   └── Project1.Module (Project1/Project1.Module/Project1.Module.csproj)
│   ├── 📁 UI & Presentation
│   │   ├── Project1.Blazor.Server (Project1/Project1.Blazor.Server/Project1.Blazor.Server.csproj)
│   │   └── Project1.Win (Project1/Project1.Win/Project1.Win.csproj)
│   └── 📁 Tests
│       └── Project1.Module.Tests (Project1/Project1.Module.Tests/Project1.Module.Tests.csproj)
└── 📁 2. Project2
    └── Project2 (Project2/Project2.csproj)
```

## 3. Yapılacak Değişiklikler

1. **Solution Oluşturma:**
   - `dotnet new sln -n InternshipProject` komutu ile kök dizinde boş `InternshipProject.sln` oluşturulması.
2. **Projelerin Eklenmesi:**
   - 7 projenin tamamının `dotnet sln InternshipProject.sln add ...` ile eklenmesi.
3. **Solution Klasörleri (Nested Projects) Yapılandırması:**
   - `.sln` dosyası içerisinde mantıksal organizasyonun (`Project1`, `Core & Domain`, `UI & Presentation`, `Tests`, `Project2`) tanımlanması.
4. **Dokümantasyon Güncellemesi:**
   - `README.md` dosyasında derleme talimatlarının `dotnet build InternshipProject.sln` olarak güncellenmesi.
   - Visual Studio'da Çözüm Görünümüne (Solution View) geçiş adımlarının dokümante edilmesi.

## 4. Doğrulama Kriterleri
- `dotnet build InternshipProject.sln` komutunun hatasız çalışması ve tüm projeleri derlemesi.
- Visual Studio / IDE üzerinde `.sln` açıldığında projelerin kategorize edilmiş klasörler altında listelenmesi.
