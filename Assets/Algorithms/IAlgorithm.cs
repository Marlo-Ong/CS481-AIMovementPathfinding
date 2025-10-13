using UnityEngine;
using System.Collections.Generic;

public interface IAlgorithm
{
    public List<Node> FindPath((int x, int y) startPos, (int x, int y) targetPos);
}