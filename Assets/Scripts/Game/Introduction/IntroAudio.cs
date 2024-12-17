using System.Collections;
using UnityEngine;

public class IntroAudio : MonoBehaviour
{
    public AudioSource voiceNova;

    
    public int voiceNovaDelay = 26;
    public AudioSource voiceStella;

    public int voiceStellaDelay = 16;
    public AudioSource musicIntro;

    void Start()
    {
        StartCoroutine(PlayIntroSequence());
    }

    IEnumerator PlayIntroSequence()
    {
        // Jouer la musique douce d'introduction
        musicIntro.Play();
        float originalVolume = musicIntro.volume;

        // Attendre avant la voix de Nova
        yield return new WaitForSeconds(voiceNovaDelay);
        
        // Baisser le volume pendant que Nova parle
        musicIntro.volume = originalVolume * 0.5f;
        voiceNova.Play();
        
        // Attendre que la voix de Nova soit terminée
        yield return new WaitForSeconds(voiceNova.clip.length);
        
        // Remettre le volume normal entre les deux voix
        musicIntro.volume = originalVolume;
        
        yield return new WaitForSeconds(voiceStellaDelay);
        
        // Baisser le volume pendant que Stella parle
        musicIntro.volume = originalVolume * 0.5f;
        voiceStella.Play();
        
        // Remettre le volume normal après que Stella ait fini de parler
        yield return new WaitForSeconds(voiceStella.clip.length);
        musicIntro.volume = originalVolume;
    }
}

