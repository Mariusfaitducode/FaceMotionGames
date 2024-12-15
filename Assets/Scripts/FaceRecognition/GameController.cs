using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class GameController : MonoBehaviour
{
    private WebSocketClient webSocketClient;
    
    [SerializeField]
    private GameObject playerPrefab; // Le prefab du joueur à instancier
    
    // Dictionnaire pour suivre les joueurs actifs
    private Dictionary<int, GameObject> activePlayers = new Dictionary<int, GameObject>();

    [SerializeField] private GameObject scoreTextPrefab; // Prefab pour le texte du score
    [SerializeField] private Transform scoreContainer;

    // Events modifiés pour inclure l'ID du visage
    [SerializeField]
    public UnityEvent<int, bool> onBlinkEvent = new UnityEvent<int, bool>();
    [SerializeField]
    public UnityEvent<int, bool> onMouthStateEvent = new UnityEvent<int, bool>();
    [SerializeField]
    public UnityEvent<int, float> onMouthRatioEvent = new UnityEvent<int, float>();

    void Start()
    {
        webSocketClient = FindObjectOfType<WebSocketClient>();
        
        webSocketClient.OnBlinkDetected += HandleBlink;
        webSocketClient.OnMouthStateChanged += HandleMouthState;
        webSocketClient.OnMouthRatioChanged += HandleMouthRatio;
    }

    private void HandleBlink(FaceData faceData)
    {
        EnsurePlayerExists(faceData.face_id);
        onBlinkEvent?.Invoke(faceData.face_id, faceData.blink_detected);
    }

    private void HandleMouthState(FaceData faceData)
    {
        EnsurePlayerExists(faceData.face_id);
        onMouthStateEvent?.Invoke(faceData.face_id, faceData.mouth_open);
    }

    private void HandleMouthRatio(FaceData faceData)
    {
        EnsurePlayerExists(faceData.face_id);
        onMouthRatioEvent?.Invoke(faceData.face_id, faceData.mouth_ratio);
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
            newPlayer.GetComponent<SpriteRenderer>().color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            
            // Créer le texte du score
            GameObject scoreTextObj = Instantiate(scoreTextPrefab, scoreContainer);
            RectTransform rectTransform = scoreTextObj.GetComponent<RectTransform>();
            float width = rectTransform.rect.width;
            
            rectTransform.anchoredPosition = new Vector2(width * faceId, 0);
            TextMeshProUGUI scoreText = scoreTextObj.GetComponent<TextMeshProUGUI>();
            scoreText.text = $"Player {faceId}: 0";

            // Associer le texte du score au joueur
            playerLogic.SetScoreText(scoreText);

            // Ajouter le joueur à la liste des joueurs actifs  
            activePlayers.Add(faceId, newPlayer);
        }
    }

    void OnDestroy()
    {
        if (webSocketClient != null)
        {
            webSocketClient.OnBlinkDetected -= HandleBlink;
            webSocketClient.OnMouthStateChanged -= HandleMouthState;
            webSocketClient.OnMouthRatioChanged -= HandleMouthRatio;
        }
    }
}