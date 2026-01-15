using UnityEngine;
using System.Collections;

public class ApplySavedStats : MonoBehaviour
{
    IEnumerator Start()
    {
        // GameManager đã tự apply trong ApplyAfterSceneLoaded()
        yield return null;

        if (GameManager.Instance != null) ;
            //Debug.Log("[ApplySavedStats] Skip (handled by GameManager.ApplyAfterSceneLoaded)");
    }
}
