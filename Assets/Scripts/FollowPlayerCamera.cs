using UnityEngine;

public class FollowPlayerCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Offset & Zoom")]
    public Vector3 offset = new Vector3(0, 2, -4);
    public float currentZoom = 10f;
    public float zoomSpeed = 4f;
    public float minZoom = 5f;
    public float maxZoom = 15f;

    [Header("Rotation")]
    public float yawSpeed = 200f;
    public float pitchSpeed = 120f;
    public float pitchMin = -20f;
    public float pitchMax = 60f;

    float yaw;
    float pitch;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
        SetCursorLock(true);
    }

    public void SetCursorLock(bool locked)
    {
        Cursor.visible = !locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
    }

    void Update()
    {
        currentZoom -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Toggle khóa chuột
        if (Input.GetKeyDown(KeyCode.Q))
        {
            bool locked = Cursor.lockState != CursorLockMode.Locked;
            SetCursorLock(locked);
        }

        // Chỉ xoay quanh player khi chuột đang lock
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            yaw += Input.GetAxis("Mouse X") * yawSpeed * Time.deltaTime;
            pitch -= Input.GetAxis("Mouse Y") * pitchSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
        }
        // Chỉ dùng yaw/pitch từ chuột

        Quaternion camRot = Quaternion.Euler(pitch, yaw, 0);


        // Vị trí camera = target + offset quay theo camRot và zoom
        Vector3 camOffset = camRot * (offset.normalized * currentZoom);
        transform.position = target.position + camOffset;

        // Nhìn vào điểm hơi cao hơn (ngực/đầu)
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }

}
