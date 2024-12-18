using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IntroductionManager : MonoBehaviour
{
    [Header("Audio Components")]
    [SerializeField] private AudioSource voiceNova;
    [SerializeField] private AudioSource voiceStella;
    [SerializeField] private AudioSource musicIntro;
    
    [Header("Visual Components")]
    [SerializeField] private NightSkyGenerator nightSky;
    [SerializeField] private SpriteRenderer commandCenterRenderer; // Changé pour SpriteRenderer
    
    [Header("Timing Settings")]
    [SerializeField] private float initialDelay = 2f;
    [SerializeField] private float novaStartDelay = 26f;
    [SerializeField] private float stellaStartDelay = 26f;
    [SerializeField] private float transitionCommandCenterDuration = 16f;
    
    [Header("Transition Settings")]
    [SerializeField] private float zoomDuration = 30f; // 10s Nova + 20s transition
    [SerializeField] private float initialCommandCenterScale = 0.01f;
    [SerializeField] private float finalCommandCenterScale = 1f;
    [SerializeField] private float skyScaleMultiplier = 2f;
    [SerializeField] private Vector3 finalCommandCenterPosition;
    [SerializeField] private AnimationCurve zoomSkyProgressCurve = new AnimationCurve(
        new Keyframe(0, 0, 0, 0),      // Début très lent
        new Keyframe(0.6f, 0.1f, 0.2f, 0.2f),  // Pendant que Nova parle (lent)
        new Keyframe(0.8f, 0.4f, 1f, 1f),      // Accélération après Nova
        new Keyframe(1, 1, 2f, 0)      // Arrivée rapide
    );

    [SerializeField] private AnimationCurve zoomCommandCenterProgressCurve = new AnimationCurve(
        new Keyframe(0, 0, 0, 0),      // Début très lent
        new Keyframe(0.6f, 0.1f, 0.2f, 0.2f),  // Pendant que Nova parle (lent)
        new Keyframe(0.8f, 0.4f, 1f, 1f),      // Accélération après Nova
        new Keyframe(1, 1, 2f, 0)      // Arrivée rapide
    );

    [Header("Final Transition Settings")]
    [SerializeField] private float finalTransitionDuration = 3f;
    [SerializeField] private Vector3 commandCenterFinalOffset = new Vector3(-8f, -2f, 0f);
    [SerializeField] private float commandCenterFinalScale = 0.7f;
    [SerializeField] private Vector3 skyFinalOffset = new Vector3(-2f, 0f, 0f);
    [SerializeField] private AnimationCurve finalTransitionCurve = new AnimationCurve(
        new Keyframe(0, 0, 0, 0),
        new Keyframe(1, 1, 2, 0)
    );

    private float originalMusicVolume;
    private Vector3 initialCommandCenterPosition;
    private Vector3 initialSkyScale;

    private bool isZooming = false;
    private float zoomTimer = 0f;

    private Vector3 commandCenterBaseScale;
    private Vector3 commandCenterBasePosition;

    void Start()
    {
        InitializeScene();
        StartCoroutine(PlayIntroSequence());
    }

    void InitializeScene()
    {
        originalMusicVolume = musicIntro.volume;

        if (nightSky != null)
        {
            nightSky.GenerateSky();
        }
        
        if (commandCenterRenderer != null)
        {
            // Initialiser le centre de commande très petit et loin
            commandCenterRenderer.transform.localScale = Vector3.one * initialCommandCenterScale;
            initialCommandCenterPosition = commandCenterRenderer.transform.position;
            commandCenterRenderer.color = new Color(1, 1, 1, 0); // Invisible au début
        }

        if (nightSky != null)
        {
            initialSkyScale = nightSky.transform.localScale;
        }

        if (commandCenterRenderer != null)
        {
            commandCenterBaseScale = Vector3.one * finalCommandCenterScale;
            commandCenterBasePosition = finalCommandCenterPosition;
        }

        if (nightSky != null)
        {
            nightSky.InitializePositions();
        }
    }

    IEnumerator PlayIntroSequence()
    {
        yield return new WaitForSeconds(initialDelay);

        Debug.Log("Initial delay over");

        if (nightSky != null)
        {
            nightSky.StartTransition();
            Debug.Log("Night sky transition started");
        }

        // Démarrer la musique
        musicIntro.volume = 0f;
        musicIntro.Play();
        yield return StartCoroutine(FadeMusicVolume(0f, originalMusicVolume, 2f));

        Debug.Log("Music intro started");

        // Attendre avant Nova
        yield return new WaitForSeconds(novaStartDelay);
        
        // Démarrer le zoom en même temps que Nova
        
        
        // Nova parle
        yield return StartCoroutine(FadeMusicVolume(originalMusicVolume, originalMusicVolume * 0.5f, 1f));
        voiceNova.Play();
        Debug.Log("Nova started speaking");
        
        yield return new WaitForSeconds(voiceNova.clip.length - 10f);

        nightSky.StopTransition();
        StartCoroutine(ZoomTransition(zoomDuration));
        Debug.Log("Zoom transition started");

        yield return new WaitForSeconds(10f);

        StartCoroutine(FadeMusicVolume(musicIntro.volume, originalMusicVolume, 2f));
        Debug.Log("Nova finished speaking");

        yield return new WaitForSeconds(5f);
        

        // Attendre la fin du zoom
        while (isZooming)
        {
            yield return null;
        }
        
        Debug.Log("Zoom transition completed");
        yield return new WaitForSeconds(5f);

        // Transition vers le centre de commandement
        // yield return StartCoroutine(FadeCommandCenter(0f, 1f, transitionCommandCenterDuration));
        // Debug.Log($"Command center transition sequence finished. Total duration was {transitionCommandCenterDuration} seconds");

        Debug.Log("Command center transition finished");
        
        yield return new WaitForSeconds(5f);


        // Attendre avant Stella
        // yield return new WaitForSeconds(stellaStartDelay);
        
        // Baisser le volume pour Stella
        yield return StartCoroutine(FadeMusicVolume(originalMusicVolume, originalMusicVolume * 0.5f, 1f));
        voiceStella.Play();

        Debug.Log("Stella started speaking");
        
        // Attendre que Stella finisse de parler
        yield return new WaitForSeconds(voiceStella.clip.length - 4f);

        Debug.Log("Stella finished speaking");
        
        // Remettre le volume normal
        yield return StartCoroutine(FadeMusicVolume(musicIntro.volume, originalMusicVolume, 1f));

        // Transition finale
        yield return StartCoroutine(FinalTransition());
    }

    IEnumerator FadeMusicVolume(float startVolume, float targetVolume, float duration)
    {
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            
            musicIntro.volume = Mathf.Lerp(startVolume, targetVolume, normalizedTime);
            yield return null;
        }
        
        musicIntro.volume = targetVolume;
    }

    // IEnumerator FadeCommandCenter(float startAlpha, float targetAlpha, float duration)
    // {
    //     if (commandCenterRenderer == null) yield break;

    //     Debug.Log($"Starting command center transition. Duration: {duration} seconds");
    //     float elapsed = 0;
    //     Color startColor = commandCenterRenderer.color;
        
    //     while (elapsed < duration)
    //     {
    //         elapsed += Time.deltaTime;
    //         float normalizedTime = elapsed / duration;
            
    //         commandCenterRenderer.color = new Color(
    //             startColor.r,
    //             startColor.g,
    //             startColor.b,
    //             Mathf.Lerp(startAlpha, targetAlpha, normalizedTime)
    //         );

    //         // Log pour déboguer la progression
    //         if (elapsed % 1 < Time.deltaTime) // Log chaque seconde environ
    //         {
    //             Debug.Log($"Command center transition progress: {elapsed}/{duration} seconds - Alpha: {commandCenterRenderer.color.a}");
    //         }

    //         yield return null;
    //     }
        
    //     // S'assurer que la valeur finale est exacte
    //     commandCenterRenderer.color = new Color(
    //         startColor.r,
    //         startColor.g,
    //         startColor.b,
    //         targetAlpha
    //     );
    //     Debug.Log("Command center transition completed");
    // }

    IEnumerator ZoomTransition(float duration)
    {
        isZooming = true;
        zoomTimer = 0f;
        
        // Rendre le centre de commande visible mais très petit
        commandCenterRenderer.color = Color.white;

        while (zoomTimer < duration)
        {
            zoomTimer += Time.deltaTime;
            float rawProgress = zoomTimer / duration;
            
            // Utiliser la courbe pour un contrôle précis de la progression
            float skyProgress = zoomSkyProgressCurve.Evaluate(rawProgress);
            float commandCenterProgress = zoomCommandCenterProgressCurve.Evaluate(rawProgress);

            // Agrandir le ciel
            if (nightSky != null)
            {
                nightSky.transform.localScale = Vector3.Lerp(
                    initialSkyScale,
                    initialSkyScale * skyScaleMultiplier,
                    skyProgress
                );
            }

            // Agrandir et déplacer le centre de commande
            commandCenterRenderer.transform.localScale = Vector3.Lerp(
                Vector3.one * initialCommandCenterScale,
                Vector3.one * finalCommandCenterScale,
                commandCenterProgress
            );

            commandCenterRenderer.transform.position = Vector3.Lerp(
                initialCommandCenterPosition,
                finalCommandCenterPosition,
                commandCenterProgress
            );

            yield return null;
        }

        // Valeurs finales
        commandCenterRenderer.transform.localScale = Vector3.one * finalCommandCenterScale;
        commandCenterRenderer.transform.position = finalCommandCenterPosition;
        if (nightSky != null)
        {
            nightSky.transform.localScale = initialSkyScale * skyScaleMultiplier;
        }

        isZooming = false;
    }

    IEnumerator FinalTransition()
    {
        float elapsed = 0;
        Vector3 startScale = commandCenterRenderer.transform.localScale;
        Vector3 startPosition = commandCenterRenderer.transform.position;

        // Démarrer la translation du ciel
        if (nightSky != null)
        {
            nightSky.TranslateSky(skyFinalOffset, finalTransitionDuration, finalTransitionCurve);
        }

        while (elapsed < finalTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / finalTransitionDuration;
            float curvedProgress = finalTransitionCurve.Evaluate(progress);

            // Déplacer et redimensionner le centre de commande
            commandCenterRenderer.transform.localScale = Vector3.Lerp(
                startScale,
                commandCenterBaseScale * commandCenterFinalScale,
                curvedProgress
            );

            commandCenterRenderer.transform.position = Vector3.Lerp(
                startPosition,
                commandCenterBasePosition + commandCenterFinalOffset,
                curvedProgress
            );

            yield return null;
        }

        Debug.Log("Final transition completed");
    }
} 