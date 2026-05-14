# VeriYapilari_grup12
# BSP Ağacı Tabanlı Görüş Alanı ve Çarpışma Tespiti

## Ekip Üyeleri
| Ad - Soyad | Numara |
|---|---|
| Rojin Topuz | 032390078 |
| Arda İnanç | 032390080 |
| Hasan Emre Kartal | 032390081 |
| Beyzanur Postlu | 032390082 |
| Selsabil Aya Belkabla | 032390092 |


## Proje Adı: Veri Yapıları ile BSP Ağacı Tabanlı Görüş Alanı ve Çarpışma Tespiti 
### Senaryo: 
Bu projede, kuşbakışı (top-down) oynanan 2 boyutlu bir gizlilik oyunu sadeleştirilmiş bir model üzerinden 
geliştirilecektir. Oyuncu, duvarlar ve engeller içeren bir haritada hareket ederek hedef noktaya ulaşmaya çalışırken, 
haritada devriye gezen düşmanlara yakalanmamaya çalışacaktır. 

### Temel amaç: 
- oyun haritasını uygun veri yapıları ile temsil etmek 
- görüş alanı (line of sight) hesaplamasını verimli şekilde yapmak 
- çarpışma ve yol bulma problemlerini çözmek

## Mevcut Durum (Ara Rapor - 30 Nisan 2026)
- Repo kuruldu ve branch yapısı oluşturuldu.
- BSP Tree veri yapısı oluşturuldu ve test edildi.
- Graph veri yapısı eklendi.
- Proje mimarisi mikroservis yapısına dönüştürüldü.
- Linked List veri yapısı eklendi.
- MinHeap veri yapısı eklendi.

## Tespit Edilen Bulgular / Tartışmalar
- BSP Tree başarıyla yapıldı ve çalıştığı doğrulandı.
- Raycasting için kullanılacak yöntem araştırılıyor.
- Veri yapıları arasındaki hiyerarşi netleştirildi.
- LinkedList tabanlı dinamik AI hafıza mekanizması ve konum takibi yöntemleri araştırılıyor.
- MinHeap başarıyla yapıldı ve çalıştığı doğrulandı.

## Kullanılan Veri Yapıları
| Veri Yapısı | Kullanım Amacı | Durum |
|---|---|---|
| BSP Tree | Duvar segmentleri, görüş/çarpışma testi | Tamamlandı |
| Graph    | Yürünebilir alanların düğüm-kenar modeli| Tamamlandı |
| Linked List    | Yürünebilir alanların düğüm-kenar modeli| Geliştiriliyor |
| MinHeap| Pathfinding algoritmalarında, A* algoritmasında kullanılacak | Geliştiriliyor  |
## Branch Yapısı
| Üye | Branch | Sorumluluk |
|---|---|---|
| Rojin Topuz | feature/bsp-tree | BSP Tree yapısı |
| Arda İnanç  | feature/graph    | Graph yapısı    |
| Beyzanur Postlu | feature/linkedlist-architecture | Docker Konfigürasyonu, LinkedList |
|Selsabil Aya Belkabla | feature/minheap | MinHeap yapısı |

##  Genel Kurallar

###  Proje Klasör Yapısı
```
Project6/
├── GameEngine.csproj
├── Program.cs
├── DataStructures/
│   ├── BspTree.cs
│   ├── Graph.cs
│   ├── MinHeap.cs
│   └── CustomLinkedList.cs
├── Algorithms/
│   ├── Ray.cs
│   ├── HitPoint.cs
│   ├── RayIntersection.cs
│   └── Raycaster.cs
└── README.md
```



   
- Her dosyanın başında bu olmalı    >> namespace [SERVİS İSMİ].DataStructures
- Sınıf adları → BüyükHarfle başlar >> public class BspTree { } >> public class GraphNode { }
- Metodlar → BüyükHarfle başlar     >> public void AddNode() { } >> public int GetHeight() { }
- Private değişkenler → _altçizgiyle başlar >> private BspNode _root; >> private int _nodeCount;
- Public değişkenler → BüyükHarfle başlar >> public float X, Y; >> public int Id;
- Sabitler → TAMAMI_BÜYÜK >> public const int MAP_WIDTH = 800;

### Ortak Tipler
- Nokta
public class Point
{
    public float X, Y;
    public Point(float x, float y) { X = x; Y = y; }
}

- Duvar
public class WallSegment
{
    public float X1, Y1, X2, Y2;
    public WallSegment(float x1, float y1, float x2, float y2)
    { X1=x1; Y1=y1; X2=x2; Y2=y2; }
}

- Graf düğümü
public class GraphNode
{
    public int Id;
    public float X, Y;
    public bool IsWalkable;
}

- Graf kenarı
public class Edge
{
    public int FromId, ToId;
    public float Weight;
}

- Oyuncu/Düşman
public class Entity
{
    public float X, Y;
    public float Speed;
    public bool IsPlayer;
}
### Sabit Değerler
public static class GameConstants
{
    public const int MAP_WIDTH = 800;
    
    public const int MAP_HEIGHT = 600;
    
    public const float PLAYER_SPEED = 5.0f;
    
    public const float ENEMY_SPEED = 3.0f;
    
    public const float FOV_ANGLE = 60.0f;
    
    public const int FOV_RAYS = 60;
}
### Git Kuralları
- Her özellik için ayrı branch aç
   feature/graph
   feature/minHeap
   feature/linkedList
   feature/bspTree

- Commit mesajları Türkçe ve açıklayıcı olsun
   "Graph veri yapisi eklendi"
   "MinHeap Insert metodu tamamlandi"

- Main'e direkt kod atma!
- PR aç, en az 1 kişi onaylasın
- Her sınıfın başına Big-O analizi yaz >> Zaman Karmasikligi: O(log n) >> Uzay Karmasikligi: O(n)
- Her metodun üstüne ne yaptığını yaz
