using System;
using System.Collections.Generic;

namespace GameEngine.DataStructures
{
    public class WallSegment
    {
        public float X1, Y1, X2, Y2;

        public WallSegment(float x1, float y1, float x2, float y2)
        {
            X1 = x1; Y1 = y1;
            X2 = x2; Y2 = y2;
        }

        public override string ToString()
        {
            return $"Wall[({X1},{Y1}) -> ({X2},{Y2})]";
        }
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

    // Zaman Karmasikligi: O(n log n) ortalama
    // Uzay Karmasikligi: O(n)
    public class BspTree
    {
        private BspNode _root;

        public BspTree()
        {
            _root = null;
        }

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
                int side = ClassifySegment(walls[i], partition);
                if (side >= 0)
                    frontList.Add(walls[i]);
                else
                    backList.Add(walls[i]);
            }

            node.Front = BuildRecursive(frontList);
            node.Back = BuildRecursive(backList);

            return node;
        }

        private int ClassifySegment(WallSegment wall, WallSegment partition)
        {
            float dx = partition.X2 - partition.X1;
            float dy = partition.Y2 - partition.Y1;
            float d1 = dx * (wall.Y1 - partition.Y1) - dy * (wall.X1 - partition.X1);
            float d2 = dx * (wall.Y2 - partition.Y1) - dy * (wall.X2 - partition.X1);
            return (int)(d1 + d2);
        }

        public List<WallSegment> GetVisibleWalls(float viewX, float viewY)
        {
            List<WallSegment> result = new List<WallSegment>();
            TraverseNode(_root, viewX, viewY, result);
            return result;
        }

        private void TraverseNode(BspNode node, float vx, float vy,
                                   List<WallSegment> result)
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

        public int GetHeight()
        {
            return HeightRecursive(_root);
        }

        private int HeightRecursive(BspNode node)
        {
            if (node == null) return 0;
            return 1 + Math.Max(HeightRecursive(node.Front),
                                HeightRecursive(node.Back));
        }
    }
}