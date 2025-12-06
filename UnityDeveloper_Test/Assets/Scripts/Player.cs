using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private LayerMask ground;

    private Rigidbody rb;
    private Animator animator;
    private int runningID = Animator.StringToHash("Running");
    private int fallingIdleID = Animator.StringToHash("FallingIdle");

    private float currentVelocity = 0;
    private Vector3 prevInput;
    private Vector3 selectedGravity = Vector3.down;
    private Vector3 curGravity = Vector3.down;
    private Quaternion targetRotation;
    private Quaternion targetHollowgramRotation;
    private bool wasGrounded;

    [Header("Settings")]
    [SerializeField] private Transform feetPoint;
    [SerializeField] private float acce = 5f;
    [SerializeField] private float dece = 3f;
    [SerializeField] private float speedPow = 0.9f;
    [SerializeField] private float maxVelocity = 10f;
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private float gravityForce = 30f;
    [SerializeField] private float rotateSpeed = 5f;
    [SerializeField] private Transform hollowgramTransform;
    [SerializeField] private float hollowgramDistance = 2f;

    private bool isGrounded;
    public static Action Onfall;
    public static Action OnLand;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        if (hollowgramTransform == null)
        {
            Debug.LogWarning("Hollowgram transform is not assigned");
        }
        else
        {
            hollowgramTransform.gameObject.SetActive(false);
        }
        wasGrounded = true;
    }

    void Update()
    {
        float x = 0f;
        float z = 0f;

        if (Input.GetKey(KeyCode.A)) x = -1;
        if (Input.GetKey(KeyCode.D)) x = 1;
        if (Input.GetKey(KeyCode.W)) z = 1;
        if (Input.GetKey(KeyCode.S)) z = -1;

        Vector3 input = new Vector3(x, 0, z).normalized;

        isGrounded = IsGrounded();

        // Fire landing/falling events
        if (isGrounded && !wasGrounded)
        {
            OnLand?.Invoke();
        }
        else if (!isGrounded && wasGrounded)
        {
            Onfall?.Invoke();
        }
        wasGrounded = isGrounded;

        // Prevent movement input while airborne
        if (!isGrounded)
        {
            input = Vector3.zero;
        }

        // Direction change → braking
        if (Vector3.Dot(prevInput, input) < 0)
            currentVelocity *= 0.3f;

        prevInput = input;

        // Accel / Decel
        if (input.magnitude > 0.1f)
        {
            currentVelocity += acce * Time.deltaTime;
            currentVelocity = Mathf.Clamp(currentVelocity, 0, maxVelocity);
            animator.SetBool(runningID, true);
        }
        else
        {
            animator.SetBool(runningID, false);
            currentVelocity -= dece * Time.deltaTime;
            currentVelocity = Mathf.Max(0, currentVelocity);
        }

        // Gravity selection logic
        HandleGravityInput();

        // Apply gravity change
        if (Input.GetKeyDown(KeyCode.Return))
        {
            curGravity = selectedGravity;
            SetTargetRotation(curGravity);
            if (hollowgramTransform != null)
            {
                hollowgramTransform.gameObject.SetActive(false);
            }
        }

        Vector3 desiredVel = input * currentVelocity;

        // Curved input for smoother feel
        float vx = Mathf.Pow(Mathf.Abs(desiredVel.x), speedPow * 0.5f) * Mathf.Sign(desiredVel.x);
        float vz = Mathf.Pow(Mathf.Abs(desiredVel.z), speedPow * 0.5f) * Mathf.Sign(desiredVel.z);

        // Align move plane to custom gravity
        Vector3 gravityUp = -curGravity.normalized;
        Quaternion align = Quaternion.FromToRotation(Vector3.up, gravityUp);

        Vector3 moveDir = align * new Vector3(input.x, 0, input.z);

        // Rotate player to face movement direction
        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion moveRot = Quaternion.LookRotation(moveDir, gravityUp);
            transform.rotation = Quaternion.Slerp(transform.rotation, moveRot, rotateSpeed * Time.deltaTime);
        }

        // Calculate movement in world space
        Vector3 worldMove = align * new Vector3(vx, 0, vz);
        Vector3 gravityDir = curGravity.normalized;

        // Preserve vertical velocity component
        Vector3 verticalVel = Vector3.Project(rb.velocity, gravityDir);
        Vector3 finalVel = worldMove + verticalVel;

        rb.velocity = finalVel;

        // Jump - now properly aligned with gravity
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            float jumpVel = Mathf.Sqrt(2f * jumpHeight * gravityForce);
            // Jump opposite to gravity direction
            rb.velocity = worldMove + (-gravityDir * jumpVel);
        }

        animator.SetBool(fallingIdleID, !isGrounded);
    }

    private void HandleGravityInput()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                selectedGravity = Vector3.down;
                SetHollowgramTargetRotation(selectedGravity);
                ShowHollowgram();
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                selectedGravity = Vector3.up;
                SetHollowgramTargetRotation(selectedGravity);
                ShowHollowgram();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                selectedGravity = Vector3.forward;
                SetHollowgramTargetRotation(selectedGravity);
                ShowHollowgram();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                selectedGravity = Vector3.back;
                SetHollowgramTargetRotation(selectedGravity);
                ShowHollowgram();
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectedGravity = Vector3.left;
            SetHollowgramTargetRotation(selectedGravity);
            ShowHollowgram();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedGravity = Vector3.right;
            SetHollowgramTargetRotation(selectedGravity);
            ShowHollowgram();
        }
    }

    private void ShowHollowgram()
    {
        if (hollowgramTransform != null)
        {
            hollowgramTransform.gameObject.SetActive(true);
        }
    }

    private void FixedUpdate()
    {
        rb.AddForce(curGravity * gravityForce, ForceMode.Acceleration);
    }

    private void LateUpdate()
    {
        if (hollowgramTransform != null)
        {
            // Position hollowgram in the direction of selected gravity
            Vector3 gravityUp = -selectedGravity.normalized;
            hollowgramTransform.position = transform.position + gravityUp * hollowgramDistance;
        }

        // Smooth rotation towards target
        if (Quaternion.Angle(transform.rotation, targetRotation) < 0.5f)
            transform.rotation = targetRotation;
        else
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

        if (hollowgramTransform != null)
        {
            if (Quaternion.Angle(hollowgramTransform.rotation, targetHollowgramRotation) < 0.5f)
                hollowgramTransform.rotation = targetHollowgramRotation;
            else
                hollowgramTransform.rotation = Quaternion.Slerp(hollowgramTransform.rotation, targetHollowgramRotation, rotateSpeed * Time.deltaTime);
        }
    }

    private void SetTargetRotation(Vector3 gravity)
    {
        targetRotation = Quaternion.FromToRotation(transform.up, -gravity.normalized) * transform.rotation;
    }

    private void SetHollowgramTargetRotation(Vector3 gravity)
    {
        targetHollowgramRotation = Quaternion.FromToRotation(transform.up, -gravity.normalized) * transform.rotation;
    }

    private bool IsGrounded()
    {
        if (feetPoint == null) return false;

        Vector3 gravityDir = curGravity.normalized;
        Vector3 gravityUp = -gravityDir;

        // Box dimensions (small and flat)
        Vector3 boxHalfExtents = new Vector3(0.25f, 0.05f, 0.25f);
        float checkDist = 0.1f;

        // Position box at feet point
        Vector3 boxCenter = feetPoint.position;

        // Align box so its local Y-axis points opposite to gravity
        Quaternion boxOrientation = Quaternion.LookRotation(
            Vector3.Cross(gravityUp, Vector3.right).magnitude > 0.1f
                ? Vector3.Cross(gravityUp, Vector3.right)
                : Vector3.Cross(gravityUp, Vector3.forward),
            gravityUp
        );

        // BoxCast toward gravity
        return Physics.BoxCast(
            boxCenter,
            boxHalfExtents,
            gravityDir,
            out RaycastHit hit,
            boxOrientation,
            checkDist,
            ground
        );
    }

    private void OnDrawGizmos()
    {
        if (feetPoint == null) return;

        Vector3 gravityDir = curGravity.normalized;
        Vector3 gravityUp = -gravityDir;
        Vector3 boxHalfExtents = new Vector3(0.25f, 0.05f, 0.25f);
        float checkDist = 0.1f;
        Vector3 boxCenter = feetPoint.position;

        Quaternion boxOrientation = Quaternion.LookRotation(
            Vector3.Cross(gravityUp, Vector3.right).magnitude > 0.1f
                ? Vector3.Cross(gravityUp, Vector3.right)
                : Vector3.Cross(gravityUp, Vector3.forward),
            gravityUp
        );

        // Draw gravity direction
        Gizmos.color = Color.red;
        Gizmos.DrawLine(feetPoint.position, feetPoint.position + gravityDir * 0.5f);

        // Draw ground check boxes
        Gizmos.color = isGrounded ? Color.green : Color.yellow;
        Gizmos.matrix = Matrix4x4.TRS(boxCenter, boxOrientation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2);
        Gizmos.matrix = Matrix4x4.identity;

        Gizmos.matrix = Matrix4x4.TRS(boxCenter + gravityDir * checkDist, boxOrientation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2);
        Gizmos.matrix = Matrix4x4.identity;

        // Draw feet point
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(feetPoint.position, 0.05f);
    }
}