using UnityEngine;

public class SpawnSlashOnState : StateMachineBehaviour
{
    [Header("Slash VFX")]
    public GameObject slashPrefab;

    [Header("Condition")]
    public bool requireWeapon = true;

    [Header("VFX Settings")]
    public float vfxSpeed = 1.4f;
    public float lifeTime = 0.3f;

    [Range(0f, 1f)]
    public float spawnAtNormalizedTime = 0.35f;

    public string slashPointName = "SlashPoint";
    public bool faceForward = true;
    public Vector3 rotationOffsetEuler = Vector3.zero;
    public bool destroyOnExit = true;

    // =======================
    //        SOUND
    // =======================
    [Header("Sound")]
    public bool playSound = true;
    public AudioClip slashClip;
    [Range(0f, 1f)] public float slashVolume = 0.8f;

    private GameObject spawnedSlash;
    private bool spawned;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        spawnedSlash = null;
        spawned = false;
        Debug.Log("[SpawnSlashOnState] Enter state: " + stateInfo.fullPathHash);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (spawned)
            return;

        if (slashPrefab == null)
        {
            Debug.LogWarning("[SpawnSlashOnState] slashPrefab NULL");
            spawned = true;
            return;
        }

        float t = stateInfo.normalizedTime % 1f;
        if (t < spawnAtNormalizedTime)
            return;

        Debug.Log("[SpawnSlashOnState] Time reached, try spawn");
        TrySpawn(animator);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (destroyOnExit && spawnedSlash != null)
        {
            Debug.Log("[SpawnSlashOnState] Destroy VFX on exit");
            Object.Destroy(spawnedSlash);
        }

        spawnedSlash = null;
        spawned = false;
    }

    private void TrySpawn(Animator animator)
    {
        if (spawned) return;

        // 1. Check weapon (nếu bật)
        if (requireWeapon)
        {
            var equipper = animator.GetComponent<WeaponEquipper>();
            if (equipper == null)
            {
                Debug.LogWarning("[SpawnSlashOnState] Không tìm thấy WeaponEquipper trên object Animator");
                spawned = true;
                return;
            }
            if (!equipper.HasWeapon())
            {
                Debug.LogWarning("[SpawnSlashOnState] HasWeapon() == false, không spawn VFX");
                spawned = true;
                return;
            }
        }

        // 2. Tìm SlashPoint
        Transform slashPoint = FindDeepChild(animator.transform, slashPointName);
        if (slashPoint == null)
        {
            Debug.LogWarning("[SpawnSlashOnState] Không tìm thấy Transform tên '" + slashPointName + "'");
            spawned = true;
            return;
        }

        Quaternion rot = slashPoint.rotation;

        if (faceForward)
        {
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

        Debug.Log("[SpawnSlashOnState] SPAWN thành công: " + slashPrefab.name);

        ApplyVfxSpeed(spawnedSlash, vfxSpeed);

        if (lifeTime > 0f)
            Object.Destroy(spawnedSlash, lifeTime);

        // 🔊 phát âm thanh chém kiếm
        PlaySlashSound(animator);

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

            // CHỖ THÊM QUAN TRỌNG
            ps.Clear();         // reset lại
            ps.Play(true);      // bắt VFX chạy
        }

        var animators = go.GetComponentsInChildren<Animator>(true);
        foreach (var a in animators)
        {
            a.speed = speed;
        }
    }

    private void PlaySlashSound(Animator animator)
    {
        if (!playSound || slashClip == null) return;

        // Tìm AudioSource trên Player
        AudioSource src = animator.GetComponent<AudioSource>();
        if (src == null)
            src = animator.GetComponentInChildren<AudioSource>();

        if (src != null)
        {
            src.PlayOneShot(slashClip, slashVolume);
            //Debug.Log("[SpawnSlashOnState] Play slash sound");
        }
        else
        {
            Debug.LogWarning("[SpawnSlashOnState] Không tìm thấy AudioSource để phát âm thanh slash!");
        }
    }

    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            var result = FindDeepChild(child, name);
            if (result != null)
                return result;
        }
        return null;
    }
}
