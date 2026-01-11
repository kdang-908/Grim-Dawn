// ===============================
// 1) MinimapBinder.cs  ✅ FULL
// ===============================
using UnityEngine;

public class MinimapBinder : MonoBehaviour
{
    [Header("Refs (optional)")]
    public MinimapCameraFollow minimapCamera;
    public MinimapArrowRotate minimapArrow;

    void Awake()
    {
        if (minimapCamera == null)
            minimapCamera = FindObjectOfType<MinimapCameraFollow>(true);

        if (minimapArrow == null)
            minimapArrow = FindObjectOfType<MinimapArrowRotate>(true);
    }

    public void BindPlayer(Transform player)
    {
        if (player == null) return;

        if (minimapCamera != null) minimapCamera.SetTarget(player);
        if (minimapArrow != null) minimapArrow.SetTarget(player);
    }
}
