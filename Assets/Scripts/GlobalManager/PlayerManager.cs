using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{


    [SerializeField] private List<GameObject> playerPrefabs = new List<GameObject>();

    // Dictionnaire pour suivre les joueurs actifs
    private Dictionary<int, GameObject> activePlayers = new Dictionary<int, GameObject>();
    private Dictionary<int, bool> requestedPlayerSnapshots = new Dictionary<int, bool>();

    


    private StartGame startGame;
    private GameController gameController;

    private CanvasManager canvasManager;
    // Start is called before the first frame update
    void Start()
    {

        gameController = FindObjectOfType<GameController>();
        if (gameController != null)
        {
            gameController.onMouthStateEvent.AddListener(HandleMouthState);

            gameController.onSnapshotReceived.AddListener(HandleSnapshotReceived);
        }

        startGame = FindObjectOfType<StartGame>();
        canvasManager = FindObjectOfType<CanvasManager>();

        // Debug.Log("StartGame enabled: " + startGame.enabled);

        // Debug
        // for (int i = 0; i < 4; i++){
        //     EnsurePlayerExists(i);
        //     PlayPlayerAnimation(i, startGame.playersAnimations[i]);
        // }
    }  


    public void SetStartGame(StartGame startGame){
        this.startGame = startGame;
    }
    

    private void HandleMouthState(int id, bool isOpen)
    {

        
        if (activePlayers.ContainsKey(id))
        {
            activePlayers[id].GetComponent<Player>().SetMouthState(isOpen);
            

            startGame.SetMouthsOpen(id, isOpen, activePlayers.Count);
        }
        else if (startGame != null)
        {
            EnsurePlayerExists(id);
        }
    }

    private void EnsurePlayerExists(int faceId)
    {
        if (!activePlayers.ContainsKey(faceId))
        {
            // Créer un nouveau joueur à une position décalée
            // Vector3 spawnPosition = new Vector3(-faceId * 2, 0, 0); // Décalage horizontal
            GameObject newPlayer = Instantiate(playerPrefabs[faceId], playerPrefabs[faceId].transform.position, Quaternion.identity);
            
            
            Player playerLogic = newPlayer.GetComponent<Player>();
            playerLogic.SetFaceId(faceId);


            // Calculate avatar position based on screen width
            // float screenWidth = Screen.width;
            // float positionX = (screenWidth / 4) * faceId + (screenWidth / 8); // Center in each quarter
            // Vector2 viewportPoint = new Vector2(positionX / screenWidth, 1);
            // Vector2 screenPoint = Camera.main.ViewportToScreenPoint(new Vector3(viewportPoint.x, viewportPoint.y, 0));
            // Vector2 localPoint;
            // RectTransformUtility.ScreenPointToLocalPointInRectangle(playerHeaderReference, screenPoint, null, out localPoint);


            canvasManager.InitPlayerOnHeader(faceId, playerLogic);

            // Associer le texte du score au joueur
            // playerLogic.SetPlayerHeader(avatar, scoreText);

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

        canvasManager.ShowPlayerHeader();
    }


    public void PlayPlayerAnimation(int id, AnimationClip animationClip){
        Debug.Log("Playing animation " + animationClip.name + " for player " + id);

        // activePlayers[id].GetComponent<Animator>().runtimeAnimatorController = animationClip;

        activePlayers[id].GetComponent<Animator>().Play(animationClip.name);
    }

    public void ResetPlayerAnimation(int id){

        activePlayers[id].GetComponent<Animator>().Play("Idle");

    }

    public void StopAllPlayerAnimator(){
        foreach (var player in activePlayers)
        {
            player.Value.GetComponent<Animator>().enabled = false;
        }
    }




    

    public void PlayersStartJetpackGame(){
        foreach (var player in activePlayers)
        {

            // player.Value.GetComponent<Animator>().StopPlayback();
            // player.Value.GetComponent<Animator>().Play("Idle");

            // player.Value.GetComponent<Animator>().enabled = false;

            player.Value.GetComponent<Player>().StartJetpackGame();
        }
    }

    public IEnumerator PlayersSimpleTransition(Vector3 translation, AnimationCurve curve, float duration){
        float elapsed = 0;

        // StopAllPlayerAnimator();

        List<Vector3> playerInitialPositions = new List<Vector3>();

        foreach (var player in activePlayers)
        {
            playerInitialPositions.Add(player.Value.transform.position);
            player.Value.GetComponent<Player>().SetPlayerForTransition();
        }
        

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            

            foreach (var player in activePlayers)
            {
                player.Value.transform.position = Vector3.Lerp(
                    playerInitialPositions[player.Key],
                    playerInitialPositions[player.Key] + translation,
                    curve.Evaluate(progress)
                );
            }

            yield return null;
        }

        // foreach (var player in activePlayers)
        // {
        //     player.Value.GetComponent<Player>().ResetPlayerAfterTransition();
        // }
    }
}
