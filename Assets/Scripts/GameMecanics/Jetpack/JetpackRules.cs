using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetpackRules : MonoBehaviour
{
    private float jetpackSpeed = 5f; 

    private bool isAlive = true;

    private bool collideWithPlanet = false;

    private int deathCount = 0;



    private Vector3 initialPosition;
    private Vector3 originalScale;


    [SerializeField] private float attractionSpeed = 5f;    // Vitesse d'attraction vers la planète
    [SerializeField] private float shrinkSpeed = 1f;        // Vitesse de réduction de taille
    [SerializeField] private float minScaleBeforeRespawn = 0.1f; // Taille minimum avant réapparition

    private bool isBeingAttracted = false;
    private GameObject attractorPlanet;

    private Coroutine rotateCoroutine;


    private bool isMouthOpen = false;

    private Rigidbody2D rb;

    private Player playerLogic;

    // private void Start(){

    //     initialPosition = transform.position;
    // }

    public void InitializeJetpackRules(Player playerLogic, Rigidbody2D rb){
        this.playerLogic = playerLogic;
        this.rb = rb;
        // this.collider = collider;

        initialPosition = transform.position;
        originalScale = transform.localScale;
        
        rb.velocity = Vector2.zero;
        rb.gravityScale = 1;
        GetComponent<Collider2D>().enabled = true;

        // playerLogic.isJetpackGame = true;

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
            rb.velocity = new Vector2(0, jetpackSpeed);
        }
        // else{
        //     rb.velocity = new Vector2(0, -jetpackSpeed);;
        // }
    }



    public void SetMouthState(bool isOpen, GameObject player){
        isMouthOpen = isOpen;
        
        if (isOpen)
        {    
            if (rotateCoroutine != null){
                StopCoroutine(rotateCoroutine);
            }
            rotateCoroutine = StartCoroutine(RotatePlayer(player, 30f, 0.5f));
        }
        else{
            if (rotateCoroutine != null){
                StopCoroutine(rotateCoroutine);
            }
            rotateCoroutine = StartCoroutine(RotatePlayer(player, -30f, 0.5f));
        }
    }


    IEnumerator RotatePlayer(GameObject player, float targetRotation, float duration)
    {
        float startRotation = player.transform.rotation.eulerAngles.z;
        float elapsed = 0f;

        // Normalize angles to -180 to 180 range
        if (startRotation > 180f) startRotation -= 360f;
        if (targetRotation > 180f) targetRotation -= 360f;

        // Calculate shortest rotation direction
        float angleDiff = targetRotation - startRotation;
        if (Mathf.Abs(angleDiff) > 180f)
        {
            if (angleDiff > 0)
                startRotation += 360f;
            else
                targetRotation += 360f;
        }

        while (elapsed < duration)
        {
            player.transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(startRotation, targetRotation, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
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


    private void OnCollisionEnter2D(Collision2D collision)
    {

        transform.rotation = Quaternion.Euler(0, 0, 0);
        
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            deathCount++;
            playerLogic.UpdateScoreDisplay($"{deathCount}");

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
}
