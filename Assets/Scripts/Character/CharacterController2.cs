using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController2 : MonoBehaviour
{
    //Sorunlar için yorumlar
// wall jumpta belli bir süreden sonra aşırı uçma ve kayma sorunu var : Çözüldü
// wall jumpta belli bir süreden sonra tutukluk yapma sorunu var, şu anlık sorun yok ama tekrar olursa walljump kodundaki forcemode ile ilgili olabilir
// variable jumpta küçük zıplamalarda takılma sorunu var : Çözüldü
// duvara mekanikleri çalışırken karakterin yönünü dönemem ve duvara doğru bakma sorunu var
// air drag ara sıra buglı çalışıyo
// flip ara sıra buglı çalışıyo

#region MainVariables
[Header("Main")]
public float mainGravity;
private Rigidbody2D rb;
private TrailRenderer tr;
private Animator anim;
private SpringJoint2D sr;
//public SpiderRope spi;

#endregion

#region OpenVariables
[Header("Open")]
//variable jumpı kontrol etmek için variJumpOpen boolu kullanılabilir. bu boolun kapatılması normal sıçramayı aktif edecektir.
// aynı şekilde airDragOpen ile Air Drag olup olmayacağıda kontrol edilebilir.
// Wall mekaniklerinin çalışmasını denetlemek için wallOpen değişkeni başka scriptte değiştirilebilir
// mekanikleri özel olarak kapatıp açmak için wallGrabOpen, wallRunOpen ve wallSlideOpen boolları kontrol edilebilir
public bool gGunOpen;
public bool variJumpOpen;
public bool airDragOpen;
public bool wallOpen = false;
public bool wallRunOpen;
public bool wallSlideOpen;
public bool wallGrabOpen;
public bool dashOpen;

#endregion

#region LayerMaskVariables
[Header("Layer Masks")]
public LayerMask whatIsGround;
public LayerMask whatIsWall;
public LayerMask whatIsCorner;

#endregion

#region SoundVariables
[Header("Sounds")]
public AudioSource audioSrc;
public AudioClip walkSound;
public AudioClip deathSound;
public AudioClip jumpSound;

#endregion

#region WalkVariables
[Header("Walk")]
public float speed;
[HideInInspector] public float moveInput;
private float horizontalVelocity;
private bool faceRight = true;
[Range(0, 1)] public float horizontalDampingStop;
[Range(0, 1)] public float horizontalDampingTurn;
[Range(0, 1)] public float horizontalDampingBasic;
private bool canMove => !wallGrab;
private float heightInput;
private float direction = 1;
private bool walkSBool = false;

#endregion

#region JumpVariables
[Header("Jump")]
public int extraJumpsValue;
private int extraJumps;
public float fallMultiplier = 2.5f;
public float lowJumpMultiplier = 2f;
public float movementForceInAir;
public float jumpForce;
public float airDragMultiplier = 0.9f;
private float jumpPressedRemember = 0;
public float jumpPressedRememberTime = 0.2f;
[Range(0, 1)] public float cutJumpHeight;
private bool canJump;
public float normalJumpTime = 1f;
private bool variJump = true;
public float dJumpDustTime = 0.1f;
public ParticleSystem dJumpDustParticle;
private bool dJumpAnim;
private bool jumpAnim;
[HideInInspector] public float yVelocity;

#endregion

#region GroundVariables
[Header("Ground Check")]
public Transform groundCheck;
private bool isGrounded;
private float groundedRemember = 0;
public float groundedRememberTime = 0.2f;
public float groundCheckRadius;

#endregion

#region CornerCorrectionVariables
[Header("Corner Correction")]
[SerializeField] private float topRaycastLength;
[SerializeField] private Vector3 edgeRaycastOffset;
[SerializeField] private Vector3 innerRaycastOffset;
private bool canCornerCorrect;

#endregion

#region WallVariables
[Header("Wall")]
public float wallRaycastLenght;
private bool isWall;
private bool isRightWall;
private bool wallGrab => isWall && !isGrounded && Input.GetButton("WallGrab") && !wallRun && wallOpen && wallGrabOpen;
private bool wallSlide => isWall && !isGrounded && rb.velocity.y < 0f && !Input.GetButton("WallGrab") && !wallRun && wallOpen && wallSlideOpen;
private bool wallRun => isWall && heightInput > 0f && wallOpen && wallRunOpen;
public float wallSlideModifier = 0.5f;
public float wallSlideSpeed;
public float wallRunModifier = 0.85f;
public float wallRunSpeed;
public float wallJumpForce = 18f;
public float wallJumpDirection = -1f;
public Vector2 wallJumpAngle;

#endregion

#region DashVariables
[Header("Dash")]
public float dashingPower = 24f;
public float dashingTime = 0.2f;
public float dashingCooldown = 1f;
private bool canDash = true;
private bool isDashing;
IEnumerator dashCoroutine;
public ParticleSystem dashParticle;
public float distanceBetweenImages;
private float lastImageXPos;

#endregion

/* #region GrapplingHookVariables
// grap kodu sıkıntılı tekrardan kontrol et
[Header("Grap")]
public Transform rightPos;
public Transform leftPos;
public Transform upPos;
public Transform downPos;
private Transform mainPos;
public GameObject rope;
public float ropeSpeed = 20f;
public float pullForce = 50f;
[SerializeField] private bool hasMaxDistance = false;
[SerializeField] private float maxDistnace = 20;
public float grapDirection = 1f;
#endregion*/

#region MainMethods
private void Start()
{
    rb = GetComponent<Rigidbody2D>();
    tr = GetComponent<TrailRenderer>();
    anim = GetComponent<Animator>();
    sr = GetComponent<SpringJoint2D>();
    extraJumps = extraJumpsValue;
    rb.gravityScale = mainGravity;
    // spi.speed = ropeSpeed;
    // spi.pullForce = pullForce;
}

private void Update()
{
    CheckInput();
    CheckDirection();
    CheckIfCanJump();
    TimeManager();
    FastFall();
    Jump(Vector2.up);
    AnimationControl();

    if (variJumpOpen && variJump)
    {
        VariableJump();
    }
}

private void FixedUpdate()
{
    ApplyMovement();
    CheckSurroundings();
    WallCheck();
    WallJump();
    DashForce();

    if (canCornerCorrect)
    {
        CornerCorrect(rb.velocity.y);
    }
}
#endregion

#region CheckMethods
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

    //Jump Inputs
    if (Input.GetButtonDown("Jump"))
    {
        jumpPressedRemember = jumpPressedRememberTime;
        jumpAnim = true;
    }

    if (Input.GetButtonUp("Jump"))
    {
        if (rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * cutJumpHeight);
        }
    }

    //Double Jump
    if (Input.GetButtonDown("Jump") && extraJumps > 1 && !isWall)
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        dJumpAnim = true;
        extraJumps--;
    }

    else if (Input.GetButtonDown("Jump") && extraJumps == 0)
    {
        jumpPressedRemember = jumpPressedRememberTime;
    }

    //Dash
    if (Input.GetKeyDown(KeyCode.LeftShift) && canDash == true)
    {
        if (dashOpen)
        {
            if (dashCoroutine != null)
            {
                StopCoroutine(dashCoroutine);
            }

            dashCoroutine = Dash(dashingTime, dashingCooldown);
            StartCoroutine(dashCoroutine);
        }
    }

    //GrapplingHook
    /*
    // wall directiona göre ropeun posunu ayarla
    if (Input.GetKeyDown(KeyCode.D) && Input.GetKey(KeyCode.F))
    {
        mainPos = rightPos;
        GrapPoint();
    }

    if (Input.GetKeyDown(KeyCode.A) && Input.GetKey(KeyCode.F))
    {
        mainPos = leftPos;
        GrapPoint();
    }

    if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.F))
    {
        mainPos = downPos;
        GrapPoint();
    }

    if (Input.GetKeyDown(KeyCode.W) && Input.GetKey(KeyCode.F))
    {
        mainPos = upPos;
        GrapPoint();
    }

    if (Input.GetKey(KeyCode.G) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A))
    {
        sr.connectedAnchor = rope.transform.position;
    }

    if (Input.GetKeyUp(KeyCode.G) || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.A))
    {
        sr.enabled = false;
        rope.SetActive(false);
    }

    if (Vector2.Distance(rope.transform.position, this.transform.position) >= maxDistnace || !hasMaxDistance)
    {
        rope.SetActive(false);
    }
    */
}
#endregion

#region MovementMethods
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
        else if (!isGrounded && !wallSlide && moveInput != 0)
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
    wallJumpDirection *= -1;
  //  grapDirection *= -1;
    faceRight = !faceRight;
    Vector3 scaler = transform.localScale;
    scaler.x *= -1;
    transform.localScale = scaler;

}

#endregion

#region WallMethods
private void WallCheck()
{
    if (wallGrab)
    {
        WallGrab();
    }

    else
    {
        rb.gravityScale = mainGravity;
    }

    if (wallSlide)
    {
        WallSlide();
    }

    if (wallRun)
    {
        WallRun();
    }

    if (isWall)
    {
        StickToWall();
    }
}

private void WallGrab()
{
    rb.gravityScale = 0;
    rb.velocity = Vector2.zero;
}

private void WallSlide()
{
    rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed * wallSlideModifier);
}

private void WallRun()
{
    rb.velocity = new Vector2(rb.velocity.x, heightInput * wallRunSpeed * wallRunModifier);
}

private void WallJump()
{
    if ((wallSlide || isWall || isRightWall || wallGrab) && Input.GetButtonDown("Jump"))
    {
        rb.AddForce(new Vector2(wallJumpForce * wallJumpDirection * wallJumpAngle.x, wallJumpForce * wallJumpAngle.y), ForceMode2D.Impulse);
    }
}

private void StickToWall()
{
    //Push player torwards wall
    if (isRightWall && moveInput >= 0f)
    {
        rb.velocity = new Vector2(1f, rb.velocity.y);
    }

    else if (!isRightWall && moveInput <= 0f)
    {
        rb.velocity = new Vector2(-1f, rb.velocity.y);
    }

    //Face correct direction
    if (isRightWall && !faceRight)
    {
        Flip();
    }
    else if (!isRightWall && faceRight)
    {
        Flip();
    }
}

#endregion

#region JumpMethods
private void Jump(Vector2 direction)
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
        groundedRemember = groundedRememberTime;
        extraJumps = extraJumpsValue;
        dJumpAnim = false;
    }

    if ((groundedRemember > 0) && rb.velocity.y <= 0 && (jumpPressedRemember > 0))
    {
        canJump = true;
    }

    else
    {
        canJump = false;
    }
}

private void VariableJump()
{
    if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
    {
        rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
    }
}

private void FastFall()
{
    if (rb.velocity.y < 0)
    {
        rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
    }
}

private IEnumerator ReturneNormalJump()
{
    variJump = false;
    yield return new WaitForSeconds(normalJumpTime);
    variJump = true;
}
#endregion

#region DashMethods
private void DashForce()
{
    if (isDashing)
    {
        rb.AddForce(new Vector2(direction * dashingPower, 0), ForceMode2D.Impulse);

        if (Mathf.Abs(transform.position.x - lastImageXPos) > distanceBetweenImages)
        {
            AfterImagePool.instance.GetFromPool();
            lastImageXPos = transform.position.x;
        }
    }
}

private IEnumerator Dash(float dashTime, float dashCooldown)
{
    Vector2 originalVelocity = rb.velocity;

    canDash = false;
    isDashing = true;

    tr.emitting = true;

    rb.gravityScale = 0;
    rb.velocity = Vector2.zero;

    dashParticle.Play();

    AfterImagePool.instance.GetFromPool();
    lastImageXPos = transform.position.x;

    yield return new WaitForSeconds(dashTime);

    dashParticle.Stop();

    tr.emitting = false;

    isDashing = false;

    rb.gravityScale = mainGravity;
    rb.velocity = originalVelocity;

    yield return new WaitForSeconds(dashCooldown);

    canDash = true;
}

#endregion

/* #region GrapplingHookMethods
private void GrapPoint()
{
if (gGunOpen == true)
{
spi.firePoint = mainPos;
rope.SetActive(true);
rope.transform.position = mainPos.transform.position;
rope.transform.rotation = mainPos.transform.rotation;
sr.anchor = mainPos.transform.localPosition;
sr.connectedAnchor = rope.transform.position;
}
}

#endregion */

#region SoundMethods
private void CharacterRunSound()
{
    audioSrc.PlayOneShot(walkSound);
}

#endregion

#region OtherMethods
private void AnimationControl()
{
    anim.SetFloat("Speed", Mathf.Abs(moveInput));

    if (wallSlide)
    {
        anim.SetFloat("yVelocity", 0);
    }
    else
    {
        anim.SetFloat("yVelocity", rb.velocity.y);
    }

    if (jumpAnim)
    {
        anim.SetBool("isJumping", true);
    }

    else
    {
        anim.SetBool("isJumping", false);
    }

    if (wallSlide)
    {
        anim.SetBool("isDJumping", false);
        anim.SetBool("isJumping", false);
        anim.SetBool("isWallSliding", true);
        anim.SetFloat("heightInput", 0f);
    }
    else
    {
        anim.SetBool("isWallSliding", false);
    }

    if (wallGrab)
    {
        anim.SetBool("isDJumping", false);
        anim.SetBool("isJumping", false);
        anim.SetBool("WallGrab", true);
    }
    else
    {
        anim.SetBool("WallGrab", false);
    }

    if (rb.velocity.y < 0f)
    {
        anim.SetFloat("heightInput", 0f);
    }

    if (wallRun)
    {
        anim.SetBool("isDJumping", false);
        anim.SetBool("isJumping", false);
        anim.SetFloat("heightInput", Mathf.Abs(heightInput));
    }

    if (dJumpAnim && !wallGrab && !wallRun && !wallSlide)
    {
        anim.SetBool("isDJumping", true);
        jumpAnim = false;
    }

    if (isGrounded)
    {
        anim.SetBool("isGrounded", true);
        anim.SetBool("isDJumping", false);
        anim.SetBool("isJumping", false);
    }

    else
    {
        anim.SetBool("isGrounded", false);
    }

    if (isDashing == true)
    {
        anim.SetBool("isDashing", true);
        anim.SetBool("isGrounded", false);
        anim.SetBool("isFalling", false);
        anim.SetBool("WallGrab", false);
        anim.SetBool("isJumping", false);
        anim.SetBool("isDJumping", false);
        anim.SetFloat("heightInput", 0f);
    }
    else
    {
        anim.SetBool("isDashing", false);
    }
}

private void TimeManager()
{
    jumpPressedRemember -= Time.deltaTime;
    groundedRemember -= Time.deltaTime;
}

private void CheckSurroundings()
{
    //Ground Collision
    isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

    //Corner Collision
    canCornerCorrect = Physics2D.Raycast(transform.position + edgeRaycastOffset, Vector2.up, topRaycastLength, whatIsCorner) &&
                       !Physics2D.Raycast(transform.position + innerRaycastOffset, Vector2.up, topRaycastLength, whatIsCorner) ||
                       Physics2D.Raycast(transform.position - edgeRaycastOffset, Vector2.up, topRaycastLength, whatIsCorner) &&
                       !Physics2D.Raycast(transform.position - innerRaycastOffset, Vector2.up, topRaycastLength, whatIsCorner);

    //Wall Collision
    isWall = Physics2D.Raycast(transform.position, Vector2.right, wallRaycastLenght, whatIsWall) ||
             Physics2D.Raycast(transform.position, Vector2.left, wallRaycastLenght, whatIsWall);

    isRightWall = Physics2D.Raycast(transform.position, Vector2.right, wallRaycastLenght, whatIsWall);
}

private void OnDrawGizmos()
{
    //Ground Check
    Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

    //Corner Check
    Gizmos.DrawLine(transform.position + edgeRaycastOffset, transform.position + edgeRaycastOffset + Vector3.up * topRaycastLength);
    Gizmos.DrawLine(transform.position - edgeRaycastOffset, transform.position - edgeRaycastOffset + Vector3.up * topRaycastLength);
    Gizmos.DrawLine(transform.position + innerRaycastOffset, transform.position + innerRaycastOffset + Vector3.up * topRaycastLength);
    Gizmos.DrawLine(transform.position - innerRaycastOffset, transform.position - innerRaycastOffset + Vector3.up * topRaycastLength);

    //Corner Distance Check
    Gizmos.DrawLine(transform.position - innerRaycastOffset + Vector3.up * topRaycastLength,
                    transform.position - innerRaycastOffset + Vector3.up * topRaycastLength + Vector3.left * topRaycastLength);
    Gizmos.DrawLine(transform.position + innerRaycastOffset + Vector3.up * topRaycastLength,
                    transform.position + innerRaycastOffset + Vector3.up * topRaycastLength + Vector3.right * topRaycastLength);

    //Wall Check
    Gizmos.DrawLine(transform.position, transform.position + Vector3.right * wallRaycastLenght);
    Gizmos.DrawLine(transform.position, transform.position + Vector3.left * wallRaycastLenght);

    //Grap Check
    /*
    if (spi.firePoint != null && hasMaxDistance)
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(this.transform.position, maxDistnace);
    } */
}

private void CornerCorrect(float Yvelocity)
{
    //Push Right
    RaycastHit2D hit = Physics2D.Raycast(transform.position - innerRaycastOffset + Vector3.up * topRaycastLength,
        Vector3.left, topRaycastLength, whatIsCorner);

    if (hit.collider != null)
    {
        float newPos = Vector3.Distance(new Vector3(hit.point.x, transform.position.y, 0f) + Vector3.up * topRaycastLength,
            transform.position - edgeRaycastOffset + Vector3.up * topRaycastLength);

        transform.position = new Vector3(transform.position.x + newPos, transform.position.y, transform.position.z);

        rb.velocity = new Vector2(rb.velocity.x, Yvelocity);

        return;
    }

    //Push Left
    hit = Physics2D.Raycast(transform.position + innerRaycastOffset + Vector3.up * topRaycastLength,
        Vector3.right, topRaycastLength, whatIsCorner);

    if (hit.collider != null)
    {
        float newPos = Vector3.Distance(new Vector3(hit.point.x, transform.position.y, 0f) + Vector3.up * topRaycastLength,
            transform.position + edgeRaycastOffset + Vector3.up * topRaycastLength);

        transform.position = new Vector3(transform.position.x - newPos, transform.position.y, transform.position.z);

        rb.velocity = new Vector2(rb.velocity.x, Yvelocity);
    }
}
#endregion
}
/*
███████╗███████╗
██╔════╝╚══███╔╝
█████╗    ███╔╝
██╔══╝   ███╔╝

███████╗███████╗
╚══════╝╚══════╝
*/

/*
private Rigidbody2D rb;
private Animator animator;
private TrailRenderer traRen;

[Header("Movement")]
[SerializeField] private int Speed;
private bool faceRight = true;
public float moveInput1;
public float moveInput2;

[Header("Jumping")]
[SerializeField] private int jumpSpeed;
private bool isGrounded;
public Transform groundCheck;
public float checkRadius;
public LayerMask whatIsGround;
private float fJumpPressedRemember;
public float fJumpPressedRememberTime;
private float fGroundedRemember;
public float fGroundedRememberTime;
private float jumpTimeCounter;
public float jumpTime;
private bool isJumping;
private int extraJumps;
public int extraJumpsValue;
[SerializeField] Animator dustAnim;
[SerializeField] GameObject dust;

[Header("Dashing")]
public float dashSpeed;
private float dashTime;
public float startDashTime;
private int direction;
public bool isDashing = false;

private void Start()
{
    animator = GetComponent<Animator>();
    rb = GetComponent<Rigidbody2D>();
    traRen = GetComponent<TrailRenderer>();
    extraJumps = extraJumpsValue;
    dashTime = startDashTime;
}

private void FixedUpdate()
{
    float moveInput = Input.GetAxisRaw("Horizontal");
    rb.velocity = new Vector2(moveInput * Speed, rb.velocity.y);
    moveInput1 = Input.GetAxisRaw("Horizontal");
    moveInput2 = rb.velocity.y;
    animator.SetFloat("Speed", Mathf.Abs(moveInput));

    if (faceRight == true && moveInput < 0)
    {
        Flip();
    }
    else if (faceRight == false && moveInput > 0)
    {
        Flip();
    }



}

private void Update()
{
    isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround); // karakterin altında yerle etkileşimi denetlemek için bir daire oluşturuyor
    fJumpPressedRemember -= Time.deltaTime;
    fGroundedRemember -= Time.deltaTime;
    Jump();

}

void Flip()
{
    faceRight = !faceRight;
    Vector3 scaler = transform.localScale;
    scaler.x *= -1;
    transform.localScale = scaler;
}

void Jump()
{
    if (isGrounded == true)
    {
        animator.SetBool("isJumping", false);
        animator.SetBool("isDJumping", false);
        extraJumps = extraJumpsValue;
        fGroundedRemember = fGroundedRememberTime;
    }

    if ((fGroundedRemember > 0) && Input.GetKeyDown(KeyCode.Z) )
    {
        fGroundedRemember = 0;
        isJumping = true;
        jumpTimeCounter = jumpTime;
        fJumpPressedRemember = fJumpPressedRememberTime;
    }


    if (Input.GetKey(KeyCode.Z) && isJumping == true)
    {
        if (jumpTimeCounter > 0)
        {
            rb.velocity = Vector2.up * jumpSpeed;
            animator.SetBool("isJumping", true);

            jumpTimeCounter -= Time.deltaTime;
        }
        else
        {
            isJumping = false;
        }
    }

    // Double Jump Start
    if (Input.GetKeyUp(KeyCode.Z))
    {
        isJumping = false;
    }
    if (isGrounded == true)
    {
        extraJumps = extraJumpsValue;
    }

    if (Input.GetKeyDown(KeyCode.Z) && extraJumps > 0)
    {
        rb.velocity = Vector2.up * jumpSpeed;
        animator.SetBool("isDJumping", true);
        extraJumps--;
    }
    else if (Input.GetKeyDown(KeyCode.Z) && extraJumps == 0)
    {
        fJumpPressedRemember = fJumpPressedRememberTime;
    }

    if ((fJumpPressedRemember > 0) && (fGroundedRemember > 0))
    {
        fJumpPressedRemember = 0;
        fGroundedRemember = 0;
        rb.velocity = Vector2.up * jumpSpeed;
    }
    // Double Jump End
}
*/
