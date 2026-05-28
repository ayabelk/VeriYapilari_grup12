using System;
using System.Collections.Generic;
using System.Linq;

namespace a_i.DataStructures
{
    /// <summary>
    /// Graf (Adjacency List)
    /// Yürünebilir alanları temsil eder.
    ///
    /// === GENEL ZAMAN KARMAŞIKLIĞI ===
    /// AddNode      : O(1) ort. — Dictionary hash ekleme.
    ///                O(V) en kötü durum — hash çakışması (rehash).
    /// GetNode      : O(1) ort. — Dictionary hash arama.
    /// GetAllNodes  : O(V) — tüm node'lar ToList() ile kopyalanır.
    /// AddEdge      : O(1) ort. — Liste başına ekleme + Dictionary erişimi.
    ///                O(V) en kötü durum — rehash tetiklenirse.
    /// GetNeighbors : O(1) ort. — Dictionary erişimi, liste referansı döner.
    /// RemoveNode   : O(V + E) — tüm kenar listeleri taranır.
    /// RemoveEdge   : O(deg(u) + deg(v)) — iki listenin kenarları taranır.
    /// GetEdgeCount : O(V) — tüm adjacency listeleri toplanır.
    /// HasNode      : O(1) ort. — Dictionary ContainsKey.
    ///
    /// === UZAY KARMAŞIKLIĞI ===
    /// O(V + E) — V düğüm + E kenar saklanır.
    /// Çift yönlü kenarlarda E yerine 2E kenar nesnesi oluşur.
    /// </summary>
    public class GraphNode
    {
        public int Id;
        public float X, Y;
        public bool IsWalkable;

        // O(1) — dört alan sabit sürede atanır.
        public GraphNode(int id, float x, float y, bool isWalkable = true)
        { Id = id; X = x; Y = y; IsWalkable = isWalkable; }

        public override string ToString() => $"Node[{Id}] ({X},{Y}) Walkable={IsWalkable}";
    }

    public class Edge
    {
        public int FromId, ToId;
        public float Weight;

        // O(1) — üç alan sabit sürede atanır.
        public Edge(int fromId, int toId, float weight)
        { FromId = fromId; ToId = toId; Weight = weight; }

        public override string ToString() => $"Edge[{FromId} -> {ToId}, Weight={Weight}]";
    }

    public class Graph
    {
        private readonly Dictionary<int, GraphNode> _nodes;
        private readonly Dictionary<int, List<Edge>> _adjacencyList;

        // O(1) — iki boş Dictionary sabit sürede oluşturulur.
        public Graph()
        {
            _nodes = new Dictionary<int, GraphNode>();
            _adjacencyList = new Dictionary<int, List<Edge>>();
        }

        // O(1) ort. — ContainsKey + iki Dictionary yazma işlemi, her biri O(1) ort.
        // O(V) en kötü durum — load factor aşılır ve Dictionary rehash yaparsa.
        public void AddNode(GraphNode node)
        {
            if (!_nodes.ContainsKey(node.Id))                       // O(1) ort.
            { _nodes[node.Id] = node; _adjacencyList[node.Id] = new List<Edge>(); } // O(1) ort.
        }

        // O(1) ort. — Dictionary hash ile doğrudan erişim.
        // O(V) en kötü durum — çok sayıda hash çakışması varsa (pratikte nadir).
        public GraphNode GetNode(int nodeId)
        {
            if (!_nodes.ContainsKey(nodeId)) throw new Exception("Node bulunamadi"); // O(1) ort.
            return _nodes[nodeId];                                                     // O(1) ort.
        }

        // O(V) — _nodes.Values tüm V düğümü tek geçişle ToList()'e kopyalar.
        // Çağıran her seferinde yeni bir liste tahsis edilir; sık çağrılmamalı.
        public List<GraphNode> GetAllNodes() => _nodes.Values.ToList();

        // O(1) ort. — iki Dictionary erişimi + bir veya iki List.Add (O(1) amortized).
        // O(V) en kötü durum — rehash tetiklenirse.
        // Çift yönlü kenarda iki Edge nesnesi oluşturulur; uzay maliyeti 2× artar.
        public void AddEdge(int fromId, int toId, float weight, bool bidirectional = true)
        {
            if (!_nodes.ContainsKey(fromId) || !_nodes.ContainsKey(toId))
                throw new Exception("Node bulunamadi");                    // O(1) ort.
            _adjacencyList[fromId].Add(new Edge(fromId, toId, weight));   // O(1) amortized
            if (bidirectional) _adjacencyList[toId].Add(new Edge(toId, fromId, weight)); // O(1) amortized
        }

        // O(1) ort. — Dictionary erişimi; listenin referansı döner, kopyalanmaz.
        // Not: Dönen liste üzerinde yapılan değişiklikler orijinal listeyi etkiler.
        public List<Edge> GetNeighbors(int nodeId)
        {
            if (!_adjacencyList.ContainsKey(nodeId)) throw new Exception("Node bulunamadi"); // O(1) ort.
            return _adjacencyList[nodeId];                                                     // O(1) ort.
        }

        // O(1) ort. — Dictionary ContainsKey hash araması.
        public bool HasNode(int nodeId) => _nodes.ContainsKey(nodeId);

        // O(1) — Dictionary.Count önceden tutulan sayacı döner.
        public int GetNodeCount() => _nodes.Count;

        // O(V) — her adjacency listesinin eleman sayısı toplanır (V liste gezilir).
        // Toplam kenar sayısı: yönsüz grafta 2E, yönlü grafta E.
        public int GetEdgeCount() => _adjacencyList.Values.Sum(e => e.Count);

        // O(V + E) — iki aşamalı işlem:
        //   1) _nodes ve _adjacencyList'ten silme: O(1) ort.
        //   2) Kalan tüm V düğümün kenar listesi RemoveAll ile taranır: O(E) toplam.
        //      Her RemoveAll kendi listesinde O(deg(v)) çalışır; toplam O(E).
        public void RemoveNode(int nodeId)
        {
            if (!_nodes.ContainsKey(nodeId)) return;              // O(1) ort.
            _nodes.Remove(nodeId);                                 // O(1) ort.
            _adjacencyList.Remove(nodeId);                         // O(1) ort.
            foreach (var edges in _adjacencyList.Values)           // O(V) döngü
                edges.RemoveAll(e => e.ToId == nodeId);            // O(deg(v)) her liste için
        }                                                          // Toplam: O(V + E)

        // O(deg(fromId) + deg(toId)) — yalnızca iki listenin kenarları taranır.
        // RemoveAll her listede doğrusal tarama yapar: O(deg(u)) + O(deg(v)).
        // En kötü durum: tam bağlı grafta deg(v) = V-1 → O(V) per çağrı.
        public void RemoveEdge(int fromId, int toId)
        {
            if (_adjacencyList.ContainsKey(fromId))                          // O(1) ort.
                _adjacencyList[fromId].RemoveAll(e => e.ToId == toId);       // O(deg(fromId))
            if (_adjacencyList.ContainsKey(toId))                            // O(1) ort.
                _adjacencyList[toId].RemoveAll(e => e.ToId == fromId);       // O(deg(toId))
        }
    }
}