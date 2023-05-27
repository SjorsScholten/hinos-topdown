using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, DefaultActions.IPlayerActions, IDamageHandler
{
    // Movement
    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    public float moveAcceleration = 12f;
    public float turnAcceleration = 360f;

    private Vector2 _heading;
    private Vector2 _crossHeading;
    private float _targetSpeed;
    private float _acceleration;
    private Vector2 _forwardVelChange;
    private Vector2 _latVelChange;

    // Shooting
    [Space(), Header("Weapon Settings")]
    public int weaponDamage = 10;
    public float timeBetweenShots = 0.2f;
    public Projectile bulletPrefab;
    public Transform bulletOrigin;

    private float _timeSinceLastShot = 0;
    private bool _isFiring = false;

    // Health
    [Space(), Header("Health Settings")]
    public int cells;
    public int charges;
    public int energy;

    // Input
    private DefaultActions _actions;
    private Vector2 _moveInputValue;
    private Vector2 _targetInputValue;

    // Components
    private Transform _transform;
    private Rigidbody2D _rigidbody2D;

    private void Awake() {
        _transform = GetComponent<Transform>();
        _rigidbody2D = GetComponent<Rigidbody2D>();

        _actions = new DefaultActions();
        _actions.Player.AddCallbacks(this);
    }

    private void OnEnable() {
        _actions.Player.Enable();
    }

    private void OnDisable() {
        _actions.Player.Disable();
    }

    private void Update() {

        _timeSinceLastShot += Time.deltaTime;
        if(_isFiring && CanShoot()) {
            Fire();
        }
    }

    private void FixedUpdate() {
        { // Apply Movement
            var moveAmount = _moveInputValue.magnitude;

            // we are influencing movement
            if (moveAmount > float.Epsilon) {        
                _heading = _moveInputValue.normalized;
                _acceleration = moveAcceleration * moveAmount;
                _targetSpeed = moveSpeed;
            }

            // we are not influencing movement
            else {
                _heading = _rigidbody2D.velocity.normalized;
                _acceleration = moveAcceleration;
                _targetSpeed = 0;
            }

            // calculate forward velocity
            var alignedSpeed = Vector2.Dot(_rigidbody2D.velocity, _heading);
            var finalSpeed = Mathf.MoveTowards(alignedSpeed, _targetSpeed, _acceleration * Time.deltaTime);
            _forwardVelChange = _heading * (finalSpeed - alignedSpeed);

            // calculate lateral velocity correction
            _crossHeading = new Vector2(_heading.y, -_heading.x);
            var unalignedSpeed = Vector2.Dot(_rigidbody2D.velocity, _crossHeading);
            var speedCorrection = Mathf.MoveTowards(unalignedSpeed, 0, _acceleration * Time.deltaTime);
            _latVelChange = _crossHeading * (speedCorrection - unalignedSpeed);

            _rigidbody2D.velocity += _forwardVelChange + _latVelChange;
        }

        { // Apply Targeting
            var lookDirection = Vector2.zero;
            var targetAmount = _targetInputValue.magnitude;
            if (targetAmount > float.Epsilon) {
                lookDirection = _targetInputValue.normalized;
            }
            else {
                var moveAmount = _moveInputValue.magnitude;
                if(moveAmount > float.Epsilon) {
                    lookDirection = _moveInputValue.normalized;
                }
            }

            if(lookDirection.sqrMagnitude > float.Epsilon) {
                var angle = _rigidbody2D.rotation;
                var targetAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
                _rigidbody2D.rotation = Mathf.MoveTowardsAngle(angle, targetAngle - 90f, turnAcceleration * Time.deltaTime);
            }
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawRay((Vector2)transform.position + _heading * 0.5f, _heading * 0.3f);

        Gizmos.color = Color.red;
        Gizmos.DrawRay((Vector2)transform.position + _crossHeading * 0.5f, _crossHeading * 0.3f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, _forwardVelChange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(transform.position, _latVelChange);
    }

    private bool CanShoot() {
        if (_timeSinceLastShot < timeBetweenShots) {
            return false;
        }

        return true;
    }

    private void Fire() {
        _timeSinceLastShot = 0;
        var projectile = Instantiate<Projectile>(bulletPrefab, bulletOrigin.position, bulletOrigin.rotation);
        projectile.tag = tag;
        projectile.OnHitEvent += OnProjectileHit;
    }

    private void OnProjectileHit(GameObject targetObject) {
        if(targetObject.TryGetComponent<IDamageHandler>(out var damageHandler)) {
            damageHandler.HandleDamage(weaponDamage);
        }
    }

    public void HandleDamage(int damage) {

    }

    public void OnMove(InputAction.CallbackContext context) {
        _moveInputValue = context.ReadValue<Vector2>();
    }

    public void OnTarget(InputAction.CallbackContext context) {
        _targetInputValue = context.ReadValue<Vector2>();
    }

    public void OnFire(InputAction.CallbackContext context) {
        if(context.started) {
            _isFiring = true;
        }
        else if(context.canceled) {
            _isFiring = false;
        }
    }
}
