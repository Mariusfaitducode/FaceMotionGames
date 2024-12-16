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

    private bool collideWithPlanet = false;

    private int deathCount = 0;
    private bool isMouthOpen = false;
    private int faceId; // ID du visage associé à ce mouvement
    private TextMeshProUGUI scoreText;

    private Vector3 initialPosition;

    [SerializeField] private float attractionSpeed = 5f;    // Vitesse d'attraction vers la planète
    [SerializeField] private float shrinkSpeed = 1f;        // Vitesse de réduction de taille
    [SerializeField] private float minScaleBeforeRespawn = 0.1f; // Taille minimum avant réapparition

    private bool isBeingAttracted = false;
    private GameObject attractorPlanet;
    private Vector3 originalScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        initialPosition = transform.position;
        originalScale = transform.localScale;
        
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
        if (isBeingAttracted)
        {

            if (attractorPlanet == null)
            {
                Respawn();
                return;
            }

            // Désactiver la physique normale pendant l'attraction
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;

            // Déplacer vers la planète
            // Vector2 directionToPlanet = (attractorPlanet.position - transform.position).normalized;
            
            // Récupérer le PlanetMover pour avoir la vitesse de la planète
            PlanetMover planetMover = attractorPlanet.GetComponent<PlanetMover>();
            if (planetMover != null)
            {
                // Déplacer le joueur à la même vitesse que la planète sur l'axe X
                transform.position += Vector3.left * planetMover.speed * Time.fixedDeltaTime;
            }
            
            // Continuer l'attraction vers la planète
            transform.position = Vector2.MoveTowards(
                transform.position,
                attractorPlanet.transform.position,
                attractionSpeed * Time.fixedDeltaTime
            );

            // Réduire la taille
            transform.localScale = Vector3.MoveTowards(
                transform.localScale,
                Vector3.zero,
                shrinkSpeed * Time.fixedDeltaTime
            );

            // Vérifier si assez petit pour réapparaître
            // if (transform.localScale.x <= minScaleBeforeRespawn)
            // {
            //     Respawn();
            // }
        }
        else if (isMouthOpen)
        {
            rb.velocity = new Vector2(rb.velocity.x, jetpackSpeed);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            deathCount++;
            UpdateScoreDisplay();

            // Commencer l'attraction
            StartAttraction(collision.gameObject);
        }
    }

    private void StartAttraction(GameObject planet)
    {
        isBeingAttracted = true;
        attractorPlanet = planet;
        rb.gravityScale = 0; // Désactiver la gravité pendant l'attraction
        
        // Désactiver les collisions pendant l'attraction
        GetComponent<Collider2D>().enabled = false;

        // Mettre le joueur au premier plan
        SpriteRenderer playerRenderer = GetComponent<SpriteRenderer>();
        if (playerRenderer != null)
        {
            playerRenderer.sortingOrder = 10; // Utiliser un nombre plus grand que celui des planètes
        }
    }

    private void Respawn()
    {
        // Réinitialiser l'état du joueur
        isBeingAttracted = false;
        attractorPlanet = null;
        transform.position = initialPosition;
        transform.localScale = originalScale;
        rb.gravityScale = 1; // Réactiver la gravité
        GetComponent<Collider2D>().enabled = true;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // Remettre l'ordre de rendu à sa valeur d'origine
        SpriteRenderer playerRenderer = GetComponent<SpriteRenderer>();
        if (playerRenderer != null)
        {
            playerRenderer.sortingOrder = 1; // Ou la valeur par défaut que vous souhaitez
        }
    }
}
