using GameEngine.DataStructures;

// BSP Tree testi
Console.WriteLine("BSP Tree testi basliyor...");

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

Console.WriteLine("Test tamamlandi!");