using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetpackGame : MonoBehaviour
{
    [Header("Audio Components")]
    [SerializeField] private AudioSource musicJetpackGame;
    [SerializeField] private AudioSource explosionSound;

    [Header("Previous Scene")]
    [SerializeField] private GameObject waitingRoom;

    [Header("Next Scene")]
    [SerializeField] private GameObject pianoTilesGame;

    [Header("Visual Components")]
    [SerializeField] private NightSkyGenerator nightSky;
    [SerializeField] private SpriteRenderer commandCenterRenderer;

    private ScrollingBackground scrollingBackground;



    [Header("Game Components")]
    [SerializeField] private GameObject gameCanvas;

    private PlanetSpawner planetSpawner;
    private MeteoriteSpawner meteorSpawner;

    private int difficultyLevel = 1;


    [Header("Transition Settings")]

    [SerializeField] private AnimationCurve initialTransitionCurve = new AnimationCurve(
        new Keyframe(0, 0, 0, 0),
        new Keyframe(1, 1, 2, 0)
    );


    private PlayerManager playerManager;

    private Vector3 commandCenterFinalOffset = new Vector3(-6, 0, 0);

    void Start()
    {
        waitingRoom.SetActive(false);
        playerManager = FindObjectOfType<PlayerManager>();

        planetSpawner = FindObjectOfType<PlanetSpawner>();
        meteorSpawner = FindObjectOfType<MeteoriteSpawner>();

        scrollingBackground = FindObjectOfType<ScrollingBackground>();

        planetSpawner.gameObject.SetActive(false);
        meteorSpawner.gameObject.SetActive(false);

        StartCoroutine(InitialTransition());

        StartCoroutine(IncreaseDifficultyCoroutine());
    }


    IEnumerator InitialTransition(){

        // Initial transition
        StartCoroutine(CommandCenterTransition(4f));

        // nightSky.StartSkyTranslation(new Vector3(-1, 0, 0), 10f);
        scrollingBackground.StartScrolling(1f, new Vector3(-1, 0, 0), nightSky.gameObject);

        StartCoroutine(playerManager.PlayersSimpleTransition(commandCenterFinalOffset * 0.6f, initialTransitionCurve, 4f));

        yield return new WaitForSeconds(4f);

        musicJetpackGame.Play();
        StartCoroutine(MusicUtils.FadeMusicVolume(musicJetpackGame, 0f, 1f, 2f));

        yield return new WaitForSeconds(1f);


        // Start the jetpack game
        playerManager.PlayersStartJetpackGame(explosionSound);

        planetSpawner.gameObject.SetActive(true);
        meteorSpawner.gameObject.SetActive(true);
    }

    IEnumerator IncreaseDifficultyCoroutine(){

        InitializeDifficulty();

        // yield return new WaitForSeconds(20f);
        // StartCoroutine(PianoTilesGameTransition());


        yield return new WaitForSeconds(44f);
        scrollingBackground.UpdateScrollingSpeed(3f);
        IncreaseDifficulty();

        yield return new WaitForSeconds(40f);
        scrollingBackground.UpdateScrollingSpeed(6f);
        IncreaseDifficulty();

        yield return new WaitForSeconds(40f);
        scrollingBackground.UpdateScrollingSpeed(10f);
        IncreaseDifficulty();

        yield return new WaitForSeconds(40f);
        StartCoroutine(PianoTilesGameTransition());

    }


    // Command Center Transition
    IEnumerator CommandCenterTransition(float duration)
    {
        float elapsed = 0;
        // Vector3 startScale = commandCenterRenderer.transform.localScale;
        Vector3 startPosition = commandCenterRenderer.transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            commandCenterRenderer.transform.position = Vector3.Lerp(
                startPosition,
                startPosition + commandCenterFinalOffset,
                initialTransitionCurve.Evaluate(progress)
            );

            yield return null;
        }

        Debug.Log("Initial transition completed");
    }

    private void InitializeDifficulty(){
        difficultyLevel = 1;
        planetSpawner.spawnDelay = 3f;
        planetSpawner.planetSpeed = 4f;
    }

    public void IncreaseDifficulty(){
        difficultyLevel++;

        Debug.Log("Difficulty increased to " + difficultyLevel);
        
        switch(difficultyLevel){
            case 1:
                planetSpawner.spawnDelay = 2.5f;
                planetSpawner.planetSpeed = 5f;
                // meteorSpawner.IncreaseDifficulty();
                break;
            case 2:
                planetSpawner.spawnDelay = 1.8f;
                planetSpawner.planetSpeed = 6f;
                // meteorSpawner.IncreaseDifficulty();
                break;
            case 3:
                planetSpawner.spawnDelay = 1.5f;
                planetSpawner.planetSpeed = 7f;
                // meteorSpawner.IncreaseDifficulty();
                break;
            case 4:
                planetSpawner.spawnDelay = 1.2f;
                planetSpawner.planetSpeed = 8f;
                // meteorSpawner.IncreaseDifficulty();
                break;
        }
    }


    private IEnumerator PianoTilesGameTransition()
    {
        // Fade out de la musique
        StartCoroutine(MusicUtils.FadeMusicVolume(musicJetpackGame, 0.5f, 0f, 2f));


        scrollingBackground.StopScrolling();

        // Fade out des textes
        // StartCoroutine(TextUtils.FadeTextToTransparent(countdownText, 2f));
        // StartCoroutine(TextUtils.FadeTextToTransparent(playerCountText, 2f));

        // Attendre que les fades soient terminés
        yield return new WaitForSeconds(2f);

        // Activer le jeu
        pianoTilesGame.SetActive(true);
        
    }
}
