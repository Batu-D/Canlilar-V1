# Dashboard'ı Çalıştırma Rehberi

Bu klasördeki statik Chart.js panelini yerel ortamınızda açmak için aşağıdaki adımları izleyin.

## 1. Simülasyon Çıktısını Üretin

1. Projenin kök dizinine gidin ve simülasyonu çalıştırın:
   ```bash
   dotnet run --project ConsoleApp1
   ```
2. Konsoldaki yönergeleri takip ederek yılları ilerletin. [Enter] ile yılları artırabilir, [Esc] ile simülasyonu bitirebilirsiniz.
3. Uygulama kapanırken sonuçlar `ConsoleApp1/bin/<Yapı-Konfigürasyonu>/<Hedef-Cilt>/output/simulation-history.json` dosyasına kaydedilir.
   - Varsayılan `Debug` çalıştırmasında tipik yol `ConsoleApp1/bin/Debug/net7.0/output/simulation-history.json` olur.

## 2. JSON'u Dashboard'a Kopyalayın

1. Üretilen `simulation-history.json` dosyasını bu klasördeki `data` dizinine kopyalayın:
   ```bash
   cp ConsoleApp1/bin/Debug/net7.0/output/simulation-history.json dashboard/data/
   ```
2. Dosyanın adı `simulation-history.json` kalmalıdır; dashboard bu ismi arar.

_(İsterseniz, ilk denemeler için `data/sample-results.json` dosyasını kullanmaya devam edebilirsiniz.)_

## 3. Statik Sunucu Başlatın

Tarayıcılar `file://` protokolü üzerinden `fetch` çağrılarını engellediği için dosyaları bir HTTP sunucusu üzerinden servis etmek gerekir. Aşağıdaki seçeneklerden birini kullanabilirsiniz:

- Python 3 ile:
  ```bash
  cd dashboard
  python3 -m http.server 5173
  ```
- Node.js ile (Serve paketini global kurduysanız):
  ```bash
  cd dashboard
  npx serve -l 5173
  ```

## 4. Tarayıcıda Açın

1. Tarayıcınızda `http://localhost:5173` adresine gidin.
2. Sayfa yüklendiğinde sol üstteki açılır menüden `Simülasyon Çıktısı` seçeneğini seçerek kopyaladığınız gerçek verileri görüntüleyin.
3. Grafikler ve olay günlüğü otomatik olarak güncellenecektir.

Bu adımları takip ederek terminal çıktılarınızı görsel panele dönüştürebilirsiniz.
