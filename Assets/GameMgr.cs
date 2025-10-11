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
    private Entity entity = null;

    [Header("Assign in Inspector")]
    public AlgorithmType algorithmType;
    public Vector3 startPosition;
    public Vector3 endPosition;

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
        if (inst.isGameActive)
            StopGame();

        inst.isGameActive = true;

        EnvironmentMgr.CreateEnvironment(Environment);

        if (inst.entity != null)
            EntityMgr.inst.DestroyEntity(inst.entity);

        inst.entity = EntityMgr.inst.CreateEntity(EntityType.TugBoat, Vector3.zero, Vector3.zero);
        inst.entity.isSelected = false;
        inst.StartCoroutine(inst.StartAlgorithm(
            inst.algorithmType,
            inst.entity,
            inst.startPosition,
            inst.endPosition));

        OnGameStarted?.Invoke();
    }

    private IEnumerator StartAlgorithm(AlgorithmType type, Entity ent, Vector3 startPos, Vector3 endPos)
    {
        yield return new WaitForSeconds(1.0f);

        IAlgorithm algorithm = AlgorithmFactory.GetAlgorithm(type);
        var path = algorithm.FindPath(startPos, endPos);

        UnitAI uai = ent.GetComponent<UnitAI>();
        foreach (var node in path)
            uai.AddCommand(new Move(ent, node));
    }

    public static void StopGame()
    {
        if (!inst.isGameActive)
            return;

        inst.isGameActive = false;
        OnGameStopped?.Invoke();
    }
}
