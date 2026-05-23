using System;
using System.Collections.Generic;
using System.Linq;

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

    // Graph veri yapisi:
    // - Node'lari _nodes icinde saklar.
    // - Kenarlari _adjacencyList icinde saklar.
    // Zaman Karmasikligi: O(V + E)
    // Uzay Karmasikligi: O(V + E)
    public class Graph
    {
        private readonly Dictionary<int, GraphNode> _nodes;
        private readonly Dictionary<int, List<Edge>> _adjacencyList;

        public Graph()
        {
            _nodes = new Dictionary<int, GraphNode>();
            _adjacencyList = new Dictionary<int, List<Edge>>();
        }

        // Graf'a yeni bir node ekler
        public void AddNode(GraphNode node)
        {
            if (!_nodes.ContainsKey(node.Id))
            {
                _nodes[node.Id] = node;
                _adjacencyList[node.Id] = new List<Edge>();
            }
        }

        // Node id'sine gore node'u dondurur
        // A* algoritmasi X, Y ve IsWalkable bilgilerine buradan ulasacak
        public GraphNode GetNode(int nodeId)
        {
            if (!_nodes.ContainsKey(nodeId))
            {
                throw new Exception("Node bulunamadi");
            }

            return _nodes[nodeId];
        }

        // Tum node'lari dondurur
        public List<GraphNode> GetAllNodes()
        {
            return _nodes.Values.ToList();
        }

        // Iki node arasina kenar ekler
        // Varsayilan olarak cift yonlu baglanti kurar
        public void AddEdge(int fromId, int toId, float weight, bool bidirectional = true)
        {
            if (!_nodes.ContainsKey(fromId) || !_nodes.ContainsKey(toId))
            {
                throw new Exception("Node bulunamadi");
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
                throw new Exception("Node bulunamadi");
            }

            return _adjacencyList[nodeId];
        }

        // Node olup olmadigini kontrol eder
        public bool HasNode(int nodeId)
        {
            return _nodes.ContainsKey(nodeId);
        }

        // Toplam node sayisini dondurur
        public int GetNodeCount()
        {
            return _nodes.Count;
        }

        // Toplam edge sayisini dondurur
        public int GetEdgeCount()
        {
            return _adjacencyList.Values.Sum(edges => edges.Count);
        }

        // Node siler
        public void RemoveNode(int nodeId)
        {
            if (!_nodes.ContainsKey(nodeId))
                return;

            _nodes.Remove(nodeId);
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