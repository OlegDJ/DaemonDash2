using UnityEngine;

public class InputManager : MonoBehaviour
{
    public Vector2 move, rotation;
    public bool run;

    PlayerInputScheme playerInputScheme;

    private void OnEnable()
    {
        if (playerInputScheme == null)
        {
            playerInputScheme = new();

            playerInputScheme.Controls.Movement.performed += playerInputScheme =>
            move = playerInputScheme.ReadValue<Vector2>();

            playerInputScheme.Controls.Rotation.performed += playerInputScheme =>
            rotation = playerInputScheme.ReadValue<Vector2>();

            playerInputScheme.Controls.Run.performed += playerInputScheme =>
            run = true;
            playerInputScheme.Controls.Run.canceled += playerInputScheme =>
            run = false;
        }

        playerInputScheme.Enable();
    }

    private void OnDisable() { playerInputScheme.Disable(); }
}
