using System;
using System.Collections.Generic;
using UnityEngine;

public class UICharacterPreview : MonoBehaviour
{
    [Header("Refs")]
    public Camera previewCam;
    public Transform previewRoot;

    [Header("Layer")]
    public string previewLayerName = "UIPreview";

    [Header("Rotate (drag mouse)")]
    public float rotateSpeed = 180f;

    [Serializable]
    public class PreviewConfig
    {
        [Tooltip("Keyword match theo tên prefab/player. Ví dụ: 'malemain', 'femalemain'")]
        public string prefabKeyword = "default";

        [Header("Model")]
        [Tooltip("Scale base (nhân thêm sau AutoFit nếu bật AutoFit)")]
        public float scale = 1.0f;

        [Tooltip("Yaw (xoay Y) để model quay mặt ra trước. 0 hoặc 180 thường dùng.")]
        public float yaw = 180f;

        [Tooltip("Bật: tự fit theo chiều cao Bounds")]
        public bool autoFit = true;

        [Tooltip("Chiều cao mục tiêu muốn hiển thị (đơn vị world)")]
        public float targetHeight = 1.8f;

        [Header("Camera")]
        public float camDistance = 2.2f;
        public float camHeight = 1.6f;
        public float lookAtHeight = 1.2f;
    }

    [Header("Per Character Preview Config")]
    public List<PreviewConfig> previewConfigs = new List<PreviewConfig>();

    [Header("Fallback (nếu không match keyword nào)")]
    public PreviewConfig defaultConfig = new PreviewConfig()
    {
        prefabKeyword = "default",
        scale = 1.0f,
        yaw = 180f,
        autoFit = true,
        targetHeight = 1.8f,
        camDistance = 2.2f,
        camHeight = 1.6f,
        lookAtHeight = 1.2f
    };

    private GameObject current;
    private int previewLayer;

    void Awake()
    {
        previewLayer = LayerMask.NameToLayer(previewLayerName);
        if (previewLayer < 0)
        {
            Debug.LogError($"[UICharacterPreview] Layer '{previewLayerName}' not found");
            enabled = false;
            return;
        }

        if (previewRoot == null) previewRoot = transform;
    }

    void Update()
    {
        if (current == null) return;

        if (Input.GetMouseButton(0))
        {
            float mx = Input.GetAxis("Mouse X");
            current.transform.Rotate(Vector3.up, -mx * rotateSpeed * Time.deltaTime, Space.World);
        }
    }

    public void BindFromPlayer(GameObject player)
    {
        if (player == null)
        {
            Debug.LogError("[UICharacterPreview] BindFromPlayer: player NULL");
            return;
        }

        // 1) Clear preview root
        for (int i = previewRoot.childCount - 1; i >= 0; i--)
            Destroy(previewRoot.GetChild(i).gameObject);

        // 2) Clone vào preview
        current = Instantiate(player, previewRoot);
        current.name = "Preview_Player";

        // 3) Remove gameplay components (để đứng yên)
        RemoveGameplayComponents(current);

        // 4) Set layer UI preview
        SetLayerRecursively(current, previewLayer);

        // 5) Apply config theo keyword
        var cfg = GetConfigFor(player.name);
        ApplyConfig(cfg, player.name);

        Debug.Log($"[UICharacterPreview] Bind OK -> player='{player.name}', cfg='{cfg.prefabKeyword}', scale={cfg.scale}, yaw={cfg.yaw}");
    }

    private PreviewConfig GetConfigFor(string playerName)
    {
        string n = (playerName ?? "").ToLower();

        // match config theo keyword
        foreach (var c in previewConfigs)
        {
            if (c == null) continue;
            string k = (c.prefabKeyword ?? "").ToLower().Trim();
            if (string.IsNullOrEmpty(k)) continue;

            if (n.Contains(k))
                return c;
        }

        return defaultConfig;
    }

    private void ApplyConfig(PreviewConfig cfg, string playerName)
    {
        if (cfg == null) cfg = defaultConfig;

        // reset transform
        current.transform.localPosition = Vector3.zero;
        current.transform.localRotation = Quaternion.identity;
        current.transform.localScale = Vector3.one;

        // --- AutoFit theo Bounds (để nam/nữ đều đẹp)
        float autoScale = 1f;
        if (cfg.autoFit)
        {
            if (TryGetRendererBounds(current, out Bounds b))
            {
                float h = Mathf.Max(0.0001f, b.size.y);
                autoScale = cfg.targetHeight / h;
            }
        }

        // scale cuối = autoScale * cfg.scale
        float finalScale = autoScale * Mathf.Max(0.0001f, cfg.scale);
        current.transform.localScale = Vector3.one * finalScale;

        // --- Yaw fix quay lưng
        current.transform.localRotation = Quaternion.Euler(0f, cfg.yaw, 0f);

        // --- Camera
        if (previewCam != null)
        {
            Vector3 center = previewRoot.position;

            previewCam.transform.position = center + new Vector3(0, cfg.camHeight, -cfg.camDistance);
            previewCam.transform.LookAt(center + new Vector3(0, cfg.lookAtHeight, 0));

            // quan trọng: chỉ render layer preview
            previewCam.cullingMask = (1 << previewLayer);
        }
    }

    private void RemoveGameplayComponents(GameObject go)
    {
        var hc = go.GetComponent<HumanController>();
        if (hc != null) Destroy(hc);

        var rb = go.GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);

        foreach (var col in go.GetComponentsInChildren<Collider>(true))
            Destroy(col);

        // nếu bạn có script khác điều khiển player (input, follow cam, v.v) thêm ở đây
    }

    private bool TryGetRendererBounds(GameObject go, out Bounds bounds)
    {
        var rends = go.GetComponentsInChildren<Renderer>(true);
        if (rends == null || rends.Length == 0)
        {
            bounds = new Bounds(go.transform.position, Vector3.one);
            return false;
        }

        bounds = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++)
            bounds.Encapsulate(rends[i].bounds);

        return true;
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform c in obj.transform)
            SetLayerRecursively(c.gameObject, layer);
    }
}
