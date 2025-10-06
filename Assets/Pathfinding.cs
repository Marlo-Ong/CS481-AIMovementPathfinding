using UnityEngine;
using System.Collections.Generic;

public class AStar
{
    private Grid grid;
    private List<Node> openSet;
    private HashSet<Node> closedSet;

    public void FindPath(Grid grid, Vector3 startPos, Vector3 targetPos)
    {
        this.grid = grid;
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node target = grid.NodeFromWorldPoint(targetPos);

        this.openSet = new();
        this.closedSet = new();

        this.openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            // Get open node with lowest f cost
            Node current = GetLowestFCostNode(this.openSet);

            // Close node
            this.openSet.Remove(current);
            this.closedSet.Add(current);

            // Check if target found
            if (current == target)
                break;

            // Evaluate non-closed, non-obstacle neighbors
            foreach (Node neighbor in this.GetNeighbors(current))
            {
                if (!neighbor.walkable || this.closedSet.Contains(neighbor))
                    continue;

                // Check if this node needs to be added/updated (unexplored node, or found lower g-cost)
                int newNeighborGCost = current.gCost + this.GetDistance(current, neighbor);
                if (newNeighborGCost < neighbor.gCost || !this.openSet.Contains(neighbor))
                {
                    // Update costs
                    neighbor.gCost = newNeighborGCost;
                    neighbor.hCost = GetDistance(neighbor, target);
                    neighbor.parent = current;

                    // Open node
                    if (!this.openSet.Contains(neighbor))
                        this.openSet.Add(neighbor);
                }
            }
        }

        return this.RetracePath(start, target);
    }

    private List<Node> RetracePath(Node start, Node end)
    {
        List<Node> path = new List<Node>();
        Node current = end;

        while (current != start)
        {
            path.Add(current);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }

    private int GetDistance(Node a, Node b)
    {
        // Prioritize diagonal moves from A
        //      (with simplified length floor(10 * sqrt(2)))
        // to get to the same axis as B, then use straight-line distance
        //      (with simplified length floor(10 * 1))

        Vector2 diff = b - a;
        return diff.x > diff.y
            ? 14 * diff.y + 10 * (diff.x - diff.y)
            : 14 * diff.x + 10 * (diff.y - diff.x);
    }

    private IEnumerable<Node> GetNeighbors(Node node)
    {
        IEnumerable<Node> neighbors = new();
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int newX = node.x + i;
                int newY = node.y + j;

                // Find neighboring nodes only
                if (newX == 0 && newY == 0)
                    continue;

                if (this.grid.TryGetNode(newX, newY, out Node neighbor))
                    neighbors.Add(neighbor);
            }
        }
    }

    private Node GetLowestFCostNode(IEnumerable<Node> set)
    {
        if (set.Count < 1)
            return null;

        Node lowest = set[0];
        foreach (Node node in set)
        {
            if (node.fCost < lowest.fCost || node.fCost == lowest.fCost && node.hCost < lowest.hCost)
                lowest = node;
        }
        return lowest;
    }
}