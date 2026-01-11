// ===============================
// 2) MinimapCameraFollow.cs ✅ FULL
// ===============================
using System.Collections;
using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow")]
    public float height = 30f;
    public float followSpeed = 20f;

    void OnEnable()
    {
        // Nếu chưa có target thì auto find
        if (target == null) StartCoroutine(AutoFindPlayer());
    }

    // ✅ để Binder/Spawner gọi
    public void SetTarget(Transform t)
    {
        target = t;
    }

    IEnumerator AutoFindPlayer()
    {
        while (target == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null)
            {
                target = go.transform;
                Debug.Log("[MinimapCamera] Auto bind -> " + go.name);
                yield break;
            }
            yield return null;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 pos = target.position;
        pos.y = height;

        transform.position = Vector3.Lerp(transform.position, pos, followSpeed * Time.deltaTime);
    }
}
