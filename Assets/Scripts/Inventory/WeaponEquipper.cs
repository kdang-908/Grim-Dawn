using UnityEngine;

public class WeaponEquipper : MonoBehaviour
{
    [Header("Assign socket right hand")]
    public Transform socketR;

    [Header("Optional: child pivot under socketR (recommended)")]
    public Transform weaponGrip; // kéo WeaponGrip vào đây (nếu có)

    private GameObject currentWeapon;

    public void Equip(WeaponData data)
    {
        Equip(data, -1);
    }

    // layer = -1 nghĩa là không ép layer
    public void Equip(WeaponData data, int layer)
    {
        if (data == null)
        {
            Debug.LogWarning("[WeaponEquipper] Equip: data NULL");
            return;
        }

        if (socketR == null)
        {
            Debug.LogError("[WeaponEquipper] socketR NULL (chưa kéo WeaponSocket_R)");
            return;
        }

        if (data.prefab == null)
        {
            Debug.LogError("[WeaponEquipper] data.prefab NULL -> WeaponData chưa gán Prefab");
            return;
        }

        // ✅ auto-find WeaponGrip nếu bạn chưa kéo trong Inspector
        if (weaponGrip == null)
        {
            var found = socketR.Find("WeaponGrip");
            if (found != null) weaponGrip = found;
        }

        // ✅ ưu tiên gắn vào WeaponGrip để dễ xoay bù
        Transform parent = (weaponGrip != null) ? weaponGrip : socketR;

        Unequip();

        currentWeapon = Instantiate(data.prefab, parent);
        currentWeapon.name = data.prefab.name + "_Equipped";

        // ✅ Reset rồi apply offset từ WeaponData
        var t = currentWeapon.transform;
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;

        t.localPosition = data.localPos;
        t.localRotation = Quaternion.Euler(data.localEuler);
        t.localScale = data.localScale;

        // ✅ Force layer for preview camera render
        if (layer != -1)
            SetLayerRecursively(currentWeapon, layer);

        Debug.Log($"[WeaponEquipper] Spawned '{currentWeapon.name}' under '{parent.name}', layer={layer} pos={data.localPos} euler={data.localEuler}");
    }

    public void Unequip()
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
            currentWeapon = null;
        }
    }

    private void SetLayerRecursively(GameObject go, int layer)
    {
        if (go == null) return;
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}
