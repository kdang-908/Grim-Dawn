using UnityEngine;
using System.Collections;

public class PlayerSpawnFX : MonoBehaviour
{
    [Header("VFX")]
    public GameObject spawnVFXPrefab;   // prefab hiệu ứng spawn
    public float vfxLifeTime = 2f;      // tự huỷ sau X giây

    [Header("SFX")]
    public AudioClip spawnSFX;          // âm thanh spawn
    public AudioSource audioSource;     // nếu để trống sẽ mượn AudioSource ở Main Camera

    [Header("Timing")]
    [Tooltip("Delay nhỏ để âm thanh khớp đúng đoạn cao trào")]
    public float sfxDelay = 0f;      // 0.1–0.2s = đẹp nhất

    public void PlaySpawnFX()
    {
        // phát nhạc trước
        if (spawnSFX != null)
            StartCoroutine(PlaySpawnSFX());

        // spawn VFX ngay sau đó
        if (spawnVFXPrefab != null)
        {
            var fx = Instantiate(spawnVFXPrefab, transform.position, transform.rotation);
            Destroy(fx, vfxLifeTime);
        }
    }


    IEnumerator PlaySpawnSFX()
    {
        // Delay để khớp cao trào audio
        if (sfxDelay > 0f)
            yield return new WaitForSeconds(sfxDelay);

        // nếu chưa gán AudioSource → tự tìm Camera
        if (audioSource == null)
        {
            var cam = Camera.main;
            if (cam != null)
                audioSource = cam.GetComponent<AudioSource>();
        }

        if (audioSource != null)
            audioSource.PlayOneShot(spawnSFX);
        else
            Debug.LogWarning("[PlayerSpawnFX] Không tìm thấy AudioSource để phát SFX");
    }
}
