using System;
using System.Collections.Generic;
using GameEngine.DataStructures;

namespace GameEngine.Algorithms
{
    // README'de tanimli ortak Point tipi.
    // TODO: Ortak bir CommonTypes.cs olusturulursa oraya tasinmali.
    public class Point
    {
        public float X, Y;
        public Point(float x, float y) { X = x; Y = y; }
    }

    // --------------------------------------------------------------
    // Raycasting Algoritmasi
    // --------------------------------------------------------------
    // Zaman Karmasikligi:
    //   - Tek isin: Ortalama O(log W + k); W = toplam duvar, k = aday duvar
    //               En kotu: O(W) (isin tum bolgeleri kesiyorsa)
    //   - FOV taramasi: O(R * (log W + k));  R = isin sayisi (FOV_RAYS)
    // Uzay Karmasikligi: O(R + W)
    // --------------------------------------------------------------
    public class Raycasting
    {
        private BspTree _bspTree;

        public Raycasting(BspTree bspTree)
        {
            _bspTree = bspTree;
        }

        // Tek bir isin gonderir, en yakin duvar kesisimini dondurur.
        // origin   : isinin baslangici (oyuncu/dusman konumu)
        // angleRad : isin yonu (radyan)
        // maxDist  : maksimum tarama mesafesi
        public Point CastRay(Point origin, float angleRad, float maxDist)
        {
            float dx = (float)Math.Cos(angleRad);
            float dy = (float)Math.Sin(angleRad);

            // Engel yoksa donulecek nokta (isin ucu)
            Point endPoint = new Point(
                origin.X + dx * maxDist,
                origin.Y + dy * maxDist);

            // BSP'den yalnizca isinin gectigi bolgelerdeki duvarlari aliyoruz
            List<WallSegment> candidates = _bspTree.CollectAlongRay(
                origin.X, origin.Y, endPoint.X, endPoint.Y);

            float closestT = maxDist;
            Point hitPoint = endPoint;

            foreach (var wall in candidates)
            {
                if (TryIntersectSegment(origin, dx, dy, wall, out float t, out Point p))
                {
                    if (t > 0 && t < closestT)
                    {
                        closestT = t;
                        hitPoint = p;
                    }
                }
            }
            return hitPoint;
        }

        // FOV konisi icinde birden cok isin gonderir (dusman gorus alani).
        public List<Point> CastFOV(Point origin, float facingRad,
            float fovDeg, int rayCount, float maxDist)
        {
            List<Point> hits = new List<Point>();
            float fovRad = fovDeg * (float)Math.PI / 180f;
            float startAngle = facingRad - fovRad / 2f;
            float step = (rayCount > 1) ? fovRad / (rayCount - 1) : 0f;

            for (int i = 0; i < rayCount; i++)
            {
                float angle = startAngle + step * i;
                hits.Add(CastRay(origin, angle, maxDist));
            }
            return hits;
        }

        // Iki nokta arasinda gorus var mi? (Line of Sight)
        // Ornek: dusman, oyuncuyu goruyor mu?
        public bool HasLineOfSight(Point from, Point to)
        {
            float dxAbs = to.X - from.X;
            float dyAbs = to.Y - from.Y;
            float dist = (float)Math.Sqrt(dxAbs * dxAbs + dyAbs * dyAbs);
            float ang = (float)Math.Atan2(dyAbs, dxAbs);

            Point hit = CastRay(from, ang, dist);
            float ex = hit.X - to.X;
            float ey = hit.Y - to.Y;
            return (ex * ex + ey * ey) < 1e-3f;
        }

        // Isin (parametric) ile duvar segmentinin kesisimini hesaplar.
        // Lineer sistem cozumu: origin + t*(dx,dy)  =  P3 + u*(P4-P3)
        private bool TryIntersectSegment(Point origin, float dx, float dy,
            WallSegment wall, out float t, out Point p)
        {
            t = 0; p = new Point(0, 0);

            float x3 = wall.X1, y3 = wall.Y1;
            float x4 = wall.X2, y4 = wall.Y2;

            float denom = dx * (y4 - y3) - dy * (x4 - x3);
            if (Math.Abs(denom) < 1e-6f) return false;  // paralel

            float tRay = ((x3 - origin.X) * (y4 - y3) - (y3 - origin.Y) * (x4 - x3)) / denom;
            float uSeg = ((x3 - origin.X) * dy - (y3 - origin.Y) * dx) / denom;

            if (tRay >= 0f && uSeg >= 0f && uSeg <= 1f)
            {
                t = tRay;
                p = new Point(origin.X + dx * tRay, origin.Y + dy * tRay);
                return true;
            }
            return false;
        }
    }
}