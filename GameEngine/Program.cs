using GameEngine.Algorithms;
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



// Raycasting testi

Console.WriteLine("\nRaycasting testi basliyor...");

var rc = new Raycasting(tree);

var origin = new Point(5f, 5f);
var hit = rc.CastRay(origin, 0f, 100f);
Console.WriteLine($"Saga isin -> vurus: ({hit.X:F2}, {hit.Y:F2})");

var hit2 = rc.CastRay(origin, (float)(Math.PI / 2), 100f);
Console.WriteLine($"Yukari isin -> vurus: ({hit2.X:F2}, {hit2.Y:F2})");

bool los = rc.HasLineOfSight(new Point(2f, 2f), new Point(8f, 8f));
Console.WriteLine($"(2,2) -> (8,8) gorus var mi? {los}");

var fov = rc.CastFOV(origin, 0f, 60f, 10, 100f);
Console.WriteLine($"FOV: {fov.Count} isin atildi");

Console.WriteLine("Raycasting testi tamamlandi!");