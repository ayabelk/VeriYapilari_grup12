using GameEngine.DataStructures;

Console.WriteLine("====================================");
Console.WriteLine("MINHEAP BRANCH - GAMEENGINE TESTLERI");
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

var neighbors = graph.GetNeighbors(2);

Console.WriteLine("Node 2 komsulari:");
foreach (var edge in neighbors)
{
    Console.WriteLine(edge);
}

Console.WriteLine("[OK] Graph testi tamamlandi.");
Console.WriteLine();


// ===============================
// 3. MINHEAP TESTI
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

Console.WriteLine("Tum testler tamamlandi.");