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
    public event Action<FaceData> OnBlinkDetected;
    public event Action<FaceData> OnMouthStateChanged;
    public event Action<FaceData> OnMouthRatioChanged;

    [SerializeField]
    private bool showDebugLogs = true;

    private double startupTime;

    private void Start()
    {
        cancellationTokenSource = new CancellationTokenSource();
        ConnectToWebSocket();

        // Calculer le timestamp Unix au démarrage
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        startupTime = (System.DateTime.UtcNow - epochStart).TotalSeconds;
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
                            $"TotalBlinks={wsMessage.data.total_blinks}, " +
                            $"Timestamp={wsMessage.timestamp}");

                    // Calculer la latence en millisecondes
                    // Obtenir le timestamp Unix actuel
                    System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
                    double currentUnixTime = (System.DateTime.UtcNow - epochStart).TotalSeconds;

                    // Calculer la latence
                    double latency = (currentUnixTime - wsMessage.timestamp) * 1000;
                    Debug.Log($"Latency: {latency:F2}ms");
                }

                MainThreadDispatcher.Enqueue(() => {
                    OnBlinkDetected?.Invoke(wsMessage.data);
                    OnMouthStateChanged?.Invoke(wsMessage.data);
                    OnMouthRatioChanged?.Invoke(wsMessage.data);
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
