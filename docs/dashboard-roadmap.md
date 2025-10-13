# Toplum Simülasyonu Dashboard Yol Haritası

Bu yol haritası, mevcut .NET konsol uygulamasının terminal çıktısını görsel bir web paneline dönüştürmek için izlenebilecek adımları detaylandırır.

## 1. Veri Toplama ve Dışa Aktarım

1. **Simülasyon sonuçlarını yakalayın.** Konsol uygulamasında her yılın sonucunu `SimulationYearResult` listesine ekleyin.
2. **JSON olarak kaydedin.** Simülasyon bittiğinde `output/simulation-history.json` dosyasına yazın. (Bu depoda örnek uygulama eklendi.)
3. **Dosya paylaşımı stratejisi belirleyin.**
   - Geliştirici ortamında: JSON dosyasını `dashboard/data` klasörüne kopyalayın.
   - Canlı ortamda: API veya statik dosya sunucusu ile paylaşın.

## 2. Veri Servis Katmanı (Opsiyonel fakat önerilir)

1. **Minimal Web API kurun.** `dotnet new webapi` ile ayrı bir proje oluşturup simülasyon dosyasını servis eden bir uç nokta yazın.
2. **Gerçek zamanlı güncelleme için SignalR.** Uzun süren simülasyonlarda ilerlemeyi anlık göstermek için SignalR hub ekleyin.
3. **Konfigürasyon yönetimi.** API üzerinden konfigürasyon parametrelerini okuyup güncellenebilir hale getirin.

## 3. Frontend Mimarisi

1. **Teknoloji seçimi.**
   - Hafif kullanım için: Statik HTML + Chart.js (bu depoda örneği var).
   - Orta/uzun vadeli büyüme için: React + Vite veya Next.js, Tailwind CSS, Zustand/Redux.
2. **Bileşen tasarımı.** Grafikler, metrik kartları, filtre paneli ve olay günlüğü gibi bölümleri belirleyin.
3. **Veri akışı.** JSON dosyasını fetch ederek state'e alın, grafik bileşenlerine prop olarak geçin.

## 4. Görselleştirme ve UX

1. **Zaman serisi grafikleri.** Toplam nüfus, insan/hayvan trendleri için line ve stacked bar grafikleri.
2. **Olay analizi.** Doğum, ölüm, evlilik, kaza adetlerini radar/bar grafikleri ile sunun.
3. **Metin tabanlı log.** Yıllara göre gruplanmış olay listesi ve filtre seçenekleri ekleyin.
4. **Karanlık/aydınlık tema.** Chart.js temalarını özelleştirerek marka kimliği sağlayın.

## 5. Yayınlama ve Otomasyon

1. **CI/CD pipeline.**
   - .NET simülasyonunu çalıştırıp JSON çıktısını artefact olarak üretin.
   - Frontend build komutunu çalıştırıp statik çıktıları yayınlayın.
2. **Barındırma seçenekleri.**
   - Frontend: GitHub Pages, Netlify, Vercel.
   - API: Azure App Service, Render, Railway.
3. **Gözlemleme.**
   - Uygulama loglarını toplayın.
   - Kullanıcı etkileşimlerini (ör. Google Analytics) izleyin.

## 6. Gelecek Geliştirmeler

- **Filtreler:** Belirli yılları veya sadece insan/ hayvan verilerini göster.
- **Senaryo karşılaştırmaları:** Farklı koşullarda çalıştırılan simülasyonların çıktısını yan yana göster.
- **Etkileşimli ayarlar:** Frontend üzerinden simülasyon parametrelerini düzenleyip tekrar çalıştır.
- **Uluslararasılaştırma:** Çok dilli destek (Türkçe/İngilizce) için i18n altyapısı kur.

Bu adımlar izlenerek terminal tabanlı çıktılar modern, etkileşimli bir dashboard deneyimine dönüştürülebilir.
