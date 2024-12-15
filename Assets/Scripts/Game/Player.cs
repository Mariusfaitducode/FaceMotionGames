using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float jetpackSpeed = 5f;  // Vitesse constante en unités par seconde
    
    private Rigidbody2D rb;
    private bool isAlive = true;

    private int deathCount = 0;
    private bool isMouthOpen = false;
    private int faceId; // ID du visage associé à ce mouvement
    private TextMeshProUGUI scoreText;

    private Vector3 initialPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        initialPosition = transform.position;
        
        // GameController gameController = FindObjectOfType<GameController>();
        // if (gameController != null)
        // {
        //     gameController.onMouthStateEvent.AddListener(HandleMouthState);
        // }
    }

    public void SetFaceId(int id)
    {
        faceId = id;
    }

    public void SetScoreText(TextMeshProUGUI text)
    {
        scoreText = text;
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Player {faceId}: {deathCount}";
        }
    }

    private void HandleMouthState(int id, bool isOpen)
    {
        // Ne réagir qu'aux événements correspondant à notre ID
        if (id == faceId)
        {
            this.isMouthOpen = isOpen;
        }
    }

    public void SetMouthState(bool isOpen)
    {
        isMouthOpen = isOpen;
    }

    void FixedUpdate()
    {
        // if (!isAlive) return;

        if (isMouthOpen)
        {
            rb.velocity = new Vector2(rb.velocity.x, jetpackSpeed);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            // isAlive = false;
            deathCount++;
            UpdateScoreDisplay();


            // Rendre le joueur invisible pendant 2 secondes
            StartCoroutine(TemporaryInvisibility());

            // Reset la position du joueur
            transform.position = initialPosition;
            // Reset les forces et vitesses
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private IEnumerator TemporaryInvisibility()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        yield return new WaitForSeconds(2);
        GetComponent<SpriteRenderer>().enabled = true;
    }
}
