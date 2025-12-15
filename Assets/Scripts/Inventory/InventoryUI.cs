using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Inventory Settings")]
    public GameObject inventoryPanel;  

    [Header("Audio Settings")]
    public AudioSource audioSource;     // AudioSource for Inventory
    public AudioClip openSound;         // Open inventory sound
    public AudioClip closeSound;        // Close inventory sound

    private bool isOpen = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Inventory close by default
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

        // Play Sound
        if (audioSource != null)
        {
            if (isOpen && openSound != null)
            {
                audioSource.PlayOneShot(openSound);
            }
            else if (!isOpen && closeSound != null)
            {
                audioSource.PlayOneShot(closeSound);
            }
        }
    }

}
