using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    [Header("UI Root to toggle")]
    public GameObject inventoryRoot;   // Panel gốc của túi đồ (Inventory UI)

    [Header("Player controller to block input")]
    public HumanController player;     // kéo HumanController của Player vào đây (hoặc để auto)

    [Header("Hotkey")]
    public KeyCode toggleKey = KeyCode.I;

    [Header("Options")]
    public bool pauseGameWhenOpen = true;
    public bool unlockCursorWhenOpen = true;
    public bool autoFindPlayerByTag = true;
    public string playerTag = "Player";

    private bool isOpen;
    private float prevTimeScale = 1f;

    void Awake()
    {
        if (inventoryRoot != null)
            inventoryRoot.SetActive(false);

        if (player == null && autoFindPlayerByTag)
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go != null) player = go.GetComponent<HumanController>();
        }

        // trạng thái ban đầu khi vào scene: gameplay
        ApplyCursor(false);
        ApplyBlockInput(false);
    }

    void Update()
    {
        // nếu đang thiếu player thì thử tìm lại
        if (player == null && autoFindPlayerByTag)
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go != null) player = go.GetComponent<HumanController>();
        }

        if (Input.GetKeyDown(toggleKey))
        {
            if (isOpen) Close();
            else Open();
        }

        // Optional: ESC để đóng nhanh
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    public void Open()
    {
        isOpen = true;

        if (inventoryRoot != null)
            inventoryRoot.SetActive(true);

        if (pauseGameWhenOpen)
        {
            prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        ApplyCursor(true);
        ApplyBlockInput(true);
    }

    public void Close()
    {
        isOpen = false;

        if (inventoryRoot != null)
            inventoryRoot.SetActive(false);

        if (pauseGameWhenOpen)
            Time.timeScale = prevTimeScale;

        ApplyCursor(false);
        ApplyBlockInput(false);
    }

    private void ApplyCursor(bool open)
    {
        if (!unlockCursorWhenOpen) return;

        if (open)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void ApplyBlockInput(bool open)
    {
        if (player != null)
            player.isUIOpen = open; // biến này nằm trong HumanController (mình đã đưa cho bạn)
    }
}
