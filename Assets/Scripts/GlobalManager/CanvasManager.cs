using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{


    [SerializeField] private GameObject playerScoreTextPrefab; // Prefab pour le texte du score

    [SerializeField] private GameObject playerAvatarPrefab; // Prefab pour le texte du score

    [SerializeField] private Transform playerHeaderReference;

    // Start is called before the first frame update
    void Start()
    {
        playerHeaderReference.gameObject.SetActive(false);

        
    }


    public void ShowPlayerHeader(){
        playerHeaderReference.gameObject.SetActive(true);
    }

    public void InitPlayerOnHeader(int faceId, Player player){
        RectTransform playerHeaderRect = playerHeaderReference.GetComponent<RectTransform>();
        float playerHeaderWidth = playerHeaderRect.rect.width;

        // Calculer l'espacement entre les avatars pour 4 joueurs maximum
        float spacing = playerHeaderWidth / 5; // Divise en 5 pour avoir 4 espaces égaux entre les bords
        float leftMargin = spacing; // Commence après le premier espace

        // Position de l'avatar basée sur l'ID du joueur
        float avatarOffset = leftMargin / 2 + (spacing * faceId) - playerHeaderWidth / 2 ;

        // Créer l'avatar du joueur
        GameObject avatarObj = Instantiate(playerAvatarPrefab, playerHeaderReference);
        RectTransform avatarRectTransform = avatarObj.GetComponent<RectTransform>();
        avatarRectTransform.anchoredPosition = new Vector2(avatarOffset, -avatarRectTransform.rect.height / 4);

        RawImage avatar = avatarObj.GetComponentInChildren<RawImage>();

        // Créer le texte du score
        GameObject scoreTextObj = Instantiate(playerScoreTextPrefab, playerHeaderReference);
        RectTransform rectTransform = scoreTextObj.GetComponent<RectTransform>();
        float textWidth = rectTransform.rect.width;
        
        rectTransform.anchoredPosition = new Vector2(avatarOffset + textWidth / 2, -avatarRectTransform.rect.height / 4);
        TextMeshProUGUI scoreText = scoreTextObj.GetComponent<TextMeshProUGUI>();
        scoreText.text = $"0";

        player.SetPlayerHeader(avatar, scoreText);
    }
}
