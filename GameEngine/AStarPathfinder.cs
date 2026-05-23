using System;
using System.Collections.Generic;
using GameEngine.DataStructures;

namespace GameEngine.Algorithms
{
    public class AStarPathfinder
    {
        private readonly Graph _graph;

        public AStarPathfinder(Graph graph)
        {
            _graph = graph;
        }

        public List<int> FindPath(int startId, int goalId)
        {
            if (!_graph.HasNode(startId))
            {
                throw new Exception("Baslangic node'u graph icinde bulunamadi.");
            }

            if (!_graph.HasNode(goalId))
            {
                throw new Exception("Hedef node graph icinde bulunamadi.");
            }

            GraphNode startNode = _graph.GetNode(startId);
            GraphNode goalNode = _graph.GetNode(goalId);

            if (!startNode.IsWalkable || !goalNode.IsWalkable)
            {
                return new List<int>();
            }

            MinHeap<AStarRecord> openSet = new MinHeap<AStarRecord>();

            Dictionary<int, float> gCost = new Dictionary<int, float>();
            Dictionary<int, int> cameFrom = new Dictionary<int, int>();
            HashSet<int> closedSet = new HashSet<int>();

            gCost[startId] = 0;

            float startHeuristic = CalculateHeuristic(startNode, goalNode);
            openSet.Insert(new AStarRecord(startId, 0, startHeuristic));

            while (!openSet.IsEmpty())
            {
                AStarRecord currentRecord = openSet.ExtractMin();
                int currentId = currentRecord.NodeId;

                if (closedSet.Contains(currentId))
                {
                    continue;
                }

                if (currentId == goalId)
                {
                    return ReconstructPath(cameFrom, startId, goalId);
                }

                closedSet.Add(currentId);

                foreach (Edge edge in _graph.GetNeighbors(currentId))
                {
                    int neighborId = edge.ToId;

                    if (closedSet.Contains(neighborId))
                    {
                        continue;
                    }

                    GraphNode neighborNode = _graph.GetNode(neighborId);

                    if (!neighborNode.IsWalkable)
                    {
                        continue;
                    }

                    float currentGCost = gCost[currentId];
                    float tentativeGCost = currentGCost + edge.Weight;

                    if (!gCost.ContainsKey(neighborId) || tentativeGCost < gCost[neighborId])
                    {
                        cameFrom[neighborId] = currentId;
                        gCost[neighborId] = tentativeGCost;

                        float hCost = CalculateHeuristic(neighborNode, goalNode);
                        openSet.Insert(new AStarRecord(neighborId, tentativeGCost, hCost));
                    }
                }
            }

            return new List<int>();
        }

        private List<int> ReconstructPath(Dictionary<int, int> cameFrom, int startId, int goalId)
        {
            List<int> path = new List<int>();
            int currentId = goalId;

            path.Add(currentId);

            while (currentId != startId)
            {
                if (!cameFrom.ContainsKey(currentId))
                {
                    return new List<int>();
                }

                currentId = cameFrom[currentId];
                path.Add(currentId);
            }

            path.Reverse();
            return path;
        }

        private float CalculateHeuristic(GraphNode fromNode, GraphNode toNode)
        {
            float dx = fromNode.X - toNode.X;
            float dy = fromNode.Y - toNode.Y;

            return MathF.Sqrt(dx * dx + dy * dy);
        }

        private class AStarRecord : IComparable<AStarRecord>
        {
            public int NodeId { get; }
            public float GCost { get; }
            public float HCost { get; }
            public float FCost
            {
                get { return GCost + HCost; }
            }

            public AStarRecord(int nodeId, float gCost, float hCost)
            {
                NodeId = nodeId;
                GCost = gCost;
                HCost = hCost;
            }

            public int CompareTo(AStarRecord? other)
            {
                if (other == null)
                {
                    return -1;
                }

                int fCostComparison = FCost.CompareTo(other.FCost);

                if (fCostComparison != 0)
                {
                    return fCostComparison;
                }

                return HCost.CompareTo(other.HCost);
            }
        }
    }
}