using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{


    [SerializeField] private GameObject playerPrefab;

    // Dictionnaire pour suivre les joueurs actifs
    private Dictionary<int, GameObject> activePlayers = new Dictionary<int, GameObject>();
    private Dictionary<int, bool> requestedPlayerSnapshots = new Dictionary<int, bool>();

    [SerializeField] private GameObject playerScoreTextPrefab; // Prefab pour le texte du score

    [SerializeField] private GameObject playerAvatarPrefab; // Prefab pour le texte du score

    [SerializeField] private Transform playerHeaderReference;


    private StartGame startGame;
    private GameController gameController;
    // Start is called before the first frame update
    void Start()
    {
        playerHeaderReference.gameObject.SetActive(false);

        gameController = FindObjectOfType<GameController>();
        if (gameController != null)
        {
            gameController.onMouthStateEvent.AddListener(HandleMouthState);

            gameController.onSnapshotReceived.AddListener(HandleSnapshotReceived);
        }

        startGame = FindObjectOfType<StartGame>();
    }  
    

    private void HandleMouthState(int id, bool isOpen)
    {
        if (activePlayers.ContainsKey(id))
        {
            activePlayers[id].GetComponent<Player>().SetMouthState(isOpen);
            
            startGame.SetMouthsOpen(id, isOpen, activePlayers.Count);
        }
        else
        {
            EnsurePlayerExists(id);
        }
    }

    private void EnsurePlayerExists(int faceId)
    {
        if (!activePlayers.ContainsKey(faceId))
        {
            // Créer un nouveau joueur à une position décalée
            Vector3 spawnPosition = new Vector3(-faceId * 2, 0, 0); // Décalage horizontal
            GameObject newPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            
            // Configurer le mouvement pour ce joueur
            Player playerLogic = newPlayer.GetComponent<Player>();
            playerLogic.SetFaceId(faceId);

            // Donner une couleur aléatoire à chaque joueur
            // newPlayer.GetComponent<SpriteRenderer>().color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            
            // Créer l'avatar du joueur
            GameObject avatarObj = Instantiate(playerAvatarPrefab, playerHeaderReference);
            RectTransform avatarRectTransform = avatarObj.GetComponent<RectTransform>();
            float avatarWidth = avatarRectTransform.rect.width;
            avatarRectTransform.anchoredPosition = new Vector2(avatarWidth * faceId, 0);

            RawImage avatar = avatarObj.GetComponentInChildren<RawImage>();
            

            // Créer le texte du score
            GameObject scoreTextObj = Instantiate(playerScoreTextPrefab, playerHeaderReference);
            RectTransform rectTransform = scoreTextObj.GetComponent<RectTransform>();
            float width = rectTransform.rect.width;
            
            rectTransform.anchoredPosition = new Vector2(width * faceId + avatarWidth, 0);
            TextMeshProUGUI scoreText = scoreTextObj.GetComponent<TextMeshProUGUI>();
            scoreText.text = $"0";

            // Associer le texte du score au joueur
            playerLogic.SetPlayerHeader(avatar, scoreText);

            // Ajouter le joueur à la liste des joueurs actifs  
            activePlayers.Add(faceId, newPlayer);
        }
    }


    public void RequestSnapshot(int id)
    {
        if (activePlayers.ContainsKey(id) && !requestedPlayerSnapshots.ContainsKey(id))
        {
            gameController.RequestSnapshot(id);
            requestedPlayerSnapshots.Add(id, true);
        }
    }

    private void HandleSnapshotReceived(int faceId, Texture2D texture)
    {
        Debug.Log("Snapshot received for face " + faceId);

        if (activePlayers.ContainsKey(faceId))
        {
            activePlayers[faceId].GetComponent<Player>().SetSnapshot(texture);
        }
    }

    public void ShowPlayersAvatar(){
        // foreach (var player in activePlayers)
        // {
        //     player.GetComponent<Player>().ShowAvatar();
        // }

        playerHeaderReference.gameObject.SetActive(true);
    }
}
