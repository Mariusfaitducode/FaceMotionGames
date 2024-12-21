using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class LaneManager : MonoBehaviour
{
    [Header("Lane Settings")]
    [SerializeField] private GameObject lanePrefab;
    private float verticalPadding = 0.2f; // Marge en haut et en bas

    private float topMargin = 0.5f;
    
    private List<Vector3> lanesPositions = new List<Vector3>();


    

    // private List<GameObject> lanes = new List<GameObject>();

    private float spawnPosition = 10f;
    
    public List<Vector3> InitializeLaneManager(int playerCount)
    {
        float screenHeight = Camera.main.orthographicSize * 2;
        float playableHeight = screenHeight - (2 * verticalPadding);
        
        // Si on a N joueurs, on aura N+1 espaces égaux entre les lanes
        float spacing = playableHeight / (playerCount + 1);
        float topY = (screenHeight / 2) - verticalPadding - topMargin;
        
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
}




