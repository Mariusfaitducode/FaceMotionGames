using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileSpawner : MonoBehaviour
{

    private List<Vector3> lanePositions;

    [Header("Spawn Settings")]
    [SerializeField] private GameObject normalTilePrefab;
    [SerializeField] private GameObject longTilePrefab;

    private float baseSpawnInterval = 0.5f;
    private float spawnXPosition = 10f;
    private float nextSpawnTime = 0f;
    
    [Header("Pattern Settings")]
    [SerializeField] [Range(0f, 1f)] private float longTileChance = 0.2f;
    [SerializeField] [Range(0f, 1f)] private float multiLaneChance = 0.2f;
    // [SerializeField] [Range(2, 4)] private int maxSimultaneousLanes = 2;

    public float tileSpeed = 5f;


    public bool isGameStarted = false;

    // Dictionnaire pour tracker les tiles actives par lane
    private Dictionary<int, List<TileObject>> activeTilesPerLane = new Dictionary<int, List<TileObject>>();
    
    public void InitializeTileSpawner(List<Vector3> lanes)
    {
        lanePositions = lanes;
        // nextSpawnTime = Time.time + baseSpawnInterval;
        
        // Initialiser le dictionnaire de tracking
        activeTilesPerLane.Clear();
        for (int i = 0; i < lanes.Count; i++)
        {
            activeTilesPerLane[i] = new List<TileObject>();
        }
    }

    public void StartGame(){
        isGameStarted = true;
        nextSpawnTime = Time.time + baseSpawnInterval;
    }

    public IEnumerator PauseSpawner(float duration){
        isGameStarted = false;
        yield return new WaitForSeconds(duration);
        isGameStarted = true;
        nextSpawnTime = Time.time + baseSpawnInterval;
    }
    
    void Update()
    {
        // Nettoyer les tiles détruites
        CleanupDestroyedTiles();
        
        if (isGameStarted && Time.time >= nextSpawnTime && lanePositions != null && lanePositions.Count > 0)
        {
            SpawnTilePattern();
            nextSpawnTime = Time.time + baseSpawnInterval;
        }
    }

private void SpawnTilePattern()
    {
        // Décider si on spawn sur plusieurs lanes
        bool isMultiLane = Random.value < multiLaneChance;
        
        if (isMultiLane)
        {
            SpawnMultiLaneTiles();
        }
        else
        {
            SpawnSingleTile();
        }
    }

    
    private void CleanupDestroyedTiles()
    {
        foreach (var lane in activeTilesPerLane.Keys.ToList())
        {
            activeTilesPerLane[lane].RemoveAll(tile => tile == null);
        }
    }
    
    private bool CanSpawnInLane(int laneIndex, TileType tileType)
    {
        // Vérifier si la lane est libre
        var tilesInLane = activeTilesPerLane[laneIndex];
        
        // Si pas de tiles actives, on peut spawner
        if (tilesInLane.Count == 0) return true;
        
        // Vérifier la dernière tile spawned
        var lastTile = tilesInLane[tilesInLane.Count - 1];
        
        // Si la dernière tile est longue, on ne peut pas spawner
        if (lastTile.Type == TileType.Long) return false;
        
        // Si on veut spawner une tile longue, vérifier qu'il n'y a pas de tile normale récente
        if (tileType == TileType.Long)
        {
            float minDistance = 7f; // Distance minimale entre une tile longue et une normale
            return (lastTile.transform.position.x - spawnXPosition) < -minDistance;
        }
        
        // Pour une tile normale, vérifier la distance avec la dernière tile
        float minNormalDistance = 2f;
        return (lastTile.transform.position.x - spawnXPosition) < -minNormalDistance;
    }
    
    private void SpawnTileOnLane(int laneIndex)
    {
        // Décider du type de tile
        bool isLongTile = Random.value < longTileChance;
        TileType tileType = isLongTile ? TileType.Long : TileType.Normal;
        
        // Vérifier si on peut spawner
        if (!CanSpawnInLane(laneIndex, tileType))
        {
            return;
        }
        
        Vector3 spawnPosition = new Vector3(spawnXPosition, lanePositions[laneIndex].y, 0);
        GameObject tilePrefab = isLongTile ? longTilePrefab : normalTilePrefab;
        
        // Création du tile
        GameObject tileObject = Instantiate(tilePrefab, spawnPosition, Quaternion.identity);
        tileObject.AddComponent<TileObject>();
        TileObject tileScript = tileObject.GetComponent<TileObject>();
        tileScript.Initialize(tileType, tileSpeed);
        
        // Ajouter à la liste des tiles actives
        activeTilesPerLane[laneIndex].Add(tileScript);
    }
    
    private void SpawnMultiLaneTiles()
    {
        int maxLanes = Mathf.Min(1, lanePositions.Count);
        List<int> availableLanes = new List<int>();
        
        // Vérifier d'abord quelles lanes sont disponibles
        for (int i = 0; i < lanePositions.Count; i++)
        {
            if (CanSpawnInLane(i, TileType.Normal))
            {
                availableLanes.Add(i);
            }
        }
        
        // Sélectionner aléatoirement parmi les lanes disponibles
        int lanesToUse = Mathf.Min(Random.Range(2, maxLanes + 1), availableLanes.Count);
        
        for (int i = 0; i < lanesToUse; i++)
        {
            int randomIndex = Random.Range(0, availableLanes.Count);
            SpawnTileOnLane(availableLanes[randomIndex]);
            availableLanes.RemoveAt(randomIndex);
        }
    }
    
    private void SpawnSingleTile()
    {
        int randomLaneIndex = Random.Range(0, lanePositions.Count);
        SpawnTileOnLane(randomLaneIndex);
    }
}
