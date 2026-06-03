# VeriYapilari_grup12
# BSP Ağacı Tabanlı Görüş Alanı ve Çarpışma Tespiti

## Ekip Üyeleri
| Ad - Soyad | Numara | Sorumluluk |
|---|---|---|
| Rojin Topuz | 032390078 | BSP Tree, Raycasting |
| Arda İnanç | 032390080 | Graph veri yapısı |
| Hasan Emre Kartal | 032390081 | Backend entegrasyonu |
| Beyzanur Postlu | 032390082 | Docker, LinkedList, Backend & AI Servis entegrasyonu, Oyun motoru, MapGenerator, GameController, AiController |
| Selsabil Aya Belkabla | 032390092 | MinHeap, A* Pathfinding |

---

## Proje Adı: Veri Yapıları ile BSP Ağacı Tabanlı Görüş Alanı ve Çarpışma Tespiti

### Senaryo:
Bu projede, kuşbakışı (top-down) oynanan 2 boyutlu bir gizlilik oyunu sadeleştirilmiş bir model üzerinden
geliştirilmiştir. Oyuncu, duvarlar ve engeller içeren bir haritada hareket ederek hedef noktaya ulaşmaya çalışırken,
haritada devriye gezen düşmanlara yakalanmamaya çalışır.

### Temel amaç:
- Oyun haritasını uygun veri yapıları ile temsil etmek
- Görüş alanı (line of sight) hesaplamasını verimli şekilde yapmak
- Çarpışma ve yol bulma problemlerini çözmek

---

## Mimari

```
┌─────────────────────────────────────────────────────────────────┐
│                         KULLANICI                               │
└──────────────────────────┬──────────────────────────────────────┘
                           │ Tarayıcı
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│                    front_End (Blazor WASM)                      │
│  - HTML5 Canvas üzerinde oyun çizimi                            │
│  - WASD klavye kontrolü                                         │
│  - game.js: oyun döngüsü, FOV çizimi                            │
│  Port: 9090                                                     │
└──────────────────┬───────────────────────────────────┬──────────┘
                   │ POST /api/game/update              │ GET /api/ai/fov
                   │ (oyuncu pozisyonu)                 │ (FOV konisi)
                   ▼                                    ▼
┌────────────────────────────┐        ┌────────────────────────────┐
│   back_End (ASP.NET Core)  │        │    a_i (ASP.NET Core)      │
│                            │        │                            │
│ - Harita üretimi           │ POST   │ - BSP Raycasting           │
│ - BSP ağacı inşası         │ ──────►│ - A* Pathfinding           │
│ - Çarpışma tespiti         │ /init  │ - Düşman AI kararları      │
│ - Düşman hareketi          │        │ - FOV hesabı               │
│ Port: 5050                 │        │ Port: 5051                 │
└────────────────────────────┘        └────────────────────────────┘
```

> **Not:** a_i servisi back_End'den tamamen bağımsız çalışır. Kendi BSP ağacını ve Graf'ını tutar. back_End ile asenkron HTTP üzerinden iletişim kurar.

---

## Nasıl Çalıştırılır

### Docker ile (Önerilen)

> **Not:** Windows'ta terminali yönetici olarak çalıştır.

```bash
git clone https://github.com/ayabelk/VeriYapilari_grup12
cd VeriYapilari_grup12
docker-compose up --build
```

Tarayıcıda: `http://localhost:9090/game`

Projeyi kapatmak için:
```bash
docker-compose down
```

### Manuel (Geliştirme)

**3 ayrı terminal açın:**

```bash
# Terminal 1 — Backend
cd back_End
dotnet run
# Port: 5050

# Terminal 2 — AI Servisi
cd a_i
dotnet run --urls "http://localhost:5051"
# Port: 5051

# Terminal 3 — Frontend
cd front_End
dotnet run
# Terminal çıktısında "Now listening on: http://localhost:XXXX" yazar
# O portu tarayıcıya yaz: http://localhost:XXXX/game
```

---

## Oyun Nasıl Oynanır

1. **"Yeni Harita Üret"** butonuna bas — programatik rastgele harita oluşturulur
2. **"Oyunu Başlat"** butonuna bas
3. **WASD** veya **Ok tuşları** ile hareket et
4. Sağ alt köşedeki **sarı hedefe** ulaş
5. **Kırmızı düşmanların** FOV konisinden kaç
6. Düşman seni görürse **"TAKİP!"** yazısı çıkar ve A* ile peşinden gelir

---

## Kullanılan Veri Yapıları

| Veri Yapısı | Kullanım Amacı | Big-O | Durum |
|---|---|---|---|
| BSP Tree | Duvar segmentleri, görüş/çarpışma testi | Build: O(N log N), Sorgu: O(log N) | Tamamlandı |
| Graph | Yürünebilir alanların düğüm-kenar modeli | AddNode/Edge: O(1), BFS/DFS: O(V+E) | Tamamlandı |
| MinHeap | A* algoritmasında öncelik kuyruğu | Insert/ExtractMin: O(log N) | Tamamlandı |
| DynamicArray | FOV ışın sonuçlarını saklamak | Add: O(1) amortized, Get: O(1) | Tamamlandı |
| LinkedList | Raycasting'de geçici duvar adayları | AddFirst: O(1), Traverse: O(N) | Tamamlandı |

## Kullanılan Algoritmalar

| Algoritma | Kullanım Amacı | Big-O | Durum |
|---|---|---|---|
| Raycasting (CastRay) | Tek ışınla en yakın duvar kesişimi | O(log W + k) | Tamamlandı |
| FOV Taraması (CastFOV) | Düşman görüş konisi içinde çoklu ışın | O(R × log W) | Tamamlandı |
| Line of Sight | İki nokta arası görüş kontrolü | O(log N) | Tamamlandı |
| BSP CollectAlongRay | Işın hattındaki duvarları filtreleme | O(log N) | Tamamlandı |
| A* Pathfinding | Graph üzerinde düşman yol bulma | O((V+E) log V) | Tamamlandı |
| MapGenerator | Programatik rastgele harita üretimi | O(cols × rows) | Tamamlandı |

---

## Mevcut Durum

- ✅ Repo kuruldu ve branch yapısı oluşturuldu
- ✅ BSP Tree veri yapısı oluşturuldu ve test edildi
- ✅ Graph veri yapısı eklendi
- ✅ Linked List veri yapısı eklendi
- ✅ MinHeap veri yapısı eklendi
- ✅ DynamicArray veri yapısı eklendi
- ✅ Raycasting algoritması oluşturuldu
- ✅ BSP Tree'ye ışın-yönlü tarama desteği eklendi (CollectAlongRay)
- ✅ Raycasting ile BSP Tree entegrasyonu tamamlandı
- ✅ A* Pathfinding algoritması eklendi
- ✅ A* algoritması Graph ve MinHeap ile entegre edildi
- ✅ 3 mikroservis mimarisi kuruldu (back_End, a_i, front_End)
- ✅ Docker konfigürasyonları tamamlandı
- ✅ Oyun arayüzü (HTML5 Canvas) tamamlandı
- ✅ Düşman AI servisi (a_i) bağımsız mikroservis olarak çalışıyor
- ✅ BSP destekli çarpışma tespiti eklendi
- ✅ MapGenerator ile programatik harita üretimi tamamlandı

---

## Proje Yapısı

```
project6/
├── back_End/
│   ├── Algorithms/
│   │   ├── AStarPathfinder.cs
│   │   ├── LineOfSight.cs
│   │   ├── MapGenerator.cs
│   │   └── Raycasting.cs
│   ├── Controllers/
│   │   └── GameController.cs
│   ├── DataStructures/
│   │   ├── BspTree.cs
│   │   ├── DynamicArray.cs
│   │   ├── Graph.cs
│   │   ├── LinkedList.cs
│   │   └── MinHeap.cs
│   ├── Dockerfile
│   └── Program.cs
├── a_i/
│   ├── Algorithms/
│   │   ├── AStarPathfinder.cs
│   │   ├── LineOfSight.cs
│   │   └── Raycasting.cs
│   ├── Controllers/
│   │   └── AiController.cs
│   ├── DataStructures/
│   │   ├── BspTree.cs
│   │   ├── DynamicArray.cs
│   │   ├── Graph.cs
│   │   ├── LinkedList.cs
│   │   └── MinHeap.cs
│   ├── Dockerfile
│   └── Program.cs
├── front_End/
│   ├── Pages/
│   │   └── Game.razor
│   ├── Layout/
│   │   └── NavMenu.razor
│   ├── wwwroot/
│   │   ├── game.js
│   │   └── index.html
│   └── Dockerfile
├── docker-compose.yml
└── README.md
```

---

## Branch Yapısı

| Üye | Numara | Branch | Sorumluluk |
|---|---|---|---|
| Rojin Topuz | 032390078 | feature/bsp-tree | BSP Tree yapısı |
| Arda İnanç | 032390080 | feature/graph | Graph yapısı |
| Beyzanur Postlu | 032390082 | feature/linkedlist-architecture | Docker konfigürasyonu, LinkedList |
| Selsabil Aya Belkabla | 032390092 | feature/minheap | MinHeap yapısı |
| Rojin Topuz | 032390078 | feature/raycasting | Raycasting algoritması, BSP CollectAlongRay |
| Selsabil Aya Belkabla | 032390092 | feature/pathfinding | A* Pathfinding algoritması ve MinHeap entegrasyonu |
| Beyzanur Postlu | 032390082 | feature/game-engine | Backend & AI Servis entegrasyonu, Oyun motoru, MapGenerator, GameController, AiController, Docker düzenleme |

---

## Genel Kurallar

### Namespace Yapısı
- Her dosyanın başında: `namespace [SERVİS_İSMİ].DataStructures` veya `namespace [SERVİS_İSMİ].Algorithms`

### İsimlendirme
- Sınıf adları → BüyükHarfle başlar: `public class BspTree { }`
- Metodlar → BüyükHarfle başlar: `public void AddNode() { }`
- Private değişkenler → `_altçizgiyle` başlar: `private BspNode _root;`
- Public değişkenler → BüyükHarfle başlar: `public float X, Y;`

### Git Kuralları
- Her özellik için ayrı branch aç
- Commit mesajları Türkçe ve açıklayıcı olsun
- Main'e direkt kod atma, PR aç, en az 1 kişi onaylasın
- Her sınıfın başına Big-O analizi yaz

### Ortak Tipler
```csharp
public class Point       { public float X, Y; }
public class WallSegment { public float X1,Y1,X2,Y2; }
public class GraphNode   { public int Id; public float X,Y; public bool IsWalkable; }
public class Edge        { public int FromId,ToId; public float Weight; }
```