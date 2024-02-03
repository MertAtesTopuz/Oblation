using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController : MonoBehaviour
{
    #region Not
    // karakter variable jump yaparken yükseldikçe yükselme hızı azalsın
    // wall slide animasyonu takılıyor sebebi any stateden bağlı olması buna bir çözüm bul
    // variables için ayrı kod yazılabilir
    // variablelerin düzenlenmesi lazım
    // Air Drag buglı çalışıyor
    // Animasyonlar ara sıra buga girebiliyor
    // wall jumpta belli bir süreden sonra tutukluk yapma sorunu var, şu anlık sorun yok ama tekrar olursa walljump kodundaki forcemode ile ilgili olabilir
    // kod genel olarak çok emanet duruyor. bazı yerlerde boollar (!isGrounded, isGrounded) gibi kullanılırken bazı yerlerde (isGrounded == false, isGrounded == true) şeklinde kullanılıyor.
    // aynı şekilde bazı boollar if ile kontrol edilirken bazıları variable olarak tanımlandığı yerde kontrol ediliyor
    // yukarıdaki iki sebepten dolayı kod toplama gibi gözüküyor.
    // düşme animasyonu ara sıra buga giriyor. tekrar girerse ilgilen
    // zemindeyken bile karakter haraket edince kendine bir y velociysi ekliyor
    // bazen duvardayken karakter allah katına uçabiliyor corner correct ile ilgili olabilir
    // double jump arasıra buglı çalışıyor
    // wall grab ile wall run animasyonları çakışıyor

    // ANİMATOR HELLDEN KURTULLLLLLLL!!!!!

    /*
    Air Drag
    Air Force
    Damping
    Variable Jump
    Fast Fall
    Coyote Time
    Jump Buffer
    Double Jump
    Max Fall Speed
    Jump Corner Correction
    Wall Slide
    Wall Jump
    Wall Run
    Wall Grab
    Dash
    */

    #endregion

    #region Main Variables

    public static CharacterController instance; //Kodu public yapıyor

    public float mainGravity; //oyundaki temel yerçekimi
    private Rigidbody2D rb; //rigidbody2d componenti
    private Animator animator; //animator componenti
    private CamerFollowObject camFollowObj; //CameraFollowObject kodu
    private PlayerInput playerInput; //PlayerInputun kod hali

    #endregion

    #region Open Variables

    [Header("Open")]
    public bool airDragOpen; //air dragin kullanılıp kullanılmayacağını denetliyor
    public bool variJumpOpen; //variable jumpın kullanılıp kullanılmayacağını denetliyor
    public bool variJump2Open; //variable jumpın cut jump height ile kullanılıp kullanılmayacağını denetliyor
    public bool cornerCorrectOpen; //corner correctin kullanılıp kullanılmayacağını denetliyor
    public bool wallOpen; //duvar mekaniklerinin genelinin kullanılıp kullanılmayacağını denetliyor. başka bir scripten değiştirmek mantıklı olabilir
    public bool wallJumpOpen; //wall jump kullanılıp kullanılmayacağını denetliyor
    public bool wallGrabOpen; //wall grab kullanılıp kullanılmayacağını denetliyor
    public bool wallRunOpen; //wall run kullanılıp kullanılmayacağını denetliyor
    public bool wallSlideOpen; //wall slide kullanılıp kullanılmayacağını denetliyor
    public bool dashOpen;
    public bool turnWS; //scaleyi değiştirerek dön
    public bool turnWR; //rotationu değiştirerek dön

    #endregion

    #region Movement Variables

    [Header("Movement")]
    [SerializeField] private float fallGravityMult; //yere düşerken yer çekimi ne kadar artacak
    [SerializeField] private float maxFallSpeed; //yere düşerkenki limit hız
    [Range(0, 1)] [SerializeField] private float horizontalDampingStop; //dururkenki sürtünme
    [Range(0, 1)] [SerializeField] private float horizontalDampingTurn; //dönerkenki sürtünme
    [Range(0, 1)] [SerializeField] private float horizontalDampingBasic; //genel sürtünme
    private float horizontalVelocity; //x eksenindeki velocity değişimi
    private float fallSpeedYDampingChangeTrashold; //kamera gecikmesi için y değeri tutucu
    private float horizontal; //karakterin x eksenindeki hareketi
    private float vertical; //karakterin y eksenindeki hareketi
    private float direction; //karakterin baktığı yönün sayısal karşılığı
    private bool canMove => !wallGrab; //hareket edip edilemeyeceğini denetliyor
    [HideInInspector] public bool faceRight = true; //karakterin yön değişimi

    #endregion

    #region Jump Variables

    [Header("Jumping")]
    [SerializeField] private int airSpeed; //havadaki hız
    [SerializeField] private int extraJumpsValue; //extra kaç defa zıplayabileceimiz
    [SerializeField] private float jumpForce; //zıplama hızı
    [SerializeField] private float jumpPressedRememberTime; //butona basıldıktan sonra ne kadar süre içinde butona tekrar basarsak sıçrayabileceğimiz
    [SerializeField] private float movementForceInAir; //air speed 2
    [SerializeField] private float airDragMultiplier = 0.9f; //havadaki hızın ne kadar sönümleneceği
    [SerializeField] private float lowJumpMultiplier = 2f; //küçük zıplamaların süresi
    [Range(0, 1)] [SerializeField] private float cutJumpHeight; //variable jump 2 için minimum sıçrama yüksekliği
    private int extraJumps; //kaç tane zıplama hakkımız kaldığını denetliyor
    private bool canJump; //zıplayıp zıplayamayacağımızı denetliyor
    private bool isJumpButtonPressed; //zıplama tuşuna basılıp basılmadığı
    private float jumpPressedRemember; //zıplama tuşuna basılması üstünden geçen süre
    
    #endregion

    #region Dash Variables

    [Header("Dash")]
    [SerializeField] private float dashingPower = 24f; //dashin gücü
    [SerializeField] private float dashingTime = 0.2f; //dash ne kadar sürecek
    [SerializeField] private float dashingCooldown = 1f; //dash attıktan sonra yeniden ne zaman dash atabilicez
    [SerializeField] private float distanceBetweenImages; //arkada oluşan resimler arasındaki mesafe
    private float lastImageXPos; //en son oluşan resmin konumu
    private bool canDash = true; //dash atıp atamayacağımızı denetler
    private bool isDashing; //dash atıyot muyuz onu denetler
    private IEnumerator dashCoroutine; //dash atma coroutini

    #endregion

    #region Wall Variables

    [Header("Wall")]
    [SerializeField] private Transform wallRaycastPos; //duvar raycastinin başlangıç pozisyonu
    [SerializeField] private float wallRaycastLenght; //duvar raycastinin uzunluğu
    [SerializeField] private float wallSlideSpeed; //duvardan kayma hızı
    [SerializeField] private float wallRunSpeed; //duvarda koşma hızı
    [SerializeField] private float wallJumpForce = 18f; //duvardan sıçrama kuvveti
    [SerializeField] private Vector2 wallJumpAngle; //duvardan sıçradıktan sonra karaktere x ve y ekseninde eklenecek olan yön. yani duvardan sıçrayınca xte ve yde ne kadar ilerleyecek
    private float wallJumpDirection = -1f; //duvardayken ne tarafa sıçrayacağımız
    private bool isWallGrabButtonPressed; //wall grab tuşuna basılıp basılmadığı
    private bool isWall; //duvara değilip değilmediği
    private bool isRightWall; //sağ taratan duvara değilip değilmediği
    private bool wallGrab => isWall && !isGrounded && isWallGrabButtonPressed && !wallRun && wallOpen && wallGrabOpen; //duvara tutunmayı denetliyor
    private bool wallSlide => isWall && !isGrounded && rb.velocity.y < 0f && !isWallGrabButtonPressed && !wallRun && wallOpen && wallSlideOpen; //duvarda kaymayı denetliyor
    private bool wallRun => isWall && vertical > 0f && wallOpen && wallRunOpen; //duvarda koşmayı denetliyor

    #endregion

    #region Ground Variables

    [Header("Ground Check")]
    [SerializeField] private float groundedRememberTime; //yere en son temasımıza göre ne zaman içinde yere değmeden tekrar zıplayabileceğimiz
    [SerializeField] private float checkRadius; //yeri inceleyen raycastin yarıçapı
    [SerializeField] private Transform groundCheck; //yeri inceleyen raycastin konumu
    private float groundedRemember; //yere en temasımız üstünden ne kadar zaman geçtiği
    private bool isGrounded; //yere değip değmediğimiz

    #endregion
    
    #region Camera Variables

    [Header("Camera")]
    [SerializeField] private GameObject camFollowObject;

    #endregion

    #region Corner Variables
    
    [Header("Corner Correction")]
    [SerializeField] private float topRaycastLength;
    [SerializeField] private Vector3 edgeRaycastOffset;
    [SerializeField] private Vector3 innerRaycasrtOffset;
    private bool canCornerCorrect;

    #endregion

    #region Animation Variables

    private bool dJumpAnimControl;
    private bool jumpAnimControl;
    private bool fallAnimControl;

    #endregion

    #region Layer Variables

    [Header("Layers")]
    [SerializeField] private LayerMask whatIsGround; //zeminin layer maski
    [SerializeField] private LayerMask cornerLayer; //kenarların layer maski
    [SerializeField] private LayerMask whatIsWall; //duvar layer maski

    #endregion

    #region SoundVariables

    [Header("Sounds")]
    [SerializeField] private AudioSource audioSrc;
    [SerializeField] private AudioClip walkSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip jumpSound;

    #endregion

    #region ParticleVariables

    [Header("Particles")]
    [SerializeField] private ParticleSystem runParticle; //koşunca çıkan particle (daha eklenmedi)
    [SerializeField] private ParticleSystem jumpParticle; //zıplayınca çıkan particle (daha eklenmedi)
    [SerializeField] private ParticleSystem dashParticle; //dash atarken arkadan çıkan particle affect

    #endregion

    #region Main Methods

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }

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
        camFollowObj = camFollowObject.GetComponent<CamerFollowObject>();

        //Equalization
        fallSpeedYDampingChangeTrashold = CameraManager.instance.fallSpeedYDampingChangeThreshold;
        extraJumps = extraJumpsValue;
    }

    private void FixedUpdate()
    {
        //Methods
        CheckSurroundings();
        CheckDirection();
        AirForceAndDrag();
        WallCheck();
        WallJump();
        DashForce();

        //Corner Check
        if(canCornerCorrect == true && cornerCorrectOpen == true)
        {
            CornerCorrect(rb.velocity.y);
        }
    }

    private void Update()
    {
        //Methods
        JumpControl();
        DoubleJump();
        FastFall();
        AnimationControl();
        VelocityUpdater();
        Damping();
        CameraLearp();
        TimeManager();
        CheckIfCanJump();
        DirectionCheck();
        JumpMain(Vector2.up);
        
        if (variJumpOpen == true)
        {
            VariableJump();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        //Ground Check
        Gizmos.DrawSphere(groundCheck.position, checkRadius);

        //Corner Check
        if(cornerCorrectOpen == true)
        {
            Gizmos.DrawLine(transform.position + edgeRaycastOffset, transform.position + edgeRaycastOffset + Vector3.up * topRaycastLength);
            Gizmos.DrawLine(transform.position - edgeRaycastOffset, transform.position - edgeRaycastOffset + Vector3.up * topRaycastLength);
            Gizmos.DrawLine(transform.position + innerRaycasrtOffset, transform.position + innerRaycasrtOffset + Vector3.up * topRaycastLength);
            Gizmos.DrawLine(transform.position - innerRaycasrtOffset, transform.position - innerRaycasrtOffset + Vector3.up * topRaycastLength);
        }

        //Corner Distance Check
        if(cornerCorrectOpen == true)
        {
            Gizmos.DrawLine(transform.position - innerRaycasrtOffset + Vector3.up * topRaycastLength, 
                            transform.position - innerRaycasrtOffset + Vector3.up * topRaycastLength + Vector3.left * topRaycastLength);
            Gizmos.DrawLine(transform.position + innerRaycasrtOffset + Vector3.up * topRaycastLength, 
                            transform.position + innerRaycasrtOffset + Vector3.up * topRaycastLength + Vector3.right * topRaycastLength);
        }

        //Wall Check
        if(wallOpen == true)
        {
            Gizmos.DrawLine(wallRaycastPos.position, wallRaycastPos.position + Vector3.right * wallRaycastLenght);
            Gizmos.DrawLine(wallRaycastPos.position, wallRaycastPos.position + Vector3.left * wallRaycastLenght);
        }
    }

    #endregion

    #region Check

    private void CheckDirection()
    {
        if (faceRight == true && horizontal < 0)
        {
            Flip();
        }
        else if (faceRight == false && horizontal > 0)
        {
            Flip();
        }
    }

    #endregion

    #region Move

    public void Move(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
        vertical = context.ReadValue<Vector2>().y;
    }

    private void VelocityUpdater()
    {
        if (canMove == true)
        {
            horizontalVelocity = rb.velocity.x;
            horizontalVelocity += horizontal;
        }
    }

    private void ApplyMovement()
    {
        rb.velocity = new Vector2(horizontalVelocity, rb.velocity.y);
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

    void Flip()
    {
        //Turn With Scale
        if(turnWS == true)
        {
            wallJumpDirection *= -1;
            faceRight = !faceRight;
            Vector3 scaler = transform.localScale;
            scaler.x *= -1;
            transform.localScale = scaler;
        }

        //Turn With Rotation
        else if(turnWR == true)
        {
            if(faceRight == true)
            {
                Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
                transform.rotation = Quaternion.Euler(rotator);
                faceRight = !faceRight;
                camFollowObj.CallTurn();
                wallJumpDirection *= -1;
            }
            else
            {
                Vector3 rotator = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
                transform.rotation = Quaternion.Euler(rotator);
                faceRight = !faceRight;
                camFollowObj.CallTurn();
                wallJumpDirection *= -1;
            }
        }
    }

    private void DirectionCheck()
    {
        //Direction Check
        if (horizontal != 0)
        {
            direction = horizontal;
        }
    }

    #endregion

    #region Jump

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
            jumpAnimControl = true;
        }

        if (playerInput.Player.Jump.WasReleasedThisFrame() && variJump2Open == true)
        {
            if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * cutJumpHeight);
            }
        }
    }

    private void JumpMain(Vector2 direction)
    {
        if (canJump == true && extraJumps > 0)
        {
            groundedRemember = 0;
            jumpPressedRemember = 0;
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(direction * jumpForce, ForceMode2D.Impulse);
        }
    }

    private void CheckIfCanJump()
    {
        if (isGrounded == true)
        {
            dJumpAnimControl = false;
            fallAnimControl = false;
            groundedRemember = groundedRememberTime;
            extraJumps = extraJumpsValue;
        }

        if(rb.velocity.y < 0 && isGrounded == false)
        {
            jumpAnimControl = false;
            dJumpAnimControl = false;
            fallAnimControl = true;
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
        if (rb.velocity.y > 0 && isJumpButtonPressed == false)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    private void DoubleJump()
    {
        if (playerInput.Player.Jump.WasPressedThisFrame() && extraJumps > 1 && isWall == false)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            dJumpAnimControl = true;
            extraJumps--;
        }
        else if (playerInput.Player.Jump.WasPressedThisFrame() && extraJumps == 0)
        {
            jumpPressedRemember = jumpPressedRememberTime;
        }
    }

    private void FastFall()
    {
        if(rb.velocity.y < 0 && isGrounded == false)
        {
            rb.gravityScale = mainGravity * fallGravityMult;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }
        else if(isDashing == false)
        {
            rb.gravityScale = mainGravity;
        }
    }

    private void AirForceAndDrag()
    {
        if (airDragOpen == false)
        {
            ApplyMovement();

            //Air Force
            if (isGrounded == false && horizontal != 0)
            {
                Vector2 forceToAdd = new Vector2(movementForceInAir * horizontal, 0);
                rb.AddForce(forceToAdd);

                if (Mathf.Abs(rb.velocity.x) > airSpeed)
                {
                    rb.velocity = new Vector2(airSpeed * horizontal, rb.velocity.y);
                }
            }
        }

        if (airDragOpen == true)
        {
            if (isGrounded == true)
            {
                ApplyMovement();
            }

            //Air Force
            else if (isGrounded == false && wallSlide == false && horizontal != 0)
            {
                Vector2 forceToAdd = new Vector2(movementForceInAir * horizontal, 0);
                rb.AddForce(forceToAdd);

                if (Mathf.Abs(rb.velocity.x) > airSpeed)
                {
                    rb.velocity = new Vector2(airSpeed * horizontal, rb.velocity.y);
                }
            }

            //Air Drag
            else if (isGrounded == true && horizontal == 0)
            {
                rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
            }
        }
    }

    #endregion

    #region Wall

    private void WallCheck()
    {
        if (wallGrab == true)
        {
            WallGrab();
        }
        else if(isDashing == false)
        {
            rb.gravityScale = mainGravity;
        }

        if (wallSlide == true)
        {
            WallSlide();
        }

        if (wallRun == true)
        {
            WallRun();
        }

        if (isWall == true)
        {
            StickToWall();
        }
    }

    public void WallGrab(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            isWallGrabButtonPressed = true;
        }
        else if(context.canceled)
        {
            isWallGrabButtonPressed = false;
        }
    }

    private void WallGrab()
    {
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
    }

    private void WallSlide()
    {
        rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
    }

    private void WallRun()
    {
        rb.velocity = new Vector2(rb.velocity.x, vertical * wallRunSpeed);
    }

    private void WallJump()
    {
        if ((wallSlide == true || isWall == true || isRightWall == true || wallGrab == true) && playerInput.Player.Jump.WasPressedThisFrame() && isGrounded == false && wallJumpOpen == true)
        {
            rb.AddForce(new Vector2(wallJumpForce * wallJumpDirection * wallJumpAngle.x, wallJumpForce * wallJumpAngle.y), ForceMode2D.Impulse);
        }
    }

    private void StickToWall()
    {
        //Push player torwards wall
        if (isRightWall == true && horizontal >= 0f)
        {
            rb.velocity = new Vector2(1f, rb.velocity.y);
        }

        else if (isRightWall == false && horizontal <= 0f)
        {
            rb.velocity = new Vector2(-1f, rb.velocity.y);
        }

        //Face correct direction
        if (isRightWall == true && faceRight == false)
        {
            Flip();
        }
        else if (isRightWall == false && faceRight == true)
        {
            Flip();
        }
    }
    
    #endregion

    #region Dash

    public void Dash(InputAction.CallbackContext context)
    {
        if (canDash == true && dashOpen == true)
        {
            if (dashCoroutine != null)
            {
                StopCoroutine(dashCoroutine);
            }

            dashCoroutine = Dash(dashingTime, dashingCooldown);
            StartCoroutine(dashCoroutine);
        }
    }

    private void DashForce()
    {
        if (isDashing == true)
        {
            rb.velocity = new Vector2(direction * dashingPower, 0);

            if (Mathf.Abs(transform.position.x - lastImageXPos) > distanceBetweenImages)
            {
                AfterImagePool.instance.GetFromPool();
                lastImageXPos = transform.position.x;
            }
        }
    }

    private IEnumerator Dash(float dashTime, float dashCooldown)
    {
        canDash = false;
        isDashing = true;

        //tr.emitting = true;

        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;

        dashParticle.Play();

        AfterImagePool.instance.GetFromPool();
        lastImageXPos = transform.position.x;

        yield return new WaitForSeconds(dashTime);

        //tr.emitting = false;

        isDashing = false;

        rb.velocity = Vector2.zero;
        rb.gravityScale = mainGravity;

        dashParticle.Stop();

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    #endregion

    #region Attack

    #endregion

    #region Element Ability

    public void ChangeElement(InputAction.CallbackContext context)
    {
        
    }

    #endregion

    #region SoundMethods

    private void CharacterRunSound()
    {
        audioSrc.PlayOneShot(walkSound);
    }

    #endregion
    
    #region Animation

    private void AnimationControl()
    {
        //Walk Animations
        animator.SetFloat("Speed", Mathf.Abs(horizontal));

        //Jump Animations
        animator.SetBool("isFall", fallAnimControl);
        animator.SetBool("isJumping", jumpAnimControl);

        if(wallGrab == false && wallRun == false && wallSlide == false)
        {
            animator.SetBool("isDJumping", dJumpAnimControl);
        }

        //Wall Animations
        animator.SetBool("isWallSliding", wallSlide);
        animator.SetBool("isWallGrabing", wallGrab);
        animator.SetBool("isWallRunning", wallRun);

        //Dash Animation
        animator.SetBool("isDashing", isDashing);
    }

    #endregion

    #region Other

    private void CameraLearp()
    {
        if (rb.velocity.y < fallSpeedYDampingChangeTrashold && CameraManager.instance.isLearpingYDamping == false && CameraManager.instance.learpedFromPlayerFalling == false)
        {
            CameraManager.instance.LearpYDamping(true);
        }

        if (rb.velocity.y >= 0f && CameraManager.instance.isLearpingYDamping == false && CameraManager.instance.learpedFromPlayerFalling == true)
        {
            CameraManager.instance.learpedFromPlayerFalling = false;
            CameraManager.instance.LearpYDamping(false);
        }
    }

    private void TimeManager()
    {
        jumpPressedRemember -= Time.deltaTime;
        groundedRemember -= Time.deltaTime;  
    }

    private void CheckSurroundings()
    {
        //Ground Raycast
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);

        //Corner Raycast
        canCornerCorrect = Physics2D.Raycast(transform.position+ edgeRaycastOffset, Vector2.up, topRaycastLength, cornerLayer) &&
                           !Physics2D.Raycast(transform.position + innerRaycasrtOffset, Vector2.up, topRaycastLength, cornerLayer) ||
                           Physics2D.Raycast(transform.position - edgeRaycastOffset, Vector2.up, topRaycastLength, cornerLayer) &&
                           !Physics2D.Raycast(transform.position - innerRaycasrtOffset, Vector2.up, topRaycastLength, cornerLayer);

        //Wall Collision
        isWall = Physics2D.Raycast(wallRaycastPos.position, Vector2.right, wallRaycastLenght, whatIsWall) ||
                Physics2D.Raycast(wallRaycastPos.position, Vector2.left, wallRaycastLenght, whatIsWall);

        isRightWall = Physics2D.Raycast(wallRaycastPos.position, Vector2.right, wallRaycastLenght, whatIsWall);
    }

    private void CornerCorrect(float yVelocity)
    {
        //Push Right
        RaycastHit2D hit = Physics2D.Raycast(transform.position - innerRaycasrtOffset + Vector3.up * topRaycastLength, Vector3.left, topRaycastLength, cornerLayer);
        if(hit.collider != null)
        {
            float newPos = Vector3.Distance(new Vector3(hit.point.x, transform.position.y, 0f) + Vector3.up * topRaycastLength, 
                transform.position - edgeRaycastOffset + Vector3.up * topRaycastLength);
            transform.position = new Vector3(transform.position.x + newPos, transform.position.y, transform.position.z);
            rb.velocity = new Vector2(rb.velocity.x, yVelocity);
            return;
        }

        //Push Left
        hit = Physics2D.Raycast(transform.position + innerRaycasrtOffset + Vector3.up * topRaycastLength, Vector3.right, topRaycastLength, cornerLayer);
        if(hit.collider != null)
        {
            float newPos = Vector3.Distance(new Vector3(hit.point.x, transform.position.y, 0f) + Vector3.up * topRaycastLength, 
                transform.position + edgeRaycastOffset + Vector3.up * topRaycastLength);
            transform.position = new Vector3(transform.position.x - newPos, transform.position.y, transform.position.z);
            rb.velocity = new Vector2(rb.velocity.x, yVelocity);
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