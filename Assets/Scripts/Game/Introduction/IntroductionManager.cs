using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IntroductionManager : MonoBehaviour
{
    [Header("Audio Components")]
    [SerializeField] private AudioSource voiceNova;
    [SerializeField] private AudioSource voiceStella;
    [SerializeField] private AudioSource musicIntro;
    // [SerializeField] private AudioSource musicWaitingRoom;

    [Header("Next Scene")]
    [SerializeField] private GameObject waitingRoom;

    [Header("Visual Components")]
    [SerializeField] private NightSkyGenerator nightSky;
    [SerializeField] private SpriteRenderer commandCenterRenderer; // Changé pour SpriteRenderer
    
    [Header("Timing Settings")]
    [SerializeField] private float initialDelay = 2f;
    [SerializeField] private float novaStartDelay = 26f;
    // [SerializeField] private float stellaStartDelay = 26f;
    // [SerializeField] private float transitionCommandCenterDuration = 16f;
    
    [Header("Zoom Transition Settings")]
    [SerializeField] private float zoomDuration = 30f; // 10s Nova + 20s transition
    [SerializeField] private float initialCommandCenterScale = 0.0001f;
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

    [Header("Star Transition Settings")]
    [SerializeField] private float starTransitionDuration = 70f;
    [SerializeField] private Color starEndColor = new Color(0.01f, 0.01f, 0.02f, 1f);
    
    [SerializeField] private AnimationCurve starDistributionCurve = new AnimationCurve(
        new Keyframe(0, 0, 2, 2),       // Départ rapide
        new Keyframe(0.3f, 0.7f),       // Beaucoup d'étoiles au début
        new Keyframe(0.7f, 0.9f),       // Ralentissement
        new Keyframe(1, 1, 0.2f, 0)     // Fin progressive
    );
    [SerializeField] private float starFadeOutDuration = 3f;
    [SerializeField] private float starRedshiftIntensity = 2f;
    [SerializeField] private float starFlickerSpeed = 5f;
    [SerializeField] private float starFlickerIntensity = 0.2f;

    private float originalMusicVolume;
    private Vector3 initialCommandCenterPosition;
    private Vector3 initialSkyScale;

    private bool isZooming = false;
    private float zoomTimer = 0f;

    private Vector3 commandCenterBaseScale;
    private Vector3 commandCenterBasePosition;

    private bool isSkipping = false;
    private Coroutine introSequenceCoroutine;

    void Start()
    {
        InitializeScene();
        introSequenceCoroutine = StartCoroutine(PlayIntroSequence());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isSkipping)
        {
            StartCoroutine(SkipIntroduction());
        }
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

    void StartNightSkyTransition()
    {
        if (nightSky != null)
        {
            var settings = new NightSkyGenerator.StarTransitionSettings
            {
                duration = starTransitionDuration,
                endColor = starEndColor,
                distributionCurve = starDistributionCurve,
                starFadeOutDuration = starFadeOutDuration,
                redshiftIntensity = starRedshiftIntensity,
                flickerSpeed = starFlickerSpeed,
                flickerIntensity = starFlickerIntensity
            };

            nightSky.StartTransition(settings);
        }
    }

    IEnumerator PlayIntroSequence()
    {
        yield return new WaitForSeconds(initialDelay);

        Debug.Log("Initial delay over");

        if (nightSky != null)
        {
            StartNightSkyTransition();
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
        while (isZooming && !isSkipping)
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
        yield return StartCoroutine(FinalTransition(finalTransitionDuration));


        // Vérifier si la musique a dépassé 3 minutes
        if (musicIntro != null && musicIntro.time >= 180f)
        {
            musicIntro.Stop();
            Debug.Log("Music stopped after 3 minutes");
        }

        LoadWaitingRoom();
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

    IEnumerator FinalTransition(float duration)
    {
        float elapsed = 0;
        Vector3 startScale = commandCenterRenderer.transform.localScale;
        Vector3 startPosition = commandCenterRenderer.transform.position;

        // Démarrer la translation du ciel
        if (nightSky != null)
        {
            nightSky.TranslateSky(skyFinalOffset, duration, finalTransitionCurve);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
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

    IEnumerator SkipIntroduction()
    {
        isSkipping = true;

        // Arrêter la séquence en cours
        if (introSequenceCoroutine != null)
        {
            StopCoroutine(introSequenceCoroutine);
        }

        // Arrêter tous les sons
        if (voiceNova != null) voiceNova.Stop();
        if (voiceStella != null) voiceStella.Stop();
        if (musicIntro != null) musicIntro.volume = originalMusicVolume;

        // Arrêter la transition des étoiles en cours
        if (nightSky != null)
        {
            nightSky.StopTransition();
        }

        // Appliquer directement l'état final des étoiles
        var finalStarSettings = new NightSkyGenerator.StarTransitionSettings
        {
            duration = 1f, // Transition rapide
            endColor = starEndColor,
            distributionCurve = starDistributionCurve,
            starFadeOutDuration = 1f,
            redshiftIntensity = starRedshiftIntensity,
            flickerSpeed = starFlickerSpeed,
            flickerIntensity = starFlickerIntensity
        };
        nightSky.StartTransition(finalStarSettings);
        

        // Attendre que la transition rapide des étoiles soit terminée
        yield return new WaitForSeconds(1f);


        // Placer directement le centre de commande à sa position finale
        if (commandCenterRenderer != null)
        {
            commandCenterRenderer.color = Color.white;
            // commandCenterRenderer.transform.localScale = Vector3.one * finalCommandCenterScale;
            // commandCenterRenderer.transform.position = finalCommandCenterPosition;
        }


        StartCoroutine(ZoomTransition(2f));

        yield return new WaitForSeconds(2f);



        // Transition finale
        yield return StartCoroutine(FinalTransition(3f));

        Debug.Log("Introduction skipped");
        isSkipping = false;

        if (musicIntro != null)
        {
            yield return StartCoroutine(FadeMusicVolume(musicIntro.volume, 0f, 3f));
            Debug.Log("Music stopped after 3 minutes");
        }

        LoadWaitingRoom();
    }


    void LoadWaitingRoom(){
        waitingRoom.SetActive(true);
    }
} 