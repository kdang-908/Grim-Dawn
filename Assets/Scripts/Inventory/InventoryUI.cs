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
        yield return null; // đợi 1 frame cho UI + Animator init xong

        var em = FindFirstObjectByType<EquipmentManager>();
        if (em != null)
            em.BindPreviewNow();   // gọi bind tại đây
    }


}
