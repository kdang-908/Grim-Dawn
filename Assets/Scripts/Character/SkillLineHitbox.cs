using UnityEngine;
using MaykerStudio.Demo;   // để dùng Projectile của asset

public class SkillLineHitbox : MonoBehaviour
{
    [Header("Owner & Target")]
    public CharacterStats owner;
    public LayerMask targetLayer;

    [Header("Vùng sát thương thẳng")]
    public float range = 10f;
    public float width = 2f;
    public float height = 2f;

    [Header("VFX (optional)")]
    public GameObject vfxPrefab;
    public Transform vfxSpawn;          // thường gán SkillOrigin
    public float vfxLifeTime = 1.5f;

    [Header("VFX Rotation")]
    [Tooltip("Xoay thêm cho VFX nếu nó bị lệch trục")]
    public Vector3 vfxRotationOffsetEuler = new Vector3(0f, -90f, 0f);

    [Header("VFX Position Offset")]
    [Tooltip("Đẩy hiệu ứng ra xa khỏi người chơi theo hướng forward")]
    public float vfxForwardOffset = 8f;
    [Tooltip("Đẩy hiệu ứng lên cao (0 = sát mặt đất)")]
    public float vfxHeightOffset = 1f;

    [Header("Audio")]
    public AudioClip skillSfx;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    // ==================== MAIN ====================
    public void DoSkillAttack()
    {
        // ----- Lấy owner -----
        if (owner == null)
            owner = GetComponentInParent<CharacterStats>();

        if (owner == null)
        {
            //Debug.LogWarning("[SkillLineHitbox] Owner null");
            return;
        }

        // ----- Tính vùng sát thương -----
        Vector3 forward = transform.forward;
        Vector3 center = transform.position + forward * (range * 0.5f);
        Vector3 halfExtents = new Vector3(width * 0.5f, height * 0.5f, range * 0.5f);
        Quaternion orientation = Quaternion.LookRotation(forward);

        Collider[] hits = Physics.OverlapBox(
            center,
            halfExtents,
            orientation,
            targetLayer,
            QueryTriggerInteraction.Ignore
        );

        foreach (var col in hits)
        {
            CharacterStats target = col.GetComponentInParent<CharacterStats>();
            if (target == null) continue;
            if (target == owner) continue;

            int damage = Mathf.Max(1, owner.atk - target.def);
            target.TakeDamage(damage);
        }

        // ----- Spawn VFX -----
        SpawnVfx(forward);

        // ----- Audio -----
        if (skillSfx != null)
        {
            // phát tại vị trí player
            AudioSource.PlayClipAtPoint(skillSfx, transform.position, sfxVolume);
        }
    }

    // ==================== SPAWN VFX ====================
    void SpawnVfx(Vector3 forward)
    {
        if (vfxPrefab == null)
        {
            //Debug.LogWarning("[SkillLineHitbox] vfxPrefab is null");
            return;
        }

        Transform spawnT = vfxSpawn != null ? vfxSpawn : transform;

        // vị trí: từ spawnT, đẩy lên cao + đẩy ra trước
        Vector3 spawnPos = spawnT.position
                           + forward * vfxForwardOffset
                           + Vector3.up * vfxHeightOffset;

        // hướng nhìn + offset xoay
        Quaternion spawnRot =Quaternion.LookRotation(forward) *Quaternion.Euler(vfxRotationOffsetEuler);

        GameObject vfx = Instantiate(vfxPrefab, spawnPos, spawnRot);
        //Debug.Log("[SkillLineHitbox] Spawn VFX at " + spawnPos);

        // Nếu prefab có Projectile (của MaykerStudio) -> dùng Fire() theo đúng demo
        Projectile proj = vfx.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.Fire();
            // Projectile tự lo dừng / spawn impact, không cần Destroy ở đây
            return;
        }

        // Nếu không có Projectile -> fallback: play particle và tự destroy
        var particles = vfx.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var p in particles)
            p.Play(true);

        if (vfxLifeTime > 0f)
            Destroy(vfx, vfxLifeTime);
    }

    // ==================== GIZMOS ====================
    void OnDrawGizmosSelected()
    {
        Vector3 forward = transform.forward;
        Vector3 center = transform.position + forward * (range * 0.5f);
        Vector3 halfExtents = new Vector3(width * 0.5f, height * 0.5f, range * 0.5f);
        Quaternion orientation = Quaternion.LookRotation(forward);

        Gizmos.color = Color.yellow;
        Gizmos.matrix = Matrix4x4.TRS(center, orientation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
    }
}
