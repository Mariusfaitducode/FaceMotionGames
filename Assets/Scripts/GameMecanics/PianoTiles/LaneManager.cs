using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class LaneManager : MonoBehaviour
{
    [Header("Lane Settings")]
    [SerializeField] private GameObject lanePrefab;
    private float verticalPadding = 0.5f; // Marge en haut et en bas
    
    private List<Vector3> lanesPositions = new List<Vector3>();


    [Header("Spawn Settings")]
    [SerializeField] private GameObject tilePrefab;
    private float spawnInterval = 0.5f;
    private float spawnXPosition = 10f;
    private float nextSpawnTime = 0f;

    public bool isGameStarted = false;

    // private List<GameObject> lanes = new List<GameObject>();

    private float spawnPosition = 10f;
    
    public List<Vector3> InitializeLaneManager(int playerCount)
    {
        float screenHeight = Camera.main.orthographicSize * 2;
        float playableHeight = screenHeight - (2 * verticalPadding);
        
        // Si on a N joueurs, on aura N+1 espaces égaux entre les lanes
        float spacing = playableHeight / (playerCount + 1);
        float topY = (screenHeight / 2) - verticalPadding;
        
        // Création des lanes pour chaque joueur
        for (int i = 0; i < playerCount; i++)
        {
            // Position Y = Haut de la zone jouable - (numéro de l'espace × taille de l'espace)
            float yPosition = topY - ((i + 1) * spacing);
            lanesPositions.Add(new Vector3(0, yPosition, 0));

            GameObject lane = Instantiate(lanePrefab, new Vector3(0, yPosition, 1f), Quaternion.identity);
            // lanes.Add(lane);
        }

        return lanesPositions;
    }

    // Update is called once per frame
    void Update()
    {
        if (isGameStarted && Time.time >= nextSpawnTime && lanesPositions != null && lanesPositions.Count > 0)
        {
            SpawnTile();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }
    
    private void SpawnTile()
    {
        // Sélection aléatoire d'une lane
        int randomLaneIndex = Random.Range(0, lanesPositions.Count);
        Vector3 lanePosition = lanesPositions[randomLaneIndex];
        
        // Position de spawn (à droite de l'écran, sur la lane sélectionnée)
        Vector3 spawnPosition = new Vector3(spawnXPosition, lanePosition.y, 0);
        
        // Création du tile
        GameObject tile = Instantiate(tilePrefab, spawnPosition, Quaternion.identity);
        tile.AddComponent<TileMovement>();
    }
}


public class TileMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    
    void Update()
    {
        // Déplacement vers la gauche
        transform.Translate(Vector3.left * speed * Time.deltaTime);
        
        // Destruction de l'objet s'il sort de l'écran
        if (transform.position.x < -10f)
        {
            Destroy(gameObject);
        }
    }
}
