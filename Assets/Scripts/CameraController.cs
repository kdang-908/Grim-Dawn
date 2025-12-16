using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Attribute Camera")]
    public Transform target;

    [SerializeField]
    private Vector3 offset;

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

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Zoom
        currentZoom -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
    }

    void LateUpdate()
    {
        // Toggle chuột
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
        }

        CameraMove();

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // Công thức zoom đúng
        Vector3 zoomedOffset = rotation * (offset * currentZoom);

        // Cập nhật vị trí camera
        transform.position = target.position + zoomedOffset;

        // Nhìn vào nhân vật (có cộng thêm Vector3.up nếu muốn nhìn cao hơn)
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }

    private void CameraMove()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            yaw += Input.GetAxis("Mouse X") * yawSpeed * Time.deltaTime;
            pitch -= Input.GetAxis("Mouse Y") * pitchSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
        }
    }
}
