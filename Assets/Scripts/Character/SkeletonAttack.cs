using UnityEngine;

public class SkeletonAttack : MonoBehaviour

{
    public DamageHitbox hitbox;
    public float attackInterval = 1.2f;

    void Start()
    {
        InvokeRepeating(nameof(DoAttack), 1f, attackInterval);
    }

    void DoAttack()
    {
        hitbox.EnableHit();
        Invoke(nameof(StopHit), 0.2f);
    }

    void StopHit()
    {
        hitbox.DisableHit();
    }
}
