using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopSlotUI : MonoBehaviour, IPointerClickHandler
{
    public WeaponData data;
    public Image icon;
    public ShopUIController shop;

    [Header("Optional UI")]
    public GameObject soldOverlay; // nếu có (Text/Panel “SOLD”) thì kéo vào

    private Button btn;

    void Awake()
    {
        if (icon == null) icon = GetComponentInChildren<Image>(true);
        btn = GetComponentInChildren<Button>(true);
    }

    public void Setup(WeaponData d, ShopUIController controller)
    {
        data = d;
        shop = controller;
        Refresh();
    }

    public void Refresh()
    {
        if (icon == null) return;

        if (data != null && data.icon != null)
        {
            icon.sprite = data.icon;
            icon.enabled = true;
        }
        else
        {
            icon.sprite = null;
            icon.enabled = false;
        }

        RefreshSoldState();
    }

    public void RefreshSoldState()
    {
        bool sold = (shop != null && data != null && shop.IsSold(data));

        if (soldOverlay != null)
            soldOverlay.SetActive(sold);

        // khóa click slot luôn
        if (btn != null)
            btn.interactable = !sold;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (shop == null || data == null) return;

        // nếu sold thì không cho select nữa (hoặc bạn muốn vẫn xem info thì bỏ return)
        if (shop.IsSold(data)) return;

        shop.Select(data);
    }
}
