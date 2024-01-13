using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Manager))]
public class InputManager : MonoBehaviour
{
    [Header("Smoothing")]
    public float inputSmoothValue = 0.5f;

    [Header("Values")]
    public Vector2 movement;
    public Vector2 smoothedMovement, rotation;
    public float focusPointMovement;

    private Manager mngr;
    private PlayerInputScheme playerInputScheme;
    private PlayerController playerController;
    private EnemyManager enemy;
    private QualityManager quality;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        mngr = GetComponent<Manager>();
        playerController = mngr.playerController;
        enemy = GetComponent<EnemyManager>();
        quality = GetComponent<QualityManager>();
    }

    public void EnableFunction()
    {
        if (playerInputScheme == null)
        {
            playerInputScheme = new PlayerInputScheme();

            // Controls
            playerInputScheme.Controls.Movement.performed += playerInputScheme =>
            movement = playerInputScheme.ReadValue<Vector2>();
            playerInputScheme.Controls.Rotation.performed += playerInputScheme =>
            rotation = playerInputScheme.ReadValue<Vector2>();

            playerInputScheme.Controls.Run.performed += playerInputScheme =>
            playerController.Action(ActType.RunPerf);
            playerInputScheme.Controls.Run.canceled += playerInputScheme =>
            playerController.Action(ActType.RunCanc);

            // Actions
            playerInputScheme.Actions.Attack.started += playerInputScheme =>
            playerController.Action(ActType.AttackStrtd);

            playerInputScheme.Actions.Dash.started += playerInputScheme =>
            playerController.Action(ActType.DashStrtd);

            playerInputScheme.Actions.Block.started += playerInputScheme =>
            playerController.Action(ActType.BlockStrtd);
            playerInputScheme.Actions.Block.canceled += playerInputScheme =>
            playerController.Action(ActType.BlockCanc);

            playerInputScheme.Actions.Focus.performed += playerInputScheme =>
            playerController.Action(ActType.FocusChangePerf);
            playerInputScheme.Actions.Focus.canceled += playerInputScheme =>
            playerController.Action(ActType.FocusChangeCanc);

            playerInputScheme.Actions.MoveFocusPoint.performed += playerInputScheme =>
            focusPointMovement = playerInputScheme.ReadValue<Vector2>().y;

            playerInputScheme.Actions.NearestSpawn.started += playerInputScheme =>
            enemy.NearestSpawn();
            playerInputScheme.Actions.RandomSpawn.started += playerInputScheme =>
            enemy.RandomSpawn();

            playerInputScheme.Actions.Restart.started += playerInputScheme =>
            SceneManager.LoadScene(0);

            // Quality Change
            playerInputScheme.QualityChange.LowQuality.started += playerInputScheme =>
            quality.SetQuality(Quality.Low);
            playerInputScheme.QualityChange.MediumQuality.started += playerInputScheme =>
            quality.SetQuality(Quality.Medium);
            playerInputScheme.QualityChange.HighQuality.started += playerInputScheme =>
            quality.SetQuality(Quality.High);
            playerInputScheme.QualityChange.UltraQuality.started += playerInputScheme =>
            quality.SetQuality(Quality.Ultra);
        }

        playerInputScheme.Enable();
    }
    public void DisableFunction() { playerInputScheme.Disable(); }

    public void UpdateFunction()
    {
        if (smoothedMovement != movement) smoothedMovement = 
                Vector2.MoveTowards(smoothedMovement, movement, inputSmoothValue * mngr.GetUnscaledDeltaTime());
    }
}

public enum ActType
{
    RunPerf, RunCanc,
    AttackStrtd,
    FocusChangePerf, FocusChangeCanc,
    DashStrtd,
    BlockStrtd, BlockCanc
}