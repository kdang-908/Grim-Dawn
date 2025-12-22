using UnityEngine;

public class PlayerDeathSound : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip deathClip;

    bool played = false;

    public void PlayDeathSound()
    {
        if (played) return;
        played = true;

        if (audioSource != null && deathClip != null)
        {
            audioSource.PlayOneShot(deathClip);
        }
    }
}
