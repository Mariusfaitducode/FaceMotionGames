using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class GameController : MonoBehaviour
{
    private WebSocketClient webSocketClient;
    

    // Events modifiés pour inclure l'ID du visage
    [SerializeField]
    public UnityEvent<int, bool> onBlinkEvent = new UnityEvent<int, bool>();
    [SerializeField]
    public UnityEvent<int, bool> onMouthStateEvent = new UnityEvent<int, bool>();
    [SerializeField]
    public UnityEvent<int, float> onMouthRatioEvent = new UnityEvent<int, float>();

    [SerializeField]
    public UnityEvent<int, Texture2D> onSnapshotReceived = new UnityEvent<int, Texture2D>();



    void Start()
    {
        webSocketClient = FindObjectOfType<WebSocketClient>();
        
        webSocketClient.OnBlinkDetected += HandleBlink;
        webSocketClient.OnMouthStateChanged += HandleMouthState;
        webSocketClient.OnMouthRatioChanged += HandleMouthRatio;

        webSocketClient.OnSnapshotReceived += HandleSnapshotReceived;
    }

    private void HandleBlink(FaceData faceData)
    {
        // EnsurePlayerExists(faceData.face_id);
        onBlinkEvent?.Invoke(faceData.face_id, faceData.blink_detected);
    }

    private void HandleMouthState(FaceData faceData)
    {
        // EnsurePlayerExists(faceData.face_id);
        onMouthStateEvent?.Invoke(faceData.face_id, faceData.mouth_open);
    }

    private void HandleMouthRatio(FaceData faceData)
    {
        // EnsurePlayerExists(faceData.face_id);
        onMouthRatioEvent?.Invoke(faceData.face_id, faceData.mouth_ratio);
    }


    private void HandleSnapshotReceived(int faceId, Texture2D texture)
    {
        onSnapshotReceived?.Invoke(faceId, texture);
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

    public void RequestSnapshot(int id)
    {
        if (webSocketClient != null)
        {
            _ = webSocketClient.RequestSnapshot(id);
        }
    }
}