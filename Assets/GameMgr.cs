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

    void Update()
    {
        // Restart current environment
        if (Input.GetKeyDown(KeyCode.C))
        {
            StopGame();
            StartGame();
        }
    }

    public static void SetMode(Mode newMode)
    {
        if (Mode != newMode)
        {
            Mode = newMode;
            OnModeChanged?.Invoke(newMode);

            if (newMode == Mode.GroupMovementPotentialField || newMode == Mode.AStarPotentialField)
            {
                AIMgr.inst.isPotentialFieldsMovement = true;
            }
            else
            {
                AIMgr.inst.isPotentialFieldsMovement = false;
            }
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
        OnGameStarted?.Invoke();
    }

    public static void StopGame()
    {
        if (!inst.isGameActive)
            return;

        inst.isGameActive = false;
        OnGameStopped?.Invoke();
    }
}
