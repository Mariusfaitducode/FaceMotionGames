using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoRules : MonoBehaviour
{


    private bool isMouthOpen = false;
    private bool isOnLongTile = false;
    private bool validateLongTile = true;
    private bool validateShortTile = false;

    private float openingDuration = 0.2f;
    private float timer = 0;

    private Rigidbody2D rb;

    private Player playerLogic;

    private float originalScale = 1f;

    private int failedTiles = 0;


    public void InitializePianoRules(Player playerLogic, Rigidbody2D rb){
        this.playerLogic = playerLogic;
        this.rb = rb;

        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;

        originalScale = this.gameObject.transform.localScale.x;
        
        Collider2D collider = GetComponent<Collider2D>();
        collider.enabled = true;
        collider.isTrigger = true;


        // playerLogic.isPianoGame = true;
    }



    public void SetMouthState(bool isOpen, GameObject player){
        isMouthOpen = isOpen;
        
        if (isOpen)
        {    
            this.gameObject.transform.localScale = new Vector3(originalScale * 1.5f, originalScale * 1.5f, originalScale * 1.5f);
        }
        else
        {
            this.gameObject.transform.localScale = new Vector3(originalScale, originalScale, originalScale);
        }
    }



    // Update is called once per frame
    void Update()
    {
        if (isMouthOpen){

            timer += Time.deltaTime;
            
            if (timer >= openingDuration && !isOnLongTile){
                isMouthOpen = false;
                timer = 0;
                this.gameObject.transform.localScale = new Vector3(originalScale, originalScale, originalScale);
            }
        }

    }


    private void OnTriggerStay2D(Collider2D collider)
    {

        // transform.rotation = Quaternion.Euler(0, 0, 0);
        
        if (collider.gameObject.CompareTag("Tile"))
        {

            Debug.Log("Tile hit");

            if (isMouthOpen){
                Debug.Log("Tile hit while mouth is open");
                collider.gameObject.GetComponent<SpriteRenderer>().color = Color.green;
                // collider.enabled = false;
                // validateTile = true;
                validateShortTile = true;
            }
            else if (!validateShortTile){
                Debug.Log("Tile hit while mouth is closed");
                // collider.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
                // validateTile = false;
                validateShortTile = false;
            }
        }
        else if (collider.gameObject.CompareTag("LongTile") && validateLongTile){
            Debug.Log("Long tile hit");

            isOnLongTile = true;

            if (!isMouthOpen){
                collider.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
                isOnLongTile = false;
                validateLongTile = false;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {

        if (collider.gameObject.CompareTag("Tile")){

            if (validateShortTile){
                collider.gameObject.GetComponent<SpriteRenderer>().color = Color.green;
            }
            else{
                failedTiles++;
                collider.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
                playerLogic.UpdateScoreDisplay($"{failedTiles}");
            }

            validateShortTile = false;

        }
        
        
        else if (collider.gameObject.CompareTag("LongTile")){
            isOnLongTile = false;

            if (validateLongTile){
                collider.gameObject.GetComponent<SpriteRenderer>().color = Color.green;
            }
            else{
                failedTiles++;
                playerLogic.UpdateScoreDisplay($"{failedTiles}");
            }

            validateLongTile = true;

        }
    }
}
