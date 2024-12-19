using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class TextUtils
{
    public static IEnumerator FadeTextColor(TextMeshProUGUI text, Color startColor, Color targetColor, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            text.color = Color.Lerp(startColor, targetColor, elapsed / duration);
            yield return null;
        }
        text.color = targetColor; // Assure que la couleur finale est exactement celle voulue
    }

    // Version simplifiée pour faire un fade vers transparent
    public static IEnumerator FadeTextToTransparent(TextMeshProUGUI text, float duration)
    {
        Color startColor = text.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        return FadeTextColor(text, startColor, targetColor, duration);
    }
}
