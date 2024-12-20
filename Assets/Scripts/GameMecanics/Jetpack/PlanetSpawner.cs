using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class PlanetSpawner : MonoBehaviour
{
    [SerializeField] private GameObject planetPrefab;
    public float spawnDelay = 3f;
    public float planetSpeed = 4f;
    [SerializeField] private float destroyXPosition = -10f;
    
    // Paramètres pour le contrôle des hauteurs de spawn
    [SerializeField] private int numberOfLanes = 5; // Nombre de "couloirs" verticaux
    [SerializeField] private float minHeight = 0.1f;
    [SerializeField] private float maxHeight = 0.9f;
    
    // Paramètres pour le mouvement oscillatoire
    [SerializeField] private float oscillationAmplitude = 1f;
    [SerializeField] private float oscillationFrequency = 1f;

    private float nextSpawnTime;
    private Camera mainCamera;
    private List<int> availableLanes;

    private void Start()
    {
        mainCamera = Camera.main;
        nextSpawnTime = Time.time + spawnDelay;
        InitializeLanes();
    }

    private void InitializeLanes()
    {
        availableLanes = new List<int>();
        for (int i = 0; i < numberOfLanes; i++)
        {
            availableLanes.Add(i);
        }
    }

    private void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnPlanet();
            nextSpawnTime = Time.time + spawnDelay;
        }
    }

    private void SpawnPlanet()
    {
        if (availableLanes.Count == 0)
        {
            InitializeLanes(); // Réinitialiser les lanes si toutes sont utilisées
        }

        // Sélectionner une lane aléatoire parmi celles disponibles
        int laneIndex = Random.Range(0, availableLanes.Count);
        int selectedLane = availableLanes[laneIndex];
        availableLanes.RemoveAt(laneIndex);

        // Calculer la hauteur en fonction de la lane
        float heightPercentage = Mathf.Lerp(minHeight, maxHeight, (float)selectedLane / (numberOfLanes - 1));
        Vector3 spawnPosition = mainCamera.ViewportToWorldPoint(new Vector3(1.1f, heightPercentage, 10f));

        GameObject planet = Instantiate(planetPrefab, spawnPosition, Quaternion.identity);

        // Add planet mover component
        float speed = planetSpeed * (1 + Random.Range(-0.4f, 0.4f));
        planet.AddComponent<PlanetMover>().Initialize(speed, destroyXPosition, oscillationAmplitude, oscillationFrequency);
    }
}
