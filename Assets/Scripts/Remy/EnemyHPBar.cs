using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

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

        // nếu chưa có followPoint thì thử bind lại (tránh trường hợp spawn muộn)
        if (followPoint == null) AutoBindFollowPointSafe();

        // ====== Update HP ======
        int maxHP = GetInt(target, "MaxHP", "maxHP", "MaxHp", "maxHp");
        int curHP = GetInt(target, "CurrentHP", "currentHP", "CurrentHp", "curHP", "CurHP");

        float pct = (maxHP <= 0) ? 0f : (float)curHP / maxHP;
        fillImage.fillAmount = Mathf.Clamp01(pct);

        // ====== Follow head ======
        if (followPoint != null)
        {
            transform.position = followPoint.position + offset;
        }
        else
        {
            // fallback: nếu vẫn chưa có followPoint thì đặt theo bounds của target
            transform.position = GetAutoHeadWorldPos(target.transform) + offset;
        }

        // ====== Billboard to camera ======
        if (cam != null)
        {
            // quay mặt theo hướng camera nhìn
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
            // Không tạo/không set parent khi đang ở Prefab Asset / Prefab Mode
#if UNITY_EDITOR
            if (PrefabUtility.IsPartOfPrefabAsset(target.gameObject) || PrefabUtility.IsPartOfPrefabAsset(gameObject))
                return;
#endif
            if (!Application.isPlaying) return;

            // tạo 1 empty runtime point và parent vào target (scene instance OK)
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
        // mặc định: ngay tại root + 2m
        Vector3 pos = root.position + Vector3.up * 2f;

        // gom bounds của tất cả renderer con
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

    // Find child sâu theo tên
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

    // đọc int field/property bằng reflection (tự động hợp nhiều tên)
    int GetInt(object obj, params string[] names)
    {
        var type = obj.GetType();
        foreach (var n in names)
        {
            var f = type.GetField(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null && f.FieldType == typeof(int)) return (int)f.GetValue(obj);

            var p = type.GetProperty(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (p != null && p.PropertyType == typeof(int)) return (int)p.GetValue(obj);
        }
        return 0;
    }
}
