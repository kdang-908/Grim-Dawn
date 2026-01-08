using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

public class ReturnToSelectChapter : MonoBehaviour
{
    [Header("Names must match exactly")]
    public string selectChapterSceneName = "SelectChapter";
    public string baseSceneName = "Map";
    public KeyCode key = KeyCode.M;

    void Update()
    {
        if (Input.GetKeyDown(key))
            Toggle();
    }

    void Toggle()
    {
        var sc = SceneManager.GetSceneByName(selectChapterSceneName);

        if (sc.IsValid() && sc.isLoaded)
            StartCoroutine(CloseRoutine());
        else
            StartCoroutine(OpenRoutine());
    }

    IEnumerator OpenRoutine()
    {
        var op = SceneManager.LoadSceneAsync(selectChapterSceneName, LoadSceneMode.Additive);
        while (!op.isDone) yield return null;

        var sc = SceneManager.GetSceneByName(selectChapterSceneName);

        // ✅ AUTO FIX: tắt mọi Camera/AudioListener trong SelectChapter để không cướp màn hình
        foreach (var root in sc.GetRootGameObjects())
        {
            foreach (var cam in root.GetComponentsInChildren<Camera>(true))
                cam.enabled = false;

            foreach (var al in root.GetComponentsInChildren<AudioListener>(true))
                al.enabled = false;
        }

        // ✅ AUTO FIX: nếu Map đã có EventSystem thì tắt EventSystem của SelectChapter (tránh double)
        if (EventSystem.current != null)
        {
            foreach (var root in sc.GetRootGameObjects())
            {
                foreach (var es in root.GetComponentsInChildren<EventSystem>(true))
                    es.gameObject.SetActive(false);
            }
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[SelectChapter] OPEN");
    }

    IEnumerator CloseRoutine()
    {
        // luôn set Active về Map trước khi unload
        var baseScene = SceneManager.GetSceneByName(baseSceneName);
        if (baseScene.IsValid() && baseScene.isLoaded)
            SceneManager.SetActiveScene(baseScene);

        var op = SceneManager.UnloadSceneAsync(selectChapterSceneName);
        while (op != null && !op.isDone) yield return null;

        // nếu game bạn cần lock chuột thì để Locked, không thì đổi sang None
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("[SelectChapter] CLOSE");
    }
}
