using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region States
    [Header("States")]
    public bool canRotateCamera = true;
    private bool inFChangeTransition, inFocusTransition;
    #endregion

    #region Rotation
    [Header("Rotation")]
    [SerializeField] private float sensivity = 6f;
    private float xRotation, yRotation;
    private Vector3 normalOffset = new(0f, 0f, -2f);
    [HideInInspector] public float yAngleOffset;
    #endregion

    #region Camera
    [Header("Camera")]
    [SerializeField] private float camRadius = 0.3f;
    [SerializeField] private Vector2 camClamp = new(-45f, 45f);
    #endregion

    #region Focus
    [Header("Focus")]
    [SerializeField] private float pointMoveSpeed = 0.2f;
    [SerializeField] private float focusChangeTransitionSpeed = 0.1f, focusTransitionSpeed = 1.5f;
    [SerializeField] private Vector3 focusOffset = new(0.3f, 0.1f, -1f);
    public int pointIndex;
    private Vector3 focusPointOffset;
    #endregion

    #region Field of View
    [Header("Field of View")]
    [SerializeField] private float normalFOV = 40f;
    [SerializeField] private float dashFOV = 60f, runFOV = 45f, fOVChangeSpeed = 1f;
    private float desirableFOV;
    #endregion

    #region References
    [Header("References")]
    [SerializeField] private LayerMask obstacleLayer;
    public Transform camHandleTrnsfrm;
    private Manager mngr;
    [HideInInspector] public Camera mCam;
    [HideInInspector] public Transform camPivotTrnsfrm;
    [HideInInspector] public Transform focusPoint, focusTarget;
    [HideInInspector] public Transform[] focusPoints;
    [HideInInspector] public Transform nextPoint, previousPoint;
    private Animator anim;
    #endregion

    private void Awake()
    {
        mCam = Camera.main;
        camPivotTrnsfrm = transform;
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        mngr = FindObjectOfType<Manager>();
        normalOffset = camHandleTrnsfrm.localPosition;
        desirableFOV = normalFOV;
    }

    private void Update()
    {
        ControlFOV();
        if (inFChangeTransition) FChangeTransitionCamera();
        else if (inFocusTransition) FocusTransitionCamera();
        if (canRotateCamera) Rotation();
    }

    public void StartFChangeTransition()
    {
        inFChangeTransition = true;
        canRotateCamera = false;
    }

    private void FChangeTransitionCamera()
    {
        if (camHandleTrnsfrm.localPosition == (mngr.playerController.isFocusChanging ? focusOffset : normalOffset))
        {
            inFChangeTransition = false;
            canRotateCamera = true;
        }
        else
        {
            if (mngr.playerController.isFocusChanging)
            {
                camHandleTrnsfrm.localPosition = Vector3.MoveTowards(camHandleTrnsfrm.localPosition,
                    focusOffset, focusChangeTransitionSpeed * mngr.GetUnscaledDeltaTime());
            }
            else
            {
                camHandleTrnsfrm.localPosition = Vector3.MoveTowards(camHandleTrnsfrm.localPosition,
                    normalOffset, focusChangeTransitionSpeed * mngr.GetUnscaledDeltaTime());
            }
        }
    }

    public void StartFocusTransition()
    {
        inFocusTransition = true;
        canRotateCamera = false;
    }

    private void FocusTransitionCamera()
    {
        Quaternion desPivotRot, desCamRot;
        if (mngr.playerController.isFocusing)
        {
            desPivotRot = Quaternion.LookRotation(focusPoint.position - camPivotTrnsfrm.position);
            desCamRot = camHandleTrnsfrm.localRotation;
        }
        else
        {
            desPivotRot = mngr.playerController.playerBody.localRotation;
            desCamRot = Quaternion.identity;
        }

        if (camPivotTrnsfrm.localRotation == desPivotRot && camHandleTrnsfrm.localRotation == desCamRot)
        {
            inFocusTransition = false;
            canRotateCamera = true;

            if (!mngr.playerController.isFocusing)
            {
                yRotation = camPivotTrnsfrm.eulerAngles.y;
                xRotation = camPivotTrnsfrm.eulerAngles.x;
            }
        }
        else
        {
            camPivotTrnsfrm.localRotation = Quaternion.RotateTowards(camPivotTrnsfrm.localRotation,
                desPivotRot, focusTransitionSpeed * mngr.GetUnscaledDeltaTime());
            if (!mngr.playerController.isFocusing)
            {
                camHandleTrnsfrm.localRotation = Quaternion.RotateTowards(camHandleTrnsfrm.localRotation,
                    desCamRot, focusTransitionSpeed * mngr.GetUnscaledDeltaTime());
            }
        }
    }

    public void BeforeFocus(Transform _focusTarget, int _index,
        Transform[] _points)
    {
        focusTarget = _focusTarget;
        pointIndex = _index;
        focusPoints = _points;
        nextPoint = focusPoints[pointIndex + 1] != null ? focusPoints[pointIndex + 1] : focusPoints[pointIndex];
        previousPoint = focusPoints[pointIndex - 1] != null ? focusPoints[pointIndex - 1] : focusPoints[pointIndex];
        focusPoint.position = focusPoints[pointIndex].position;
        xRotation = yRotation = 0f;
        StartFocusTransition();
    }

    private void Rotation()
    {
        if (!mngr.playerController.isFocusing)
        {
            yRotation += mngr.input.rotation.x * sensivity;
            if (yRotation >= 360f || yRotation <= -360f) yRotation = 0f;
            xRotation -= mngr.input.rotation.y * sensivity;
            xRotation = Mathf.Clamp(xRotation, camClamp.x, camClamp.y);

            camPivotTrnsfrm.eulerAngles = new Vector3(xRotation, yRotation, 0f);
            yAngleOffset = Mathf.Atan2(camPivotTrnsfrm.forward.z, camPivotTrnsfrm.forward.x) *
                Mathf.Rad2Deg - 90f;

            Ray camRay = new(camPivotTrnsfrm.position, -camPivotTrnsfrm.forward);
            float maxDistance = Mathf.Abs(mngr.playerController.isFocusChanging ?
                focusOffset.z : normalOffset.z);
            if (Physics.SphereCast(camRay, 0.25f, out RaycastHit hit, maxDistance, obstacleLayer))
                maxDistance = (hit.point - camPivotTrnsfrm.position).magnitude - camRadius;
            camHandleTrnsfrm.localPosition = new(camHandleTrnsfrm.localPosition.x,
                camHandleTrnsfrm.localPosition.y, -maxDistance);
        }
        else
        {
            float pointMove = mngr.input.focusPointMovement;
            if (pointMove != 0f)
                focusPointOffset = Vector3.MoveTowards(focusPointOffset,
                    pointMove > 0f ? nextPoint.localPosition : previousPoint.localPosition,
                    pointMoveSpeed * mngr.GetUnscaledDeltaTime());
            if (focusPointOffset == nextPoint.localPosition &&
                focusPoints[^1].localPosition != focusPointOffset)
            {
                previousPoint = focusPoints[pointIndex];
                nextPoint = focusPoints[pointIndex + 1];
                pointIndex++;
            }
            if (focusPointOffset == previousPoint.localPosition &&
                focusPoints[0].localPosition != focusPointOffset)
            {
                nextPoint = focusPoints[pointIndex];
                previousPoint = focusPoints[pointIndex - 1];
                pointIndex--;
            }
            focusPoint.position = focusPointOffset + focusTarget.position;
            camPivotTrnsfrm.LookAt(focusPoint);
            camHandleTrnsfrm.LookAt(focusPoint);
        }
    }

    private void ControlFOV()
    {
        if (mCam.fieldOfView != desirableFOV)
            mCam.fieldOfView = Mathf.MoveTowards(
                mCam.fieldOfView, desirableFOV, fOVChangeSpeed * mngr.GetDeltaTime());
    }

    public void NormalFOV() { desirableFOV = normalFOV; }

    public void RunFOV() { desirableFOV = runFOV; }

    public void DashFOV() { desirableFOV = dashFOV; }

    public void SetRunning(bool _val) { anim.SetBool("Is Running", _val); }

    public void SetDashing(bool _val) { anim.SetBool("Is Dashing", _val); }

    public void SetHit() { anim.SetTrigger("Hit"); }
}
