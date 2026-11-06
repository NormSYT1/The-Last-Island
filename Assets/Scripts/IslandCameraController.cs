using UnityEngine;

public class IslandCameraController : MonoBehaviour
{
    [Header("Target & Positioning")]
    public Vector3 targetPoint = Vector3.zero;
    public float distance = 90f;
    public float minDistance = 20f;
    public float maxDistance = 180f;
    
    [Header("Angles")]
    public float pitch = 50f;
    public float yaw = 45f;
    public float minPitch = 15f;
    public float maxPitch = 85f;

    [Header("Speed Settings")]
    public float rotateSpeed = 120f;
    public float zoomSpeed = 25f;
    public float panSpeed = 35f;
    public float smoothSpeed = 12f;

    private float currentYaw;
    private float currentPitch;
    private float currentDistance;
    private Vector3 currentTarget;

    private void Start()
    {
        currentYaw = yaw;
        currentPitch = pitch;
        currentDistance = distance;
        currentTarget = targetPoint;
        UpdateTransformInstant();
    }

    private void LateUpdate()
    {
        HandleInput();
        SmoothUpdate();
    }

    private void HandleInput()
    {
        // Sağ tık ile 360 derece döndürme
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            currentYaw += mouseX * rotateSpeed * 0.05f;
            currentPitch -= mouseY * rotateSpeed * 0.05f;
            currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
        }

        // Fare tekeri ile yakınlaşma / uzaklaşma
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentDistance -= scroll * zoomSpeed * 15f;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        }

        // Klavye ile ada üzerinde geniş gezinme (WASD veya Ok Tuşları)
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
        {
            Vector3 forward = Quaternion.Euler(0, currentYaw, 0) * Vector3.forward;
            Vector3 right = Quaternion.Euler(0, currentYaw, 0) * Vector3.right;
            Vector3 dir = (forward * v + right * h).normalized;
            currentTarget += dir * panSpeed * Time.deltaTime;
            currentTarget.x = Mathf.Clamp(currentTarget.x, -80f, 80f);
            currentTarget.z = Mathf.Clamp(currentTarget.z, -80f, 80f);
        }

        // Space veya 'R' tuşuna basınca merkeze sıfırla
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.R))
        {
            currentTarget = Vector3.zero;
            currentYaw = 45f;
            currentPitch = 50f;
            currentDistance = 90f;
        }
    }

    private void SmoothUpdate()
    {
        targetPoint = Vector3.Lerp(targetPoint, currentTarget, Time.deltaTime * smoothSpeed);
        yaw = Mathf.Lerp(yaw, currentYaw, Time.deltaTime * smoothSpeed);
        pitch = Mathf.Lerp(pitch, currentPitch, Time.deltaTime * smoothSpeed);
        distance = Mathf.Lerp(distance, currentDistance, Time.deltaTime * smoothSpeed);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + targetPoint;

        transform.rotation = rotation;
        transform.position = position;
    }

    private void UpdateTransformInstant()
    {
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -currentDistance);
        transform.rotation = rotation;
        transform.position = rotation * negDistance + currentTarget;
    }
}
