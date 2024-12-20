using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoTilesGame : MonoBehaviour
{

     [Header("Audio Components")]
    // [SerializeField] private AudioSource musicJetpackGame;

    [Header("Previous Scene")]
    [SerializeField] private GameObject jetpackGame;

    // [Header("Next Scene")]
    // [SerializeField] private GameObject endGame;

    [Header("Visual Components")]
    // [SerializeField] private NightSkyGenerator nightSky;
    // [SerializeField] private SpriteRenderer commandCenterRenderer;

    // private ScrollingBackground scrollingBackground;



    [Header("Game Components")]
    [SerializeField] private GameObject gameCanvas;

    private PlayerManager playerManager;

    private LaneManager laneManager;

    private int playerCount = 0;


    private int difficultyLevel = 1;

    private float playerInitialXPosition = -6f;




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
        playerManager = FindObjectOfType<PlayerManager>();
        laneManager = FindObjectOfType<LaneManager>();

        playerCount = playerManager.GetPlayerCount();


        List<Vector3> lanesPositions = laneManager.InitializeLaneManager(4);

        // Modify x position of each lane
        List<Vector3> playerPositions = GetPlayerPositions(lanesPositions);

        StartCoroutine(InitialTransition(playerPositions));

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

        Debug.Log("Players transition to positions");
        yield return playerManager.PlayersTransitionToPositions(playerPositions, initialTransitionCurve, 2f);

        Debug.Log("Players transition finished ?");

        // Start the piano tiles game
        playerManager.PlayersStartPianoTilesGame();

        laneManager.isGameStarted = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
