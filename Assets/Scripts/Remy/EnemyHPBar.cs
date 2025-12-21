using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EnemyHPBar : MonoBehaviour
{
    [Header("Bind")]
    public CharacterStats target;     // Máu của ai
    public Image fillImage;           // Image Fill (type = Filled)

    [Header("Follow (auto)")]
    public Transform followPoint;     // HPBarPoint (nếu null sẽ auto)
    public Vector3 offset = new Vector3(0f, 0.2f, 0f); // cộng thêm chút cho lên đầu

    [Header("Camera")]
    public Camera cam;

    [Header("Auto create HPBarPoint runtime")]
    public string followPointName = "HPBarPoint";
    public float autoHeadHeightPadding = 0.25f; // lấy Bounds + thêm padding

    // runtime-only helper
    Transform _runtimePoint;

    void Awake()
    {
        if (cam == null) cam = Camera.main;

        // Auto target từ parent
        if (target == null)
            target = GetComponentInParent<CharacterStats>();

        // Auto fill image từ BG/Fill
        if (fillImage == null)
        {
            var t = transform.Find("BG/Fill");
            if (t != null) fillImage = t.GetComponent<Image>();
        }

        // Auto follow point
        AutoBindFollowPointSafe();
    }

    void LateUpdate()
    {
        if (cam == null) cam = Camera.main;
        if (target == null || fillImage == null) return;

        if (followPoint == null) AutoBindFollowPointSafe();

        // ====== Update HP (ĐÃ SỬA LẠI) ======
        // Thay vì dùng Reflection dò tên, ta gọi thẳng vào biến TotalMaxHP mới
        float maxHP = target.TotalMaxHP;
        float curHP = target.currentHP;

        // Tính phần trăm (Tránh chia cho 0)
        float pct = (maxHP <= 0) ? 0f : curHP / maxHP;

        fillImage.fillAmount = Mathf.Clamp01(pct);

        // ====== Follow head ======
        if (followPoint != null)
        {
            transform.position = followPoint.position + offset;
        }
        else
        {
            transform.position = GetAutoHeadWorldPos(target.transform) + offset;
        }

        // ====== Billboard to camera ======
        if (cam != null)
        {
            transform.forward = cam.transform.forward;
        }
    }

    // =========================
    // Auto bind followPoint SAFE (không phá prefab asset)
    // =========================
    void AutoBindFollowPointSafe()
    {
        if (target == null) return;

        // 1) Thử tìm HPBarPoint trong hierarchy của target
        if (followPoint == null)
        {
            var found = FindDeepChild(target.transform, followPointName);
            if (found != null) followPoint = found;
        }

        // 2) Nếu không có -> tạo runtime point (CHỈ khi đang chạy game)
        if (followPoint == null)
        {
#if UNITY_EDITOR
            if (PrefabUtility.IsPartOfPrefabAsset(target.gameObject) || PrefabUtility.IsPartOfPrefabAsset(gameObject))
                return;
#endif
            if (!Application.isPlaying) return;

            var go = new GameObject(followPointName);
            _runtimePoint = go.transform;

            _runtimePoint.SetParent(target.transform, false);
            _runtimePoint.position = GetAutoHeadWorldPos(target.transform);
            followPoint = _runtimePoint;
        }
    }

    // Lấy vị trí trên đầu dựa theo Renderer bounds (auto head)
    Vector3 GetAutoHeadWorldPos(Transform root)
    {
        Vector3 pos = root.position + Vector3.up * 2f;
        var rends = root.GetComponentsInChildren<Renderer>();
        if (rends != null && rends.Length > 0)
        {
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++)
                b.Encapsulate(rends[i].bounds);

            pos = new Vector3(b.center.x, b.max.y + autoHeadHeightPadding, b.center.z);
        }
        return pos;
    }

    Transform FindDeepChild(Transform parent, string childName)
    {
        if (parent == null) return null;
        foreach (Transform c in parent)
        {
            if (c.name == childName) return c;
            var r = FindDeepChild(c, childName);
            if (r != null) return r;
        }
        return null;
    }
}