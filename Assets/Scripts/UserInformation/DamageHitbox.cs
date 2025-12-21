using UnityEngine;

public class DamageHitbox : MonoBehaviour
{
    public CharacterStats owner;
    public LayerMask targetLayer;

    private bool canHit = false;

    public void EnableHit()
    {
        canHit = true;
    }

    public void DisableHit()
    {
        canHit = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canHit) return;

        if (((1 << other.gameObject.layer) & targetLayer) == 0)
            return;

        CharacterStats target =
            other.GetComponentInParent<CharacterStats>();

        if (target == null) return;
        if (target == owner) return;

        int damage = Mathf.Max(1, owner.TotalATK - target.TotalDEF);
        target.TakeDamage(damage);

        Debug.Log($"{owner.characterName} hit {target.characterName} for {damage}");
    }
}
