using UnityEngine;

public class SpawnDeathVFXOnState : StateMachineBehaviour
{
    [Header("VFX Prefab")]
    public GameObject vfxPrefab;

    [Header("Spawn Time (0..1)")]
    [Range(0f, 1f)]
    public float spawnAtNormalizedTime = 0.2f;

    [Header("Spawn Point (optional)")]
    [Tooltip("Tên Transform để spawn VFX. Để trống = spawn tại animator.transform")]
    public string pointName = "";

    [Header("VFX Settings")]
    public float vfxSpeed = 1f;
    public float lifeTime = 2f;

    [Tooltip("Nếu prefab bị ngược, chỉnh offset (vd: 0,180,0)")]
    public Vector3 rotationOffsetEuler = Vector3.zero;

    [Tooltip("Lấy hướng (Yaw) theo nhân vật")]
    public bool faceForward = true;

    private bool spawned;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        spawned = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (spawned || vfxPrefab == null) return;

        float t = stateInfo.normalizedTime % 1f;   // lấy phần lẻ (đề phòng loop)
        if (t < spawnAtNormalizedTime) return;

        Transform point = animator.transform;

        if (!string.IsNullOrEmpty(pointName))
        {
            var found = FindDeepChild(animator.transform, pointName);
            if (found != null) point = found;
        }

        Quaternion rot = point.rotation;

        if (faceForward)
        {
            var e = rot.eulerAngles;
            e.y = animator.transform.eulerAngles.y;
            rot = Quaternion.Euler(e);
        }

        rot *= Quaternion.Euler(rotationOffsetEuler);

        GameObject vfx = Object.Instantiate(vfxPrefab, point.position, rot);
        ApplyVfxSpeed(vfx, vfxSpeed);

        if (lifeTime > 0f) Object.Destroy(vfx, lifeTime);

        spawned = true;
    }

    private void ApplyVfxSpeed(GameObject go, float speed)
    {
        if (go == null) return;
        if (speed <= 0f) speed = 1f;

        foreach (var ps in go.GetComponentsInChildren<ParticleSystem>(true))
        {
            var main = ps.main;
            main.simulationSpeed = speed;
        }

        foreach (var a in go.GetComponentsInChildren<Animator>(true))
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
