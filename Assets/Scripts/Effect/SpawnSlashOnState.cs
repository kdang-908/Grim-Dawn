using UnityEngine;

public class SpawnSlashOnState : StateMachineBehaviour
{
    [Header("Slash VFX")]
    public GameObject slashPrefab;

    [Header("Condition")]
    public bool requireWeapon = true;

    [Header("VFX Settings")]
    [Tooltip("1 = bình thường. >1 nhanh hơn, <1 chậm hơn")]
    public float vfxSpeed = 1.4f;

    [Tooltip("Thời gian tồn tại của VFX (giây). <=0: không auto destroy")]
    public float lifeTime = 0.3f;

    [Tooltip("Spawn theo % thời lượng animation state (0..1). Ví dụ 0.35 là 35% của nhát chém")]
    [Range(0f, 1f)]
    public float spawnAtNormalizedTime = 0.35f;

    [Tooltip("Tên Transform đặt điểm xuất hiện VFX. (VD: SlashPoint)")]
    public string slashPointName = "SlashPoint";

    [Tooltip("Nếu true: VFX sẽ lấy yaw theo hướng nhân vật")]
    public bool faceForward = true;

    [Tooltip("Bù góc nếu prefab bị ngược (vd: 0,180,0)")]
    public Vector3 rotationOffsetEuler = Vector3.zero;

    [Tooltip("Nếu true: thoát state sẽ xóa VFX ngay (tránh bị dính khi cancel animation)")]
    public bool destroyOnExit = true;

    private GameObject spawnedSlash;
    private bool spawned;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        spawnedSlash = null;
        spawned = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (spawned || slashPrefab == null) return;

        // normalizedTime có thể >1 nếu loop, lấy phần lẻ
        float t = stateInfo.normalizedTime % 1f;
        if (t < spawnAtNormalizedTime) return;

        TrySpawn(animator);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (destroyOnExit && spawnedSlash != null)
            Object.Destroy(spawnedSlash);

        spawnedSlash = null;
        spawned = false;
    }

    private void TrySpawn(Animator animator)
    {
        if (spawned) return;

        // check vũ khí ngay lúc spawn cho chắc
        if (requireWeapon)
        {
            var equipper = animator.GetComponent<WeaponEquipper>();
            if (equipper == null || !equipper.HasWeapon())
            {
                spawned = true; // tránh spam spawn
                return;
            }
        }

        Transform slashPoint = FindDeepChild(animator.transform, slashPointName);
        if (slashPoint == null)
        {
            Debug.LogWarning($"❌ Không tìm thấy '{slashPointName}'. Hãy tạo GameObject đúng tên này trong hierarchy.");
            spawned = true;
            return;
        }

        Quaternion rot = slashPoint.rotation;

        if (faceForward)
        {
            // lấy yaw theo nhân vật
            Vector3 e = rot.eulerAngles;
            e.y = animator.transform.eulerAngles.y;
            rot = Quaternion.Euler(e);
        }

        rot *= Quaternion.Euler(rotationOffsetEuler);

        spawnedSlash = Object.Instantiate(
            slashPrefab,
            slashPoint.position,
            rot,
            slashPoint
        );

        ApplyVfxSpeed(spawnedSlash, vfxSpeed);

        if (lifeTime > 0f)
            Object.Destroy(spawnedSlash, lifeTime);

        spawned = true;
    }

    private void ApplyVfxSpeed(GameObject go, float speed)
    {
        if (go == null) return;
        if (speed <= 0f) speed = 1f;

        var particles = go.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in particles)
        {
            var main = ps.main;
            main.simulationSpeed = speed;
        }

        var animators = go.GetComponentsInChildren<Animator>(true);
        foreach (var a in animators)
        {
            a.speed = speed;
        }
    }

    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;

            var result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }
}
