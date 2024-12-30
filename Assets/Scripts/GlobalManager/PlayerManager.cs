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


    public int GetPlayerCount(){
        return activePlayers.Count;
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

            canvasManager.InitPlayerOnHeader(faceId, playerLogic);

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

    public IEnumerator PlayersSimpleTransition(Vector3 translation, AnimationCurve curve, float duration){

        yield return PlayerUtils.PlayersSimpleTransition(activePlayers.Values.ToArray(), translation, curve, duration);
    }

    public IEnumerator PlayersTransitionToPositions(List<Vector3> positions, AnimationCurve curve, float duration){

        yield return PlayerUtils.PlayersTransitionToPositions(activePlayers.Values.ToArray(), positions, curve, duration);
    }


    // public IEnumerator PlayersPianoTilesTransition(List<Vector3> lanesPositions){


        
    // }





    

    public void PlayersStartJetpackGame(AudioSource explosionSound){
        foreach (var player in activePlayers)
        {

            player.Value.GetComponent<Player>().StartJetpackGame(explosionSound);
        }
    }


    public void PlayersStartPianoTilesGame(){
        foreach (var player in activePlayers)
        {

            player.Value.GetComponent<Player>().StartPianoTilesGame();
        }
    }

    
}
