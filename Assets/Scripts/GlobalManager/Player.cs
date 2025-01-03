using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Player : MonoBehaviour
{

    // Face infos
    private RawImage playerAvatar;
    private int faceId;
    private bool isMouthOpen = false;


    // Object infos
    private Rigidbody2D rb;
    private TextMeshProUGUI scoreText;

    // Game infos
    // public bool isJetpackGame = false;

    private JetpackRules jetpackRules = null;

    private PianoRules pianoRules = null;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    public void SetMouthState(bool isOpen)
    {
        if (jetpackRules != null){
            // Debug.Log("Setting mouth state for player in Jetpack game");
            isMouthOpen = isOpen;

            jetpackRules.SetMouthState(isOpen, gameObject);
        }
        else if (pianoRules != null){
            // Debug.Log("Setting mouth state for player in Piano game");
            isMouthOpen = isOpen;

            pianoRules.SetMouthState(isOpen, gameObject);
        }
    }

    public void SetSnapshot(Texture2D texture)
    {
        playerAvatar.texture = texture;
    }

    public void SetFaceId(int id)
    {
        faceId = id;
    }

    public void SetPlayerHeader(RawImage avatar, TextMeshProUGUI text)
    {
        playerAvatar = avatar;
        scoreText = text;
        UpdateScoreDisplay("");
    }

    public void UpdateScoreDisplay(string score)
    {
        if (scoreText != null)
        {
            scoreText.text = score;
        }
    }



    public void SetPlayerForTransition(){
        GetComponent<Animator>().enabled = false;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;
        GetComponent<Collider2D>().enabled = false;
    }

    public void ResetPlayerAfterTransition(){
        GetComponent<Animator>().enabled = true;
        rb.gravityScale = 1;
        GetComponent<Collider2D>().enabled = true;
    }


    public void StartJetpackGame(AudioSource explosionSound){

        ClearAllRules();

        Debug.Log("Starting jetpack game for player " + faceId);

        this.gameObject.AddComponent<JetpackRules>();
        jetpackRules = GetComponent<JetpackRules>();

        jetpackRules.InitializeJetpackRules(this, rb, explosionSound);  
    }

    public void StartPianoTilesGame(){

        ClearAllRules();

        Debug.Log("Starting piano tiles game for player " + faceId);

        this.gameObject.AddComponent<PianoRules>();
        pianoRules = GetComponent<PianoRules>();

        pianoRules.InitializePianoRules(this, rb);
    }


    public void ClearAllRules(){
        Destroy(GetComponent<JetpackRules>());
        Destroy(GetComponent<PianoRules>());
    }
}
