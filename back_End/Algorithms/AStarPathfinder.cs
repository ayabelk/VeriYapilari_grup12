using System;
using System.Collections.Generic;
using back_End.DataStructures;

namespace back_End.Algorithms
{
    /// <summary>
    /// A* Pathfinding Algoritması
    /// Düşmanların engelleri aşarak hedefe ulaşmasını sağlar.
    ///
    /// === GENEL ZAMAN KARMAŞIKLIĞI ===
    /// FindPath     : O((V + E) log V) — her düğüm en fazla bir kez openSet'e
    ///                eklenir; her ekleme/çıkarma MinHeap ile O(log V).
    ///                E kenar toplam işlenir, her biri O(log V) heap işlemi tetikler.
    ///                O(V²) en kötü durum — tam bağlı grafta E = V² olursa.
    /// Reconstruct  : O(P) — P = yol uzunluğu; en kötü durum O(V).
    ///                + O(P) Reverse() çağrısı → toplam O(P).
    /// Heuristic    : O(1) — iki koordinat farkı ve bir karekök; sabit işlem.
    /// CompareTo    : O(1) — iki float karşılaştırması; MinHeap sıralamasında kullanılır.
    ///
    /// === UZAY KARMAŞIKLIĞI ===
    /// O(V) — openSet, gCost, cameFrom, closedSet en fazla V düğüm saklar.
    ///
    /// === HEURİSTİK ===
    /// Öklid mesafesi admissible (gerçek maliyeti hiçbir zaman aşmaz) ve
    /// consistent (üçgen eşitsizliğini sağlar) → A* optimal çözümü garanti eder.
    /// </summary>
    public class AStarPathfinder
    {
        private readonly Graph _graph;

        // O(1) — yalnızca graf referansı atanır.
        public AStarPathfinder(Graph graph) { _graph = graph; }

        // O((V + E) log V) — ana döngü analizi:
        //   - Her düğüm closedSet'e en fazla bir kez girer: V ExtractMin → O(V log V).
        //   - Her kenar en fazla bir kez işlenir: E Insert → O(E log V).
        //   - Toplam: O(V log V + E log V) = O((V + E) log V).
        // Düğüm erişimleri (HasNode, GetNode) O(1) ort.; HashSet işlemleri O(1) ort.
        public List<int> FindPath(int startId, int goalId)
        {
            // O(1) ort. — iki Dictionary erişimi
            if (!_graph.HasNode(startId) || !_graph.HasNode(goalId)) return new List<int>();
            var startNode = _graph.GetNode(startId); // O(1) ort.
            var goalNode = _graph.GetNode(goalId);  // O(1) ort.
            if (!startNode.IsWalkable || !goalNode.IsWalkable) return new List<int>();

            // O(1) — dört veri yapısı başlatılır; hepsi başlangıçta boş.
            var openSet = new a_i.DataStructures.MinHeap<AStarRecord>();
            var gCost = new Dictionary<int, float>();
            var cameFrom = new Dictionary<int, int>();
            var closedSet = new HashSet<int>();

            gCost[startId] = 0;
            openSet.Insert(new AStarRecord(startId, 0, Heuristic(startNode, goalNode))); // O(1)

            // Ana döngü: en fazla V kez çalışır → O(V log V) toplam heap işlemi.
            while (!openSet.IsEmpty())
            {
                var cur = openSet.ExtractMin(); // O(log V) — heap yeniden düzenlenir
                int cid = cur.NodeId;

                if (closedSet.Contains(cid)) continue; // O(1) — zaten işlenmiş, atla
                if (cid == goalId) return Reconstruct(cameFrom, startId, goalId); // O(P)
                closedSet.Add(cid); // O(1)

                // Komşu döngüsü: toplam tüm iterasyonlarda E kez çalışır.
                // Her iterasyon: O(1) erişim + O(log V) olası Insert → O(E log V) toplam.
                foreach (var edge in _graph.GetNeighbors(cid)) // O(1) erişim
                {
                    int nid = edge.ToId;
                    if (closedSet.Contains(nid)) continue;             // O(1)
                    if (!_graph.GetNode(nid).IsWalkable) continue;     // O(1) ort.

                    float tg = gCost[cid] + edge.Weight;               // O(1)

                    if (!gCost.ContainsKey(nid) || tg < gCost[nid])   // O(1) ort.
                    {
                        cameFrom[nid] = cid;                           // O(1) ort.
                        gCost[nid] = tg;                            // O(1) ort.
                        // Lazy deletion: aynı düğüm birden fazla kez eklenebilir;
                        // closedSet kontrolü ile eski kayıtlar atlanır.
                        openSet.Insert(new AStarRecord(                // O(log V)
                            nid, tg, Heuristic(_graph.GetNode(nid), goalNode)));
                    }
                }
            }
            return new List<int>(); // O(1) — yol bulunamadı
        }

        // O(P) — P = yol üzerindeki düğüm sayısı; en kötü O(V).
        // İki geçiş: geri izleme O(P) + Reverse() O(P) → toplam O(P).
        private List<int> Reconstruct(Dictionary<int, int> came, int start, int goal)
        {
            var path = new List<int>();
            int cur = goal;
            path.Add(cur);                         // O(1) amortized

            // O(P) — hedeften başlangıca geriye doğru izlenir.
            while (cur != start)
            {
                if (!came.ContainsKey(cur)) return new List<int>(); // O(1) ort.
                cur = came[cur];                                      // O(1) ort.
                path.Add(cur);                                        // O(1) amortized
            }

            path.Reverse(); // O(P) — liste tersine çevrilir
            return path;
        }

        // O(1) — iki çıkarma, iki kare, bir toplama, bir karekök; sabit işlem sayısı.
        // Admissible: Öklid mesafesi gerçek en kısa yolu hiçbir zaman aşmaz.
        // Consistent: h(n) ≤ cost(n,n') + h(n') → A* optimal ve verimli çalışır.
        private float Heuristic(GraphNode a, GraphNode b)
        { float dx = a.X - b.X, dy = a.Y - b.Y; return MathF.Sqrt(dx * dx + dy * dy); }

        private class AStarRecord : IComparable<AStarRecord>
        {
            public int NodeId { get; }
            public float GCost { get; }
            public float HCost { get; }

            // O(1) — FCost her seferinde hesaplanır.
            // Not: Sık erişimde performans için backing field'a alınabilir.
            public float FCost => GCost + HCost;

            // O(1) — üç alan sabit sürede atanır.
            public AStarRecord(int id, float g, float h) { NodeId = id; GCost = g; HCost = h; }

            // O(1) — en fazla iki float karşılaştırması.
            // FCost eşitliğinde HCost ile tie-breaking → hedefe daha yakın düğüm önce işlenir.
            public int CompareTo(AStarRecord? o)
            {
                if (o == null) return -1;
                int c = FCost.CompareTo(o.FCost);          // O(1) birincil sıralama
                return c != 0 ? c : HCost.CompareTo(o.HCost); // O(1) tie-breaking
            }
        }
    }
}