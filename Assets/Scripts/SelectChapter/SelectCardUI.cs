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
        var gm = GameManager.Instance;

        // ✅ CHECK LOCK TRƯỚC KHI LOAD
        bool unlocked = (gm == null) ? (mapIndex == 0) : gm.IsMapUnlocked(mapIndex);
        if (!unlocked)
        {
            // rung card nếu bị khóa
            if (!isShaking)
            {
                if (shakeCo != null) StopCoroutine(shakeCo);
                shakeCo = StartCoroutine(ShakeOnce());
            }
            return; // ❌ KHÔNG CHUYỂN MAP
        }

        // ✅ Save (chỉ khi unlocked)
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && gm != null)
        {
            var stats = player.GetComponent<CharacterStats>() ?? player.GetComponentInChildren<CharacterStats>(true);
            if (stats != null) gm.SavePlayer(stats);

            gm.SavePotions();
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
