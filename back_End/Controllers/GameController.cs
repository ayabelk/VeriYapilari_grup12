using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using back_End.DataStructures;
using back_End.Algorithms;

namespace back_End.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private static GeneratedMap? _currentMap;
        private static BspTree? _bspTree;
        private static LineOfSight? _los;
        private static List<EnemyState> _enemies = new();
        private static PlayerState _player = new();
        private static readonly HttpClient _http = new HttpClient();
        private static readonly string AI_URL =
           Environment.GetEnvironmentVariable("AI_URL") ?? "http://localhost:5051";

        // POST /api/game/generate — O(cols*rows) + O(W log W) BSP inşası
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateMap([FromQuery] int cols = 16,
                                                      [FromQuery] int rows = 12,
                                                      [FromQuery] int? seed = null)
        {
            var gen = new MapGenerator(seed);
            _currentMap = gen.Generate(cols, rows);

            _bspTree = new BspTree();
            _bspTree.Build(_currentMap.Walls);
            _los = new LineOfSight(_bspTree);

            _enemies = _currentMap.Enemies.Select((e, i) => new EnemyState
            { Id = i, X = e.X, Y = e.Y, Angle = i * 90f, State = "patrol", PatrolAngle = i * 90f }).ToList();

            _player = new PlayerState { X = _currentMap.PlayerStart.X, Y = _currentMap.PlayerStart.Y };

            // a_i servisine harita gönder (asenkron)
            _ = Task.Run(async () =>
            {
                try
                {
                    var edges = new List<object>();
                    foreach (var n in _currentMap.WaypointGraph.GetAllNodes())
                        foreach (var e in _currentMap.WaypointGraph.GetNeighbors(n.Id))
                            if (e.ToId > n.Id)
                                edges.Add(new { from = n.Id, to = e.ToId, cost = e.Weight });

                    await _http.PostAsJsonAsync($"{AI_URL}/api/ai/init", new
                    {
                        walls = _currentMap.Walls.Select(w => new { x1 = w.X1, y1 = w.Y1, x2 = w.X2, y2 = w.Y2 }),
                        nodes = _currentMap.WaypointGraph.GetAllNodes().Select(n => new { id = n.Id, x = n.X, y = n.Y }),
                        edges
                    });
                    Console.WriteLine("[Backend] a_i servisine harita gonderildi");
                }
                catch { Console.WriteLine("[Backend] a_i servisi calismiyor"); }
            });

            return Ok(new
            {
                gridWidth = _currentMap.GridWidth,
                gridHeight = _currentMap.GridHeight,
                wallCount = _currentMap.Walls.Count,
                nodeCount = _currentMap.WaypointGraph.GetNodeCount(),
                edgeCount = _currentMap.WaypointGraph.GetEdgeCount(),
                playerStart = new { x = _currentMap.PlayerStart.X, y = _currentMap.PlayerStart.Y },
                goal = new { x = _currentMap.Goal.X, y = _currentMap.Goal.Y },
                enemies = _enemies.Select(e => new { e.Id, x = e.X, y = e.Y }),
                walls = _currentMap.Walls.Select(w => new { x1 = w.X1, y1 = w.Y1, x2 = w.X2, y2 = w.Y2 })
            });
        }

        // POST /api/game/update
        [HttpPost("update")]
        public async Task<IActionResult> UpdateGame([FromBody] UpdateRequest req)
        {
            if (_currentMap == null || _los == null) return BadRequest("Once /generate cagirin");

            if (!_bspTree!.IsPointBlocked(req.PlayerX, req.PlayerY))
            { _player.X = req.PlayerX; _player.Y = req.PlayerY; }

            foreach (var enemy in _enemies)
            {
                AiDecision? decision = null;
                try
                {
                    var resp = await _http.PostAsJsonAsync($"{AI_URL}/api/ai/decide", new
                    { enemyId = enemy.Id, enemyX = enemy.X, enemyY = enemy.Y, playerX = _player.X, playerY = _player.Y, state = enemy.State });
                    decision = await resp.Content.ReadFromJsonAsync<AiDecision>();
                }
                catch { }

                if (decision?.Action == "chase" && decision.Path?.Count > 1)
                {
                    enemy.State = "chase";
                    var next = decision.Path[1];
                    float dx = next.X - enemy.X, dy = next.Y - enemy.Y;
                    float d = MathF.Sqrt(dx * dx + dy * dy);
                    if (d > 1f) { enemy.X += (dx / d) * 2.5f; enemy.Y += (dy / d) * 2.5f; enemy.Angle = MathF.Atan2(dy, dx) * 180f / MathF.PI; }
                }
                else
                {
                    enemy.State = decision?.Action ?? "patrol";
                    enemy.PatrolAngle += 1.5f;
                    float rad = enemy.PatrolAngle * MathF.PI / 180f;
                    float newX = enemy.X + MathF.Cos(rad) * 0.8f;
                    float newY = enemy.Y + MathF.Sin(rad) * 0.8f;
                    if (!_bspTree!.IsPointBlocked(newX, newY)) { enemy.X = newX; enemy.Y = newY; }
                    else enemy.PatrolAngle += 90f;
                    enemy.Angle = enemy.PatrolAngle;
                }
            }

            bool caught = _enemies.Any(e => { float dx = e.X - _player.X, dy = e.Y - _player.Y; return MathF.Sqrt(dx * dx + dy * dy) < 20f; });
            float gdx = _currentMap.Goal.X - _player.X, gdy = _currentMap.Goal.Y - _player.Y;
            bool reached = MathF.Sqrt(gdx * gdx + gdy * gdy) < 25f;

            return Ok(new
            {
                enemies = _enemies.Select(e => new { e.Id, x = e.X, y = e.Y, angle = e.Angle, state = e.State }),
                caught,
                reached,
                playerX = _player.X,
                playerY = _player.Y
            });
        }

        [HttpGet("map")]
        public IActionResult GetMap()
        {
            if (_currentMap == null) return NotFound("Henuz harita uretilmedi");
            return Ok(new
            {
                gridWidth = _currentMap.GridWidth,
                gridHeight = _currentMap.GridHeight,
                walls = _currentMap.Walls.Select(w => new { x1 = w.X1, y1 = w.Y1, x2 = w.X2, y2 = w.Y2 }),
                playerStart = new { x = _currentMap.PlayerStart.X, y = _currentMap.PlayerStart.Y },
                goal = new { x = _currentMap.Goal.X, y = _currentMap.Goal.Y },
                enemies = _enemies.Select(e => new { e.Id, x = e.X, y = e.Y })
            });
        }

        [HttpGet("visible")]
        public IActionResult IsVisible([FromQuery] float fx, [FromQuery] float fy, [FromQuery] float tx, [FromQuery] float ty)
        {
            if (_los == null) return BadRequest("Once /generate cagirin");
            return Ok(new { visible = _los.IsVisible(fx, fy, tx, ty) });
        }

        
        // GET /api/game/team
        [HttpGet("team")]
        public IActionResult GetTeamMembers()
        {
            return Ok(new[]
            {
        new { name = "Rojin Topuz",           id = "032390078" },
        new { name = "Arda Inanc",            id = "032390080" },
        new { name = "Hasan Emre Kartal",     id = "032390081" },
        new { name = "Beyzanur Postlu",       id = "032390082" },
        new { name = "Selsabil Aya Belkabla", id = "032390092" },
        
    });
        }
        public class EnemyState { public int Id { get; set; } public float X { get; set; } public float Y { get; set; } public float Angle { get; set; } public string State { get; set; } = "patrol"; public float PatrolAngle { get; set; } }
        public class PlayerState { public float X { get; set; } public float Y { get; set; } }
        public class UpdateRequest { public float PlayerX { get; set; } public float PlayerY { get; set; } }
        public class PathPoint { public float X { get; set; } public float Y { get; set; } }
        public class AiDecision { public int EnemyId { get; set; } public string Action { get; set; } = "patrol"; public int AlertLevel { get; set; } public List<PathPoint> Path { get; set; } = new(); }
    }
}