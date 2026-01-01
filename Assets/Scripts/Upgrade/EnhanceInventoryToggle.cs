using UnityEngine;

public class EnhanceInventoryToggle : MonoBehaviour
{
    [Header("Panel túi đồ")]
    [Tooltip("Kéo object Inventory (panel túi đồ) vào đây")]
    public GameObject inventoryPanel;

    public void ToggleInventory()
    {
        if (inventoryPanel == null)
        {
            Debug.LogWarning("[EnhanceInventoryToggle] Chưa gán Inventory Panel");
            return;
        }

        bool show = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(show);
        // Chuột đã được xử lý bởi BlacksmithInteraction, 
        // nên không cần đụng tới Cursor ở đây nữa.
    }
}
