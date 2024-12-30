using UnityEngine;
using System.Collections.Generic;

public class PlanetGeneration : MonoBehaviour
{
    [System.Serializable]
    public class ColorPalette
    {
        public List<Color> colors = new List<Color>();
    }

    [Header("Configuration")]
    [SerializeField] private Texture2D baseTexture;

    [SerializeField] private Texture2D[] patternTextures;
    [SerializeField] private Texture2D[] shadowTextures;

    [SerializeField] private Texture2D[] lightTextures;

    [SerializeField] private Texture2D[] finalPatternTextures;

    [Header("Color Configuration")]
    [SerializeField] private ColorPalette[] colorPalettes;

    private SpriteRenderer spriteRenderer;
    private const int TEXTURE_SIZE = 100;

    private void Awake()
    {
        GenerateRandomPlanet();

        // Ajouter un collider circulaire pour les collisions
        CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
        // collider.isTrigger = true;
        // gameObject.tag = "Obstacle";

        // Réduire la taille du sprite de manière aléatoire
        float scale = Random.Range(0.6f, 1f);
        transform.localScale = new Vector3(transform.localScale.x * scale, transform.localScale.y * scale, 1f);
    }

    public void GenerateRandomPlanet()
    {
        // S'assurer que nous avons un SpriteRenderer
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sortingOrder = 1;
            }
        }

        // Créer une nouvelle texture pour notre planète
        Texture2D finalTexture = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE, TextureFormat.RGBA32, false);
        
        // Remplir avec de la transparence
        Color[] clearPixels = new Color[TEXTURE_SIZE * TEXTURE_SIZE];
        for (int i = 0; i < clearPixels.Length; i++)
            clearPixels[i] = Color.clear;
        finalTexture.SetPixels(clearPixels);

        
        // Choisir une palette de couleurs aléatoire
        List<Color> palette = new List<Color>(colorPalettes[Random.Range(0, colorPalettes.Length)].colors);

        // * Base Layer
        // Sélectionner une couleur de base aléatoire
        Color baseColor = palette[Random.Range(0, palette.Count)];
        palette.Remove(baseColor);

        // Appliquer la base
        ApplyLayerToTexture(finalTexture, baseTexture, baseColor);


        // * Pattern Layer

        // Choisir un pattern aléatoire
        Texture2D patternTexture = patternTextures[Random.Range(0, patternTextures.Length)];

        // Choisir une couleur de pattern aléatoire
        Color patternColor = palette[Random.Range(0, palette.Count)];
        palette.Remove(patternColor);

        // Appliquer le pattern
        ApplyLayerToTexture(finalTexture, patternTexture, patternColor);


        // * Shadow Layer

        // Appliquer toutes les shadow layers

        int count = 0;
        foreach (var shadowTexture in shadowTextures)
        {
            float shadowIntensity = Random.Range(0.1f, 0.3f) + count * 0.2f; // Intensité de l'assombrissement
            ApplyShadowToTexture(finalTexture, shadowTexture, shadowIntensity);
            count++;
        }

        // Appliquer les ombres en assombrissant les couleurs existantes
        
        // ApplyShadowToTexture(finalTexture, shadowLayer.possibleTextures[Random.Range(0, shadowLayer.possibleTextures.Length)], shadowIntensity);

        // Appliquer le contour
        ApplyPixelPerfectOutline(finalTexture);


        // * Light Layer

        // Appliquer les light layers

        foreach (var lightTexture in lightTextures)
        {
            float lightRangeIntensity = Random.Range(0.1f, 0.3f);
            ApplyLightToTexture(finalTexture, lightTexture, lightRangeIntensity);
        }

        // Appliquer le pattern final

        if (Random.Range(0, 4) == 0)
        {
            patternColor = palette[Random.Range(0, palette.Count)];
            ApplyLayerToTexture(finalTexture, finalPatternTextures[Random.Range(0, finalPatternTextures.Length)], patternColor);
        }

        // Appliquer les changements
        finalTexture.Apply();

        // Créer un sprite à partir de la texture
        Sprite newSprite = Sprite.Create(finalTexture, new Rect(0, 0, TEXTURE_SIZE, TEXTURE_SIZE), new Vector2(0.5f, 0.5f));
        spriteRenderer.sprite = newSprite;
    }

    private void ApplyLayerToTexture(Texture2D targetTexture, Texture2D layerTexture, Color colorToApply)
    {
        Color[] layerPixels = layerTexture.GetPixels();
        Color[] targetPixels = targetTexture.GetPixels();

        for (int i = 0; i < layerPixels.Length; i++)
        {
            if (layerPixels[i].a > 0)
            {
                // Modification de la façon dont on applique la couleur
                Color newColor = colorToApply;
                // L'opacité finale est basée sur l'opacité de la texture source et la couleur à appliquer
                newColor.a = layerPixels[i].a * colorToApply.a;
                
                // Utiliser un mélange plus doux
                targetPixels[i] = Color.Lerp(targetPixels[i], newColor, newColor.a);
            }
        }

        targetTexture.SetPixels(targetPixels);
    }

    // Nouvelle méthode spécifique pour les ombres
    private void ApplyShadowToTexture(Texture2D targetTexture, Texture2D shadowTexture, float shadowIntensity)
    {
        Color[] shadowPixels = shadowTexture.GetPixels();
        Color[] targetPixels = targetTexture.GetPixels();

        for (int i = 0; i < shadowPixels.Length; i++)
        {
            if (shadowPixels[i].a > 0)
            {
                // Calculer l'intensité de l'ombre pour ce pixel
                float darkening = shadowPixels[i].a * shadowIntensity;
                
                // Assombrir la couleur existante
                Color originalColor = targetPixels[i];
                targetPixels[i] = new Color(
                    originalColor.r * (1 - darkening),
                    originalColor.g * (1 - darkening),
                    originalColor.b * (1 - darkening),
                    originalColor.a
                );
            }
        }

        targetTexture.SetPixels(targetPixels);
    }

    // Nouvelle méthode pour générer un contour pixel perfect
    private void ApplyPixelPerfectOutline(Texture2D targetTexture)
    {
        Color[] pixels = targetTexture.GetPixels();
        Color[] outlinePixels = new Color[pixels.Length];
        System.Array.Copy(pixels, outlinePixels, pixels.Length);

        for (int y = 0; y < TEXTURE_SIZE; y++)
        {
            for (int x = 0; x < TEXTURE_SIZE; x++)
            {
                int currentIndex = y * TEXTURE_SIZE + x;
                
                // Si le pixel actuel n'est pas transparent
                if (pixels[currentIndex].a > 0)
                {
                    // Vérifier les pixels adjacents
                    bool needsOutline = false;

                    // Vérifier pixel à gauche
                    if (x <= 0 || pixels[currentIndex - 1].a == 0)
                        needsOutline = true;
                    // Vérifier pixel à droite
                    else if (x >= TEXTURE_SIZE - 1 || pixels[currentIndex + 1].a == 0)
                        needsOutline = true;
                    // Vérifier pixel au-dessus
                    else if (y >= TEXTURE_SIZE - 1 || pixels[currentIndex + TEXTURE_SIZE].a == 0)
                        needsOutline = true;
                    // Vérifier pixel en-dessous
                    else if (y <= 0 || pixels[currentIndex - TEXTURE_SIZE].a == 0)
                        needsOutline = true;

                    if (needsOutline)
                    {
                        outlinePixels[currentIndex] = Color.black;
                    }
                }
            }
        }

        targetTexture.SetPixels(outlinePixels);
    }

    private void ApplyLightToTexture(Texture2D targetTexture, Texture2D lightTexture, float lightRangeIntensity)
    {
        Color[] lightPixels = lightTexture.GetPixels();
        Color[] targetPixels = targetTexture.GetPixels();

        for (int i = 0; i < lightPixels.Length; i++)
        {
            if (lightPixels[i].a > 0)
            {
                float lightIntensity = lightPixels[i].a * lightRangeIntensity;
                Color originalColor = targetPixels[i];
                
                // Version plus douce qui préserve mieux les couleurs originales
                Color lightColor = Color.Lerp(originalColor, Color.white, lightIntensity);
                targetPixels[i] = new Color(
                    lightColor.r,
                    lightColor.g,
                    lightColor.b,
                    originalColor.a
                );
            }
        }

        targetTexture.SetPixels(targetPixels);
    }

    // Pour tester la génération dans l'éditeur
    [ContextMenu("Generate New Planet")]
    private void GenerateNewPlanet()
    {
        GenerateRandomPlanet();
    }
}
