using UnityEngine;

[RequireComponent(typeof(InputManager))]
[RequireComponent(typeof(TimeManager))]
[RequireComponent(typeof(UIManager))]
[RequireComponent(typeof(NavMeshManager))]
[RequireComponent(typeof(PostProcessingManager))]
[RequireComponent(typeof(PopUpTextManager))]
public class Manager : MonoBehaviour
{
    [HideInInspector] public GameObject player;
    [HideInInspector] public PlayerController playerController;

    [HideInInspector] public InputManager input;
    [HideInInspector] public TimeManager time;
    [HideInInspector] public UIManager ui;
    [HideInInspector] public QualityManager quality;
    [HideInInspector] public NavMeshManager navMesh;
    [HideInInspector] public PostProcessingManager postProc;
    [HideInInspector] public PopUpTextManager popUpTxt;
    [HideInInspector] public EnemyManager enemy;
    
    public static float deltaTimeMultiplier = 62.5f;

    public float GetDeltaTime() { return Time.deltaTime * deltaTimeMultiplier; }
    public float GetUnscaledDeltaTime() { return Time.unscaledDeltaTime * deltaTimeMultiplier; }
    public float GetFixedDeltaTime() { return Time.fixedDeltaTime * deltaTimeMultiplier; }
    public float GetFixedUnscaledDeltaTime() { return Time.fixedUnscaledDeltaTime * deltaTimeMultiplier; }

    private void OnEnable() { input.EnableFunction(); }
    private void OnDisable() { input.DisableFunction(); }

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<PlayerController>();

        input = GetComponent<InputManager>();
        time = GetComponent<TimeManager>();
        ui = GetComponent<UIManager>();
        navMesh = GetComponent<NavMeshManager>();
        postProc = GetComponent<PostProcessingManager>();
        popUpTxt = GetComponent<PopUpTextManager>();
        enemy = GetComponent<EnemyManager>();

        navMesh.GenerateNavMeshSurface();
    }

    private void Update()
    {
        input.UpdateFunction();
        time.UpdateFunction();
        ui.UpdateFunction();
        postProc.UpdateFunction();
    }

    public void OnPlayerDeath()
    {

    }
}