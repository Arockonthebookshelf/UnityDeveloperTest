using UnityEngine;

public class CamThird : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 1.7f, -4f);

    [Header("Orbit Settings")]
    public float sensitivity = 150f;
    public float minY = -40f;
    public float maxY = 70f;

    [Header("Follow Smooth")]
    public float followSpeed = 10f;

    [Header("Collision Settings")]
    public LayerMask collisionLayers;
    public float collisionRadius = 0.3f;
    public float minDistanceFromWall = 0.2f;

    private float yaw;
    private float pitch;

    void LateUpdate()
    {
        if (!target) return;

        // Mouse input
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minY, maxY);

        // Calculate rotation based on gravity
        Vector3 gravityUp = target.up;
        Vector3 worldForward = Vector3.forward;
        Vector3 gravityForward = Vector3.ProjectOnPlane(worldForward, gravityUp);

        if (gravityForward.sqrMagnitude < 0.01f)
        {
            gravityForward = Vector3.ProjectOnPlane(Vector3.right, gravityUp);
        }
        gravityForward.Normalize();

        Quaternion yawRotation = Quaternion.AngleAxis(yaw, gravityUp);
        Vector3 rotatedForward = yawRotation * gravityForward;
        Vector3 rightVector = Vector3.Cross(gravityUp, rotatedForward).normalized;
        Quaternion pitchRotation = Quaternion.AngleAxis(pitch, rightVector);
        Quaternion rotation = pitchRotation * yawRotation * Quaternion.LookRotation(gravityForward, gravityUp);

        // Calculate desired position
        Vector3 desiredPos = target.position + rotation * offset;

        // Perform collision check
        Vector3 direction = desiredPos - target.position;
        float desiredDistance = direction.magnitude;

        float actualDistance = desiredDistance;

        if (desiredDistance > 0.01f)
        {
            Vector3 directionNormalized = direction.normalized;

            // SphereCast from target towards camera position
            if (Physics.SphereCast(
                target.position,
                collisionRadius,
                directionNormalized,
                out RaycastHit hit,
                desiredDistance,
                collisionLayers))
            {
                // Camera would collide, pull it closer
                actualDistance = Mathf.Max(hit.distance - minDistanceFromWall, 0.5f);
                desiredPos = target.position + directionNormalized * actualDistance;
            }
        }

        // Smooth position
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);
        transform.rotation = rotation;
    }

    // Visualize the collision sphere in the editor
    private void OnDrawGizmos()
    {
        if (!target) return;

        Vector3 gravityUp = target.up;
        Vector3 worldForward = Vector3.forward;
        Vector3 gravityForward = Vector3.ProjectOnPlane(worldForward, gravityUp);

        if (gravityForward.sqrMagnitude < 0.01f)
        {
            gravityForward = Vector3.ProjectOnPlane(Vector3.right, gravityUp);
        }
        gravityForward.Normalize();

        Quaternion yawRotation = Quaternion.AngleAxis(yaw, gravityUp);
        Vector3 rotatedForward = yawRotation * gravityForward;
        Vector3 rightVector = Vector3.Cross(gravityUp, rotatedForward).normalized;
        Quaternion pitchRotation = Quaternion.AngleAxis(pitch, rightVector);
        Quaternion rotation = pitchRotation * yawRotation * Quaternion.LookRotation(gravityForward, gravityUp);

        Vector3 desiredPos = target.position + rotation * offset;
        Vector3 direction = (desiredPos - target.position).normalized;
        float distance = (desiredPos - target.position).magnitude;

        // Draw the collision check sphere
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(target.position, collisionRadius);

        // Draw the path
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(target.position, target.position + direction * distance);

        // Draw camera position sphere
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(desiredPos, collisionRadius);
    }
}