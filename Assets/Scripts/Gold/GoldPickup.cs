using UnityEngine;

public class GoldPickup : MonoBehaviour
{
    [Header("Gold")]
    public int value = 10;           // số vàng coin này cho khi nhặt
    public string playerTag = "Player";

    [Header("Lifetime")]
    public float lifeTime = 20f;     // tự hủy sau X giây (0 = không tự hủy)

    void Start()
    {
        // coin tự biến mất nếu không ai nhặt
        if (lifeTime > 0)
            Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        var gm = GameManager.Instance;

        if (gm != null)
        {
            gm.AddGold(value);

            //Debug.Log($"[GoldPickup] Player picked gold = {value} | Total = {gm.gold}");
        }
        else
        {
            //Debug.LogWarning("[GoldPickup] GameManager not found, gold not added!");
        }

        Destroy(gameObject); 
    }
}