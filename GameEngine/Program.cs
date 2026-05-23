using GameEngine.DataStructures;
using GameEngine.Algorithms;

Console.WriteLine("====================================");
Console.WriteLine("FAZ 1 + A* PATHFINDING TESTLERI");
Console.WriteLine("====================================");
Console.WriteLine();


// ===============================
// 1. BSP TREE TESTI
// ===============================
Console.WriteLine("1) BSP Tree testi basliyor...");

var walls = new List<WallSegment>
{
    new WallSegment(0, 0, 10, 0),
    new WallSegment(10, 0, 10, 10),
    new WallSegment(0, 10, 10, 10),
    new WallSegment(0, 0, 0, 10)
};

BspTree tree = new BspTree();
tree.Build(walls);

Console.WriteLine($"Agac yuksekligi: {tree.GetHeight()}");

var visibleWalls = tree.GetVisibleWalls(5, 5);
Console.WriteLine($"Gorunen duvar sayisi: {visibleWalls.Count}");

foreach (var wall in visibleWalls)
{
    Console.WriteLine(wall);
}

Console.WriteLine("[OK] BSP Tree testi tamamlandi.");
Console.WriteLine();


// ===============================
// 2. GRAPH TESTI
// ===============================
Console.WriteLine("2) Graph testi basliyor...");

Graph graph = new Graph();

graph.AddNode(new GraphNode(1, 0, 0));
graph.AddNode(new GraphNode(2, 5, 5));
graph.AddNode(new GraphNode(3, 10, 10));

graph.AddEdge(1, 2, 7);
graph.AddEdge(2, 3, 5);

Console.WriteLine($"Node sayisi: {graph.GetNodeCount()}");
Console.WriteLine($"Edge sayisi: {graph.GetEdgeCount()}");

var neighbors = graph.GetNeighbors(2);

Console.WriteLine("Node 2 komsulari:");
foreach (var edge in neighbors)
{
    Console.WriteLine(edge);
}

Console.WriteLine("[OK] Graph testi tamamlandi.");
Console.WriteLine();


// ===============================
// 3. MIN HEAP TESTI
// ===============================
Console.WriteLine("3) MinHeap testi basliyor...");

MinHeap<int> heap = new MinHeap<int>();

heap.Insert(10);
heap.Insert(3);
heap.Insert(7);
heap.Insert(1);
heap.Insert(5);

Console.WriteLine("MinHeap elemanlari kucukten buyuge cikariliyor:");

while (!heap.IsEmpty())
{
    Console.Write(heap.ExtractMin() + " ");
}

Console.WriteLine();
Console.WriteLine("[OK] MinHeap testi tamamlandi.");
Console.WriteLine();


// ===============================
// 4. A* PATHFINDING TESTI
// ===============================
Console.WriteLine("4) A* Pathfinding testi basliyor...");

Graph pathGraph = new Graph();

// Kucuk test haritasi:
//
// 1 ---- 2 ---- 3
// |      |      |
// 4 ---- 5 ---- 6
//
// En kisa mantikli yol: 1 -> 2 -> 3 -> 6

pathGraph.AddNode(new GraphNode(1, 0, 0));
pathGraph.AddNode(new GraphNode(2, 1, 0));
pathGraph.AddNode(new GraphNode(3, 2, 0));
pathGraph.AddNode(new GraphNode(4, 0, 1));
pathGraph.AddNode(new GraphNode(5, 1, 1));
pathGraph.AddNode(new GraphNode(6, 2, 1));

pathGraph.AddEdge(1, 2, 1);
pathGraph.AddEdge(2, 3, 1);
pathGraph.AddEdge(3, 6, 1);

pathGraph.AddEdge(1, 4, 1);
pathGraph.AddEdge(4, 5, 2);
pathGraph.AddEdge(5, 6, 2);

pathGraph.AddEdge(2, 5, 2);

AStarPathfinder pathfinder = new AStarPathfinder(pathGraph);

int startNodeId = 1;
int goalNodeId = 6;

List<int> path = pathfinder.FindPath(startNodeId, goalNodeId);

Console.WriteLine($"Baslangic Node: {startNodeId}");
Console.WriteLine($"Hedef Node: {goalNodeId}");

if (path.Count == 0)
{
    Console.WriteLine("Yol bulunamadi.");
}
else
{
    Console.WriteLine("Bulunan Yol:");
    Console.WriteLine(string.Join(" -> ", path));
}

Console.WriteLine("[OK] A* Pathfinding testi tamamlandi.");
Console.WriteLine();

Console.WriteLine("Tum testler tamamlandi.");