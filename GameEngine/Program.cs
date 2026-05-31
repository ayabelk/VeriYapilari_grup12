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

// ===============================
// 5. A* SENTETIK HARITA TESTLERI
// ===============================
Console.WriteLine("5) A* Sentetik Harita Testleri basliyor...");
Console.WriteLine();

// --- Test 5.1: Duz yol, engel yok ---
Console.WriteLine("Test 5.1) Duz yol (engelsiz)");

Graph straightGraph = new Graph();

straightGraph.AddNode(new GraphNode(1, 0, 0));
straightGraph.AddNode(new GraphNode(2, 1, 0));
straightGraph.AddNode(new GraphNode(3, 2, 0));
straightGraph.AddNode(new GraphNode(4, 3, 0));

straightGraph.AddEdge(1, 2, 1.0f);
straightGraph.AddEdge(2, 3, 1.0f);
straightGraph.AddEdge(3, 4, 1.0f);

AStarPathfinder straightFinder = new AStarPathfinder(straightGraph);
List<int> straightPath = straightFinder.FindPath(1, 4);

Console.WriteLine("Beklenen: 1 -> 2 -> 3 -> 4");
Console.WriteLine("Bulunan:  " + (straightPath.Count > 0 ? string.Join(" -> ", straightPath) : "Yol bulunamadi"));
Console.WriteLine(straightPath.Count == 4 ? "[OK] GECTI" : "[!!] BASARISIZ");
Console.WriteLine();


// --- Test 5.2: Duvar etrafinda dolasma ---
Console.WriteLine("Test 5.2) Duvar etrafinda dolasma");

// Harita:
// 1 - 2 - 3
//     |   |
//     4   |
//     |   |
// 7 - 5 - 6
// 1'den 6'ya gidilecek, 4 dugumu yurunemez oldugu icin etrafindan dolasmasi beklenir.

Graph wallGraph = new Graph();

wallGraph.AddNode(new GraphNode(1, 0, 0));
wallGraph.AddNode(new GraphNode(2, 1, 0));
wallGraph.AddNode(new GraphNode(3, 2, 0));
wallGraph.AddNode(new GraphNode(4, 1, 1, isWalkable: false));
wallGraph.AddNode(new GraphNode(5, 1, 2));
wallGraph.AddNode(new GraphNode(6, 2, 2));
wallGraph.AddNode(new GraphNode(7, 0, 2));

wallGraph.AddEdge(1, 2, 1.0f);
wallGraph.AddEdge(2, 3, 1.0f);
wallGraph.AddEdge(2, 4, 1.0f);
wallGraph.AddEdge(4, 5, 1.0f);
wallGraph.AddEdge(5, 6, 1.0f);
wallGraph.AddEdge(5, 7, 1.0f);
wallGraph.AddEdge(3, 6, 2.0f);

AStarPathfinder wallFinder = new AStarPathfinder(wallGraph);
List<int> wallPath = wallFinder.FindPath(1, 6);

bool noWallInPath = !wallPath.Contains(4);

Console.WriteLine("Beklenen: Node 4 kullanilmadan yol bulunmali");
Console.WriteLine("Bulunan:  " + (wallPath.Count > 0 ? string.Join(" -> ", wallPath) : "Yol bulunamadi"));
Console.WriteLine(noWallInPath && wallPath.Count > 0 ? "[OK] GECTI" : "[!!] BASARISIZ");
Console.WriteLine();


// --- Test 5.3: Ulasilamaz hedef ---
Console.WriteLine("Test 5.3) Ulasilamaz hedef (izole node)");

Graph isolatedGraph = new Graph();

isolatedGraph.AddNode(new GraphNode(1, 0, 0));
isolatedGraph.AddNode(new GraphNode(2, 1, 0));
isolatedGraph.AddNode(new GraphNode(3, 5, 5));

isolatedGraph.AddEdge(1, 2, 1.0f);

AStarPathfinder isolatedFinder = new AStarPathfinder(isolatedGraph);
List<int> isolatedPath = isolatedFinder.FindPath(1, 3);

Console.WriteLine("Beklenen: Bos liste (yol yok)");
Console.WriteLine("Bulunan:  " + (isolatedPath.Count == 0 ? "Bos liste" : string.Join(" -> ", isolatedPath)));
Console.WriteLine(isolatedPath.Count == 0 ? "[OK] GECTI" : "[!!] BASARISIZ");
Console.WriteLine();


// --- Test 5.4: Hedef node gecilmez ---
Console.WriteLine("Test 5.4) Hedef node gecilmez (IsWalkable=false)");

Graph blockedGraph = new Graph();

blockedGraph.AddNode(new GraphNode(1, 0, 0));
blockedGraph.AddNode(new GraphNode(2, 1, 0));
blockedGraph.AddNode(new GraphNode(3, 2, 0, isWalkable: false));

blockedGraph.AddEdge(1, 2, 1.0f);
blockedGraph.AddEdge(2, 3, 1.0f);

AStarPathfinder blockedFinder = new AStarPathfinder(blockedGraph);
List<int> blockedPath = blockedFinder.FindPath(1, 3);

Console.WriteLine("Beklenen: Bos liste (hedef gecilmez)");
Console.WriteLine("Bulunan:  " + (blockedPath.Count == 0 ? "Bos liste" : string.Join(" -> ", blockedPath)));
Console.WriteLine(blockedPath.Count == 0 ? "[OK] GECTI" : "[!!] BASARISIZ");
Console.WriteLine();

Console.WriteLine("Tum testler tamamlandi.");