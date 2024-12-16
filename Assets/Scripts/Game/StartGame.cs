using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;


public class StartGame : MonoBehaviour
{

    public bool startGame = false;
    private Dictionary<int, float> mouthOpenTimers = new Dictionary<int, float>();
    private float requiredTimeAllOpen = 5f; // 5 secondes requises

    private float requiredTimeToRequestSnapshot = 2.5f; // 5 secondes requises
    private bool isCountingDown = false;

    [SerializeField] private GameObject planetSpawner;
    [SerializeField] private GameObject meteoriteSpawner;

    // [SerializeField] private GameObject scrollingBackground;


    [SerializeField] private GameObject waitingRoom;


    [SerializeField] private TextMeshProUGUI countdownText;

    [SerializeField] private TextMeshProUGUI playerCountText;


    // [SerializeField] private GameObject planetSpawner;


    private int playerCount = 0;

    private PlayerManager playerManager;

    public void Start(){
        playerManager = FindObjectOfType<PlayerManager>();

        if (startGame){
            waitingRoom.SetActive(false);
            planetSpawner.SetActive(true);
            meteoriteSpawner.SetActive(true);
        }
        else{
            waitingRoom.SetActive(true);
            planetSpawner.SetActive(false);
            meteoriteSpawner.SetActive(false);
        }
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
            }
        }
        else
        {
            mouthOpenTimers.Remove(id);
            isCountingDown = false;
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
            bool allTimersComplete = mouthOpenTimers.Values.All(timer => timer >= requiredTimeAllOpen);



            if (allTimersComplete)
            {
                startGame = true;
                Debug.Log("Le jeu commence !");

                // Demander un snapshot ici
                // if (gameController != null)
                // {
                //     gameController.RequestSnapshot();
                // }

                waitingRoom.SetActive(false);
                planetSpawner.SetActive(true);
                meteoriteSpawner.SetActive(true);
            }
            else
            {
                // Afficher le temps restant (prendre le timer le plus bas)
                float lowestTimer = mouthOpenTimers.Values.Min();
                float remainingTime = requiredTimeAllOpen - lowestTimer;
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

