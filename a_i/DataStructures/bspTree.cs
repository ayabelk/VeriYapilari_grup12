using System;
using System.Collections.Generic;

namespace a_i.DataStructures
{
    /// <summary>
    /// Zaman Karmaşıklığı: 
    /// - İnşa Etme (Build): Ortalama $O(n \log n)$, En Kötü $O(n^2)$
    /// - Görünürlük Sorgusu (Traversal): $O(\log n)$
    /// Uzay Karmaşıklığı: $O(n)$ (Her duvar segmenti için bir düğüm)
    /// </summary>
    public class WallSegment
    {
        public float X1, Y1, X2, Y2;

        public WallSegment(float x1, float y1, float x2, float y2)
        {
            X1 = x1; Y1 = y1;
            X2 = x2; Y2 = y2;
        }

        public override string ToString() => $"Wall[({X1},{Y1}) -> ({X2},{Y2})]";
    }

    public class BspNode
    {
        public WallSegment Partition;
        public List<WallSegment> Walls;
        public BspNode Front;
        public BspNode Back;

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
        private BspNode _root;

        public void Build(List<WallSegment> walls)
        {
            if (walls == null || walls.Count == 0) return;
            _root = BuildRecursive(walls);
        }

        private BspNode BuildRecursive(List<WallSegment> walls)
        {
            if (walls.Count == 0) return null;

            WallSegment partition = walls[0];
            BspNode node = new BspNode(partition);

            List<WallSegment> frontList = new List<WallSegment>();
            List<WallSegment> backList = new List<WallSegment>();

            for (int i = 1; i < walls.Count; i++)
            {
                // PR DOKUNUŞU: Sadece side kontrolünü daha detaylı hale getirdik
                int side = ClassifySegmentDetail(walls[i], partition, out WallSegment frontPart, out WallSegment backPart);

                if (side == 1) // Tamamen Önde
                    frontList.Add(walls[i]);
                else if (side == -1) // Tamamen Arkada
                    backList.Add(walls[i]);
                else if (side == 2) // KRİTİK: Duvar bölündü! Her iki tarafa da ekle
                {
                    frontList.Add(frontPart);
                    backList.Add(backPart);
                }
                // side == 0 (Düzlem üzerinde) ise listelere eklemene gerek yok, 
                // zaten partition ile aynı hizada demektir.
            }

            node.Front = BuildRecursive(frontList);
            node.Back = BuildRecursive(backList);
            return node;
        }

        // PR Geliştirmesi: Duvarın hangi tarafta olduğunu hesaplar, kesişirse böler.
        private int ClassifySegmentDetail(WallSegment wall, WallSegment partition, out WallSegment front, out WallSegment back)
        {
            front = null; back = null;
            float dx = partition.X2 - partition.X1;
            float dy = partition.Y2 - partition.Y1;

            // Noktaların düzleme uzaklıkları (Cross Product mantığı)
            float d1 = dx * (wall.Y1 - partition.Y1) - dy * (wall.X1 - partition.X1);
            float d2 = dx * (wall.Y2 - partition.Y1) - dy * (wall.X2 - partition.X1);

            if (d1 > 0.01f && d2 > 0.01f) return 1;  // Tamamen Ön
            if (d1 < -0.01f && d2 < -0.01f) return -1; // Tamamen Arka

            // Eğer biri artı biri eksi ise duvar kesişiyor demektir
            if (d1 * d2 < 0)
            {
                // Kesim noktasını (t) hesapla
                float t = d1 / (d1 - d2);
                float splitX = wall.X1 + t * (wall.X2 - wall.X1);
                float splitY = wall.Y1 + t * (wall.Y2 - wall.Y1);

                front = new WallSegment(wall.X1, wall.Y1, splitX, splitY);
                back = new WallSegment(splitX, splitY, wall.X2, wall.Y2);
                return 2; // Bölündü durumu
            }

            return 0; // Düzlem üzerinde
        }

        // SENİN KODUN: Bu kısımlara hiç dokunmadık, aynen çalışmaya devam edecek
        public List<WallSegment> GetVisibleWalls(float viewX, float viewY)
        {
            List<WallSegment> result = new List<WallSegment>();
            TraverseNode(_root, viewX, viewY, result);
            return result;
        }

        private void TraverseNode(BspNode node, float vx, float vy, List<WallSegment> result)
        {
            if (node == null) return;
            int side = ClassifyPoint(vx, vy, node.Partition);

            if (side >= 0)
            {
                TraverseNode(node.Back, vx, vy, result);
                result.Add(node.Partition);
                TraverseNode(node.Front, vx, vy, result);
            }
            else
            {
                TraverseNode(node.Front, vx, vy, result);
                result.Add(node.Partition);
                TraverseNode(node.Back, vx, vy, result);
            }
        }

        private int ClassifyPoint(float px, float py, WallSegment partition)
        {
            float dx = partition.X2 - partition.X1;
            float dy = partition.Y2 - partition.Y1;
            float d = dx * (py - partition.Y1) - dy * (px - partition.X1);
            return d >= 0 ? 1 : -1;
        }

        public int GetHeight() => HeightRecursive(_root);

        private int HeightRecursive(BspNode node)
        {
            if (node == null) return 0;
            return 1 + Math.Max(HeightRecursive(node.Front), HeightRecursive(node.Back));
        }
    }
}