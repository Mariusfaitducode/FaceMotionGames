using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using System.Collections;

public class MusicManager : MonoBehaviour
{
    public AudioSource explorationMusic;
    public AudioSource combatMusic;
    public AudioSource victoryMusic;

    private AudioSource currentMusic;

    void Start()
    {
        PlayMusic(explorationMusic, 2.0f); // Jouer exploration avec un fade-in de 2 secondes
    }

    public void PlayMusic(AudioSource newMusic, float fadeDuration = 1.0f)
    {
        if (currentMusic == newMusic) return; // Évite de rejouer la même musique

        StartCoroutine(FadeOutAndIn(currentMusic, newMusic, fadeDuration));
        currentMusic = newMusic;
    }

    IEnumerator FadeOutAndIn(AudioSource oldMusic, AudioSource newMusic, float duration)
    {
        float timer = 0f;

        // Fade-out de l'ancienne musique
        if (oldMusic != null)
        {
            float startVolume = oldMusic.volume;
            while (timer < duration)
            {
                oldMusic.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
                timer += Time.deltaTime;
                yield return null;
            }
            oldMusic.Stop();
        }

        // Fade-in de la nouvelle musique
        if (newMusic != null)
        {
            newMusic.volume = 0f;
            newMusic.Play();
            timer = 0f;
            while (timer < duration)
            {
                newMusic.volume = Mathf.Lerp(0f, 1f, timer / duration);
                timer += Time.deltaTime;
                yield return null;
            }
        }
    }

    public void TransitionToCombat()
    {
        PlayMusic(combatMusic, 1.5f);
    }

    public void TransitionToExploration()
    {
        PlayMusic(explorationMusic, 1.5f);
    }

    public void PlayVictory()
    {
        PlayMusic(victoryMusic, 1.0f);
    }
}


