using UnityEngine;

public class PlayerCameraController : MonoBehaviour, IPlayerCamera
{
    [Header("Attribute Camera")]
    public Transform target;
    public Transform Target
    {
        get => target;
        set => target = value;
    }

    [SerializeField] private Vector3 offset;

    private float yaw;
    private float pitch;

    [Header("Zoom Settings")]
    [SerializeField] private float currentZoom = 10f;
    public float zoomSpeed = 4f;
    public float minZoom = 5f;
    public float maxZoom = 15f;

    [Header("Rotation Settings")]
    [SerializeField] private float yawSpeed = 200f;
    [SerializeField] private float pitchSpeed = 120f;
    [SerializeField] private float pitchMin = -20f;
    [SerializeField] private float pitchMax = 60f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        SetCursorLock(true); // khóa chuột khi vào game
    }

    // ✅ THÊM HÀM NÀY
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
        // (Bạn có thể giữ Q nếu muốn)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            bool locked = Cursor.lockState != CursorLockMode.Locked;
            SetCursorLock(locked);
        }

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            yaw += Input.GetAxis("Mouse X") * yawSpeed * Time.deltaTime;
            pitch -= Input.GetAxis("Mouse Y") * pitchSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 zoomedOffset = rotation * (offset * currentZoom);

        transform.position = target.position + zoomedOffset;
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
    