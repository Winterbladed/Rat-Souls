using UnityEngine;
using UnityEngine.Events;

public class Movement : MonoBehaviour
{
    #region Variables
    [Header("Move")]
    [SerializeField] private float _speed = 2.0f;
    private float _currentSpeed = 2.0f;
    private float _slowSpeed = 1.0f;
    private float _runSpeed = 8.0f;
    [SerializeField] private float _rotationSpeed = 1000.0f;
    private Vector3 _direction;
    private bool _isWalking;
    private bool _isRunning;
    private bool _isSlowed;
    private bool _isStrafing;
    private bool _isShooting;
    private bool _isAttacking;
    private int _comboIndex = 0;

    [Header("Jump")]
    [SerializeField] private float _jumpHeight = 1.0f;
    [SerializeField] private float _gravity = -20.0f;
    private float _verticalVelocity;
    private bool _isGrounded;

    [Header("Dodge Roll")]
    [SerializeField] private float _dodgeRollSpeed = 5.0f;
    [SerializeField] private float _dodgeRollDuration = 1.0f;
    [SerializeField] private float _dodgeRollCooldown = 0.5f;
    [SerializeField] private UnityEvent _onDodgeEvt;
    private bool _isDodging;
    private float _dodgeRollTime;
    private float _dodgeCooldownTime;

    [Header("References")]
    [SerializeField] private CharacterController _controller;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Animator _animator;
    [SerializeField] private Health _health;
    #endregion

    #region Private Functions
    private void Start()
    {
        _currentSpeed = _speed;
        _slowSpeed = _speed / 2.0f;
        _runSpeed = _speed * 4.0f;
    }

    private void Update()
    {
        // Dodge Roll Logic
        if (_isDodging)
        {
            if (Time.time >= _dodgeRollTime) 
            { 
                _isDodging = false; 
                _health.Invulnerable(false); 
            }
            else
            {
                Vector3 _dodgeDirection = _direction.normalized;
                _dodgeDirection.y = 0;
                _controller.Move(_dodgeDirection * _dodgeRollSpeed * Time.deltaTime);
            }
            return;
        }

        // Movement Logic
        float _horizontal = Input.GetAxis("Horizontal");
        float _vertical = Input.GetAxis("Vertical");
        Vector3 cameraForward = _cameraTransform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();
        Vector3 cameraRight = _cameraTransform.right;
        cameraRight.y = 0;
        cameraRight.Normalize();
        _direction = cameraRight * _horizontal + cameraForward * _vertical;
        Vector3 _move = _direction * _currentSpeed * Time.deltaTime;
        _move.y = _verticalVelocity * Time.deltaTime;
        _controller.Move(_move);

        // Blocking Strafing logic 1
        if (Input.GetMouseButton(1) && !_isDodging)
        {
            Quaternion _targetRotation = Quaternion.LookRotation(cameraForward, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, _rotationSpeed * Time.deltaTime);
            _isStrafing = true;
        }
        else if (_direction != Vector3.zero && !_isAttacking && !_isShooting)
        {
            _direction.Normalize();
            Quaternion _targetRotation = Quaternion.LookRotation(_direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, _rotationSpeed * Time.deltaTime);
            _isStrafing = false;
        }

        // Speed control
        if (!_isSlowed) _currentSpeed = _isRunning ? _runSpeed : _speed;
        else _currentSpeed = _slowSpeed;

        // Walking and running state
        _isWalking = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
        _isRunning = Input.GetKey(KeyCode.LeftShift) && _isWalking && !Input.GetMouseButton(1) && !_isAttacking && !_isShooting;

        // Jumping logic
        _isGrounded = _controller.isGrounded;
        if (_isGrounded && !_isDodging)
        {
            if (Input.GetButton("Jump") && !_isAttacking) _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            else _verticalVelocity = -0.5f;
        }
        else _verticalVelocity += _gravity * Time.deltaTime;

        // Dodge Roll logic 2
        if (Input.GetKey(KeyCode.C) && Time.time >= _dodgeCooldownTime && _isGrounded && !_isAttacking && _isWalking && !_isShooting && !_isStrafing
            || Input.GetKey(KeyCode.LeftAlt) && Time.time >= _dodgeCooldownTime && _isGrounded && !_isAttacking && _isWalking && !_isShooting && !_isStrafing)
        {
            _isDodging = true;
            _dodgeRollTime = Time.time + _dodgeRollDuration;
            _dodgeCooldownTime = Time.time + _dodgeRollCooldown;
            _onDodgeEvt.Invoke();
            _health.Invulnerable(true);
        }

        // Animator updates
        _animator.SetBool("_isWalking", _isWalking);
        _animator.SetBool("_isRunning", _isRunning);
        _animator.SetBool("_isDodging", _isDodging);
        _animator.SetBool("_isStrafing", _isStrafing);
        _animator.SetBool("_isShooting", _isShooting);
        _animator.SetBool("_isAttacking", _isAttacking);
        _animator.SetInteger("_combo", _comboIndex);
    }
    #endregion

    #region Public Functions
    public bool GetIsGrounded() { return _isGrounded; }
    public bool GetIsWalking() { return _isWalking; }
    public bool GetIsRunning() { return _isRunning; }
    public bool GetIsSlowed() { return _isSlowed; }
    public bool GetIsDodging() { return _isDodging; }
    public bool GetIsStrafing() { return _isStrafing; }
    public bool GetIsAttacking() { return _isAttacking; }
    public bool GetIsShooting() { return _isShooting; }
    public void SetAttacking(bool _boolean) {  _isAttacking = _boolean; }
    public void SetShooting(bool _boolean) { _isShooting = _boolean; }
    public void SetSpeed(float _newSpeed) { _speed = _newSpeed; _currentSpeed = _newSpeed; _slowSpeed = _newSpeed / 2; _runSpeed = _newSpeed * 4.0f; }
    public void SetJumpHeight(float _newJumpHeight) { _jumpHeight = _newJumpHeight;}
    public void SetGravity(float _newGravity) { _gravity = _newGravity; }
    public void SetDodgeRollSpeed(float _newDodgeRollSpeed) { _dodgeRollSpeed = _newDodgeRollSpeed; }
    public void SetComboIndex(int _index) { _comboIndex = _index; }
    #endregion
}