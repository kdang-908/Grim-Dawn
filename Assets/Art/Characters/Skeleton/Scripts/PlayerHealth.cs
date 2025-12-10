using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int health = 100;

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        Debug.Log("Player bị đánh! Máu còn: " + health);

        if (health <= 0)
            Debug.Log("Player chết!");
    }
}
