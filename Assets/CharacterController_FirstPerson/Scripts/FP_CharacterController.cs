using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FP_CharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;
    public float airDamping = 4f; // wie sehr gegengesetzte WASD inputs den *Schwung* in der Luft beeinflussen

    [Header("Dash Settings")]
    public float dashDistance = 8f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;

    [Header("Grappling Hook Settings")]
    public float maxHookDistance = 30f;
    public float hookPullSpeed = 25f;
    public float hookStopDistance = 2f;
    public LayerMask hookableLayer;          // nicht alles ist hookable
    public LineRenderer hookLineRenderer;

    [Header("Climbing Settings")]
    public float climbSpeed = 3.5f;
    public float wallJumpForce = 8f;           // abstoßkraft
    public float wallJumpUpForce = 6f;         // vertikale kraft vom walljump
    public LayerMask climbableLayer;           // auch nicht alles ist climbable

    [Header("Weapon & Physics Settings")]
    public GameObject ballPrefab;
    public GameObject bombPrefab;
    public Transform hand;
    public float throwForce = 20f;
    
    [Header("Look Settings")]
    public Transform cameraTransform;
    public float mouseSensitivity = 10f;
    public float upLimit = -80f;
    public float downLimit = 80f;

    private CharacterController _controller;
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private Vector3 _velocity;
    private float _verticalRotation = 0f;
    private bool _isSprinting;
    private bool _isGrounded;

    private bool _canDoubleJump;
    private bool _isDashing;
    private float _dashTimer;
    private float _cooldownTimer;
    private Vector3 _dashDirection;
    private bool _isGrappling;
    private Vector3 _grapplePoint;
    private bool _isClimbing;
    private Vector3 _wallNormal;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        if (hookLineRenderer != null)
        {
            hookLineRenderer.enabled = false;
        }
    }

    void Update()
    {
        HandleRotation();

        if (_cooldownTimer > 0f)
        {
            _cooldownTimer -= Time.deltaTime;
        }

        if (_isDashing)
        {
            float dashSpeed = dashDistance / dashDuration;
            _controller.Move(_dashDirection * dashSpeed * Time.deltaTime);

            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0f)
            {
                _isDashing = false;
            }
        }

        else if (_isGrappling)
        {
            if(hookLineRenderer != null){
                DrawHookLine();
            }
            HandleGrapple();
        }
        else if (_isClimbing)
        {
            HandleClimbing();
        }

        else
        {
            HandleMovement();
        }
    }

    public void OnMove(InputAction.CallbackContext context) => _moveInput = context.ReadValue<Vector2>();
    public void OnLook(InputAction.CallbackContext context) => _lookInput = context.ReadValue<Vector2>();

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_isClimbing)
            {
                StopClimbing();
            
                Quaternion lookRotation = transform.rotation * Quaternion.Euler(_verticalRotation, 0f, 0f);
                Vector3 jumpDirection = (lookRotation * Vector3.forward).normalized;
                // Impuls in blickrichtung + bisschen nach oben
                _velocity = (jumpDirection * wallJumpForce) + (Vector3.up * wallJumpUpForce);
                _canDoubleJump = false; //kein DJ nach walljump
            return;
            }
            // abbrechen des grapples bei jump-input und mitnehmen des schwungs
            if (_isGrappling)
            {
                StopGrapple();
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                _canDoubleJump = true;
                return;
            }

            if (_isGrounded)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                _canDoubleJump = true;
            }
            else if (_canDoubleJump)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                _canDoubleJump = false;
            }
        }
    }

    public void OnClimb(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_isClimbing)
            {
                // direkt loslassen
                StopClimbing();
            }
            else
            {
                TryAttachToWall();
            }
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed) _isSprinting = true;
        else if (context.canceled) _isSprinting = false;
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed && _cooldownTimer <= 0f && !_isDashing)
        {
            if (_isGrappling) StopGrapple();

            _isDashing = true;
            _dashTimer = dashDuration;
            _cooldownTimer = dashCooldown;
            _velocity.y = 0f;

            Vector3 inputDir = transform.right * _moveInput.x + transform.forward * _moveInput.y;
            _dashDirection = (inputDir.sqrMagnitude > 0) ? inputDir.normalized : transform.forward;
        }
    }

    public void OnGrapple(InputAction.CallbackContext context)
    {
        
        if (context.started)
        {
            StartGrapple();
        }
        else if (context.canceled && _isGrappling)
        {
            StopGrapple();
        }
    }

    private void TryAttachToWall()
    {
        // mit ray nach vorne prüfen, ob eine Kletterwand direkt vor dem Spieler ist
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 1.2f, climbableLayer))
        {
            StartClimbing(hit.normal);
        }
    }

    private void StartClimbing(Vector3 wallNormal)
    {
        _isClimbing = true;
        _wallNormal = wallNormal;
        _velocity = Vector3.zero; //stoppt gravity und movement
        _canDoubleJump = false;
    }
    private void HandleClimbing()
    {
        // checken, ob der charakter noch an der wand ist/schon drüber hinweg
        Vector3 checkOrigin = transform.position + Vector3.up * (cameraTransform.localPosition.y * 0.5f);
        bool isWallStillAhead = Physics.Raycast(checkOrigin, -_wallNormal, 1.2f, climbableLayer);

        if (!isWallStillAhead)
        {
            StopClimbing();
            return;
        }

        // .Cross, dass der Spieler konstant Abstand 0 zur wand hat 
        Vector3 wallRight = Vector3.Cross(Vector3.down, _wallNormal).normalized;
        Vector3 wallUp = Vector3.up;

        Vector3 climbMovement = (wallRight * _moveInput.x + wallUp * _moveInput.y) * climbSpeed;
        _controller.Move(climbMovement * Time.deltaTime);
    }

    private void StopClimbing()
    {
        _isClimbing = false;
    }
    private void StartGrapple()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * cameraTransform.localPosition.y;
        Quaternion lookRotation = transform.rotation * Quaternion.Euler(_verticalRotation, 0f, 0f);
        Vector3 rayDirection = lookRotation * Vector3.forward;

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, maxHookDistance, hookableLayer))
        {
            _isGrappling = true;
            _grapplePoint = hit.point;
            _canDoubleJump = true;

            if (hookLineRenderer != null) hookLineRenderer.enabled = true;
        }
        else
        {
            Debug.LogWarning($"Raycast von {rayOrigin} in Richtung {rayDirection} failed.");
        }
    }

    private void HandleGrapple()
    {
        Vector3 pullDirection = (_grapplePoint - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, _grapplePoint);

        // hook punkt erreicht
        if (distanceToTarget <= hookStopDistance)
        {
            StopGrapple();
            return;
        }

        // um schwingen zu ermöglichen auch a d inkludieren
        Vector3 airControl = transform.right * _moveInput.x * (walkSpeed * 0.5f);

        _velocity = (pullDirection * hookPullSpeed) + airControl;

        _controller.Move(_velocity * Time.deltaTime);
    }

    private void StopGrapple()
    {
        _isGrappling = false;
        
        if (hookLineRenderer != null)
        {
            hookLineRenderer.enabled = false;
        }

        // zum behalten des schwungs nach hook loslassen
        _velocity = transform.forward * (walkSpeed * 1.2f);
    }

    private void DrawHookLine()
    {
        // hookanfang bis hookende also _grapplePoint
        hookLineRenderer.SetPosition(0, cameraTransform.position + cameraTransform.right * 0.3f - cameraTransform.up * 0.2f);
        hookLineRenderer.SetPosition(1, _grapplePoint);
    }

    private void HandleMovement()
    {
        _isGrounded = _controller.isGrounded;

        if (_isGrounded)
        {
            if (_velocity.y < 0)
            {
                _velocity.y = -2f; 
            }
            _canDoubleJump = true;
            // eventuellen Schwung von Grapple oder Dash resetten
            _velocity.x = 0f;
            _velocity.z = 0f;
        }

        float currentSpeed = _isSprinting ? sprintSpeed : walkSpeed;
        Vector3 moveInputVector = transform.right * _moveInput.x + transform.forward * _moveInput.y;
        _velocity.y += gravity * Time.deltaTime;

        
        Vector2 horizontalVel = new Vector2(_velocity.x, _velocity.z);

        float currentMomentumSpeed = horizontalVel.magnitude;

        if (!_isGrounded && currentMomentumSpeed > 0.01f)
        {
            // bei S-input Schwung reduzieren based on airdamping
            if (_moveInput.y < 0f)
            {
                float brakeFactor = Mathf.Abs(_moveInput.y) * airDamping;
                currentMomentumSpeed = Mathf.MoveTowards(currentMomentumSpeed, 0f, brakeFactor * Time.deltaTime);
            }

            // Schwung vom grapple oder Dash immer in blickrichtung
            Vector3 forwardXZ = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 momentumVector = forwardXZ * currentMomentumSpeed;

            _velocity.x = momentumVector.x;
            _velocity.z = momentumVector.z;
        }

        // vorherige Pos speichern, um kollisionen mit objekten zu erkennen, s.U.
        Vector3 prePos = transform.position;

        // mit möglichem Schwung von Grapple oder Dash kombinieren
        Vector3 totalMovement = (moveInputVector * currentSpeed) + _velocity;
        _controller.Move(totalMovement * Time.deltaTime);

        // schwung auf 0 setzen, wenn der spieler mit irgendwas kollidiert
        if (!_isGrounded)
        {
            Vector3 actualHorizontalDelta = transform.position - prePos;
            actualHorizontalDelta.y = 0f;
            float actualHorizontalSpeed = actualHorizontalDelta.magnitude / Time.deltaTime;

            if (actualHorizontalSpeed < horizontalVel.magnitude * 0.5f)
            {
                _velocity.x = 0f;
                _velocity.z = 0f;
            }
        }
    }
    private void HandleRotation()
    {
        transform.Rotate(Vector3.up * _lookInput.x * mouseSensitivity * Time.deltaTime);

        _verticalRotation -= _lookInput.y * mouseSensitivity * Time.deltaTime;
        _verticalRotation = Mathf.Clamp(_verticalRotation, upLimit, downLimit);
        cameraTransform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
    }

    //ermöglicht objekten wie jumppads, den spieler zu katapultieren
    public void ApplyJumpImpulse(float impulseForce)
    {
        if (_isGrappling)
        {
            StopGrapple();
        }
        if(_isClimbing)
        {
            StopClimbing();
        }
        _velocity.y = impulseForce;
        
        _canDoubleJump = true;
    }

    // callbacks für shooting und bomb
    public void OnShoot(InputAction.CallbackContext context)
    {
        if (!context.performed || ballPrefab == null) return;

        GameObject ball = Instantiate(ballPrefab, hand.position, Quaternion.identity);
        if (ball.TryGetComponent<Ball_Behaviour>(out Ball_Behaviour ballScript))
        {
            ballScript.isPlayerBullet = true;
        }
        
        if (ball.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.AddForce(cameraTransform.forward * throwForce, ForceMode.Impulse);
        }
    }

    public void OnBomb(InputAction.CallbackContext context)
    {
        if (!context.performed || bombPrefab == null) return;

        GameObject bomb = Instantiate(bombPrefab, hand.position, Quaternion.identity);
        if (bomb.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.AddForce(cameraTransform.forward * throwForce, ForceMode.Impulse);
        }
    }
}