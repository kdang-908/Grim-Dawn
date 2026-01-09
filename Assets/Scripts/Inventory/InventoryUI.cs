using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Inventory Settings")]
    public GameObject inventoryPanel;

    [Header("Camera")]
    public PlayerCameraController cameraController;    // KÉO UI_PreviewCam / Main Cam vào đây

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;

    private bool isOpen = false;

    void Start()
    {
        isOpen = false;
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    void ToggleInventory()
    {
        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);

        if (cameraController != null)
            cameraController.SetCursorLock(!isOpen);

        if (audioSource != null)
        {
            if (isOpen && openSound != null)
                audioSource.PlayOneShot(openSound);
            else if (!isOpen && closeSound != null)
                audioSource.PlayOneShot(closeSound);
        }

        // 🔥 QUAN TRỌNG
        if (isOpen)
            StartCoroutine(BindPreviewNextFrame());
    }
    System.Collections.IEnumerator BindPreviewNextFrame()
    {
        yield return null;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[InventoryUI] Player not found (tag Player).");
            yield break;
        }

        var preview = FindFirstObjectByType<UICharacterPreview>();
        if (preview != null)
            preview.BindFromPlayer(player);
        else
            Debug.LogError("[InventoryUI] UICharacterPreview not found in scene!");
    }



}
