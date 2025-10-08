using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AStarTest : MonoBehaviour
{
    private Vector3[] path;
    public void Start()
    {
        AStar aStar = new();
        var path = aStar.FindPath(Vector3.zero, new Vector3(80, 0, 50));

        Debug.Log(string.Join('\n', path));

        this.path = path.Select(node => new Vector3(node.x, 0, node.y)).ToArray();
    }

    void OnDrawGizmos()
    {
        if (this.path == null)
            return;
        Gizmos.DrawLineList(this.path);
    }
}