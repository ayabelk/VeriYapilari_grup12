# VeriYapilari_grup12
# BSP Ağacı Tabanlı Görüş Alanı ve Çarpışma Tespiti

## Ekip Üyeleri
- Rojin Topuz
- Arda İnanç
- Hasan Emre Kartal
- Beyzanur Postlu
- Selsabıl Aya Belkabla

## Proje Adı: Veri Yapıları ile BSP Ağacı Tabanlı Görüş Alanı ve Çarpışma Tespiti 
Senaryo ve Temel Amaç: 
Bu projede, kuşbakışı (top-down) oynanan 2 boyutlu bir gizlilik oyunu sadeleştirilmiş bir model üzerinden 
geliştirilecektir. Oyuncu, duvarlar ve engeller içeren bir haritada hareket ederek hedef noktaya ulaşmaya çalışırken, 
haritada devriye gezen düşmanlara yakalanmamaya çalışacaktır. 

### Temel amaç: 
- oyun haritasını uygun veri yapıları ile temsil etmek 
- görüş alanı (line of sight) hesaplamasını verimli şekilde yapmak 
- çarpışma ve yol bulma problemlerini çözmek

## Mevcut Durum (Ara Rapor - 30 Nisan 2026)
- Repo kuruldu ve branch yapısı oluşturuldu
- BSP Tree veri yapısı implemente edildi ve test edildi

## Tespit Edilen Bulgular / Tartışmalar
- BSP Tree başarıyla yapıldı ve çalıştığı doğrulandı
- Raycasting için kullanılacak yöntem araştırılıyor


## Kullanılan Veri Yapıları
| Veri Yapısı | Kullanım Amacı | Durum |
|---|---|---|
| BSP Tree | Duvar segmentleri, görüş/çarpışma testi | Tamamlandı |

## Branch Yapısı
| Üye | Branch | Sorumluluk |
|---|---|---|
| Rojin Topuz | feature/bsp-tree | BSP Tree implementasyonu |

##  Genel Kurallar
1. Klasör Yapısı
proje/
 GameEngine/
- GameTypes.cs       → ortak tipler (herkes kullanır)
- GameConstants.cs   → sabit değerler (herkes kullanır)
- BspTree.cs         → Üye1
- Graph.cs           → Üye2
- MinHeap.cs         → Üye3
- LinkedList.cs      → Üye4
- GameEngine.cs      → Üye5
   
- Her dosyanın başında bu olmalı    >> namespace GameEngine.DataStructures
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
   feature/minheap
   feature/linkedlist
   feature/gameengine

- Commit mesajları Türkçe ve açıklayıcı olsun
   "Graph veri yapisi eklendi"
   "MinHeap Insert metodu tamamlandi"

- Main'e direkt kod atma!
- PR aç, en az 1 kişi onaylasın
- Her sınıfın başına Big-O analizi yaz >> Zaman Karmasikligi: O(log n) >> Uzay Karmasikligi: O(n)
- Her metodun üstüne ne yaptığını yaz
