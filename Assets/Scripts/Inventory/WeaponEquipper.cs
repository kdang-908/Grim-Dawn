using UnityEngine;

public class WeaponEquipper : MonoBehaviour
{
    [Header("Assign socket right hand (WeaponSocket_R in the RUNTIME player)")]
    public Transform socketR;

    [Header("Optional: child pivot under socketR (WeaponGrip)")]
    public Transform weaponGrip;

    private GameObject currentWeapon;

    public void Equip(WeaponData data) => Equip(data, -1);

    // layer = -1 => do not force layer
    public void Equip(WeaponData data, int layer)
    {
        if (data == null || data.prefab == null)
        {
            Debug.LogWarning("[WeaponEquipper] Equip: data or prefab NULL");
            return;
        }

        // Always rebind from runtime hierarchy (avoid prefab-asset reference)
        LateBindSocketFromRuntime();

        if (socketR == null)
        {
            Debug.LogError("[WeaponEquipper] socketR NULL (runtime). Check WeaponSocket_R exists in spawned player.");
            return;
        }

        // auto find WeaponGrip under socket
        if (weaponGrip == null)
        {
            var found = FindChildContains(socketR, "WeaponGrip");
            if (found != null) weaponGrip = found;
        }

        Transform parent = (weaponGrip != null) ? weaponGrip : socketR;

        Unequip();

        // Instantiate WITHOUT parent first (safe), then SetParent runtime socket
        currentWeapon = Instantiate(data.prefab);
        currentWeapon.name = data.prefab.name + "_Equipped";

        // Parent to runtime socket/grip
        currentWeapon.transform.SetParent(parent, false);

        // Reset then apply offsets from WeaponData
        var t = currentWeapon.transform;
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;

        t.localPosition = data.localPos;
        t.localRotation = Quaternion.Euler(data.localEuler);
        t.localScale = data.localScale;

        if (layer != -1)
            SetLayerRecursively(currentWeapon, layer);

        Debug.Log($"[WeaponEquipper] Equipped '{currentWeapon.name}' under '{parent.name}' | localPos={data.localPos} localEuler={data.localEuler}");
    }

    public void Unequip()
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
            currentWeapon = null;
        }
    }

    private void LateBindSocketFromRuntime()
    {
        // already runtime transform => OK
        if (socketR != null && socketR.gameObject.scene.IsValid())
            return;

        // Find socket in THIS character hierarchy (include inactive)
        var all = GetComponentsInChildren<Transform>(true);
        foreach (var t in all)
        {
            // robust match: ignore trailing spaces, allow "WeaponSocket_R " etc.
            var n = t.name.Trim();
            if (n == "WeaponSocket_R")
            {
                socketR = t;
                Debug.Log("[WeaponEquipper] Rebind socket OK: " + GetPath(socketR));
                return;
            }
        }

        Debug.LogError("[WeaponEquipper] Không tìm thấy WeaponSocket_R trong runtime player này!");
    }

    private static Transform FindChildContains(Transform root, string contains)
    {
        if (root == null) return null;
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.name.Trim().Contains(contains)) return t;
        }
        return null;
    }

    private void SetLayerRecursively(GameObject go, int layer)
    {
        if (go == null) return;
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    private static string GetPath(Transform t)
    {
        string p = t.name;
        while (t.parent != null) { t = t.parent; p = t.name + "/" + p; }
        return p;
    }
}
