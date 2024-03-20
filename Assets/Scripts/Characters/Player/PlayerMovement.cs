using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Bools")]
    public bool isGrounded;
    public bool isSloped;

    [Header("Movement")]
    [SerializeField] float normalSpeed;
    [SerializeField] float moveSmoothingSpeed;
    Vector3 targetVel;

    [Space(7.5f)] // Speed Multipliers
    [SerializeField] float backSpeedMultiplier;
    [SerializeField] float runSpeedMultiplier;

    [Space(7.5f)] // Downforce
    [SerializeField] float groundedVelOffset;
    [SerializeField] float gravityModifier;

    [Space(7.5f)] // Ray Check
    [SerializeField] float groundDrag = 5f;
    [SerializeField] float minSlopeAngle;
    float slopeAngle;
    RaycastHit slopeHit;

    [Header("Rotation")]
    [SerializeField] float rotSpeed;
    //[SerializeField, Range(0f, 360f)] float minAngleToTurn;
    //[SerializeField] float turnSpeed;
    Vector2 smthMove;
    float curBodyRot, bodyRotVel;
    float desTurn;


    [Header("References")]
    [SerializeField] LayerMask playerLayer;
    [SerializeField] Transform playerBody;
    [SerializeField] Transform camPivot;
    InputManager input;
    Rigidbody rb;
    Animator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = FindObjectOfType<InputManager>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        Rotation();
        Animation();
    }

    private void FixedUpdate()
    {
        RayCheck();
        Movement();
    }

    public void RayCheck()
    {
        if (Physics.Raycast(new(transform.position.x, transform.position.y + 0.1f, transform.position.z),
            Vector3.down, out slopeHit, 0.4f, ~playerLayer))
        {
            rb.drag = groundDrag;
            isGrounded = true;

            slopeAngle = Vector3.Angle(slopeHit.normal, transform.up);
            if (slopeAngle > minSlopeAngle) isSloped = true;
            else isSloped = false;
        }
        else
        {
            rb.drag = 0f;
            isGrounded = false;

            isSloped = false;
        }
    }

    private void Movement()
    {
        Vector3 moveDir = playerBody.right * input.move.x + playerBody.forward * input.move.y;
        moveDir.y = 0f;
        moveDir.Normalize();

        if (input.move.magnitude > 0f)
        {
            float moveSpeed = normalSpeed *
                (input.move.y < 0f? backSpeedMultiplier : 1f) * (input.run? runSpeedMultiplier : 1f);
          
            if (isSloped) targetVel = Vector3.ProjectOnPlane(moveDir, slopeHit.normal) * moveSpeed;
            else targetVel = moveDir * moveSpeed;
        }
        else targetVel = Vector3.zero;

        if (isSloped) rb.useGravity = false;
        else
        {
            rb.useGravity = true;
            if (isGrounded) targetVel.y = -groundedVelOffset;
            else targetVel.y = rb.velocity.y - gravityModifier;
        }

        rb.velocity = targetVel;
    }

    private void Rotation()
    {
        if (input.move.magnitude > 0f) desTurn = camPivot.eulerAngles.y;

        //else
        //{
        //    if (Vector3.Angle(transform.forward, camPivot.forward) >= minAngleToTurn)
        //        desTurn = camPivot.eulerAngles.y;

        //    curBodyRot = Mathf.SmoothDampAngle(curBodyRot, desTurn, ref bodyRotVel, rotSpeed);
        //}
        
        curBodyRot = Mathf.SmoothDampAngle(curBodyRot, desTurn, ref bodyRotVel, rotSpeed);
        playerBody.eulerAngles = new(0f, curBodyRot, 0f);
    }

    private void Animation()
    {
        smthMove = Vector2.MoveTowards(smthMove,
            input.move * (input.run ? 2f : 1f), moveSmoothingSpeed * Time.deltaTime);

        anim.SetFloat("X Move Dir", smthMove.x);
        anim.SetFloat("Y Move Dir", smthMove.y);

        anim.SetBool("Is Falling", !isGrounded);
    }    
}
