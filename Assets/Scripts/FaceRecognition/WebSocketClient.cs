using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UI;


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
public class SnapshotData
{
    public int face_id;
    public string image;
}

[Serializable]
public class WebSocketMessage
{
    public string event_type;
    public FaceData data;
    public SnapshotData snapshot_data;
    public double timestamp;
}

public enum BinaryMessageType : byte
{
    FaceData = 1,
    Snapshot = 2
}

public class WebSocketClient : MonoBehaviour
{
    private ClientWebSocket webSocket;
    private const string WS_URL = "ws://localhost:8765";
    private bool isConnected = false;
    private CancellationTokenSource cancellationTokenSource;
    private const int MAX_BUFFER_SIZE = 1024 * 1024; // 1MB buffer
    private byte[] receiveBuffer;

    // Events pour notifier les autres composants
    public event Action<FaceData> OnBlinkDetected;
    public event Action<FaceData> OnMouthStateChanged;
    public event Action<FaceData> OnMouthRatioChanged;
    public event Action<int, Texture2D> OnSnapshotReceived;

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
        var buffer = new byte[1024 * 1024]; // 1MB buffer
        
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
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    // Traitement existant des messages texte
                    var message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                    HandleMessage(message);
                }
                else if (result.MessageType == WebSocketMessageType.Binary)
                {
                    Debug.Log("Binary message received");
                    // Nouveau traitement des messages binaires
                    HandleBinaryMessage(buffer, result.Count);
                }
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

    private void HandleBinaryMessage(byte[] buffer, int count)
    {
        try
        {
            if (showDebugLogs)
            {
                Debug.Log($"Buffer length: {buffer.Length}");
                Debug.Log($"Count: {count}");
            }

            // Lire l'en-tête
            BinaryMessageType messageType = (BinaryMessageType)buffer[0];
            
            // Convertir les entiers de big-endian (network order) vers little-endian
            int faceId = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, 1));
            int dataLength = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, 5));

            if (showDebugLogs)
            {
                Debug.Log($"Message type: {messageType}");
                Debug.Log($"Face ID: {faceId}");
                Debug.Log($"Data length: {dataLength}");
            }

            if (messageType == BinaryMessageType.Snapshot)
            {
                // Vérifier que la taille des données est cohérente
                if (dataLength > 0 && dataLength <= count - 9)  // 9 est la taille de l'en-tête
                {
                    // Extraire les données d'image
                    byte[] imageData = new byte[dataLength];
                    Array.Copy(buffer, 9, imageData, 0, dataLength);

                    // Créer la texture
                    Texture2D texture = new Texture2D(2, 2);
                    if (texture.LoadImage(imageData))
                    {
                        if (showDebugLogs)
                        {
                            Debug.Log($"Successfully created texture for face {faceId}, size: {dataLength} bytes");
                        }
                        
                        MainThreadDispatcher.Enqueue(() => {
                            OnSnapshotReceived?.Invoke(faceId, texture);
                        });
                    }
                    else
                    {
                        Debug.LogError("Failed to load image data into texture");
                    }
                }
                else
                {
                    Debug.LogError($"Invalid data length: {dataLength}, total message size: {count}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error handling binary message: {e.Message}\n{e.StackTrace}");
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
                    if (wsMessage.event_type == "snapshot_data")
                    {
                        Debug.Log("Snapshot data received for face " + wsMessage.data.face_id);
                    }

                    // if (wsMessage.event_type == "face_data")
                    // {
                    //     Debug.Log($"Face {wsMessage.data.face_id}: " +
                    //         $"Blink={wsMessage.data.blink_detected}, " +
                    //         $"MouthOpen={wsMessage.data.mouth_open}, " +
                    //         $"MouthRatio={wsMessage.data.mouth_ratio:F2}, " +
                    //         $"TotalBlinks={wsMessage.data.total_blinks}, " +
                    //         $"Timestamp={wsMessage.timestamp}");
                    // }

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

    public async Task RequestSnapshot(int id)
    {
        if (!isConnected) return;

        try
        {
            string message = "{\"type\": \"request_snapshot\", \"face_id\": " + id + "}";
            var bytes = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                cancellationTokenSource.Token
            );
            Debug.Log("Snapshot request sent");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending snapshot request: {e.Message}");
        }
    }
}
