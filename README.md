# 🐦 Raven

**Fast. Local. Simple.**

Raven, aynı Wi-Fi ağı üzerindeki Android cihazlar arasında dosyaları hızlı ve kolay bir şekilde aktarmak için geliştirilmiş modern bir dosya transfer uygulamasıdır.

Raven, klasik cihaz listesi ve manuel IP adresi girme yöntemleri yerine **QR kod tabanlı bağlantı** kullanır. Alıcı cihaz bir bağlantı noktası oluşturur, gönderici cihaz QR kodu tarayarak otomatik olarak bağlantıyı kurar ve dosya transferi başlar.

---

## ✨ Özellikler

### 📡 QR Kod ile Bağlantı

* Manuel IP adresi girme gerektirmez.
* Alıcı cihaz QR kod oluşturur.
* Gönderici cihaz QR kodu kamerayla tarar.
* Wi-Fi bağlantısı otomatik olarak hazırlanır.
* Bağlantı kurulduktan sonra dosya aktarımı başlar.

### 📁 Dosya Transferi

* Tek dosya gönderme
* Çoklu dosya gönderme
* Büyük dosya transferleri
* Gerçek zamanlı ilerleme göstergesi
* Transfer yüzdesi
* Anlık transfer hızı
* Kalan süre tahmini
* Gönderilen/alınan veri miktarı
* Transfer sırasında ekranın kapanmasını engelleme

### 🖼️ Dosya Önizleme

Seçilen dosyalar transfer başlamadan önce listelenebilir.

Dosya türüne göre uygun simgeler kullanılır ve desteklenen görseller için önizleme gösterilebilir.

Çok yüksek sayıda dosya seçildiğinde uygulamanın performansını korumak amacıyla arayüzde gösterilen öğeler sınırlandırılır; **seçilen dosyaların tamamı yine transfer edilir.**

### 📊 Gelişmiş Transfer Paneli

Transfer sırasında:

* Dosya adı
* Transfer yüzdesi
* Progress bar
* Transfer hızı
* Kalan süre
* Aktarılan veri miktarı
* Aktif transfer durumu

tek bir modern panel üzerinden görüntülenir.

### 🕘 Transfer Geçmişi

Raven gerçekleştirilen transferleri geçmişte görüntüleyebilir.

Geçmiş detaylarında:

* Dosya adı
* Dosya sayısı
* Toplam boyut
* Transfer yönü
* Transfer durumu
* Tarih ve saat
* Cihaz bilgisi

görüntülenebilir.

### 🎨 Modern Arayüz

Raven, karanlık ve minimalist bir tasarım anlayışıyla geliştirilmiştir.

Arayüzde:

* Dark UI
* Glassmorphism benzeri kartlar
* Glow efektleri
* Smooth geçişler
* Buton animasyonları
* Transfer pulse animasyonu
* Duruma göre değişen renkler
* Modern yan menü
* Premium QR tarama ekranı

kullanılmıştır.

### 📱 Alıcı / Gönderici Modları

Raven iki temel çalışma moduna sahiptir:

**Al**

Cihaz bağlantı noktası oluşturur ve başka bir cihazdan dosya bekler.

**Gönder**

Gönderilecek dosyalar seçilir ve alıcının oluşturduğu QR kod taranarak transfer başlatılır.

---

## 🔐 Güvenlik ve Gizlilik

Raven'in yerel transfer sistemi cihazlar arasındaki doğrudan yerel ağ iletişimine odaklanır.

Dosyalar için herhangi bir genel dosya paylaşım platformu veya sosyal medya servisi kullanılmaz.

Yerel aktarım senaryosunda dosyalar Raven'in transfer protokolü üzerinden alıcı cihaza gönderilir.

> Raven'in uzak cihazlar arasında internet üzerinden transfer özelliği henüz mevcut değildir.

---

## 🛠️ Teknolojiler

Raven aşağıdaki teknolojiler kullanılarak geliştirilmiştir:

* **C#**
* **.NET 10**
* **.NET MAUI**
* **Android**
* **TCP/IP**
* **Wi-Fi**
* **QR Code**
* **XAML**
* **MVVM**

### Kullanılan Kütüphaneler

* `Microsoft.Maui.Controls`
* `QRCoder`
* `ZXing.Net.Maui.Controls`
* `Microsoft.Extensions.Logging.Debug`

---

## 📱 Android Gereksinimleri

Raven'in mevcut sürümü **Android 13 (API 33) ve üzerini** desteklemektedir.

| Android            | Durum            |
| ------------------ | ---------------- |
| Android 16         | ✅ Destekleniyor  |
| Android 15         | ✅ Destekleniyor  |
| Android 14         | ✅ Destekleniyor  |
| Android 13         | ✅ Destekleniyor  |
| Android 12         | ❌ Desteklenmiyor |
| Android 11         | ❌ Desteklenmiyor |
| Android 10 ve altı | ❌ Desteklenmiyor |

Minimum Android sürümü:

**Android 13 / API 33**

---

## 🚀 Kurulum

Projeyi klonlayın:

```bash
git clone https://github.com/YOUR_USERNAME/RavenMobile.git
```

Proje klasörüne girin:

```bash
cd RavenMobile
```

Projeyi build edin:

```bash
dotnet build -f net10.0-android
```

Android cihaz veya emülatör üzerinde çalıştırın:

```bash
dotnet build -t:Run -f net10.0-android
```

> Android geliştirme ortamınızda .NET 10 SDK ve gerekli .NET MAUI workload'larının kurulu olması gerekir.

---

## 📲 Kullanım

### Dosya almak

1. Raven'i açın.
2. **AL** seçeneğine dokunun.
3. Raven yerel transfer bağlantısını oluşturur.
4. Ekranda QR kod görüntülenir.
5. Gönderici cihazdan QR kod taranır.
6. Transfer başlatılır.
7. Gelen dosyalar cihazınıza kaydedilir.

### Dosya göndermek

1. Raven'i açın.
2. **GÖNDER** seçeneğine dokunun.
3. Bir veya birden fazla dosya seçin.
4. Alıcı cihazdaki QR kodu tarayın.
5. Bağlantı kurulduğunda transfer başlar.
6. Transfer panelinden ilerlemeyi takip edin.

---

## 🏗️ Proje Yapısı

```text
RavenMobile/
│
├── Core/
│   ├── Constants/
│   └── Utils/
│
├── Features/
│   ├── Connection/
│   ├── Transfer/
│   └── WifiQr/
│
├── Models/
│
├── ViewModels/
│   ├── HomeViewModel.cs
│   ├── HistoryViewModel.cs
│   └── ...
│
├── Views/
│   ├── HomePage.xaml
│   ├── HistoryPage.xaml
│   ├── HistoryDetailPage.xaml
│   ├── MenuPage.xaml
│   ├── SettingsPage.xaml
│   ├── AboutPage.xaml
│   └── OnboardingPage.xaml
│
├── Platforms/
│   └── Android/
│
├── Resources/
│   ├── AppIcon/
│   └── Splash/
│
├── MauiProgram.cs
└── RavenMobile.csproj
```

---

## 🗺️ Yol Haritası

Raven aktif olarak geliştirilmektedir.

Planlanan geliştirmeler:

* [ ] Transfer iptal sistemi
* [ ] Alıcı tarafında transfer onayı
* [ ] Gelişmiş transfer kuyruğu
* [ ] Klasör transferi
* [ ] Transfer raporlarının geliştirilmesi
* [ ] Dosyaları geçmiş ekranından doğrudan açma
* [ ] Daha gelişmiş hata ve bağlantı teşhisi
* [ ] Uzak cihazlara internet üzerinden transfer
* [ ] Geçici paylaşım kodları
* [ ] Uçtan uca şifreli uzak transfer
* [ ] Performans optimizasyonları

---

## 🌐 Uzak Transfer

Raven'in mevcut temel amacı **yakındaki cihazlar arasında hızlı yerel dosya aktarımıdır.**

Gelecekte internet üzerinden uzak cihazlara transfer için ayrı bir sistem planlanmaktadır.

Planlanan yapı:

```text
Gönderici
    │
    ▼
Raven Internet Service
    │
    ▼
Geçici Transfer
    │
    ▼
Alıcı
```

Bu sistemde tek kullanımlık transfer kodları, geçici dosya saklama ve şifreli transfer gibi özellikler değerlendirilecektir.

---

## 🎯 Projenin Amacı

Raven'in amacı, cihazlar arasında dosya göndermeyi mümkün olduğunca basitleştirmektir.

Kullanıcının:

* IP adresi bilmesine,
* cihaz aramasına,
* karmaşık bağlantı ayarları yapmasına

gerek kalmadan **QR kodu tara ve gönder** mantığıyla çalışması hedeflenmektedir.

---

## 🤝 Katkıda Bulunma

Raven açık kaynak olarak geliştiriliyorsa katkılar memnuniyetle karşılanır.

1. Repository'yi fork edin.
2. Yeni bir branch oluşturun.
3. Değişikliklerinizi yapın.
4. Testlerinizi gerçekleştirin.
5. Pull Request gönderin.

---

## 📄 Lisans

Bu proje için lisans bilgileri daha sonra eklenecektir.

---

## 🐦 Raven

**Fast. Local. Simple.**

QR kodunu tara.
Dosyanı seç.
Gönder.

© 2026 Raven
