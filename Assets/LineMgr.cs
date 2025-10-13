using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineMgr : MonoBehaviour
{
    public static LineMgr inst;

    private void Awake()
    {
        inst = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        GameMgr.OnGameStopped += () => this.DestroyAllLines();

        this.inactiveLines = new();
        for (int i = 0; i < initialPoolSize; i++)
            this.CreatePooledLine();

        this.activeLines = new();
    }

    public int initialPoolSize;
    private Stack<LineRenderer> inactiveLines;
    private HashSet<LineRenderer> activeLines;

    public LineRenderer MovePrefab;
    public LineRenderer FollowPrefab;
    public LineRenderer InterceptPrefab;
    public LineRenderer PotentialPrefab;

    private LineRenderer GetPooledLine()
    {
        if (!this.inactiveLines.TryPop(out LineRenderer lr))
        {
            this.CreatePooledLine();
            lr = this.inactiveLines.Pop();
        }
        this.activeLines.Add(lr);

        return lr;
    }

    private LineRenderer CreatePooledLine()
    {
        LineRenderer lr = Instantiate(MovePrefab, parent: transform);
        lr.startColor = Color.red;
        lr.endColor = Color.blue;
        lr.gameObject.SetActive(false);
        this.inactiveLines.Push(lr);
        return lr;
    }

    public LineRenderer CreateMoveLine(Vector3 p1, Vector3 p2)
    {
        LineRenderer lr = GetPooledLine();
        lr.SetPosition(0, p1);
        lr.SetPosition(1, p2);
        return lr;
    }

    public LineRenderer CreatePotentialLine(Vector3 p1)
    {
        LineRenderer lr = Instantiate<LineRenderer>(PotentialPrefab, transform);
        lr.SetPosition(0, p1);
        lr.SetPosition(1, Vector3.zero);
        return lr;
    }

    public LineRenderer CreateFollowLine(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        LineRenderer lr = Instantiate<LineRenderer>(FollowPrefab, transform);
        lr.SetPosition(0, p1);
        lr.SetPosition(1, p2);
        lr.SetPosition(2, p3);
        return lr;
    }

    public LineRenderer CreateInterceptLine(Vector3 p1, Vector3 p2, Vector2 p3)
    {
        LineRenderer lr = Instantiate<LineRenderer>(InterceptPrefab, transform);
        lr.SetPosition(0, p1);
        lr.SetPosition(1, p2);
        lr.SetPosition(2, p3);
        return lr;
    }

    public GameObject tmp;
    public void DestroyLR(LineRenderer lr)
    {
        if (this.activeLines.Remove(lr))
        {
            lr.gameObject.SetActive(false);
            this.inactiveLines.Push(lr);
        }
    }

    public void Destroy(LineRenderer lr)
    {
        if (lr != null)
            Destroy(lr.gameObject);
    }

    public void DestroyAllLines()
    {
        foreach (var line in this.activeLines)
        {
            line.gameObject.SetActive(false);
            this.inactiveLines.Push(line);
        }
        this.activeLines.Clear();
    }
}
