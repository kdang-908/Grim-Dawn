// ===============================
// 3) MinimapArrowRotate.cs ✅ FULL
// ===============================
using System.Collections;
using UnityEngine;

public class MinimapArrowRotate : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    void OnEnable()
    {
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
                Debug.Log("[MinimapArrow] Auto bind -> " + go.name);
                yield break;
            }
            yield return null;
        }
    }

    void Update()
    {
        if (target == null) return;

        // minimap arrow xoay theo hướng nhìn của player (trục Y)
        transform.localRotation = Quaternion.Euler(0, 0, -target.eulerAngles.y);
    }
}
