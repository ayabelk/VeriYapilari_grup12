using System;
using System.Collections.Generic;
using back_End.DataStructures;

namespace back_End.Algorithms
{
    // O(1) — sabit sayıda alan tahsisi; Liste ve Graf başlatma O(1) amortized.
    public class GeneratedMap
    {
        public List<WallSegment> Walls { get; set; } = new();
        public Graph WaypointGraph { get; set; } = new();
        public (float X, float Y) PlayerStart { get; set; }
        public (float X, float Y) Goal { get; set; }
        public List<(float X, float Y)> Enemies { get; set; } = new();
        public int GridWidth { get; set; }
        public int GridHeight { get; set; }
    }

    /// <summary>
    /// Programatik Rastgele Harita Üretici
    ///
    /// === GENEL ZAMAN KARMAŞIKLIĞI ===
    /// Generate    : O(C*R) — tüm alt adımlar C*R üzerinde doğrusal çalışır.
    /// BuildGrid   : O(C*R) — her hücre bir kez işlenir.
    /// BuildWalls  : O(C*R) — her hücre en fazla 4 kenar üretir → O(4*C*R) = O(C*R).
    /// AddBorder   : O(1)  — sabit 4 segment eklenir.
    /// BuildGraph  : O(C*R) — düğüm ekleme O(C*R) + kenar ekleme O(C*R).
    /// FindFree    : O(C*R) — en kötü durum tüm grid taranır.
    /// FindRandom  : O(1)  — en fazla 100 deneme; sabit iterasyon sayısı.
    ///
    /// === UZAY KARMAŞIKLIĞI ===
    /// O(C*R) — grid dizisi + Graf (V = C*R düğüm, E ≤ 2*C*R kenar) + duvar listesi.
    /// Duvar sayısı en kötü durum O(C*R); bordür sabit O(1) ek alan.
    ///
    /// C = cols, R = rows
    /// </summary>
    public class MapGenerator
    {
        private readonly Random _rng;

        // O(1) — Random nesnesi sabit sürede oluşturulur.
        public MapGenerator(int? seed = null) { _rng = seed.HasValue ? new Random(seed.Value) : new Random(); }

        // O(C*R) — beş alt adımın toplamı; her biri en fazla O(C*R).
        // Düşman sayısı ec sabit (2-4 arası) → FindRandom O(1) × ec = O(1).
        public GeneratedMap Generate(int cols = 16, int rows = 12, float cellSize = 40f, float density = 0.25f)
        {
            var map = new GeneratedMap { GridWidth = cols, GridHeight = rows }; // O(1)
            bool[,] grid = new bool[cols, rows];                                // O(C*R) tahsis
            BuildGrid(grid, cols, rows, density);          // O(C*R)
            BuildWalls(map.Walls, grid, cols, rows, cellSize); // O(C*R)
            AddBorder(map.Walls, cols, rows, cellSize);    // O(1)
            BuildGraph(map.WaypointGraph, grid, cols, rows, cellSize); // O(C*R)
            map.PlayerStart = FindFree(grid, cols, rows, cellSize, true);  // O(C*R) en kötü
            map.Goal = FindFree(grid, cols, rows, cellSize, false); // O(C*R) en kötü
            int ec = _rng.Next(2, 5);                                      // O(1)
            for (int i = 0; i < ec; i++)                                   // O(1) — ec ≤ 4
                map.Enemies.Add(FindRandom(grid, cols, rows, cellSize));   // O(1) her çağrı
            return map;
        }

        // O(C*R) — iç içe iki döngü; her hücre bir kez ziyaret edilir.
        // Her hücrede O(1) safeZone kontrolü + O(1) NextDouble çağrısı.
        private void BuildGrid(bool[,] g, int c, int r, float d)
        {
            for (int x = 0; x < c; x++) for (int y = 0; y < r; y++)
            {
                bool safeZone = (x <= 2 && y <= 2) || (x >= c - 3 && y >= r - 3);
                g[x, y] = !safeZone && _rng.NextDouble() < d;
            }

            // Oyuncu başlangıcından hedefe giden yolu garantile
            // Sol üstten sağ alta düz koridor aç
            for (int x = 0; x < c; x++) g[x, r / 2] = false; // yatay koridor
            for (int y = 0; y < r; y++) g[c / 2, y] = false; // dikey koridor
        }

        // O(C*R) — her dolu hücre en fazla 4 komşu kontrolü yapar → O(4*C*R) = O(C*R).
        // Üretilen duvar sayısı: O(C*R) en kötü durum (tüm hücreler dolu ve izole).
        // Her ws.Add O(1) amortized.
        private void BuildWalls(List<WallSegment> ws, bool[,] g, int c, int r, float cs)
        {
            for (int x = 0; x < c; x++) for (int y = 0; y < r; y++)
            {
                if (!g[x, y]) continue;                              // O(1) — boş hücre atla
                float x0 = x * cs, y0 = y * cs, x1 = x0 + cs, y1 = y0 + cs;
                // Her yön: O(1) sınır + komşu kontrolü; kenar yalnızca açık kenara eklenir
                if (y == 0 || !g[x, y - 1]) ws.Add(new WallSegment(x0, y0, x1, y0)); // O(1)
                if (y == r - 1 || !g[x, y + 1]) ws.Add(new WallSegment(x0, y1, x1, y1)); // O(1)
                if (x == 0 || !g[x - 1, y]) ws.Add(new WallSegment(x0, y0, x0, y1)); // O(1)
                if (x == c - 1 || !g[x + 1, y]) ws.Add(new WallSegment(x1, y0, x1, y1)); // O(1)
            }
        }

        // O(1) — sabit 4 segment; harita boyutundan bağımsız.
        private void AddBorder(List<WallSegment> ws, int c, int r, float cs)
        {
            float w = c * cs, h = r * cs;
            ws.Add(new WallSegment(0, 0, w, 0)); ws.Add(new WallSegment(0, h, w, h)); // O(1)
            ws.Add(new WallSegment(0, 0, 0, h)); ws.Add(new WallSegment(w, 0, w, h)); // O(1)
        }

        // O(C*R) — iki aşama:
        //   1) Düğüm ekleme: C*R hücre × O(1) AddNode = O(C*R).
        //   2) Kenar ekleme: C*R hücre × 4 yön × O(1) AddEdge = O(C*R).
        //      Her kenar yalnızca nx>x || ny>y koşuluyla bir kez eklenir (çift sayım önlenir).
        // Toplam kenar: E ≤ 2*C*R (yatay + dikey) → Graf uzayı O(C*R).
        private void BuildGraph(Graph graph, bool[,] g, int c, int r, float cs)
        {
            int Id(int x, int y) => y * c + x; // O(1) — doğrusal indeksleme
            float half = cs / 2f;

            // O(C*R) — boş hücreler düğüm olarak eklenir
            for (int x = 0; x < c; x++) for (int y = 0; y < r; y++)
                if (!g[x, y]) graph.AddNode(new GraphNode(Id(x, y), x * cs + half, y * cs + half)); // O(1) ort.

            int[] dx = { 1, -1, 0, 0 }, dy = { 0, 0, 1, -1 };

            // O(C*R) — her boş hücre için 4 komşu kontrol edilir; kenar bir kez eklenir
            for (int x = 0; x < c; x++) for (int y = 0; y < r; y++)
            {
                if (g[x, y]) continue; // O(1) — dolu hücre atla
                for (int d = 0; d < 4; d++) // O(1) — sabit 4 yön
                {
                    int nx = x + dx[d], ny = y + dy[d];
                    if (nx < 0 || nx >= c || ny < 0 || ny >= r || g[nx, ny]) continue; // O(1)
                    if (nx > x || ny > y) graph.AddEdge(Id(x, y), Id(nx, ny), cs);     // O(1) ort.
                }
            }
        }

        // O(C*R) en kötü durum — tüm grid sol üstten (veya sağ alttan) taranır.
        // O(1) en iyi durum — ilk hücre boşsa hemen döner.
        private (float X, float Y) FindFree(bool[,] g, int c, int r, float cs, bool fromStart)
        {
            float half = cs / 2f;
            // fromStart=true → sol üstten, false → sağ alttan tarama
            if (fromStart)
            { for (int x = 0; x < c; x++) for (int y = 0; y < r; y++) if (!g[x, y]) return (x * cs + half, y * cs + half); }
            else
            { for (int x = c - 1; x >= 0; x--) for (int y = r - 1; y >= 0; y--) if (!g[x, y]) return (x * cs + half, y * cs + half); }
            return (half, half); // O(1) — hiç boş hücre yoksa fallback
        }

        // O(1) — en fazla 100 rastgele deneme; sabit iterasyon sayısı.
        // Her denemede 5× O(1) grid erişimi yapılır.
        // Not: Yoğun haritada 100 denemeden hiçbiri geçemezse fallback (half, half) döner.
        private (float X, float Y) FindRandom(bool[,] g, int c, int r, float cs)
        {
            float half = cs / 2f;
            for (int i = 0; i < 100; i++) // sabit 100 iterasyon → O(1)
            {
                int x = _rng.Next(2, c - 2), y = _rng.Next(2, r - 2); // O(1)
                // 5 komşu kontrolü — düşmanın sıkışmaması için çevre de boş olmalı
                if (!g[x, y] && !g[x + 1, y] && !g[x - 1, y] && !g[x, y + 1] && !g[x, y - 1])
                    return (x * cs + half, y * cs + half); // O(1)
            }
            return (half, half); // O(1) fallback
        }
    }
}