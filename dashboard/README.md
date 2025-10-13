# Dashboard'ı Çalıştırma Rehberi

Chart.js tabanlı panel artık simülasyonu doğrudan SignalR üzerinden izliyor. Aşağıdaki adımlar paneli yerel ortamınızda gerçek zamanlı olarak çalıştırmanıza yardımcı olur.

## 1. SignalR Sunucusunu Başlatın

1. Proje kök dizininde aşağıdaki komutu çalıştırın:
   ```bash
   dotnet run --project ConsoleApp1 -- --server
   ```
2. Uygulama varsayılan olarak `http://localhost:5000` adresinde SignalR hub'ını yayınlar. Farklı bir port kullanmak isterseniz `ASPNETCORE_URLS` değişkenini ayarlayabilir veya `--urls` parametresini ekleyebilirsiniz.

Sunucu terminalde açık kaldığı sürece simülasyon motoru gelen komutları bekler.

## 2. Dashboard'ı Servis Edin

Panel statik dosyalardan oluştuğu için basit bir HTTP sunucusu ile yayınlamak yeterlidir. Aşağıdaki seçeneklerden biri işinizi görür:

- **Python 3**
  ```bash
  cd dashboard
  python3 -m http.server 5173
  ```
- **Node.js (serve)**
  ```bash
  cd dashboard
  npx serve -l 5173
  ```

## 3. Tarayıcıdan Bağlanın

1. Tarayıcıda `http://localhost:5173` adresine gidin (kendi seçtiğiniz porta göre uyarlayın).
2. Sayfa, varsayılan olarak `http://localhost:5000/simulationHub` adresine bağlanmaya çalışır. Sunucuyu farklı bir host/port altında çalıştırdıysanız URL'yi sorgu parametresi ile geçebilirsiniz:
   - Örnek: `http://localhost:5173/?hub=http://192.168.1.10:6001/simulationHub`
3. Bağlantı kurulduğunda üst kısımdaki durum rozetleri yeşile döner ve kontrol düğmeleri aktifleşir.

## 4. Simülasyonu Yönetin

- **Başlat**: Otomatik yıllık ilerlemeyi tetikler (varsayılan olarak 2 saniyede bir yıl).
- **Duraklat**: Zamanlayıcıyı durdurur; kaldığı yerden devam edebilirsiniz.
- **Tek Adım**: Duraklatılmış durumda tek bir yılı manuel olarak ilerletir.
- **Sıfırla**: Simülasyonu başlangıç yılına geri alır ve nüfusu yeniden oluşturur.

Grafikler ve olay günlüğü her yıl sonunda otomatik güncellenir. Bağlantı kesildiğinde panel yeniden bağlanmayı dener ve durum rozeti turuncu/kırmızıya döner.

> **Not:** Terminal modunu kullanmaya devam etmek isterseniz `dotnet run --project ConsoleApp1` komutu ile eski etkileşimli konsol sürümü hâlâ kullanılabilir.
