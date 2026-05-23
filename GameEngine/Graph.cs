using System;
using System.Collections.Generic;

namespace GameEngine.DataStructures
{
    public class GraphNode
    {
        public int Id;
        public float X, Y;
        public bool IsWalkable;

        public GraphNode(int id, float x, float y, bool isWalkable = true)
        {
            Id = id;
            X = x;
            Y = y;
            IsWalkable = isWalkable;
        }

        public override string ToString()
        {
            return $"Node[{Id}] ({X},{Y}) Walkable={IsWalkable}";
        }
    }

    public class Edge
    {
        public int FromId, ToId;
        public float Weight;

        public Edge(int fromId, int toId, float weight)
        {
            FromId = fromId;
            ToId = toId;
            Weight = weight;
        }

        public override string ToString()
        {
            return $"Edge[{FromId} -> {ToId}, Weight={Weight}]";
        }
    }

    // Zaman Karmasikligi: O(V + E)
    // Uzay Karmasikligi: O(V + E)
    public class Graph
    {
        private Dictionary<int, List<Edge>> _adjacencyList;

        public Graph()
        {
            _adjacencyList = new Dictionary<int, List<Edge>>();
        }

        // Graf'a yeni bir node ekler
        public void AddNode(GraphNode node)
        {
            if (!_adjacencyList.ContainsKey(node.Id))
            {
                _adjacencyList[node.Id] = new List<Edge>();
            }
        }

        // Iki node arasina kenar ekler
        // Cift yonlu baglanti kurmayi saglar
        public void AddEdge(int fromId, int toId, float weight, bool bidirectional = true)
        {
            if (!_adjacencyList.ContainsKey(fromId) || !_adjacencyList.ContainsKey(toId))
            {
                throw new Exception("Node Bulunamamaktadir");
            }

            _adjacencyList[fromId].Add(new Edge(fromId, toId, weight));

            if (bidirectional)
            {
                _adjacencyList[toId].Add(new Edge(toId, fromId, weight));
            }
        }

        // Node id'sine ait komsu kenarlari dondurur
        public List<Edge> GetNeighbors(int nodeId)
        {
            if (!_adjacencyList.ContainsKey(nodeId))
            {
                throw new Exception("Node Bulunamamaktadir");
            }

            return _adjacencyList[nodeId];
        }

        // Node olup olmadigini kontrol eder
        public bool HasNode(int nodeId)
        {
            return _adjacencyList.ContainsKey(nodeId);
        }

        // Toplam node sayisini dondurur
        public int GetNodeCount()
        {
            return _adjacencyList.Count;
        }

        // Node siler
        public void RemoveNode(int nodeId)
        {
            if (!_adjacencyList.ContainsKey(nodeId))
                return;

            _adjacencyList.Remove(nodeId);

            foreach (var edges in _adjacencyList.Values)
            {
                edges.RemoveAll(e => e.ToId == nodeId);
            }
        }

        // Edge siler
        public void RemoveEdge(int fromId, int toId)
        {
            if (_adjacencyList.ContainsKey(fromId))
            {
                _adjacencyList[fromId].RemoveAll(e => e.ToId == toId);
            }

            if (_adjacencyList.ContainsKey(toId))
            {
                _adjacencyList[toId].RemoveAll(e => e.ToId == fromId);
            }
        }
    }
}