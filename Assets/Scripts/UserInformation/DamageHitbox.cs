using UnityEngine;

public class DamageHitbox : MonoBehaviour
{
    public CharacterStats owner;
    public LayerMask targetLayer;

    private bool canHit = false;

    public void EnableHit()
    {
        // Được gọi từ animation event khi bắt đầu ra đòn
        canHit = true;
    }

    public void DisableHit()
    {
        // Được gọi khi kết thúc animation đánh
        canHit = false;
    }

    private void OnTriggerStay(Collider other)
    {
        // Chỉ xử lý khi đang trong “cửa sổ” có thể gây damage
        if (!canHit) return;

        // Kiểm tra layer mục tiêu có nằm trong targetLayer không
        if (((1 << other.gameObject.layer) & targetLayer) == 0)
            return;

        // Lấy CharacterStats trên enemy
        CharacterStats target = other.GetComponentInParent<CharacterStats>();
        if (target == null) return;
        if (target == owner) return;

        int damage = Mathf.Max(1, owner.atk - target.def);
        target.TakeDamage(damage);

        Debug.Log($"{owner.characterName} hit {target.characterName} for {damage}");

        // Đảm bảo mỗi lần vung kiếm chỉ gây 1 hit
        canHit = false;
    }
}
