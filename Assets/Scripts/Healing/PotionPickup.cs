using UnityEngine;

public class PotionPickup : MonoBehaviour
{
    public int amount = 1;
    public string playerTag = "Player";
    public float lifeTime = 20f;

    void Start()
    {
        if (lifeTime > 0)
            Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (PotionManager.Instance != null)
        {
            PotionManager.Instance.AddPotions(amount);
            //Debug.Log($"[PotionPickup] +{amount} potion | Total = {PotionManager.Instance}");
        }
        else
        {
            //Debug.LogWarning("[PotionPickup] PotionManager not found!");
        }

        Destroy(gameObject);
    }
}
    