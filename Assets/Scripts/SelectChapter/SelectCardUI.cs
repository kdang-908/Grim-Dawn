using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SelectCardUI : MonoBehaviour
{
    [Header("Shake when locked")]
    public RectTransform cardToShake;     // kéo RectTransform của chính card vào
    public float shakeDuration = 0.12f;
    public float shakeStrength = 10f;

    private Coroutine shakeCo;
    private bool isShaking = false;

    public void SelectMap(int mapIndex)
    {
        // 🔒 Nếu chưa mở thì rung và không cho load
        if (GameManager.Instance != null && !GameManager.Instance.IsMapUnlocked(mapIndex))
        {
            Debug.Log($"[SelectCardUI] Map {mapIndex} is LOCKED!");

            if (!isShaking)
            {
                if (shakeCo != null) StopCoroutine(shakeCo);
                shakeCo = StartCoroutine(ShakeOnce());
            }
            return;
        }

        PlayerPrefs.SetInt("SelectedMap", mapIndex);

        switch (mapIndex)
        {
            case 0: SceneManager.LoadScene("Map"); break;
            case 1: SceneManager.LoadScene("SceneMap2"); break;
            case 2: SceneManager.LoadScene("SceneMap3"); break;
        }
    }

    IEnumerator ShakeOnce()
    {
        if (cardToShake == null) yield break;

        isShaking = true;

        Vector2 start = cardToShake.anchoredPosition;
        float t = 0f;

        while (t < shakeDuration)
        {
            t += Time.unscaledDeltaTime;
            float x = Random.Range(-1f, 1f) * shakeStrength;
            cardToShake.anchoredPosition = start + new Vector2(x, 0f);
            yield return null;
        }

        cardToShake.anchoredPosition = start;
        isShaking = false;
    }
}
