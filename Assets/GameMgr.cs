using System;
using UnityEngine;

public enum Mode
{
    WayPointGeneration,
    SingleEntityWaypointFollow,
    GroupMovementPotentialField,
    AStarPotentialField
}

public enum Environment
{
    Circles20,
    Circles30,
    Circles100,
    Rectangles20,
    Rectangles30,
    Rectangles100,
    AStar,
    Office
}

public class GameMgr : MonoBehaviour
{
    public static Mode Mode { get; private set; }
    public static Environment Environment { get; private set; }

    public static event Action<Mode> OnModeChanged;
    public static event Action<Environment> OnEnvironmentChanged;
    public static event Action OnGameStarted;
    public static event Action OnGameStopped;

    public static GameMgr inst;
    private GameInputs input;
    private bool isGameActive;

    private void Awake()
    {
        inst = this;
        input = new GameInputs();
        input.Enable();
    }

    // Start is called before the first frame update
    void Start()
    {
        Vector3 position = Vector3.zero;
        foreach (GameObject go in EntityMgr.inst.entityPrefabs)
        {
            Entity ent = EntityMgr.inst.CreateEntity(go.GetComponent<Entity>().entityType, position, Vector3.zero);
            ent.isSelected = false;
            position.x += 200;
        }

        this.isGameActive = false;
    }

    public static void SetMode(Mode newMode)
    {
        if (Mode != newMode)
        {
            Mode = newMode;
            OnModeChanged?.Invoke(newMode);
        }
    }

    public static void SetEnvironment(Environment newEnvironment)
    {
        if (Environment != newEnvironment)
        {
            Environment = newEnvironment;
            OnEnvironmentChanged?.Invoke(newEnvironment);
        }
    }

    public static void StartGame()
    {
        inst.isGameActive = true;
        OnGameStarted?.Invoke();
    }

    public static void StopGame()
    {
        inst.isGameActive = false;
        OnGameStopped?.Invoke();
    }

    public Vector3 position;
    public float spread = 20;
    public float colNum = 10;
    public float initZ;
    // Update is called once per frame
    void Update()
    {
        if (input.Entities.Create100.triggered)
        {
            initZ = position.z;
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Entity ent = EntityMgr.inst.CreateEntity(EntityType.PilotVessel, position, Vector3.zero);
                    position.z += spread;
                }
                position.x += spread;
                position.z = initZ;
            }
            DistanceMgr.inst.Initialize();
        }
    }
}
