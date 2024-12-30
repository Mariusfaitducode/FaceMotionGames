using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoTilesGame : MonoBehaviour
{

     [Header("Audio Components")]
    [SerializeField] private AudioSource musicPianoTilesGame;

    [Header("Previous Scene")]
    [SerializeField] private GameObject jetpackGame;

    // [Header("Next Scene")]
    // [SerializeField] private GameObject endGame;

    [Header("Visual Components")]
    private NightSkyGenerator nightSky;
    // [SerializeField] private SpriteRenderer commandCenterRenderer;

    private ScrollingBackground scrollingBackground;



    [Header("Game Components")]
    [SerializeField] private GameObject gameCanvas;

    private PlayerManager playerManager;

    private LaneManager laneManager;
    private TileSpawner tileSpawner;

    private int playerCount = 0;


    private int difficultyLevel = 1;

    private float playerInitialXPosition = -5f;




    [Header("Transition Settings")]

    [SerializeField] private AnimationCurve initialTransitionCurve = new AnimationCurve(
        new Keyframe(0, 0, 0, 0),
        new Keyframe(1, 1, 2, 0)
    );

    // [SerializeField] private AnimationCurve initialTransitionCurve = new AnimationCurve(
    //     new Keyframe(0, 0, 0, 0),
    //     new Keyframe(1, 1, 2, 0)
    // );



    private Vector3 commandCenterFinalOffset = new Vector3(-6, 0, 0);



    // Start is called before the first frame update
    void Start()
    {
        jetpackGame.SetActive(false);
        playerManager = FindObjectOfType<PlayerManager>();
        laneManager = FindObjectOfType<LaneManager>();
        tileSpawner = FindObjectOfType<TileSpawner>();

        scrollingBackground = FindObjectOfType<ScrollingBackground>();
        nightSky = FindObjectOfType<NightSkyGenerator>();

        playerCount = playerManager.GetPlayerCount();

        List<Vector3> lanesPositions = laneManager.InitializeLaneManager(playerCount);
        tileSpawner.InitializeTileSpawner(lanesPositions);

        // Modify x position of each lane
        List<Vector3> playerPositions = GetPlayerPositions(lanesPositions);

        StartCoroutine(InitialTransition(playerPositions));

        StartCoroutine(IncreaseDifficultyCoroutine());

    }


    public List<Vector3> GetPlayerPositions(List<Vector3> lanesPositions){
        List<Vector3> playerPositions = new List<Vector3>();
        foreach (var lane in lanesPositions)
        {
            playerPositions.Add(new Vector3(playerInitialXPosition, lane.y, lane.z));
        }
        return playerPositions;
    }


    IEnumerator InitialTransition(List<Vector3> playerPositions){


        yield return new WaitForSeconds(1f);

        scrollingBackground.StartScrolling(1f, new Vector3(-1, 0, 0), nightSky.gameObject);

        Debug.Log("Players transition to positions");
        yield return playerManager.PlayersTransitionToPositions(playerPositions, initialTransitionCurve, 2f);

        Debug.Log("Players transition finished ?");

        musicPianoTilesGame.Play();
        StartCoroutine(MusicUtils.FadeMusicVolume(musicPianoTilesGame, 0f, 1f, 2f));

        // Start the piano tiles game
        playerManager.PlayersStartPianoTilesGame();

        tileSpawner.isGameStarted = true;
    }


    IEnumerator IncreaseDifficultyCoroutine(){

        InitializeDifficulty();
        yield return new WaitForSeconds(44f);
        // scrollingBackground.UpdateScrollingSpeed(3f);
        IncreaseDifficulty();

        yield return new WaitForSeconds(40f);
        // scrollingBackground.UpdateScrollingSpeed(6f);
        IncreaseDifficulty();

        yield return new WaitForSeconds(40f);
        // scrollingBackground.UpdateScrollingSpeed(10f);
        IncreaseDifficulty();

        
    }

    private void InitializeDifficulty(){
        difficultyLevel = 1;
        // planetSpawner.spawnDelay = 3f;
        // planetSpawner.planetSpeed = 4f;
        tileSpawner.tileSpeed = 5f;
    }

    public void IncreaseDifficulty(){
        difficultyLevel++;

        Debug.Log("Difficulty increased to " + difficultyLevel);

        StartCoroutine(tileSpawner.PauseSpawner(4f));
        
        switch(difficultyLevel){
            case 1:
                tileSpawner.tileSpeed = 6f;
                // meteorSpawner.IncreaseDifficulty();
                break;
            case 2:
                tileSpawner.tileSpeed = 8f;
                // meteorSpawner.IncreaseDifficulty();
                break;
            case 3:
                tileSpawner.tileSpeed = 10f;
                // meteorSpawner.IncreaseDifficulty();
                break;
            case 4:
                tileSpawner.tileSpeed = 13f;
                // meteorSpawner.IncreaseDifficulty();
                break;
        }
    }
}
