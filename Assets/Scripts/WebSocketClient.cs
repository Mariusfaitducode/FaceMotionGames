using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;





[Serializable]
public class FaceData
{
    public bool blink_detected;
    public bool mouth_open;
    public float mouth_ratio;
    public int face_id;
    public int total_blinks;
}

[Serializable]
public class WebSocketMessage
{
    public string event_type;
    public FaceData data;
    public double timestamp;
}

public class WebSocketClient : MonoBehaviour
{
    private ClientWebSocket webSocket;
    private const string WS_URL = "ws://localhost:8765";
    private bool isConnected = false;
    private CancellationTokenSource cancellationTokenSource;

    // Events pour notifier les autres composants
    public event Action<bool> OnBlinkDetected;
    public event Action<bool> OnMouthStateChanged;
    public event Action<float> OnMouthRatioChanged;

    [SerializeField]
    private bool showDebugLogs = true;

    private void Start()
    {
        cancellationTokenSource = new CancellationTokenSource();
        ConnectToWebSocket();
    }

    private async void ConnectToWebSocket()
    {
        try
        {
            webSocket = new ClientWebSocket();
            await webSocket.ConnectAsync(new Uri(WS_URL), cancellationTokenSource.Token);
            isConnected = true;
            Debug.Log("Connected to WebSocket server");
            
            // Démarrer la réception des messages
            _ = ReceiveLoop();
        }
        catch (Exception e)
        {
            Debug.LogError($"WebSocket connection error: {e.Message}");
        }
    }

    private async Task ReceiveLoop()
    {
        var buffer = new byte[1024 * 4];
        
        while (isConnected && !cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                var result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), 
                    cancellationTokenSource.Token
                );

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await HandleWebSocketClosure();
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                HandleMessage(message);
            }
            catch (Exception e)
            {
                if (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Debug.LogError($"Error receiving message: {e.Message}");
                    await HandleWebSocketClosure();
                }
                break;
            }
        }
    }

    private void HandleMessage(string message)
    {
        try
        {
            WebSocketMessage wsMessage = JsonUtility.FromJson<WebSocketMessage>(message);

            if (wsMessage != null && wsMessage.data != null)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"Face {wsMessage.data.face_id}: " +
                            $"Blink={wsMessage.data.blink_detected}, " +
                            $"MouthOpen={wsMessage.data.mouth_open}, " +
                            $"MouthRatio={wsMessage.data.mouth_ratio:F2}, " +
                            $"TotalBlinks={wsMessage.data.total_blinks}");
                }

                // Déclencher les événements sur le thread principal
                MainThreadDispatcher.Enqueue(() => {
                    OnBlinkDetected?.Invoke(wsMessage.data.blink_detected);
                    OnMouthStateChanged?.Invoke(wsMessage.data.mouth_open);
                    OnMouthRatioChanged?.Invoke(wsMessage.data.mouth_ratio);
                });
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing message: {e.Message}\nMessage: {message}");
        }
    }

    private async Task HandleWebSocketClosure()
    {
        isConnected = false;
        if (webSocket.State == WebSocketState.Open)
        {
            await webSocket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "Closing connection",
                cancellationTokenSource.Token
            );
        }
    }

    private void OnDestroy()
    {
        cancellationTokenSource.Cancel();
        _ = HandleWebSocketClosure();
    }
}
