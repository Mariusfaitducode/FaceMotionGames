using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MusicUtils
{
    public static IEnumerator FadeMusicVolume(AudioSource music, float startVolume, float targetVolume, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            music.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }
    }
}

