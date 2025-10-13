using System;
using System.Collections.Generic;
using UnityEngine;

public enum Environment
{
    Circles20,
    Circles30,
    Circles100,
    Rectangles20,
    Rectangles30,
    Rectangles100,
    AStar,
    Office,
    Empty,
}

public class EnvironmentMgr : MonoBehaviour
{
    private static EnvironmentMgr inst;

    [Serializable]
    public struct Range
    {
        public Vector3 min;
        public Vector3 max;
        public Vector3 GetRandom(bool uniform = false)
        {
            float x = UnityEngine.Random.Range(min.x, max.x);
            if (uniform)
                return new Vector3(x, x, x);

            float y = UnityEngine.Random.Range(min.y, max.y);
            float z = UnityEngine.Random.Range(min.z, max.z);
            return new Vector3(x, y, z);
        }
    }

    [Header("Obstacles")]
    [SerializeField] private GameObject circleObstacle;
    [SerializeField] private GameObject rectangleObstacle;
    [SerializeField] private Range obstacleScaleRange;
    [SerializeField] private Range obstaclePositionRange;


    [Header("Preset Environments")]
    [SerializeField] private GameObject aStarEnvironment;
    [SerializeField] private GameObject officeEnvironment;

    private List<GameObject> circlePool;
    private List<GameObject> rectanglePool;

    void Start()
    {
        if (inst == null)
            inst = this;

        this.circlePool = new(capacity: 100);
        this.rectanglePool = new(capacity: 100);
        GameMgr.OnGameStarted += () => CreateEnvironment(GameMgr.Environment);
        GameMgr.OnGameStopped += () => CreateEnvironment(Environment.Empty);
    }

    public static void CreateEnvironment(Environment environment)
    {
        inst.CreateEnvironmentImpl(environment);
    }

    private void CreateEnvironmentImpl(Environment environment)
    {
        if (environment == Environment.Empty)
        {
            this.officeEnvironment.SetActive(false);
            this.aStarEnvironment.SetActive(false);

            foreach (var obstacle in this.circlePool)
                obstacle.SetActive(false);

            foreach (var obstacle in this.rectanglePool)
                obstacle.SetActive(false);
        }

        // A*
        if (environment == Environment.AStar)
        {
            this.officeEnvironment.SetActive(false);
            this.aStarEnvironment.SetActive(true);

            GameObject officeObPrefab = this.rectangleObstacle;
            List<GameObject> officeObstacles = this.rectanglePool;
            if (officeObstacles.Count < 5)
            {
                officeObstacles.Add(this.CreateObstacle(officeObPrefab));
                officeObstacles.Add(this.CreateObstacle(officeObPrefab));
                officeObstacles.Add(this.CreateObstacle(officeObPrefab));
                officeObstacles.Add(this.CreateObstacle(officeObPrefab));
                officeObstacles.Add(this.CreateObstacle(officeObPrefab));
            }
            
            officeObstacles[0].SetActive(true);
            officeObstacles[0].transform.position = new Vector3(395, 0, 100);
            officeObstacles[0].transform.localScale = new Vector3(225, 30, 10);

            officeObstacles[1].SetActive(true);
            officeObstacles[1].transform.position = new Vector3(287, 0, 173);
            officeObstacles[1].transform.localScale = new Vector3(300, 30, 10);
            officeObstacles[1].transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));

            officeObstacles[2].SetActive(true);
            officeObstacles[2].transform.position = new Vector3(395, 0, 319);
            officeObstacles[2].transform.localScale = new Vector3(225, 30, 10);

            officeObstacles[3].SetActive(true);
            officeObstacles[3].transform.position = new Vector3(180, 0, 102);
            officeObstacles[3].transform.localScale = new Vector3(225, 30, 10);

            officeObstacles[4].SetActive(true);
            officeObstacles[4].transform.position = new Vector3(174, 0, 318);
            officeObstacles[4].transform.localScale = new Vector3(225, 30, 10);
            return;
        }

        // Office
        else if (environment == Environment.Office)
        {
            this.aStarEnvironment.SetActive(false);
            this.officeEnvironment.SetActive(true);
            return;
        }

        // Obstacle field
        int obstacleAmount = environment switch
        {
            Environment.Circles20 => 20,
            Environment.Circles30 => 30,
            Environment.Circles100 => 100,
            Environment.Rectangles20 => 20,
            Environment.Rectangles30 => 30,
            Environment.Rectangles100 => 100,
            _ => 0
        };

        GameObject obstaclePrefab = environment switch
        {
            Environment.Circles20 => this.circleObstacle,
            Environment.Circles30 => this.circleObstacle,
            Environment.Circles100 => this.circleObstacle,
            Environment.Rectangles20 => this.rectangleObstacle,
            Environment.Rectangles30 => this.rectangleObstacle,
            Environment.Rectangles100 => this.rectangleObstacle,
            _ => null
        };

        List<GameObject> pool =
            obstaclePrefab == this.circleObstacle ? this.circlePool
            : obstaclePrefab == this.rectangleObstacle ? this.rectanglePool
            : null;

        if (pool == null)
            return;

        int max = Mathf.Max(obstacleAmount, pool.Count);
        for (int i = 0; i < max; i++)
        {
            // Disable unneeded pooled objects.
            if (i >= obstacleAmount && i < pool.Count)
            {
                pool[i].SetActive(false);
                continue;
            }

            // Create or reuse pooled objects.
            if (i >= pool.Count)
                pool.Add(this.CreateObstacle(obstaclePrefab));

            GameObject instance = pool[i];
            instance.SetActive(true);

            instance.transform.position = this.obstaclePositionRange.GetRandom();
            instance.transform.localScale = this.obstacleScaleRange.GetRandom(uniform: obstaclePrefab == circleObstacle);
        }
    }

    private GameObject CreateObstacle(GameObject obstaclePrefab)
    {
        GameObject obstacle = Instantiate(obstaclePrefab);
        obstacle.SetActive(false);
        obstacle.transform.SetParent(this.transform);
        return obstacle;
    }
}