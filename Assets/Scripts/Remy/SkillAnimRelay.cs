using UnityEngine;

public class SkillAnimRelay : MonoBehaviour
{
    [Header("Tham chiếu tới SkillLineHitbox (SkillOrigin)")]
    public SkillLineHitbox skill;

    void Awake()
    {
        // nếu quên kéo tay, tự tìm trong con
        if (skill == null)
            skill = GetComponentInChildren<SkillLineHitbox>();
    }

    // HÀM NÀY BỊ GỌI TỪ ANIMATION EVENT
    public void OnSkillImpact()
    {
        if (skill != null)
        {
            Debug.Log("[SkillAnimRelay] OnSkillImpact -> DoSkillAttack");
            skill.DoSkillAttack();
        }
        else
        {
            Debug.LogWarning("[SkillAnimRelay] skill NULL, không gọi được DoSkillAttack");
        }
    }

    // nếu ở cuối animation bạn có event OnSkillEnd thì dùng cái này
    public void OnSkillEnd()
    {
        if (skill != null)
        {
            Debug.Log("[SkillAnimRelay] OnSkillEnd");
            // hiện tại DoSkillAttack tự Destroy VFX rồi, không cần làm gì thêm
        }
    }
}
