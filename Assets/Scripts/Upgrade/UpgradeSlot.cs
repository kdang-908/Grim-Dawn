using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UpgradeSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    public Image iconImage;          // child: Icon
    public GameObject plusIcon;      // child: Plus hoặc Text (TMP) dấu +
    public GameObject inventoryPanel; // Panel túi đồ (kéo Panel_Right vào)

    [Header("Runtime")]
    public InventoryItem originalItem; // item gốc trong túi

    private float lastClickTime;
    const float DoubleClickDelay = 0.25f;

    public bool IsEmpty => originalItem == null;

    private void Awake()
    {
        // Auto-bind nếu quên kéo trong Inspector
        if (iconImage == null)
        {
            var t = transform.Find("Icon");
            if (t != null) iconImage = t.GetComponent<Image>();
        }

        if (plusIcon == null)
        {
            var t = transform.Find("Plus");
            if (t == null) t = transform.Find("Text (TMP)");
            if (t != null) plusIcon = t.gameObject;
        }
    }

    private void Start()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
            iconImage.gameObject.SetActive(true); // luôn bật, chỉ ẩn bằng enabled
        }

        if (plusIcon != null)
            plusIcon.SetActive(true);
    }

    // Gán item vào ô nâng cấp
    public void SetItem(InventoryItem item)
    {
        Debug.Log($"[UpgradeSlot] SetItem from {item.name}");

        originalItem = item;

        // 1) Ẩn item trong túi
        originalItem.gameObject.SetActive(false);

        // 2) Lấy Image của item gốc
        Image sourceImg = originalItem.GetComponent<Image>();
        if (sourceImg == null)
            sourceImg = originalItem.GetComponentInChildren<Image>(true);

        if (iconImage == null)
        {
            Debug.LogWarning("[UpgradeSlot] iconImage = NULL");
            return;
        }

        if (sourceImg == null || sourceImg.sprite == null)
        {
            Debug.LogWarning("[UpgradeSlot] sourceImg hoặc sprite NULL");
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
        else
        {
            // Copy sprite & ép hiển thị full ô
            iconImage.sprite = sourceImg.sprite;
            iconImage.type = Image.Type.Simple;
            iconImage.preserveAspect = true;
            iconImage.color = Color.white;
            iconImage.enabled = true;
            iconImage.gameObject.SetActive(true);

            RectTransform rt = iconImage.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Debug.Log($"[UpgradeSlot] Copied sprite = {sourceImg.sprite.name}, rt={rt.rect}");
        }

        // 4) Tắt dấu +
        if (plusIcon != null) plusIcon.SetActive(false);
    }

    // Trả item về túi
    public void ClearSlot()
    {
        Debug.Log("[UpgradeSlot] ClearSlot");

        if (originalItem != null)
        {
            originalItem.gameObject.SetActive(true);
            originalItem = null;
        }

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (plusIcon != null)
            plusIcon.SetActive(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        // double click = remove item
        if (Time.time - lastClickTime < DoubleClickDelay)
        {
            OnDoubleClick();
            return;
        }

        lastClickTime = Time.time;

        // Nếu slot đang TRỐNG -> chỉ toggle túi
        if (IsEmpty && inventoryPanel != null)
        {
            bool newActive = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(newActive);
            Debug.Log($"[UpgradeSlot] Toggle inventoryPanel -> {newActive}");
        }
    }

    private void OnDoubleClick()
    {
        if (IsEmpty) return;

        if (EnhancementPanel.Instance == null)
        {
            Debug.LogWarning("[UpgradeSlot] EnhancementPanel.Instance NULL");
            return;
        }

        EnhancementPanel.Instance.ReturnItemFromSlot(this);
    }
}
