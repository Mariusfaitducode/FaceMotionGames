using UnityEngine;
using System.IO;

public class WhiteToTransparentAndSave : MonoBehaviour
{
    public Texture2D sourceTexture; // Assign your texture in the Inspector

    public Color colorToMakeTransparent;
    public string outputFileName = "TransparentTexture.png"; // Output file name

    void Start()
    {
        if (sourceTexture == null)
        {
            Debug.LogError("Source texture is not assigned.");
            return;
        }

        Texture2D transparentTexture = MakeWhiteTransparent(sourceTexture);

        // Save the texture as a PNG in the Assets folder
        SaveTextureAsPNG(transparentTexture, Application.dataPath + "/" + outputFileName);
        Debug.Log($"Texture saved to: {Application.dataPath}/{outputFileName}");
    }

    Texture2D MakeWhiteTransparent(Texture2D texture)
    {
        Texture2D newTexture = new Texture2D(texture.width, texture.height);
        Color[] pixels = texture.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i] == colorToMakeTransparent)
            {
                pixels[i].a = 0f; // Make fully transparent
            }
        }

        newTexture.SetPixels(pixels);
        newTexture.Apply();

        return newTexture;
    }

    void SaveTextureAsPNG(Texture2D texture, string filePath)
    {
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);
    }
}


