using a_i.Algorithms;
using a_i.DataStructures;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Point = a_i.Algorithms.Point;

namespace a_i.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiController : ControllerBase
    {
        private static BspTree? _bspTree;
        private static Graph? _graph;
        private static AStarPathfinder? _pathfinder;
        private static LineOfSight? _los;
        private static Raycasting? _raycasting;
        private static bool _initialized = false;

        private static readonly Dictionary<int, AiDecision> _decisions = new();
        private static readonly object _lock = new();

        // POST /api/ai/init
        [HttpPost("init")]
        public IActionResult Init([FromBody] MapInitRequest req)
        {
            _bspTree = new BspTree();
            _bspTree.Build(req.Walls.Select(w => new WallSegment(w.X1, w.Y1, w.X2, w.Y2)).ToList());

            _graph = new Graph();
            foreach (var n in req.Nodes) _graph.AddNode(new GraphNode(n.Id, n.X, n.Y));
            foreach (var e in req.Edges) _graph.AddEdge(e.From, e.To, e.Cost);

            _pathfinder = new AStarPathfinder(_graph);
            _los = new LineOfSight(_bspTree);      // Line of Sight algoritması
            _raycasting = new Raycasting(_bspTree);       // Raycasting algoritması
            _initialized = true;

            Console.WriteLine($"[AI] Init: {req.Walls.Count} duvar, {req.Nodes.Count} dugum, {req.Edges.Count} kenar");
            return Ok(new { success = true, wallCount = req.Walls.Count, nodeCount = req.Nodes.Count });
        }

        // POST /api/ai/decide — A* + Raycasting ile düşman kararı
        [HttpPost("decide")]
        public IActionResult Decide([FromBody] DecideRequest req)
        {
            if (!_initialized || _raycasting == null || _pathfinder == null || _graph == null)
                return Ok(new AiDecision { EnemyId = req.EnemyId, Action = "patrol" });

            // Raycasting ile görüş testi — HasLineOfSight BSP CollectAlongRay kullanır
            bool canSee = _raycasting.HasLineOfSight(
                new Point(req.EnemyX, req.EnemyY),
                new Point(req.PlayerX, req.PlayerY));
            float dist = Dist(req.EnemyX, req.EnemyY, req.PlayerX, req.PlayerY);

            string action; int alertLevel;
            var path = new List<PathPoint>();

            if (canSee && dist < 250f)
            {
                action = "chase"; alertLevel = 3;
                int fromId = NearestNode(req.EnemyX, req.EnemyY);
                int toId = NearestNode(req.PlayerX, req.PlayerY);
                // A* pathfinding — O((V+E) log V)
                path = _pathfinder.FindPath(fromId, toId).Take(5)
                       .Select(id => new PathPoint { X = _graph.GetNode(id).X, Y = _graph.GetNode(id).Y })
                       .ToList();
            }
            else if (dist < 150f) { action = "alert"; alertLevel = 2; }
            else { action = "patrol"; alertLevel = 0; }

            var decision = new AiDecision
            { EnemyId = req.EnemyId, Action = action, AlertLevel = alertLevel, Path = path, Timestamp = DateTime.UtcNow };

            lock (_lock) { _decisions[req.EnemyId] = decision; }
            Console.WriteLine($"[AI] Dusman {req.EnemyId}: {action} (mesafe:{dist:F0}, gorus:{canSee})");
            return Ok(decision);
        }

        // GET /api/ai/fov — Raycasting ile FOV konisi hesabı
        // Zaman: O(R * log W) — R=ışın sayısı, W=duvar sayısı
        [HttpGet("fov")]
        public IActionResult ComputeFOV([FromQuery] float ex, [FromQuery] float ey,
                                         [FromQuery] float angle = 0f, [FromQuery] float fov = 90f,
                                         [FromQuery] int rays = 60, [FromQuery] float dist = 180f)
        {
            if (_raycasting == null) return BadRequest("Init edilmedi");

            // Raycasting.CastFOV — BSP destekli çoklu ışın
            var hits = _raycasting.CastFOV(new Point(ex, ey), angle * MathF.PI / 180f, fov, rays, dist);
            var points = hits.Select(p => new { x = p.X, y = p.Y, dist = Dist(ex, ey, p.X, p.Y), hitWall = true }).ToList();
            return Ok(new { origin = new { x = ex, y = ey }, fovPoints = points });
        }

        // GET /api/ai/path — A* pathfinding
        [HttpGet("path")]
        public IActionResult FindPath([FromQuery] float fx, [FromQuery] float fy, [FromQuery] float tx, [FromQuery] float ty)
        {
            if (_pathfinder == null || _graph == null) return BadRequest("Init edilmedi");
            var path = _pathfinder.FindPath(NearestNode(fx, fy), NearestNode(tx, ty));
            return Ok(new
            {
                found = path.Count > 0,
                stepCount = path.Count,
                path = path.Select(id => new { x = _graph.GetNode(id).X, y = _graph.GetNode(id).Y })
            });
        }

        [HttpGet("decisions")]
        public IActionResult GetDecisions() { lock (_lock) { return Ok(_decisions.Values.ToList()); } }

        private int NearestNode(float x, float y)
        {
            int best = 0; float bd = float.MaxValue;
            foreach (var n in _graph!.GetAllNodes())
            { float d = (n.X - x) * (n.X - x) + (n.Y - y) * (n.Y - y); if (d < bd) { bd = d; best = n.Id; } }
            return best;
        }

        private float Dist(float x1, float y1, float x2, float y2)
        { float dx = x2 - x1, dy = y2 - y1; return MathF.Sqrt(dx * dx + dy * dy); }
    }

    public class WallDto { public float X1 { get; set; } public float Y1 { get; set; } public float X2 { get; set; } public float Y2 { get; set; } }
    public class NodeDto { public int Id { get; set; } public float X { get; set; } public float Y { get; set; } }
    public class EdgeDto { public int From { get; set; } public int To { get; set; } public float Cost { get; set; } }
    public class MapInitRequest { public List<WallDto> Walls { get; set; } = new(); public List<NodeDto> Nodes { get; set; } = new(); public List<EdgeDto> Edges { get; set; } = new(); }
    public class DecideRequest { public int EnemyId { get; set; } public float EnemyX { get; set; } public float EnemyY { get; set; } public float PlayerX { get; set; } public float PlayerY { get; set; } public string State { get; set; } = "patrol"; }
    public class PathPoint { public float X { get; set; } public float Y { get; set; } }
    public class AiDecision { public int EnemyId { get; set; } public string Action { get; set; } = "patrol"; public int AlertLevel { get; set; } public List<PathPoint> Path { get; set; } = new(); public DateTime Timestamp { get; set; } }
}