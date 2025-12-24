using UnityEngine;

public class FollowPlayerCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public string targetTag = "Player";
    public string preferChildName = "PlayerRoot"; // nếu player có child tên này thì bám vào

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
        var angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
        SetCursorLock(true);
    }

    void Update()
    {
        // tự bắt player nếu bị null (vì player spawn sau)
        if (target == null)
            TryFindTarget();

        currentZoom -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
    }

    void LateUpdate()
    {
        if (target == null) return;

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

        Quaternion camRot = Quaternion.Euler(pitch, yaw, 0);
        Vector3 camOffset = camRot * (offset.normalized * currentZoom);
        transform.position = target.position + camOffset;
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }

    void TryFindTarget()
    {
        var go = GameObject.FindGameObjectWithTag(targetTag);
        if (go == null) return;

        // ưu tiên child PlayerRoot nếu có
        var child = go.transform.Find(preferChildName);
        target = (child != null) ? child : go.transform;
    }

    public void SetCursorLock(bool locked)
    {
        Cursor.visible = !locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
