using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private WebSocketClient webSocketClient;

    void Start()
    {
        webSocketClient = FindObjectOfType<WebSocketClient>();
        
        // S'abonner aux événements
        webSocketClient.OnBlinkDetected += HandleBlink;
        webSocketClient.OnMouthStateChanged += HandleMouthState;
        webSocketClient.OnMouthRatioChanged += HandleMouthRatio;
    }

    private void HandleBlink(bool isBlinking)
    {
        if (isBlinking)
        {
            Debug.Log("Blink detected!");
            // Votre code ici
        }
    }

    private void HandleMouthState(bool isOpen)
    {
        Debug.Log($"Mouth is {(isOpen ? "open" : "closed")}");
        // Votre code ici
    }

    private void HandleMouthRatio(float ratio)
    {
        Debug.Log($"Mouth ratio: {ratio}");
        // Votre code ici
    }

    void OnDestroy()
    {
        // Se désabonner des événements
        if (webSocketClient != null)
        {
            webSocketClient.OnBlinkDetected -= HandleBlink;
            webSocketClient.OnMouthStateChanged -= HandleMouthState;
            webSocketClient.OnMouthRatioChanged -= HandleMouthRatio;
        }
    }
}
