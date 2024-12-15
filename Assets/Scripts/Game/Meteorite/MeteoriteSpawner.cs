using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteoriteSpawner : MonoBehaviour
{
    [SerializeField] private GameObject meteoritePrefab;
    [SerializeField] private float spawnRate = 3f;
    [SerializeField] private float meteoriteSpeed = 7f;
    [SerializeField] private float minAngle = 15f;  // Angle minimum de la trajectoire
    [SerializeField] private float maxAngle = 75f;  // Angle maximum de la trajectoire
    [SerializeField] private float destroyDistance = 20f;  // Distance à laquelle détruire la météorite

    private float nextSpawnTime;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        nextSpawnTime = Time.time + spawnRate;
    }

    private void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnMeteorite();
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    private void SpawnMeteorite()
    {
        // Décider aléatoirement si la météorite vient du haut ou de droite
        bool spawnFromTop = Random.value > 0.5f;

        Vector3 spawnPosition;
        float angle;

        if (spawnFromTop)
        {
            // Spawn depuis le haut de l'écran
            spawnPosition = mainCamera.ViewportToWorldPoint(new Vector3(
                Random.Range(0.0f, 1.0f),  // Position X aléatoire
                1.1f,                      // Juste au-dessus de l'écran
                10f
            ));
            angle = Random.Range(-maxAngle, -minAngle);  // Angle vers le bas
        }
        else
        {
            // Spawn depuis la droite de l'écran
            spawnPosition = mainCamera.ViewportToWorldPoint(new Vector3(
                1.1f,                      // Juste à droite de l'écran
                Random.Range(0.0f, 1.0f),  // Position Y aléatoire
                10f
            ));
            angle = Random.Range(180f + minAngle, 180f + maxAngle);  // Angle vers la gauche
        }

        GameObject meteorite = Instantiate(meteoritePrefab, spawnPosition, Quaternion.Euler(0, 0, angle));
        meteorite.AddComponent<MeteoriteMover>().Initialize(meteoriteSpeed, angle, destroyDistance);
    }
}


public class MeteoriteMover : MonoBehaviour
{
    private float speed;
    private Vector2 direction;
    private float destroyDistance;
    private Vector3 startPosition;

    public void Initialize(float speed, float angle, float destroyDistance)
    {
        this.speed = speed;
        this.destroyDistance = destroyDistance;
        this.startPosition = transform.position;

        // Convertir l'angle en direction
        float angleRad = angle * Mathf.Deg2Rad;
        direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

        // Ajouter une rotation à la météorite
        // StartCoroutine(Rotate());
    }

    private void Update()
    {
        // Déplacer la météorite
        transform.Translate(direction * speed * Time.deltaTime);

        // Vérifier si la météorite est assez loin pour être détruite
        if (Vector3.Distance(transform.position, startPosition) > destroyDistance)
        {
            Destroy(gameObject);
        }
    }

    private System.Collections.IEnumerator Rotate()
    {
        float rotationSpeed = Random.Range(90f, 180f); // Vitesse de rotation aléatoire
        
        while (true)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Optionnel : effet de particules ou son lors de la collision
            Destroy(gameObject);
        }
    }
}