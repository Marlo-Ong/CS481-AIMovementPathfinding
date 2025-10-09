using UnityEngine;
using System.Collections.Generic;
using System;

public class AStar
{
    private class Grid
    {
        public int sizeX { get; private set; }
        public int sizeY { get; private set; }
        private Node[,] nodes;
        private Vector3 origin;
        LayerMask obstacleLayer = LayerMask.GetMask("Obstacle");

        public Grid(int sizeX, int sizeY, Vector3 origin)
        {
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            this.nodes = new Node[sizeX, sizeY];

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    Vector3 worldPos = new Vector3(x, 0, y);
                    this.nodes[x, y] = new Node(x, y, walkable: IsWalkable(worldPos));
                }
            }
        }

        public Node GetNode(Vector3 worldPos)
        {
            return this.GetNode((int)worldPos.x, (int)worldPos.z);
        }

        public Node GetNode(Vector2Int coordinates)
        {
            return this.GetNode(coordinates.x, coordinates.y);
        }

        public Node GetNode(int x, int y)
        {
            bool isValid = x >= 0 && y >= 0 && x < this.sizeX && y < this.sizeY;
            return isValid ? this.nodes[x, y] : null;
        }

        public bool IsWalkable(Vector3 position)
        {
            bool hit = Physics.CheckSphere(position + new Vector3(0.5f, 0, 0.5f), radius: 1f, layerMask: obstacleLayer);
            return !hit;
        }
    }

    public class Node : IEquatable<Node>
    {
        public Node parent;
        public int gCost;
        public int hCost;
        public int fCost => this.gCost + this.hCost;

        public bool walkable;
        public Vector2Int coordinates;
        public int x => this.coordinates.x;
        public int y => this.coordinates.y;

        public Node(int x, int y, bool walkable)
        {
            this.coordinates = new(x, y);
            this.walkable = walkable;
        }

        public bool Equals(Node other)
        {
            return this.coordinates == other.coordinates;
        }

        public override string ToString()
        {
            return $"{coordinates}";
        }
    }

    private Grid grid;
    private List<Node> openSet;
    private HashSet<Node> closedSet;

    public List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        this.grid = new Grid(100, 100, Vector3.zero);
        Node start = this.grid.GetNode(startPos);
        Node target = this.grid.GetNode(targetPos);

        this.openSet = new();
        this.closedSet = new();

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

        var diff = new Vector2Int(Mathf.Abs(b.x - a.x), Mathf.Abs(b.y - a.y));
        return diff.x > diff.y
            ? 14 * diff.y + 10 * (diff.x - diff.y)
            : 14 * diff.x + 10 * (diff.y - diff.x);
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

                // Find neighboring nodes only
                if (newX == 0 && newY == 0)
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