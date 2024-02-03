using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class denememove : MonoBehaviour
{
    #region ana kod
    private Rigidbody2D rb;

    [SerializeField] private int speed;

    public float mainGravity;

    public float horizontal;

    private bool isGrounded;
    public Transform groundCheck;
    public float groundCheckRadius;
    public LayerMask whatIsGround;

    public float movementForceInAir;
    public float airDragMultiplier = 0.9f;

    public bool airDragOpen;
    private bool canMove = true ;

    public float horizontalVelocity;

    [Range(0, 1)] public float horizontalDampingStop;
    [Range(0, 1)] public float horizontalDampingTurn;
    [Range(0, 1)] public float horizontalDampingBasic;

    public float jumpForce;


    public float fallGravityMult; //yere düşerken yer çekimi ne kadar artacak
    public float maxFallSpeed; //yere düşerkenki limit hız

    [SerializeField] private int airSpeed;
    private int extraJumps;
    public int extraJumpsValue;
    private bool isJumpButtonPressed;
    private bool isJumping;
    private float jumpPressedRemember;
    public float jumpPressedRememberTime;
    private float groundedRemember;
    public float groundedRememberTime;
    private float jumpTimeCounter;
    public float jumpTime;
    [Range(0, 1)] public float cutJumpHeight;
    private bool canJump;
    public float lowJumpMultiplier = 2f;
    private bool variJump = true;
    public bool variJumpOpen;

    private PlayerInput playerInput;

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
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        //rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
        AirForceAndDrag();
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
    }

    private void Update()
    {
        JumpControl();
        VelocityUpdater();
        Damping();
        FastFall();
        
        DoubleJump();

        if (variJumpOpen && variJump)
        {
            VariableJump();
        }

        CheckIfCanJump();
        JumpMain(Vector2.up);
        TimeManager();
    }

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
        rb.velocity = new Vector2(horizontalVelocity, rb.velocity.y);
    }

    private void AirForceAndDrag()
    {
        if (airDragOpen == false)
        {
            ApplyMovement();

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
                ApplyMovement();
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

    private void Damping()
    {
        if (Mathf.Abs(horizontal) < 0.01f)
        {
            horizontalVelocity *= Mathf.Pow(1f - horizontalDampingStop, Time.deltaTime * 10f);
        }
        else if (Mathf.Sign(horizontal) != Mathf.Sign(horizontalVelocity))
        {
            horizontalVelocity *= Mathf.Pow(1f - horizontalDampingTurn, Time.deltaTime * 10f);
        }
        else
        {
            horizontalVelocity *= Mathf.Pow(1f - horizontalDampingBasic, Time.deltaTime * 10f);
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            isJumpButtonPressed = true;
        }
        else if(context.canceled)
        {
            isJumpButtonPressed = false;
        }
        
    }

    void JumpControl()
    {
        if (playerInput.Player.Jump.WasPressedThisFrame())
        {
            jumpPressedRemember = jumpPressedRememberTime;
        }

        if (playerInput.Player.Jump.WasReleasedThisFrame())
        {
            if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * cutJumpHeight);
            }
        }
    }

    private void JumpMain(Vector2 direction)
    {
        if (canJump && extraJumps > 0)
        {
            groundedRemember = 0;
            jumpPressedRemember = 0;
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(direction * jumpForce, ForceMode2D.Impulse);
        }
    }

    private void CheckIfCanJump()
    {
        

        if (isGrounded)
        {
           // dJumpAnimControl = false;
           // fallAnimControl = false;
            groundedRemember = groundedRememberTime;
            extraJumps = extraJumpsValue;
        }

        if(rb.velocity.y < 0 && isGrounded == false)
        {
           // jumpAnimControl = false;
           // dJumpAnimControl = false;
           // fallAnimControl = true;
        }

        if ((groundedRemember > 0) && rb.velocity.y <= 0 && (jumpPressedRemember > 0))
        {
            canJump = true;
            //jumpAnimControl = true;
        }
        else
        {
            canJump = false;
        }
    }

    private void VariableJump()
    {
        if (rb.velocity.y > 0 && !isJumpButtonPressed)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    private void DoubleJump()
    {
        if (playerInput.Player.Jump.WasPressedThisFrame() && extraJumps > 1)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
           // dJumpAnimControl = true;
            extraJumps--;
        }

        else if (playerInput.Player.Jump.WasPressedThisFrame() && extraJumps == 0)
        {
            jumpPressedRemember = jumpPressedRememberTime;
        }
    }

    private void FastFall()
    {
        if(rb.velocity.y < 0)
        {
            rb.gravityScale = mainGravity * fallGravityMult;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = mainGravity;
        }
    }

    private void TimeManager()
    {
        jumpPressedRemember -= Time.deltaTime;
        groundedRemember -= Time.deltaTime;
    }
    #endregion
}
