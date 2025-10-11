using UnityEngine;
using System.Collections.Generic;

public interface IAlgorithm
{
    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos);
}