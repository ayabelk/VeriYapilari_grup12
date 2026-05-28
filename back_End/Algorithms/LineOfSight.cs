using System;
using System.Collections.Generic;
using a_i.DataStructures;
using back_End.DataStructures;

namespace back_End.Algorithms
{
    /// <summary>
    /// Görüş Çizgisi ve Raycasting Algoritması
    /// BSP ağacı ile hızlandırılmış ışın-duvar kesişim hesabı.
    ///
    /// === GENEL ZAMAN KARMAŞIKLIĞI ===
    /// IsVisible      : O(log N + k) ort. — CollectAlongRay O(log N + k),
    ///                  her duvar için O(1) TryIntersect; erken çıkış mümkün.
    ///                  O(N) en kötü durum — tüm duvarlar ışın üzerindeyse.
    /// ComputeFOVRays : O(R * (log N + k)) — R ışın × CastRay O(log N + k).
    ///                  O(R * N) en kötü durum.
    /// CastRay        : O(log N + k) — CollectAlongRay O(log N + k)
    ///                  + AddFirst O(k) + Traverse O(k).
    /// TryIntersect   : O(1) — sabit sayıda aritmetik işlem.
    ///
    /// === UZAY KARMAŞIKLIĞI ===
    /// O(R) — DynamicArray R sonuç saklar.
    /// O(k) — CastRay içindeki candidates LinkedList k eleman tutar.
    ///
    /// N = duvar sayısı, k = ışının kestiği aday duvar sayısı, R = ışın sayısı
    /// </summary>
    public class RayHit
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Dist { get; set; }
        public bool Hit { get; set; }
    }

    public class LineOfSight
    {
        private readonly BspTree _bsp;

        // O(1) — yalnızca BSP referansı atanır.
        public LineOfSight(BspTree bsp) { _bsp = bsp; }

        // O(log N + k) ort. — CollectAlongRay O(log N + k),
        // her aday için O(1) TryIntersect; ilk kesişimde erken çıkış.
        public bool IsVisible(float fx, float fy, float tx, float ty)
        {
            float dx = tx - fx, dy = ty - fy;
            float dist = MathF.Sqrt(dx * dx + dy * dy); // O(1)
            if (dist < 1e-6f) return true;               // O(1) — aynı nokta erken çıkış
            float ndx = dx / dist, ndy = dy / dist;      // O(1) — normalize

            // O(log N + k) — BSP ile ışın boyunca duvar adayları toplanır
            foreach (var w in _bsp.CollectAlongRay(fx, fy, tx, ty))
                if (TryIntersect(fx, fy, ndx, ndy, w, out float t) && t > 0 && t < dist - 2f)
                    return false; // O(1) — ilk çarpmada erken çıkış
            return true;
        }

        // O(R * (log N + k)) — R ışın için CastRay tekrarlanır.
        // DynamicArray.Add O(1) amortized; R ekleme → O(R) toplam.
        public DynamicArray<RayHit> ComputeFOVRays(float ox, float oy, float angleDeg,
                                                    float fov = 90f, int rays = 60, float maxDist = 200f)
        {
            var hits = new DynamicArray<RayHit>(rays); // O(1) — kapasite önceden ayrılır
            float start = angleDeg - fov / 2f;         // O(1)
            float step = fov / (rays - 1);             // O(1)

            // O(R) iterasyon; her biri O(log N + k) CastRay çağrısı içerir
            for (int i = 0; i < rays; i++)
            {
                float rad = (start + i * step) * MathF.PI / 180f;                   // O(1)
                hits.Add(CastRay(ox, oy, MathF.Cos(rad), MathF.Sin(rad), maxDist)); // O(log N + k)
            }
            return hits;
        }

        // O(log N + k) — üç aşamalı işlem:
        //   1) CollectAlongRay : O(log N + k) — k aday duvar döner
        //   2) AddFirst × k    : O(k) — LinkedList başa ekleme
        //   3) Traverse × k    : O(k) — her eleman için O(1) TryIntersect
        private RayHit CastRay(float rx, float ry, float rdx, float rdy, float maxDist)
        {
            float closest = maxDist;
            float hx = rx + rdx * maxDist, hy = ry + rdy * maxDist; // O(1) — varsayılan uç
            bool hit = false;

            // O(k) — her aday O(1) AddFirst ile LinkedList başına eklenir
            var candidates = new CustomLinkedList<WallSegment>(); // O(1)
            foreach (var w in _bsp.CollectAlongRay(rx, ry, rx + rdx * maxDist, ry + rdy * maxDist))
                candidates.AddFirst(w); // O(1) × k = O(k) toplam

            // O(k) — her aday için O(1) TryIntersect; en yakın isabet güncellenir
            candidates.Traverse(w =>
            {
                if (TryIntersect(rx, ry, rdx, rdy, w, out float t) && t > 0 && t < closest)
                { closest = t; hx = rx + rdx * t; hy = ry + rdy * t; hit = true; } // O(1)
            });

            return new RayHit { X = hx, Y = hy, Dist = closest, Hit = hit }; // O(1)
        }

        // O(1) — sabit sayıda aritmetik; paralel ışın kontrolü erken false döner.
        // Parametrik form: t = ışın parametresi, u = duvar parametresi ∈ [0,1].
        private bool TryIntersect(float rx, float ry, float rdx, float rdy,
                                   WallSegment w, out float t)
        {
            t = 0;
            float wx = w.X2 - w.X1, wy = w.Y2 - w.Y1;                  // O(1)
            float denom = rdx * wy - rdy * wx;                            // O(1) — çapraz çarpım
            if (MathF.Abs(denom) < 1e-6f) return false;                  // O(1) — paralel ışın
            float u = ((w.X1 - rx) * rdy - (w.Y1 - ry) * rdx) / denom;  // O(1)
            t = ((w.X1 - rx) * wy - (w.Y1 - ry) * wx) / denom;          // O(1)
            return t >= 0 && u >= 0 && u <= 1;                           // O(1) — aralık kontrolü
        }
    }
}