using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementPiece : MonoBehaviour
{
    /*
    #region kod 1
    public float mainGravity;
    private Rigidbody2D rb;
    private Animator animator;
    private TrailRenderer traRen;
    private CamerFollowObject camFollowObj;
    private PlayerInput playerInput;

    [Header("Movement")]
    [SerializeField] private int Speed;
    [HideInInspector] public bool faceRight = true;
    public float horizontal;
    public float gravityScale;
    public float fallGravityMult;
    public float maxFallSpeed;
    public bool turnWS;
    public bool turnWR;
    private float fallSpeedYDampingChangeTrashold;

    [Header("Camera")]
    [SerializeField] private GameObject camFollowObject;

    void Awake()
    {
        playerInput = new PlayerInput();
    }

    private void OnEnable()
    {
        playerInput.Enable();
    }

    private void OnDisable()
    {
        playerInput.Disable();
    }

    private void Start()
    {
        //Components
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        traRen = GetComponent<TrailRenderer>();
        camFollowObj = camFollowObject.GetComponent<CamerFollowObject>();

        //Equalization
        fallSpeedYDampingChangeTrashold = CameraManager.instance.fallSpeedYDampingChangeThreshold;
    }

    private void FixedUpdate()
    {
        //Movement
        rb.velocity = new Vector2(horizontal * Speed, rb.velocity.y);
        animator.SetFloat("Speed", Mathf.Abs(horizontal));
    }

    private void Update()
    {
        //Camera Learp
        if (rb.velocity.y < fallSpeedYDampingChangeTrashold && !CameraManager.instance.isLearpingYDamping && !CameraManager.instance.learpedFromPlayerFalling)
        {
            CameraManager.instance.LearpYDamping(true);
        }

        if (rb.velocity.y >= 0f && !CameraManager.instance.isLearpingYDamping && CameraManager.instance.learpedFromPlayerFalling)
        {
            CameraManager.instance.learpedFromPlayerFalling = false;
            CameraManager.instance.LearpYDamping(false);
        }
    }

    //tuşa basılırsa ona göre vektör2 değeri döndürüp horizontal değişkenine eşitliyor
    public void Move(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
    }

    //karakterin dönmesini sağlıyor
    void Flip()
    {
        //Turn With Scale
        if(turnWS == true)
        {
            faceRight = !faceRight;
            Vector3 scaler = transform.localScale;
            scaler.x *= -1;
            transform.localScale = scaler;
        }

        //Turn With Rotation
        else if(turnWR)
        {
            if(faceRight)
            {
                Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
                transform.rotation = Quaternion.Euler(rotator);
                faceRight = !faceRight;
                camFollowObj.CallTurn();
            }
            else
            {
                Vector3 rotator = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
                transform.rotation = Quaternion.Euler(rotator);
                faceRight = !faceRight;
                camFollowObj.CallTurn();
            }
        }
    }
    #endregion
    */

    /*
    #region kod 2
[Header("Main")]
public float mainGravity;
private Rigidbody2D rb;
private TrailRenderer tr;
private Animator anim;
private SpringJoint2D sr;
//public SpiderRope spi;

    [Header("Walk")]
public float speed;
[HideInInspector] public float moveInput;
private float horizontalVelocity;
private bool faceRight = true;
[Range(0, 1)] public float horizontalDampingStop;
[Range(0, 1)] public float horizontalDampingTurn;
[Range(0, 1)] public float horizontalDampingBasic;
private bool canMove;
private float heightInput;
private float direction = 1;
private bool walkSBool = false;
public bool airDragOpen;
public float airDragMultiplier = 0.9f;
public float movementForceInAir;

private void Start()
{
    rb = GetComponent<Rigidbody2D>();
    tr = GetComponent<TrailRenderer>();
    anim = GetComponent<Animator>();
    sr = GetComponent<SpringJoint2D>();
    rb.gravityScale = mainGravity;
    // spi.speed = ropeSpeed;
    // spi.pullForce = pullForce;
}

private void Update()
{
    CheckInput();
    CheckDirection();
}

private void FixedUpdate()
{
    ApplyMovement();
}

private void CheckDirection()
{
    if (faceRight && moveInput < 0)
    {
        Flip();
    }

    else if (!faceRight && moveInput > 0)
    {
        Flip();
    }
}

private void CheckInput()
{
    //MoveInput
    moveInput = Input.GetAxisRaw("Horizontal");
    heightInput = Input.GetAxisRaw("Vertical");
    yVelocity = rb.velocity.y;
    if (canMove)
    {
        walkSBool = true;
        horizontalVelocity = rb.velocity.x;
        horizontalVelocity += moveInput;
    }

    //DirectionCheck
    if (moveInput != 0)
    {
        direction = moveInput;
    }

    //Damping
    if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) < 0.01f)
    {
        horizontalVelocity *= Mathf.Pow(1f - horizontalDampingStop, Time.deltaTime * 10f);
    }

    else if (Mathf.Sign(Input.GetAxisRaw("Horizontal")) != Mathf.Sign(horizontalVelocity))
    {
        horizontalVelocity *= Mathf.Pow(1f - horizontalDampingTurn, Time.deltaTime * 10f);
    }

    else
    {
        horizontalVelocity *= Mathf.Pow(1f - horizontalDampingBasic, Time.deltaTime * 10f);
    }
}

private void ApplyMovement()
{
    if (airDragOpen == false)
    {
        rb.velocity = new Vector2(horizontalVelocity, rb.velocity.y);

        //Air Force
        if (!isGrounded && moveInput != 0)
        {
            Vector2 forceToAdd = new Vector2(movementForceInAir * moveInput, 0);
            rb.AddForce(forceToAdd);

            if (Mathf.Abs(rb.velocity.x) > speed)
            {
                rb.velocity = new Vector2(speed * moveInput, rb.velocity.y);
            }
        }
    }


    if (airDragOpen)
    {
        if (isGrounded)
        {
            rb.velocity = new Vector2(horizontalVelocity, rb.velocity.y);
        }

        //Air Force
        else if (!isGrounded && moveInput != 0)
        {
            Vector2 forceToAdd = new Vector2(movementForceInAir * moveInput, 0);
            rb.AddForce(forceToAdd);

            if (Mathf.Abs(rb.velocity.x) > speed)
            {
                rb.velocity = new Vector2(speed * moveInput, rb.velocity.y);
            }
        }

        //Air Drag
        else if (isGrounded && moveInput == 0)
        {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
        }
    }
}

void Flip()
{
    faceRight = !faceRight;
    Vector3 scaler = transform.localScale;
    scaler.x *= -1;
    transform.localScale = scaler;

}

#endregion
    */

    #region ana kod
    public float mainGravity; //ana yer çekimi
    private Rigidbody2D rb;

    [SerializeField] private int speed; // karakterin hızı

    [HideInInspector] public bool faceRight = true; //Dönme noktası

    public float horizontal; //x eksenindeki yön

    private bool isGrounded;

    public bool turnWS;
    public bool turnWR;

    public float movementForceInAir;
    public float airDragMultiplier = 0.9f;

    public bool airDragOpen;
    private bool canMove;

    private float horizontalVelocity;

    private void Start()
    {
        //Components
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        //Movement
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
    }

    private void Update()
    {
        
    }

    //tuşa basılırsa ona göre vektör2 değeri döndürüp horizontal değişkenine eşitliyor
    public void Move(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
    }

    private void VelocityUpdater()
    {
        if (canMove)
        {
            horizontalVelocity = rb.velocity.x;
            horizontalVelocity += horizontal;
        }
    }

    private void ApplyMovement()
{
    if (airDragOpen == false)
    {
        rb.velocity = new Vector2(horizontalVelocity, rb.velocity.y);

        //Air Force
        if (!isGrounded && horizontal != 0)
        {
            Vector2 forceToAdd = new Vector2(movementForceInAir * horizontal, 0);
            rb.AddForce(forceToAdd);

            if (Mathf.Abs(rb.velocity.x) > speed)
            {
                rb.velocity = new Vector2(speed * horizontal, rb.velocity.y);
            }
        }
    }


    if (airDragOpen)
    {
        if (isGrounded)
        {
            rb.velocity = new Vector2(horizontalVelocity, rb.velocity.y);
        }

        //Air Force
        else if (!isGrounded && horizontal != 0)
        {
            Vector2 forceToAdd = new Vector2(movementForceInAir * horizontal, 0);
            rb.AddForce(forceToAdd);

            if (Mathf.Abs(rb.velocity.x) > speed)
            {
                rb.velocity = new Vector2(speed * horizontal, rb.velocity.y);
            }
        }

        //Air Drag
        else if (isGrounded && horizontal == 0)
        {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
        }
    }
}

    //karakterin dönmesini sağlıyor
    void Flip()
    {
        //Turn With Scale
        if(turnWS == true)
        {
            faceRight = !faceRight;
            Vector3 scaler = transform.localScale;
            scaler.x *= -1;
            transform.localScale = scaler;
        }

        //Turn With Rotation
        else if(turnWR)
        {
            if(faceRight)
            {
                Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
                transform.rotation = Quaternion.Euler(rotator);
                faceRight = !faceRight;
            }
            else
            {
                Vector3 rotator = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
                transform.rotation = Quaternion.Euler(rotator);
                faceRight = !faceRight;
            }
        }
    }
    #endregion

    //camera lerp için ayrı metot aç
}
