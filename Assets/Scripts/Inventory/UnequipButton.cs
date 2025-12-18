using UnityEngine;

public class UnequipButton : MonoBehaviour
{
    public InventoryItem.ItemType slotTypeToUnequip;

    private EquipmentManager em;

    void Awake()
    {
        em = FindFirstObjectByType<EquipmentManager>();
    }

    public void OnClickUnequip()
    {
        if (em == null) em = FindFirstObjectByType<EquipmentManager>();
        if (em == null) { Debug.LogError("Missing EquipmentManager"); return; }

        em.UnequipItem(slotTypeToUnequip);
    }
}
