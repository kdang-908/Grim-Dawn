using UnityEngine;
using UnityEngine.EventSystems;

public class SingletonSceneObjectsKiller : MonoBehaviour
{
    void Awake()
    {
        // Giữ 1 EventSystem
        var es = FindObjectsOfType<EventSystem>(true);
        for (int i = 1; i < es.Length; i++) Destroy(es[i].gameObject);

        // Giữ 1 AudioListener
        var al = FindObjectsOfType<AudioListener>(true);
        for (int i = 1; i < al.Length; i++) Destroy(al[i]);

        // Giữ 1 MainCamera (tag MainCamera)
        var cams = FindObjectsOfType<Camera>(true);
        bool kept = false;
        foreach (var c in cams)
        {
            if (c.CompareTag("MainCamera"))
            {
                if (!kept) kept = true;
                else Destroy(c.gameObject);
            }
        }
    }
}
