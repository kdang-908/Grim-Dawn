using UnityEngine;
using System.Collections;

public class ApplySavedStats : MonoBehaviour
{
    IEnumerator Start()
    {
        // Đợi 1 frame cho CharacterStats.Start() chạy xong
        yield return null;

        var gm = GameManager.Instance;
        if (gm == null) yield break;

        var stats = GetComponent<CharacterStats>();
        if (stats != null)
        {
            gm.LoadPlayer(stats);
        }
    }
}
