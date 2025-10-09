using System;
using System.Collections;
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

        EnvironmentMgr.CreateEnvironment(Environment);

        Entity entity = EntityMgr.inst.CreateEntity(EntityType.TugBoat, Vector3.zero, Vector3.zero);
        entity.isSelected = false;
        inst.StartCoroutine(inst.StartAStar(entity, entity.GetComponent<UnitAI>()));

        OnGameStarted?.Invoke();
    }

    private IEnumerator StartAStar(Entity ent, UnitAI uai)
    {
        yield return new WaitForSeconds(1.0f);

        AStar aStar = new();
        var path = aStar.FindPath(Vector3.zero, new Vector3(80, 0, 50));
        foreach (var node in path)
            uai.SetCommand(new Move(ent, new Vector3(node.x, 0, node.y)));
    }

    public static void StopGame()
    {
        inst.isGameActive = false;
        OnGameStopped?.Invoke();
    }
}
