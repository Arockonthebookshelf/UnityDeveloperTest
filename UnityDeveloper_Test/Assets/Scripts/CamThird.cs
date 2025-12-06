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

    private float yaw;
    private float pitch;

    void LateUpdate()
    {
        if (!target) return;

        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minY, maxY);

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

        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);

        transform.rotation = rotation;
    }
}