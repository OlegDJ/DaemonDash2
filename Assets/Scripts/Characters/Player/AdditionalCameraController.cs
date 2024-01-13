using UnityEngine;

public class AdditionalCameraController : MonoBehaviour
{
    private Transform mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main.transform;
    }

    private void LateUpdate()
    {
        transform.SetLocalPositionAndRotation(mainCamera.localPosition, mainCamera.localRotation);
    }
}
