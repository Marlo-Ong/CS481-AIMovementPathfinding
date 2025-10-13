using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public enum AlgorithmType
{
    AStar,
    RTT,
}

public class Grid
{
    public int sizeX { get; private set; }
    public int sizeY { get; private set; }
    public float cellSize;
    private Node[,] nodes;
    private bool[,] walkable;

    public Grid(int sizeX, int sizeY, float cellSize = 1f)
    {
        this.sizeX = sizeX + 1;
        this.sizeY = sizeY + 1;
        this.cellSize = cellSize;

        nodes = new Node[this.sizeX, this.sizeY];
        walkable = new bool[this.sizeX, this.sizeY];

        int obstacleLayer = LayerMask.GetMask("Obstacle");

        for (int x = 0; x < this.sizeX; x++)
        {
            for (int y = 0; y < this.sizeY; y++)
            {
                // Convert grid indices to world position using cellSize
                Vector3 worldPos = new Vector3(x * cellSize, 0, y * cellSize);

                // Sample the center of the grid square
                Vector3 checkPos = worldPos + new Vector3(cellSize / 2f, 0, cellSize / 2f);

                // Adjust radius so that spheres don’t overlap or skip cells
                bool hit = Physics.CheckSphere(checkPos, radius: cellSize / 2f, layerMask: obstacleLayer);

                bool isWalkable = !hit;
                nodes[x, y] = new Node(x, y, isWalkable);
                walkable[x, y] = isWalkable;
            }
        }

        ResetGrid();
    }

    public void ResetGrid()
    {
        for (int x = 0; x < this.sizeX; x++)
        {
            for (int y = 0; y < this.sizeY; y++)
            {
                nodes[x, y].gCost = int.MaxValue;
                nodes[x, y].hCost = int.MaxValue;
                nodes[x, y].parent = null;
            }
        }
    }

    public Node GetNode((int x, int y) pos)
    {
        return GetNode(pos.x, pos.y);
    }

    public Node GetNode(int x, int y)
    {
        bool isValid = x >= 0 && y >= 0 && x < this.sizeX && y < this.sizeY;
        return isValid ? this.nodes[x, y] : null;
    }

    public bool IsWalkable(int x, int y)
    {
        bool isValid = x >= 0 && y >= 0 && x < this.sizeX && y < this.sizeY;
        return isValid && this.walkable[x, y];
    }
}

public class Node : IEquatable<Node>
{
    public Node parent;
    public int gCost;
    public int hCost;
    public int fCost => this.gCost + this.hCost;

    public bool walkable;
    public int x;
    public int y;

    public Node(int x, int y, bool walkable)
    {
        this.x = x;
        this.y = y;
        this.walkable = walkable;
    }

    public bool Equals(Node other)
    {
        return this.x == other.x && this.y == other.y;
    }

    public override string ToString()
    {
        return $"Node ({x}, {y})";
    }
}

public static class AlgorithmFactory
{
    private static AStar aStar;
    private static Grid grid;
    private static int prevWidth = -1;
    private static int prevLength = -1;
    private static CancellationTokenSource cts = new();

    public static void CancelAllTasks()
    {
        cts.Cancel();
        cts = new();
    }

    public static List<Vector3> FindPath(AlgorithmType type, Vector3 start, Vector3 target)
    {
        IAlgorithm algorithm = GetAlgorithm(type);

        // World to grid
        int startX = Mathf.FloorToInt(start.x / grid.cellSize);
        int startY = Mathf.FloorToInt(start.z / grid.cellSize);
        int targetX = Mathf.FloorToInt(target.x / grid.cellSize);
        int targetY = Mathf.FloorToInt(target.z / grid.cellSize);

        var nodes = algorithm.FindPath((startX, startY), (targetX, targetY));

        var result = new List<Vector3>();
        foreach (var node in nodes)
        {
            // Grid to world
            float worldX = node.x * grid.cellSize + grid.cellSize * 0.5f;
            float worldZ = node.y * grid.cellSize + grid.cellSize * 0.5f;
            result.Add(new Vector3(worldX, 0, worldZ));
        }

        return result;
    }

    public static async Task<List<Vector3>> FindPathAsync(AlgorithmType type, Vector3 start, Vector3 target)
    {
        IAlgorithm algorithm = GetAlgorithm(type);

        // World to grid
        int startX = Mathf.FloorToInt(start.x / GameMgr.inst.gridCellSize);
        int startY = Mathf.FloorToInt(start.z / GameMgr.inst.gridCellSize);
        int targetX = Mathf.FloorToInt(target.x / GameMgr.inst.gridCellSize);
        int targetY = Mathf.FloorToInt(target.z / GameMgr.inst.gridCellSize);

        var findPathTask = Task.Run(() => algorithm.FindPath((startX, startY), (targetX, targetY)), cts.Token);
        var timeoutTask = Task.Delay(2000, cts.Token);

        var completed = await Task.WhenAny(findPathTask, timeoutTask);
        if (completed == timeoutTask)
            return new List<Vector3>();

        var nodes = await findPathTask;
        var result = new List<Vector3>();

        foreach (var node in nodes)
        {
            // Grid to world
            float worldX = node.x * GameMgr.inst.gridCellSize + GameMgr.inst.gridCellSize * 0.5f;
            float worldZ = node.y * GameMgr.inst.gridCellSize + GameMgr.inst.gridCellSize * 0.5f;
            result.Add(new Vector3(worldX, 0, worldZ));
        }

        return result;
    }

    private static IAlgorithm GetAlgorithm(AlgorithmType type)
    {
        int width = Mathf.CeilToInt(GameMgr.inst.endPosition.x);
        int length = Mathf.CeilToInt(GameMgr.inst.endPosition.z);

        var grid = new Grid(width, length, cellSize: GameMgr.inst.gridCellSize);
        return new AStar(grid);
    }
}