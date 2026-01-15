using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Shop Item Data")]
public class ShopItemData : ScriptableObject
{
    public WeaponData item;     // món hàng (WeaponData/Head/Chest...)
    public int price = 50;      // giá
    public int level = 1;       // level khi mua (thường = 1)

    [Header("Optional")]
    public bool infiniteStock = true;
    public int stock = 1;       // nếu infiniteStock = false thì trừ dần

    public bool CanBuy()
    {
        if (item == null) return false;
        if (infiniteStock) return true;
        return stock > 0;
    }

    public void ConsumeStock()
    {
        if (infiniteStock) return;
        stock = Mathf.Max(0, stock - 1);
    }
}
