# Project1 Mapping Integration Design

## Amaç

`Project1.Mapping` ayrı bir class library olarak korunacak ve XAF/XPO entity nesnelerinin API DTO'larına dönüştürülmesi için tek merkez olacaktır. Entegrasyon, `Project1.Module` ile `Project1.Mapping` arasında döngüsel proje bağımlılığı oluşturmadan tamamlanacaktır.

## Mevcut Sorunlar

- `Project1.Mapping`, solution ve Blazor dependency injection yapılandırmasına eklenmiş olsa da uygulama akışında kullanılmamaktadır.
- `Project1.Module/Services/Implementations/NoteService.cs`, `Not` → `NoteDto` dönüşümünü kendi özel `MapToDto` metoduyla tekrar etmektedir.
- `Project1.Mapping` doğrudan `Project1.Module` projesine bağlıdır. Bu nedenle Module projesinin Mapping projesini referans alması döngüsel bağımlılık oluşturur.
- Mevcut `IObjectMapper`, desteklenen dönüşümleri çalışma zamanında `if` blokları ve tip kontrolleriyle belirlemektedir. Yeni bir mapper eklendiğinde merkezi sınıfın değiştirilmesi gerekmektedir.
- `CustomerMappingExtensions`, `ContactMappingExtensions` ve `NoteMappingExtensions` içindeki `ToEntity` metotları `IObjectSpace` üzerinden kalıcı XPO nesnesi oluşturmaktadır. Bu işlem mapping değil, uygulama/persistence sorumluluğudur.
- Mapping projesindeki `Microsoft.AspNetCore.App` framework referansı yalnızca dependency injection uzantısı için kullanılmaktadır ve kütüphaneyi gereksiz biçimde ASP.NET Core'a bağlamaktadır.

## Hedef Bağımlılık Yapısı

```text
Project1.DTOs
    ↑
Project1.Core
    ↑
Project1.Module

Project1.Mapping ──→ Project1.Core
Project1.Mapping ──→ Project1.DTOs
Project1.Mapping ──→ Project1.Module

Project1.Blazor.Server ──→ Project1.Module
Project1.Blazor.Server ──→ Project1.Mapping
```

`Project1.Module`, `Project1.Mapping` projesini referans almayacaktır. Module yalnızca Core içinde tanımlanan mapper sözleşmesini kullanacaktır. Mapping, bu sözleşmelerin XAF entity tiplerine özel implementasyonlarını sağlayacaktır. Blazor.Server composition root olarak interface ve implementasyonları birleştirecektir.

## Mapper Sözleşmesi

Core projesine aşağıdaki güçlü tipli sözleşme eklenecektir:

```csharp
public interface IMapper<in TSource, out TDestination>
{
    TDestination Map(TSource source);
}
```

Koleksiyonlar için ikinci bir mapper metodu eklenmeyecektir. Servisler standart LINQ ile `items.Select(mapper.Map)` kullanacaktır. Bu yaklaşım interface'i küçük tutar ve koleksiyon materialization kararını servise bırakır.

## Mapping Implementasyonları

Mapping projesi aşağıdaki mapper sınıflarını içerecektir:

- `NoteMapper : IMapper<Not, NoteDto>`
- `MusteriMapper : IMapper<Musteri, MusteriDto>`
- `KisiMapper : IMapper<Kisi, KisiDto>`

`NoteMapper`, varsa ek dosya bilgisini özel bir private metoda dönüştürecektir. Mapper'lar XPO nesnesini değiştirmeyecek, `IObjectSpace` kullanmayacak ve yalnızca entity → DTO dönüşümü yapacaktır.

Null bir kaynak mapper'a verildiğinde `ArgumentNullException` fırlatılacaktır. Nullable entity özellikleri DTO sözleşmesine uygun güvenli varsayılan değerlere dönüştürülecektir.

## Uygulama Akışı

Not listeleme akışı:

```text
NotesApiController
    → INoteService
    → NoteService
    → IObjectSpace ile Not kayıtlarını sorgula
    → IMapper<Not, NoteDto>.Map
    → NoteDto koleksiyonu
```

Not oluşturma akışı:

```text
CreateNoteRequestDto
    → NoteService
    → IObjectSpace.CreateObject<Not>()
    → alanları ve ilişkileri ata
    → CommitChanges()
    → IMapper<Not, NoteDto>.Map
    → NoteDto
```

DTO'dan entity oluşturma NoteService veya ilgili command handler içinde kalacaktır. İlişki çözümleme, entity oluşturma ve commit işlemleri mapper'a taşınmayacaktır.

## Dependency Injection

Mapping projesi ASP.NET Core'a bağımlı bir `DependencyInjection` uzantısı barındırmayacaktır. Mapper kayıtları Blazor.Server içindeki composition root'ta açıkça yapılacaktır:

```csharp
services.AddSingleton<IMapper<Not, NoteDto>, NoteMapper>();
services.AddSingleton<IMapper<Musteri, MusteriDto>, MusteriMapper>();
services.AddSingleton<IMapper<Kisi, KisiDto>, KisiMapper>();
```

Mapper sınıfları stateless olduğu için singleton yaşam süresi kullanılacaktır. `NoteService` scoped kalacaktır.

Win uygulamasında mapper kullanan bir servis etkinleştirilirse aynı kayıtlar Win composition root'a eklenecektir. Mevcut kullanım Blazor ile sınırlıysa Win projesine gereksiz Mapping referansı eklenmeyecektir.

## Proje Referansları

`Project1.Mapping.csproj` şu referansları koruyacaktır:

- `Project1.Core`
- `Project1.DTOs`
- `Project1.Module`

`Microsoft.AspNetCore.App` framework referansı kaldırılacaktır. `Project1.Blazor.Server` hem Module hem Mapping projelerini referans almaya devam edecektir. Module → Mapping referansı eklenmeyecektir.

## Test Tasarımı

Test projesine `Project1.Mapping` proje referansı eklenecektir. Aşağıdaki davranışlar test edilecektir:

- `NoteMapper`, temel not alanlarını doğru `NoteDto` alanlarına aktarır.
- `NoteMapper`, dosya bulunmadığında attachment alanını null döndürür.
- `MusteriMapper`, müşteri alanlarını doğru aktarır.
- `KisiMapper`, müşteri OID ilişkisini ve tam adı doğru aktarır.
- Mapper'lar null kaynak için `ArgumentNullException` fırlatır.
- `NoteService`, sorgulama ve oluşturma sonuçlarında enjekte edilen mapper'ı kullanır; servis içinde ikinci bir DTO oluşturma kodu bulunmaz.

Doğrulama sırası:

1. Mapping testlerinin önce başarısız olduğu görülür.
2. Mapper implementasyonları eklenir ve mapping testleri geçirilir.
3. NoteService entegrasyon testi önce başarısız çalıştırılır.
4. NoteService mapper kullanacak şekilde değiştirilir.
5. Mapping testleri, servis testleri ve tüm test paketi çalıştırılır.
6. Solution restore ve build işlemleri tamamlanır.

## Kapsam Dışı

- Yeni `Infrastructure`, `Application` veya `Domain` projesi oluşturulmayacaktır.
- XAF model sınıfları başka projeye taşınmayacaktır.
- Namespace'ler toplu olarak değiştirilmeyecektir.
- AutoMapper veya Mapster paketi eklenmeyecektir.
- Command/handler klasör yapısı bu değişiklik kapsamında yeniden düzenlenmeyecektir.
- Güvenlik, e-posta ve UI davranışları değiştirilmeyecektir.

## Başarı Kriterleri

- `Project1.Mapping` ayrı proje olarak solution'da kalır.
- Uygulamanın not DTO dönüşümleri gerçek çalışma akışında Mapping implementasyonundan geçer.
- Module → Mapping proje referansı ve döngüsel bağımlılık oluşmaz.
- `NoteService` içindeki tekrar eden `MapToDto` metodu kaldırılır.
- Mapping projesi ASP.NET Core framework bağımlılığı taşımaz.
- Yeni mapping testleri ve mevcut testlerin tamamı geçer.
- Solution restore ve build işlemi sıfır hata ile tamamlanır.
