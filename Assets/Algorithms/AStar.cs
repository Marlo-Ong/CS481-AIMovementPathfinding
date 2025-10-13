using System.Collections.Generic;
using Unity.Mathematics;

public class AStar : IAlgorithm
{
    private Grid grid;
    private List<Node> openSet;
    private HashSet<Node> closedSet;

    public AStar(Grid grid)
    {
        this.grid = grid;
        this.openSet = new();
        this.closedSet = new();
    }

    public List<Node> FindPath((int x, int y) startPos, (int x, int y) targetPos)
    {
        if (targetPos.x > this.grid.sizeX || targetPos.y > this.grid.sizeY)
            return new List<Node>();

        this.openSet.Clear();
        this.closedSet.Clear();

        Node start = this.grid.GetNode(startPos);
        Node target = this.grid.GetNode(targetPos);

        this.openSet.Add(start);

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
                if (neighbor == null || !neighbor.walkable || this.closedSet.Contains(neighbor))
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
        List<Node> path = new();
        Node current = end;

        while (current != null && current != start)
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

        (int x, int y) = (math.abs(b.x - a.x), math.abs(b.y - a.y));
        return x > y
            ? 14 * y + 10 * (x - y)
            : 14 * x + 10 * (y - x);
    }

    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int newX = node.x + i;
                int newY = node.y + j;

                // Skip self
                if (i == 0 && j == 0)
                    continue;

                Node neighbor = this.grid.GetNode(newX, newY);
                if (neighbor != null)
                    neighbors.Add(neighbor);
            }
        }
        return neighbors;
    }

    private Node GetLowestFCostNode(List<Node> set)
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