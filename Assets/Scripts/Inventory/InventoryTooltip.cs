using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryTooltip : MonoBehaviour
{
    public static InventoryTooltip Instance;

    [Header("UI Components")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI descText;

    private RectTransform rectTransform;
    private Canvas canvas;

    void Awake()
    {
        // Singleton Check
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        rectTransform = GetComponent<RectTransform>();

        // Lưu lại Canvas hiện tại (ở Scene 1) trước khi tách ra
        Canvas initialCanvas = GetComponentInParent<Canvas>();
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        // Gắn lại vào Canvas cũ ngay lập tức để hiện ở Scene 1
        if (initialCanvas != null)
        {
            canvas = initialCanvas;
            transform.SetParent(canvas.transform, false);
        }
        else
        {
            // Nếu không tìm thấy, thử tìm thủ công
            EnsureCanvas();
        }

        // Đảm bảo nó nằm trên cùng
        transform.SetAsLastSibling();

        gameObject.SetActive(false);
    }

    void Update()
    {
        if (gameObject.activeSelf)
        {
            // Nếu mất Canvas (do chuyển scene), tìm lại 
            if (canvas == null)
            {
                EnsureCanvas();
                // Nếu chưa tìm thấy thì chưa update vị trí 
                if (canvas == null) return;
            }

            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out mousePos);

            rectTransform.anchoredPosition = mousePos + new Vector2(15, -15);
        }
    }

    public void ShowTooltip(WeaponData data, int level)
    {
        if (data == null) return;

        // kiểm tra Canvas trước khi hiện
        EnsureCanvas();

        gameObject.SetActive(true);
        transform.SetAsLastSibling(); // Đẩy lên trên cùng

        if (nameText)
        {
            nameText.text = (level > 1) ? $"{data.displayName} (+{level})" : data.displayName;

            string stats = "";
            int atk = data.GetATK(level);
            int def = data.GetDEF(level);
            int hp = data.GetMaxHP(level);
            int energy = data.GetEnergy(level);
            if (atk > 0) stats += $"ATK: <color=green>+{atk}</color>\n";
            if (def > 0) stats += $"DEF: <color=green>+{def}</color>\n";
            if (hp > 0) stats += $"HP: <color=green>+{hp}</color>\n";
            if (energy > 0) stats += $"Energy: <color=green>+{energy}</color>\n";

            if (statsText) statsText.text = stats;
            if (descText) descText.text = data.description;
        }
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }

    private void EnsureCanvas()
    {
        // Nếu hiện tại chưa có Canvas hoặc Canvas cũ đã bị hủy (null)
        if (canvas == null || transform.parent == null || transform.parent.GetComponent<Canvas>() == null)
        {
            // Tìm Canvas chính trong Scene hiện tại
            // Hoặc lấy cái Canvas đầu tiên tìm thấy
            Canvas foundCanvas = FindFirstObjectByType<Canvas>();

            if (foundCanvas != null)
            {
                canvas = foundCanvas;
                transform.SetParent(canvas.transform, false); // false để giữ nguyên scale
                transform.localScale = Vector3.one; // Reset scale về 1 
                transform.SetAsLastSibling(); // Đẩy lên trên cùng
            }
        }
    }
}