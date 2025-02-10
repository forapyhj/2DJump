using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class AdvancedPlayerController : MonoBehaviour
{
	private static readonly int Speed = Animator.StringToHash("Speed");
	private static readonly int VerticalVelocity = Animator.StringToHash("VerticalVelocity");
	private static readonly int IsClimbing = Animator.StringToHash("IsClimbing");

	#region Parameters
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float airControl = 0.8f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float jumpBufferTime = 0.2f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private int maxAirJumps = 1;

    [Header("Climbing")]
    [SerializeField] private float climbSpeed = 4f;
	[SerializeField] private float climbCheckSize = 0.5f;
    [SerializeField] private LayerMask climbableLayer;
	[SerializeField] private LayerMask ladderLayer;

    [Header("Detection")]
    [SerializeField] private Transform groundCheck;
	[SerializeField] private float groundCheckSize = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    #endregion

    #region State
    private Rigidbody2D _rb;
    private Animator _anim;
    private PlayerControls _input;
    
    private Vector2 _moveInput;
    private bool _isJumpPressed;
    private bool _isClimbing;
    private bool _isReadyClimb;
	
    private int _airJumpCount;
    private float _jumpBufferCounter;
    private float _coyoteTimeCounter;
	
	private Collider2D _ladderTopPlatform;
    #endregion

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _input = new PlayerControls();

        _input.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _input.Player.Move.canceled += _ => OnStopMove();
        _input.Player.Jump.started += _ => _isJumpPressed = true;
        _input.Player.Jump.canceled += _ => _isJumpPressed = false;
    }
	private void OnStopMove()
	{
		_moveInput = Vector2.zero;
	}
    private void Update()
    {
        HandleJumpBuffer();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        CheckTriggered();
        HandleMovement();
        HandleJump();
        HandleClimbing();
    }
	private float _faceTo = 1;
    private void HandleMovement()
    {
		// 方向翻转
        if (_moveInput.x != 0)
		{
			_faceTo = Mathf.Sign(_moveInput.x);
			_anim.Play("Move");
		}
		else
		{
			_anim.Play("Idle");
		}
		if(!Mathf.Approximately(_faceTo, transform.localScale.x))
		{
			transform.localScale = new Vector3(_faceTo, 1, 1);
		}
        if (_isClimbing) return;

        float targetSpeed = _moveInput.x * moveSpeed;
        float accelerationRate = _isGrounded ? acceleration : acceleration * airControl;
        _rb.linearVelocity = new Vector2(
            Mathf.Lerp(_rb.linearVelocity.x, targetSpeed, accelerationRate), _rb.linearVelocity.y);

    }
    private void HandleJump()
    {
		if(!_canJump) return;
        if (_jumpBufferCounter > 0 && (_isGrounded || _coyoteTimeCounter > 0 || _airJumpCount < maxAirJumps))
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
            _jumpBufferCounter = 0;
            if (!_isGrounded && _coyoteTimeCounter <= 0)
                _airJumpCount++;
        }
		else
		{
			StartCoroutine(JumpCooldown());
		}
        // 跳跃手感优化
        if (_rb.linearVelocity.y < 0)
            _rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime);
        else if (_rb.linearVelocity.y > 0 && !_isJumpPressed)
            _rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * (0.5f - 1) * Time.fixedDeltaTime);
    }
	private void HandleJumpBuffer()
    {
        _jumpBufferCounter = _isJumpPressed ? jumpBufferTime : _jumpBufferCounter - Time.deltaTime;
        _coyoteTimeCounter = _isGrounded ? coyoteTime : _coyoteTimeCounter - Time.deltaTime;
        
        if (_isGrounded && _rb.linearVelocity.y <= 0)
		{
            _airJumpCount = 0;
		}
    }
	// 新增跳跃冷却协程
	private bool _canJump = true;
	private IEnumerator JumpCooldown()
	{
		_canJump = false;
		_isJumpPressed = false;
		yield return new WaitForSeconds(0.2f);
		_canJump = true;
	}
	private void HandleClimbing()
    {	
		// 获取圆形范围内的站立平台
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, climbCheckSize, climbableLayer);
		//如果与爬杆相接触或者在平台上向下爬
		if(_isReadyClimb && ((!_isTop && _moveInput.y > 0.1f) || _moveInput.y < -0.1f || !_isGrounded))
		{
			//将角色位于的站立平台设为触发器
			_ladderTopPlatform = hitColliders[0].transform.GetChild(0).GetComponent<Collider2D>();
			EnterClimbing();
		}
		//角色处于站立平台或地板时
		else if(_isClimbing && (_isTop || _isGrounded))
		{ 
			ExitClimbing();
			return;
		}
		if(_isClimbing)
		{
			_rb.gravityScale = 0;
			_rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _moveInput.y * climbSpeed);
		}
    }	
	private void EnterClimbing()
	{
		if (!_canClimb) return;
		_isClimbing = true;
		_rb.linearVelocity = Vector2.zero;
		SnapToClimbable();
		if(_ladderTopPlatform) _ladderTopPlatform.isTrigger = true;
	}
	private void ExitClimbing()
	{
		_isClimbing = false;
		if(_ladderTopPlatform) _ladderTopPlatform.isTrigger = false;//将角色位于的站立平台设为碰撞器
		_ladderTopPlatform = null;
		_rb.gravityScale = 1;
		// 添加0.2秒冷却防止误触发
		StartCoroutine(ClimbCooldown());
	}
	// 新增攀爬冷却协程
	private bool _canClimb = true;
	private IEnumerator ClimbCooldown()
	{
		_canClimb = false;
		yield return new WaitForSeconds(0.2f);
		_canClimb = true;
	}
	private void SnapToClimbable()
    {
        Vector2 snapPosition = new Vector2(
            _ladderTopPlatform.bounds.center.x,
            transform.position.y + 0.5f
        );
        
        if (Vector2.Distance(transform.position, snapPosition) < 0.5f)
            transform.position = snapPosition;
    }

    private void UpdateAnimations()
    {
        _anim.SetFloat(Speed, Mathf.Abs(_rb.linearVelocity.x));
        _anim.SetFloat(VerticalVelocity, _rb.linearVelocity.y);
        _anim.SetBool(IsClimbing, _isClimbing);
    }
	
	private void CheckTriggered()
    {
		//检测是否与爬杆接触
		_isReadyClimb = Physics2D.OverlapCircleAll(transform.position, climbCheckSize, climbableLayer).Length > 0;
		//检测是否在站立平台上
		_isTop = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckSize, ladderLayer).Length > 0;
		_isGrounded = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckSize, groundLayer).Length > 0;
	}
    private bool _isGrounded;
	private bool _isTop;
    private void OnEnable() => _input.Enable();
    private void OnDisable() => _input.Disable();

	public void PlayAnimator(string anim)
	{
		_anim.Play(anim);
	}


    #region Debug
    private void OnDrawGizmos()
    {
		Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, climbCheckSize);
    }
    #endregion
}