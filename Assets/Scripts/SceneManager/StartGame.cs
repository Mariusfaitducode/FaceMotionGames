using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;


public class StartGame : MonoBehaviour
{

    [Header("Audio Components")]
    [SerializeField] private AudioSource musicWaitingRoom;

    [Header("Next Scene")]
    [SerializeField] private GameObject jetpackGame;

    [Header("Visual Components")]
    [SerializeField] private GameObject waitingRoomTexts;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI playerCountText;

    [Header("Player Components")]

    [SerializeField] public List<AnimationClip> playersAnimations = new List<AnimationClip>();

    [Header("Parameters")]
    public bool startGame = false;

    // public List<Transform> playerTargetPositions;


    private Dictionary<int, float> mouthOpenTimers = new Dictionary<int, float>();
    // 5 secondes requises pour démarrer le jeu en ouvrant la bouche
    private float requiredTimeToStartGame = 5f; 

    // Demande d'une photo avatar au bout de 2.5 secondes
    private float requiredTimeToRequestSnapshot = 2.5f; 
    private bool isCountingDown = false;
    private int playerCount = 0;

    private PlayerManager playerManager;

    public void Start(){
        playerManager = FindObjectOfType<PlayerManager>();

        if (startGame){
            waitingRoomTexts.SetActive(false);
            // planetSpawner.SetActive(true);
            // meteoriteSpawner.SetActive(true);
            jetpackGame.SetActive(true);
        }
        else{
            waitingRoomTexts.SetActive(true);

            musicWaitingRoom.Play();
            // planetSpawner.SetActive(false);
            // meteoriteSpawner.SetActive(false);
            jetpackGame.SetActive(false);
        }

        // Debug
        // for (int i = 0; i < 4; i++){
        //     playerManager.PlayPlayerAnimation(i, playersAnimations[i]);
        // }
    }


    public void SetMouthsOpen(int id, bool isOpen, int playerCount){

        this.playerCount = playerCount;
        playerCountText.text = $"{playerCount} player detected";

        // Mettre à jour le timer pour ce joueur
        if (isOpen)
        {
            if (!mouthOpenTimers.ContainsKey(id))
            {
                mouthOpenTimers[id] = 0f;

                if (startGame == false){
                    playerManager.PlayPlayerAnimation(id, playersAnimations[id]);
                }

                // Debug.Log("Playing animation, player count " + playerCount);
            }
        }
        else
        {
            mouthOpenTimers.Remove(id);
            isCountingDown = false;

            if (startGame == false){
                playerManager.StopPlayerAnimation(id);
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {

        countdownText.text = $"Keep all mouths open 5 seconds to start";

        if (startGame || playerCount == 0) return;

        // Vérifier si tous les joueurs ont la bouche ouverte
        bool allMouthsOpen = playerCount == mouthOpenTimers.Count;

        if (allMouthsOpen)
        {
            if (!isCountingDown)
            {
                isCountingDown = true;
                Debug.Log("Tous les joueurs ont la bouche ouverte ! Début du compte à rebours...");
            }

            // Incrémenter tous les timers
            foreach (int id in mouthOpenTimers.Keys.ToList())
            {
                mouthOpenTimers[id] += Time.deltaTime;

                if (mouthOpenTimers[id] >= requiredTimeToRequestSnapshot)
                {
                    playerManager.RequestSnapshot(id);
                }
            }

            // Vérifier si tous les timers ont atteint le temps requis
            bool allTimersComplete = mouthOpenTimers.Values.All(timer => timer >= requiredTimeToStartGame);



            if (allTimersComplete)
            {
                startGame = true;
                Debug.Log("Le jeu commence !");

                playerManager.ShowPlayersAvatar();


                waitingRoomTexts.SetActive(false);
                // planetSpawner.SetActive(true);
                // meteoriteSpawner.SetActive(true);
                jetpackGame.SetActive(true);

                playerManager.PlayersStartJetpackGame();
            }
            else
            {
                // Afficher le temps restant (prendre le timer le plus bas)
                float lowestTimer = mouthOpenTimers.Values.Min();
                float remainingTime = requiredTimeToStartGame - lowestTimer;
                // Debug.Log($"Temps restant : {remainingTime:F1} secondes");

                countdownText.text = $"Keep all mouths open {remainingTime:F1} seconds to start";
            }
        }
        else if (isCountingDown)
        {
            isCountingDown = false;
            // Réinitialiser tous les timers
            mouthOpenTimers.Clear();
            Debug.Log("Compte à rebours interrompu !");

            
        }
    }
}

