using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField, Range(0f, 1f)] float sensivity;
    [SerializeField] Vector2 camClamp;
    [SerializeField] float camRadius = 0.25f;
    Vector3 pivotOffset, camOffset;
    float yRotation, xRotation;

    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] Transform camHandle;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] Animator anim;
    Transform camPivot;
    InputManager input;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        camPivot = transform;
        input = FindObjectOfType<InputManager>();

        pivotOffset = transform.position;
        camOffset = camHandle.position;
    }

    private void Update()
    {
        Rotation();
        Animation();
    }

    private void Rotation()
    {
        yRotation += input.rotation.x * 30f * sensivity * Time.deltaTime;
        if (yRotation >= 360f || yRotation <= -360f) yRotation = 0f;
        xRotation -= input.rotation.y * 30f * sensivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, camClamp.x, camClamp.y);

        camPivot.eulerAngles = new Vector3(xRotation, yRotation, 0f);

        Ray camRay = new(camPivot.position, -camPivot.forward);
        float maxDistance = -camOffset.z;
        if (Physics.SphereCast(camRay, 0.25f, out RaycastHit hit, maxDistance, ~playerLayer))
            maxDistance = (hit.point - camPivot.position).magnitude - camRadius;
        camHandle.localPosition = new(camHandle.localPosition.x,
            camHandle.localPosition.y, -maxDistance);

        transform.position = player.position + pivotOffset;
    }

    private void Animation()
    {
        anim.SetBool("Is Running", input.run);
    }
}
