using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    [Header("Nebula Settings")]
    [SerializeField] private Color nebulaColor1 = new Color(0.4f, 0.2f, 0.5f, 0.1f); // Violet foncé
    [SerializeField] private Color nebulaColor2 = new Color(0.2f, 0.3f, 0.5f, 0.1f); // Bleu
    [SerializeField] private Color nebulaColor3 = new Color(0.6f, 0.5f, 0.7f, 0.1f); // Violet clair
    [SerializeField] private float nebulaScale = 50f; // Échelle du Perlin noise pour les détails
    [SerializeField] private float nebulaMaskScale = 200f; // Échelle du Perlin noise pour le masque
    [SerializeField] private float nebulaIntensity = 0.5f;
    [SerializeField] private Vector2 nebulaOffset;
    [SerializeField] private float nebulaMaskThreshold = 0.6f; // Seuil pour le masque

    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 90f; // 1 minute 30
    [SerializeField] private Color endBackgroundColor = new Color(0.01f, 0.01f, 0.02f, 1f);
    [SerializeField] private float starFadeOutDuration = 3f; // Durée de fade pour chaque étoile

    private Texture2D skyTexture;
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    private int width;
    private int height;

    private bool isTransitioning = false;
    private float transitionTimer = 0f;
    private Color initialBackgroundColor;
    private Color[] initialStarColors;

    private struct StarInfo
    {
        public int pixelIndex;
        public float startFadeTime;
        public Color originalColor;
    }

    private List<StarInfo> activeStars = new List<StarInfo>();

    private Vector3 initialPosition;
    private Coroutine translationCoroutine;

    void Start()
    {
        

        // GenerateSky();
        

        // StartCoroutine(TransitionEffect());
    }

    public void GenerateSky()
    {
        mainCamera = Camera.main;
        CalculateScreenDimensions();

        InitializeSky();
        GenerateStars();
        initialBackgroundColor = backgroundColor;
        initialStarColors = skyTexture.GetPixels();
        
        // Identifier toutes les étoiles actives
        for (int i = 0; i < initialStarColors.Length; i++)
        {
            if (initialStarColors[i] != backgroundColor)
            {
                activeStars.Add(new StarInfo
                {
                    pixelIndex = i,
                    startFadeTime = Random.Range(0, transitionDuration - starFadeOutDuration),
                    originalColor = initialStarColors[i]
                });
            }
        }
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

        // Générer le fond avec nébuleuse
        GenerateNebulaBackground();

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

    void GenerateNebulaBackground()
    {
        Color[] pixels = skyTexture.GetPixels();
        
        float[,] noise1 = GeneratePerlinNoiseMap(nebulaScale, 1.0f);
        float[,] noise2 = GeneratePerlinNoiseMap(nebulaScale * 2f, 0.5f);
        float[,] maskNoise = GeneratePerlinNoiseMap(nebulaMaskScale, 0.8f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float maskValue = maskNoise[x, y];
                // Rendre la transition du masque plus nette mais pas trop brutale
                float smoothMask = Mathf.SmoothStep(0, 1, (maskValue - nebulaMaskThreshold) * 2f);
                
                if (maskValue > nebulaMaskThreshold)
                {
                    float noiseValue = (noise1[x, y] + noise2[x, y]) * 0.5f;
                    // Augmenter légèrement le contraste tout en gardant une transition douce
                    noiseValue = Mathf.Pow(noiseValue, 1.7f);

                    if (noiseValue > 0.4f) // Seuil légèrement plus élevé
                    {
                        Color nebulaColor;
                        float colorBlend = noiseValue;
                        
                        // Transitions entre les couleurs avec des seuils plus marqués
                        if (colorBlend < 0.33f)
                        {
                            float t = (colorBlend * 3f);
                            t = Mathf.SmoothStep(0, 1, t);
                            nebulaColor = Color.Lerp(nebulaColor1, nebulaColor2, t);
                        }
                        else if (colorBlend < 0.66f)
                        {
                            float t = (colorBlend - 0.33f) * 3f;
                            t = Mathf.SmoothStep(0, 1, t);
                            nebulaColor = Color.Lerp(nebulaColor2, nebulaColor3, t);
                        }
                        else
                        {
                            float t = (colorBlend - 0.66f) * 3f;
                            t = Mathf.SmoothStep(0, 1, t);
                            nebulaColor = Color.Lerp(nebulaColor3, nebulaColor1, t);
                        }

                        // Intensité plus prononcée mais toujours avec une transition douce
                        float finalIntensity = nebulaIntensity * smoothMask * Mathf.Pow(noiseValue, 1.2f);
                        
                        // Mélange des couleurs avec plus de contraste
                        Color currentColor = pixels[y * width + x];
                        pixels[y * width + x] = new Color(
                            currentColor.r + nebulaColor.r * finalIntensity,
                            currentColor.g + nebulaColor.g * finalIntensity,
                            currentColor.b + nebulaColor.b * finalIntensity,
                            1f
                        );
                    }
                }
            }
        }

        skyTexture.SetPixels(pixels);
    }

    float[,] GeneratePerlinNoiseMap(float scale, float persistence)
    {
        float[,] noiseMap = new float[width, height];
        
        // Offset aléatoire pour variation
        float offsetX = nebulaOffset.x;
        float offsetY = nebulaOffset.y;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Calculer les coordonnées du sample
                float sampleX = (x + offsetX) / scale;
                float sampleY = (y + offsetY) / scale;

                // Générer plusieurs octaves de noise
                float noise = 0f;
                float amplitude = 1f;
                float frequency = 1f;
                
                for (int i = 0; i < 3; i++) // 3 octaves
                {
                    noise += Mathf.PerlinNoise(
                        sampleX * frequency,
                        sampleY * frequency
                    ) * amplitude;

                    amplitude *= persistence;
                    frequency *= 2;
                }

                noiseMap[x, y] = noise;
            }
        }

        return noiseMap;
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

        // Nouvelles validations
        nebulaScale = Mathf.Max(1f, nebulaScale);
        nebulaIntensity = Mathf.Clamp01(nebulaIntensity);
    }

    public void StartTransition()
    {
        if (!isTransitioning)
        {
            isTransitioning = true;
            transitionTimer = 0f;
            StartCoroutine(TransitionEffect());
        }
    }

    IEnumerator TransitionEffect()
    {
        Color[] currentPixels = skyTexture.GetPixels();

        isTransitioning = true;
        
        while (transitionTimer < transitionDuration && isTransitioning)
        {

            transitionTimer += Time.deltaTime;
            float globalProgress = transitionTimer / transitionDuration;

            // Transition très lente du fond
            backgroundColor = Color.Lerp(initialBackgroundColor, endBackgroundColor, globalProgress);

            // Mettre à jour chaque étoile individuellement
            foreach (StarInfo star in activeStars)
            {
                if (transitionTimer >= star.startFadeTime)
                {
                    float starProgress = (transitionTimer - star.startFadeTime) / starFadeOutDuration;
                    starProgress = Mathf.Clamp01(starProgress);

                    if (starProgress < 1)
                    {
                        // Faire scintiller l'étoile pendant qu'elle s'éteint
                        float flicker = 1 + Mathf.Sin(transitionTimer * 5f) * 0.2f * (1 - starProgress);

                        float redShift = (transitionDuration - transitionTimer) / transitionDuration * 2f;
                        
                        // Faire disparaître progressivement
                        currentPixels[star.pixelIndex] = new Color(
                            Mathf.Lerp(star.originalColor.r * redShift, backgroundColor.r, starProgress) * flicker,
                            Mathf.Lerp(star.originalColor.g * 0f, backgroundColor.g, starProgress) * flicker,
                            Mathf.Lerp(star.originalColor.b * 0f, backgroundColor.b, starProgress) * flicker,
                            1f
                        );
                    }
                    else
                    {
                        currentPixels[star.pixelIndex] = backgroundColor;
                    }
                }
                else
                {
                    // Faire scintiller légèrement les étoiles qui n'ont pas encore commencé à s'éteindre
                    // float flicker = 1 + Mathf.Sin(transitionTimer * 3f) * 0.1f;
                    
                    // Pour environ 30% des étoiles, faire dériver la couleur vers le rouge
                    if (star.pixelIndex % 3 == 0) {
                        float redShift = Random.Range(0f, 0.01f); // Oscillation lente
                        currentPixels[star.pixelIndex] = new Color(
                            star.originalColor.r + redShift,
                            star.originalColor.g,
                            star.originalColor.b,
                            1f
                        );
                    } 
                }
            }

            skyTexture.SetPixels(currentPixels);
            skyTexture.Apply();

            yield return null;
        }

        // État final
        // Color[] finalPixels = new Color[currentPixels.Length];
        // for (int i = 0; i < finalPixels.Length; i++)
        // {
        //     finalPixels[i] = endBackgroundColor;
        // }
        // skyTexture.SetPixels(finalPixels);
        // skyTexture.Apply();

        isTransitioning = false;
    }


    public void StopTransition()
    {
        isTransitioning = false;
        transitionTimer = 0f;
    }

    // Méthode pour vérifier si la transition est terminée
    public bool IsTransitionComplete()
    {
        return !isTransitioning && transitionTimer >= transitionDuration;
    }

    public void InitializePositions()
    {
        initialPosition = transform.position;
    }

    public void TranslateSky(Vector3 targetOffset, float duration, AnimationCurve translationCurve = null)
    {
        if (translationCoroutine != null)
        {
            StopCoroutine(translationCoroutine);
        }
        translationCoroutine = StartCoroutine(TranslateCoroutine(targetOffset, duration, translationCurve));
    }

    private IEnumerator TranslateCoroutine(Vector3 targetOffset, float duration, AnimationCurve translationCurve)
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = initialPosition + targetOffset;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            if (translationCurve != null)
            {
                progress = translationCurve.Evaluate(progress);
            }

            transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
            yield return null;
        }

        transform.position = targetPosition;
    }

    public void ResetPosition(float duration = 0)
    {
        TranslateSky(Vector3.zero, duration);
    }
}
