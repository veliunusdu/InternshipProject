# Kişi Detayından Not Ekleme Penceresi

## Amaç

Kişi detay sayfasındaki Not Ekle işlemi, notun bağlı olduğu kişi ve müşteriyi otomatik belirlemelidir. Kullanıcı yalnızca not bilgilerini girmelidir.

## Davranış

- Kişi detayındaki Not Ekle işlemi bir popup penceresi açar.
- Yeni notun `Kisi` alanı açık olan kişi olarak atanır.
- Yeni notun `Musteri` alanı, kişinin bağlı olduğu müşteri olarak atanır.
- Popup'ta `Musteri` ve `Kisi` editörleri gizlenir.
- Popup'ta yalnızca `Baslik`, `Icerik` ve `Derece` editörleri görünür.
- Kaydedilen not kişi ve müşteriyle ilişkili olarak saklanır; e-posta bildirimi mevcut yetki kurallarına göre çalışmaya devam eder.

## Uygulama Sınırı

Bu değişiklik `Kisi_Notlar_ListView` ve `Musteri_Notlar_ListView` içindeki Not Ekle akışlarını etkiler. Müşteri detayından açılan not penceresinde müşteri otomatik atanır ve gizlenir; kişi seçimi görünür kalır.

## Doğrulama

1. Bir kişiyi açın.
2. Notlar bölümünden Not Ekle seçin.
3. Popup'ta yalnızca başlık, içerik ve derece alanlarının göründüğünü doğrulayın.
4. Notu kaydedin ve kişinin notları altında göründüğünü doğrulayın.
5. Bir müşteriyi açın ve Notlar bölümünden Not Ekle seçin; popup'ta müşteri alanının görünmediğini, kişi seçiminin göründüğünü doğrulayın.
