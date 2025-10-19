using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ImbroglioCombat.Core;

namespace ImbroglioCombat.AI
{
    public class Pathfinder : MonoBehaviour
    {
        public List<HexTile> FindPath(HexTile start, HexTile goal, HexGrid grid)
        {
            if (start == null || goal == null || grid == null)
            {
                return null;
            }

            // A* pathfinding algorithm
            Dictionary<HexTile, HexTile> cameFrom = new Dictionary<HexTile, HexTile>();
            Dictionary<HexTile, int> costSoFar = new Dictionary<HexTile, int>();

            PriorityQueue<HexTile> frontier = new PriorityQueue<HexTile>();
            frontier.Enqueue(start, 0);

            cameFrom[start] = null;
            costSoFar[start] = 0;

            while (frontier.Count > 0)
            {
                HexTile current = frontier.Dequeue();

                if (current == goal)
                {
                    break;
                }

                foreach (HexTile neighbor in grid.GetNeighbors(current))
                {
                    if (!neighbor.isWalkable)
                    {
                        continue;
                    }

                    int newCost = costSoFar[current] + 1;

                    if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                    {
                        costSoFar[neighbor] = newCost;
                        int priority = newCost + Heuristic(neighbor, goal);
                        frontier.Enqueue(neighbor, priority);
                        cameFrom[neighbor] = current;
                    }
                }
            }

            // Reconstruct path
            if (!cameFrom.ContainsKey(goal))
            {
                return null; // No path found
            }

            List<HexTile> path = new List<HexTile>();
            HexTile currentTile = goal;

            while (currentTile != start)
            {
                path.Add(currentTile);
                currentTile = cameFrom[currentTile];
            }

            path.Reverse();
            return path;
        }

        int Heuristic(HexTile a, HexTile b)
        {
            return HexGrid.GetDistance(a.gridPosition, b.gridPosition);
        }

        public List<HexTile> GetReachableTiles(HexTile start, int maxRange, HexGrid grid)
        {
            if (start == null || grid == null)
            {
                return new List<HexTile>();
            }

            List<HexTile> reachable = new List<HexTile>();
            Queue<HexTile> frontier = new Queue<HexTile>();
            Dictionary<HexTile, int> distance = new Dictionary<HexTile, int>();

            frontier.Enqueue(start);
            distance[start] = 0;

            while (frontier.Count > 0)
            {
                HexTile current = frontier.Dequeue();

                foreach (HexTile neighbor in grid.GetNeighbors(current))
                {
                    if (!neighbor.isWalkable || distance.ContainsKey(neighbor))
                    {
                        continue;
                    }

                    int newDistance = distance[current] + 1;

                    if (newDistance <= maxRange)
                    {
                        distance[neighbor] = newDistance;
                        frontier.Enqueue(neighbor);
                        reachable.Add(neighbor);
                    }
                }
            }

            return reachable;
        }
    }

    // Simple priority queue implementation
    public class PriorityQueue<T>
    {
        private List<PriorityQueueNode<T>> nodes = new List<PriorityQueueNode<T>>();

        public int Count => nodes.Count;

        public void Enqueue(T item, int priority)
        {
            nodes.Add(new PriorityQueueNode<T>(item, priority));
            nodes = nodes.OrderBy(n => n.Priority).ToList();
        }

        public T Dequeue()
        {
            if (nodes.Count == 0)
            {
                return default(T);
            }

            T item = nodes[0].Item;
            nodes.RemoveAt(0);
            return item;
        }

        public bool Contains(T item)
        {
            return nodes.Any(n => EqualityComparer<T>.Default.Equals(n.Item, item));
        }
    }

    public class PriorityQueueNode<T>
    {
        public T Item { get; set; }
        public int Priority { get; set; }

        public PriorityQueueNode(T item, int priority)
        {
            Item = item;
            Priority = priority;
        }
    }
}

