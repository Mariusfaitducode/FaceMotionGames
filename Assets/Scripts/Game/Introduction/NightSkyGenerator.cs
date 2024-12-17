using UnityEngine;
using System.Collections;

public class NightSkyGenerator : MonoBehaviour
{
    [Header("Sky Settings")]
    [SerializeField] private int numberOfStars = 1000;
    [SerializeField] private int numberOfBigStars = 100;
    [SerializeField] private int numberOfHugeStars = 20;
    
    [Header("Star Colors")]
    [SerializeField] private Color backgroundColor = new Color(0.02f, 0.02f, 0.05f, 1f);
    [SerializeField] private Color[] starColors = new Color[] {
        new Color(1f, 1f, 1f, 1f),        // Blanc pur
        new Color(0.9f, 0.9f, 1f, 1f),    // Blanc bleuté
        new Color(1f, 0.9f, 0.8f, 1f),    // Blanc chaud
    };

    [Header("Flicker Settings")]
    [SerializeField] private float minFlickerSpeed = 0.5f;
    [SerializeField] private float maxFlickerSpeed = 2.0f;

    private Texture2D skyTexture;
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    private int width;
    private int height;

    void Start()
    {
        mainCamera = Camera.main;
        CalculateScreenDimensions();
        InitializeSky();
        GenerateStars();
        StartCoroutine(StarFlickerEffect());
    }

    void CalculateScreenDimensions()
    {
        // Calculer la taille en pixels nécessaire pour remplir la vue de la caméra
        float orthographicSize = mainCamera.orthographicSize;
        float aspectRatio = (float)Screen.width / Screen.height;
        
        // Convertir les unités monde en pixels
        height = Mathf.RoundToInt(orthographicSize * 2 * 100); // 100 pixels par unité
        width = Mathf.RoundToInt(height * aspectRatio);

        // Ajuster le nombre d'étoiles en fonction de la surface
        float surfaceRatio = (width * height) / (1920f * 1080f); // Ratio par rapport à la résolution de référence
        numberOfStars = Mathf.RoundToInt(numberOfStars * surfaceRatio);
        numberOfBigStars = Mathf.RoundToInt(numberOfBigStars * surfaceRatio);
        numberOfHugeStars = Mathf.RoundToInt(numberOfHugeStars * surfaceRatio);
    }

    void InitializeSky()
    {
        // Créer la texture du ciel
        skyTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point // Pour garder l'aspect pixel art
        };

        // Remplir avec la couleur de fond
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = backgroundColor;
        }
        skyTexture.SetPixels(pixels);

        // Configurer le SpriteRenderer
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = Sprite.Create(skyTexture, 
            new Rect(0, 0, width, height), 
            new Vector2(0.5f, 0.5f), 
            100.0f);

        // Ajuster la position du sprite pour qu'il soit aligné avec la caméra
        transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, 1);
        
        // Ajuster l'ordre de rendu pour être sûr que le ciel est en arrière-plan
        spriteRenderer.sortingOrder = -1000;
    }

    void GenerateStars()
    {
        // Générer les petites étoiles (1x1)
        for (int i = 0; i < numberOfStars; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            Color starColor = starColors[Random.Range(0, starColors.Length)];
            starColor.a = Random.Range(0.5f, 1f);
            skyTexture.SetPixel(x, y, starColor);
        }

        // Générer les étoiles moyennes (2x2)
        for (int i = 0; i < numberOfBigStars; i++)
        {
            int x = Random.Range(1, width-1);
            int y = Random.Range(1, height-1);
            Color starColor = starColors[Random.Range(0, starColors.Length)];
            starColor.a = Random.Range(0.6f, 1f);
            
            DrawBigStar(x, y, starColor);
        }

        // Générer les grandes étoiles (3x3)
        for (int i = 0; i < numberOfHugeStars; i++)
        {
            int x = Random.Range(2, width-2);
            int y = Random.Range(2, height-2);
            Color starColor = starColors[Random.Range(0, starColors.Length)];
            starColor.a = Random.Range(0.7f, 1f);
            
            DrawHugeStar(x, y, starColor);
        }

        skyTexture.Apply();
    }

    void DrawBigStar(int centerX, int centerY, Color color)
    {
        for (int x = -1; x <= 0; x++)
        {
            for (int y = -1; y <= 0; y++)
            {
                Color pixelColor = color;
                if (x != 0 && y != 0) pixelColor.a *= 0.7f; // Coins légèrement plus transparents
                skyTexture.SetPixel(centerX + x, centerY + y, pixelColor);
            }
        }
    }

    void DrawHugeStar(int centerX, int centerY, Color color)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Color pixelColor = color;
                if (Mathf.Abs(x) + Mathf.Abs(y) == 2) // Coins
                {
                    pixelColor.a *= 0.5f;
                }
                else if (x != 0 || y != 0) // Pixels autour du centre
                {
                    pixelColor.a *= 0.8f;
                }
                skyTexture.SetPixel(centerX + x, centerY + y, pixelColor);
            }
        }
    }

    IEnumerator StarFlickerEffect()
    {
        Color[] originalPixels = skyTexture.GetPixels();
        
        while (true)
        {
            Color[] currentPixels = skyTexture.GetPixels();
            
            for (int i = 0; i < currentPixels.Length; i++)
            {
                if (currentPixels[i].a > 0 && currentPixels[i] != backgroundColor)
                {
                    float flickerIntensity = Random.Range(0.85f, 1.15f);
                    currentPixels[i] = new Color(
                        originalPixels[i].r,
                        originalPixels[i].g,
                        originalPixels[i].b,
                        originalPixels[i].a * flickerIntensity
                    );
                }
            }
            
            skyTexture.SetPixels(currentPixels);
            skyTexture.Apply();

            yield return new WaitForSeconds(Random.Range(minFlickerSpeed, maxFlickerSpeed));
        }
    }

    void OnValidate()
    {
        // S'assurer que nous avons toujours au moins quelques étoiles
        numberOfStars = Mathf.Max(100, numberOfStars);
        numberOfBigStars = Mathf.Max(10, numberOfBigStars);
        numberOfHugeStars = Mathf.Max(2, numberOfHugeStars);
    }
}
