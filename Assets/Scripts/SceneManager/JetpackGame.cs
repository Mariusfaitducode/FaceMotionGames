using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetpackGame : MonoBehaviour
{
    [Header("Audio Components")]
    [SerializeField] private AudioSource musicJetpackGame;

    [Header("Previous Scene")]
    [SerializeField] private GameObject waitingRoom;

    // [Header("Next Scene")]
    // [SerializeField] private GameObject endGame;

    [Header("Visual Components")]
    [SerializeField] private NightSkyGenerator nightSky;
    [SerializeField] private SpriteRenderer commandCenterRenderer;



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

        playerManager = FindObjectOfType<PlayerManager>();

        planetSpawner = FindObjectOfType<PlanetSpawner>();
        meteorSpawner = FindObjectOfType<MeteoriteSpawner>();

        planetSpawner.gameObject.SetActive(false);
        meteorSpawner.gameObject.SetActive(false);

        StartCoroutine(InitialTransition());

        StartCoroutine(IncreaseDifficultyCoroutine());
    }


    IEnumerator InitialTransition(){


        StartCoroutine(CommandCenterTransition(4f));

        nightSky.StartSkyTranslation(new Vector3(-1, 0, 0), 0.1f);

        StartCoroutine(playerManager.PlayersSimpleTransition(commandCenterFinalOffset, initialTransitionCurve, 4f));

        yield return new WaitForSeconds(4f);

        musicJetpackGame.Play();
        StartCoroutine(MusicUtils.FadeMusicVolume(musicJetpackGame, 0f, 1f, 2f));

        yield return new WaitForSeconds(1f);
        
        playerManager.PlayersStartJetpackGame();

        planetSpawner.gameObject.SetActive(true);
        meteorSpawner.gameObject.SetActive(true);
    }

    IEnumerator IncreaseDifficultyCoroutine(){

        

        InitializeDifficulty();
        yield return new WaitForSeconds(44f);
        IncreaseDifficulty();

        yield return new WaitForSeconds(40f);
        IncreaseDifficulty();

        yield return new WaitForSeconds(40f);
        IncreaseDifficulty();

        
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
                planetSpawner.spawnDelay = 1.3f;
                planetSpawner.planetSpeed = 6.5f;
                // meteorSpawner.IncreaseDifficulty();
                break;
            case 4:
                planetSpawner.spawnDelay = 0.8f;
                planetSpawner.planetSpeed = 7f;
                // meteorSpawner.IncreaseDifficulty();
                break;
        }
    }
}
