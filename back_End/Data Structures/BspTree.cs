using System;
using System.Collections.Generic;

namespace back_End.DataStructures
{
    /// <summary>
    /// BSP Ağacı (Binary Space Partitioning Tree)
    /// Duvar segmentlerini saklar, uzamsal sorguları hızlandırır.
    ///
    /// === GENEL ZAMAN KARMAŞIKLIĞI ===
    /// Build            : O(N log N) ort. — dengeli bölünmede log N seviye,
    ///                    her seviyede N segment işlenir.
    ///                    O(N²) en kötü durum — bölme düzlemi her seferinde
    ///                    tüm segmentleri aynı tarafa atarsa (dengesiz ağaç).
    /// GetVisibleWalls  : O(N) — Painter's Algorithm, tüm düğümler ziyaret edilir.
    /// CollectAlongRay  : O(log N + k) ort. — k = ışının kestiği segment sayısı.
    ///                    O(N) en kötü durum — tüm segmentler ışın üzerindeyse.
    /// IsPointBlocked   : O(N) — GetVisibleWalls'a bağlı doğrusal tarama.
    /// GetHeight        : O(N) — tüm düğümler özyinelemeli ziyaret edilir.
    ///
    /// === UZAY KARMAŞIKLIĞI ===
    /// O(N) — her segment tam olarak bir düğümde saklanır.
    /// Özyineleme yığını: O(log N) ort., O(N) en kötü durum (dengesiz ağaç).
    /// </summary>
    public class WallSegment
    {
        public float X1, Y1, X2, Y2;

        // O(1) — dört float alanı sabit sürede atanır.
        public WallSegment(float x1, float y1, float x2, float y2)
        { X1 = x1; Y1 = y1; X2 = x2; Y2 = y2; }

        public override string ToString() => $"Wall[({X1},{Y1}) -> ({X2},{Y2})]";
    }

    public class BspNode
    {
        public WallSegment Partition;
        public List<WallSegment> Walls;
        public BspNode? Front;
        public BspNode? Back;

        // O(1) — sabit alan tahsisi; liste başlatma O(1) amortized.
        public BspNode(WallSegment partition)
        {
            Partition = partition;
            Walls = new List<WallSegment>();
            Front = null;
            Back = null;
        }
    }

    public class BspTree
    {
        private BspNode? _root;

        // O(1) — yalnızca _root pointer'ı null olarak atanır.
        public BspTree() { _root = null; }

        // O(N log N) ort. / O(N²) en kötü — BuildRecursive'e delege edilir.
        public void Build(List<WallSegment> walls)
        {
            if (walls == null || walls.Count == 0) return; // O(1) erken çıkış
            _root = BuildRecursive(walls);
        }

        // T(N) = 2·T(N/2) + O(N)  →  O(N log N) ort. (Master Theorem, Durum 2)
        // T(N) = T(N-1) + O(N)    →  O(N²) en kötü durum (dengesiz bölünme).
        // Her çağrıda walls listesi O(N) doğrusal taramayla ikiye ayrılır.
        // Özyineleme derinliği: O(log N) ort., O(N) en kötü.
        private BspNode? BuildRecursive(List<WallSegment> walls)
        {
            if (walls.Count == 0) return null; // O(1) taban durumu

            var partition = walls[0];
            var node = new BspNode(partition);
            var frontList = new List<WallSegment>();
            var backList = new List<WallSegment>();

            // O(N) — her segment bir kez ClassifySegment'e girer.
            for (int i = 1; i < walls.Count; i++)
            {
                int side = ClassifySegment(walls[i], partition); // O(1)
                if (side >= 0) frontList.Add(walls[i]);          // O(1) amortized
                else backList.Add(walls[i]);                     // O(1) amortized
            }

            node.Front = BuildRecursive(frontList); // T(frontList.Count)
            node.Back = BuildRecursive(backList);  // T(backList.Count)
            return node;
        }

        // O(1) — sabit sayıda çarpma ve toplama işlemi; liste başına bir kez çağrılır.
        private int ClassifySegment(WallSegment wall, WallSegment partition)
        {
            float dx = partition.X2 - partition.X1;
            float dy = partition.Y2 - partition.Y1;
            float d1 = dx * (wall.Y1 - partition.Y1) - dy * (wall.X1 - partition.X1);
            float d2 = dx * (wall.Y2 - partition.Y1) - dy * (wall.X2 - partition.X1);
            return (int)(d1 + d2);
        }

        // O(N) — TraverseNode tüm ağacı Painter's Algorithm sırasıyla gezdirir.
        // Her düğüm tam olarak bir kez result listesine eklenir → O(N).

        public List<WallSegment> GetVisibleWalls(float viewX, float viewY)
        {
            var result = new List<WallSegment>();
            TraverseNode(_root, viewX, viewY, result); // O(N)
            return result;
        }

        // O(N) — her düğümde O(1) ClassifyPoint + result.Add çağrısı yapılır.
        // Sıralama: arka → düğüm → ön (Painter's Algorithm — arkadaki önce çizilir).
        private void TraverseNode(BspNode? node, float vx, float vy, List<WallSegment> result)
        {
            if (node == null) return;                            // O(1) taban durumu
            int side = ClassifyPoint(vx, vy, node.Partition);   // O(1)
            if (side >= 0)
            {
                TraverseNode(node.Back, vx, vy, result); // arka önce
                result.Add(node.Partition);                // O(1) amortized
                TraverseNode(node.Front, vx, vy, result); // ön sonra
            }
            else
            {
                TraverseNode(node.Front, vx, vy, result);
                result.Add(node.Partition);
                TraverseNode(node.Back, vx, vy, result);
            }
        }

        // O(1) — sabit sayıda çarpma ve çıkarma; her traversal adımında bir kez çağrılır.
        private int ClassifyPoint(float px, float py, WallSegment partition)
        {
            float dx = partition.X2 - partition.X1, dy = partition.Y2 - partition.Y1;
            float d = dx * (py - partition.Y1) - dy * (px - partition.X1);
            return d >= 0 ? 1 : -1;
        }

        // O(log N + k) ort. — ışın her seviyede bir bölme düzlemini değerlendirir;
        //   k = ışının kestiği segment sayısı (pratikte küçük).
        // O(N) en kötü durum — tüm segmentler ışın tarafından kesilirse.
        public List<WallSegment> CollectAlongRay(float ox, float oy, float ex, float ey)
        {
            var result = new List<WallSegment>();
            CollectRayRecursive(_root, ox, oy, ex, ey, result); // O(log N + k)
            return result;
        }

        // O(log N + k):
        //   Işın bölme düzlemini kesmiyorsa → tek dal, O(log N) derinlik.
        //   Işın her iki tarafa geçiyorsa    → her iki dal ziyaret edilir (+k).
        // Her düğümde 2× O(1) ClassifyPoint + O(1) result.Add yapılır.
        private void CollectRayRecursive(BspNode? node, float ox, float oy,
                                          float ex, float ey, List<WallSegment> result)
        {
            if (node == null) return;                                    // O(1)
            int sOrigin = ClassifyPoint(ox, oy, node.Partition);        // O(1)
            int sEnd = ClassifyPoint(ex, ey, node.Partition);        // O(1)
            result.Add(node.Partition);                                  // O(1) amortized
            if (sOrigin != sEnd)
            {
                // Işın düzlemi kesiyor → her iki alt dal da ziyaret edilir (+k maliyeti)
                CollectRayRecursive(node.Front, ox, oy, ex, ey, result);
                CollectRayRecursive(node.Back, ox, oy, ex, ey, result);
            }
            else if (sOrigin >= 0) CollectRayRecursive(node.Front, ox, oy, ex, ey, result);
            else CollectRayRecursive(node.Back, ox, oy, ex, ey, result);
        }

        // O(N) — GetVisibleWalls O(N) + her duvar için O(1) PointSegDist.

        public bool IsPointBlocked(float px, float py)
        {
            foreach (var w in GetVisibleWalls(px, py)) // O(N) iterasyon
                if (PointSegDist(px, py, w) < 4f) return true; // O(1)
            return false;
        }

        // O(1) — sabit sayıda aritmetik işlem ve bir karekök hesabı.
        private float PointSegDist(float px, float py, WallSegment w)
        {
            float dx = w.X2 - w.X1, dy = w.Y2 - w.Y1, lenSq = dx * dx + dy * dy;
            if (lenSq == 0) return MathF.Sqrt((px - w.X1) * (px - w.X1) + (py - w.Y1) * (py - w.Y1));
            float t = Math.Clamp(((px - w.X1) * dx + (py - w.Y1) * dy) / lenSq, 0f, 1f);
            float cx = w.X1 + t * dx, cy = w.Y1 + t * dy;
            return MathF.Sqrt((px - cx) * (px - cx) + (py - cy) * (py - cy));
        }

        // O(N) — tüm düğümler özyinelemeli ziyaret edilir; her biri O(1) Max çağrısı.
        // Özyineleme yığını: O(log N) ort., O(N) en kötü durum (dengesiz ağaç).
        public int GetHeight() => HeightRecursive(_root);
        private int HeightRecursive(BspNode? node) =>
            node == null ? 0 : 1 + Math.Max(HeightRecursive(node.Front), HeightRecursive(node.Back));
    }
}