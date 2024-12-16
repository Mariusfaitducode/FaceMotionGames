using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    public float scrollSpeed = 2f;
    private float backgroundWidth;
    private Transform[] backgrounds;
    private float leftBound;

    private StartGame startGame;

    void Start()
    {

        startGame = FindObjectOfType<StartGame>();

        // Récupère tous les enfants pour le scrolling
        backgrounds = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            backgrounds[i] = transform.GetChild(i);
        }

        // Calcule la largeur du background en utilisant le SpriteRenderer
        if (backgrounds[0].GetComponent<SpriteRenderer>() != null)
        {
            backgroundWidth = backgrounds[0].GetComponent<SpriteRenderer>().bounds.size.x;
        }

        // Calcule les limites
        leftBound = -backgroundWidth;
        // rightBound = backgroundWidth * 2; // Position du dernier background
    }

    void Update()
    {

        if (!startGame.startGame)
        {
            return;
        }


        foreach (Transform bg in backgrounds)
        {
            // Déplace chaque arrière-plan vers la gauche
            bg.Translate(Vector3.left * scrollSpeed * Time.deltaTime);

            // Vérifie si l'arrière-plan est sorti à gauche
            if (bg.position.x < leftBound)
            {
                // Trouve la position du background le plus à droite
                float rightmostX = -Mathf.Infinity;
                foreach (Transform otherBg in backgrounds)
                {
                    if (otherBg != bg && otherBg.position.x > rightmostX)
                    {
                        rightmostX = otherBg.position.x;
                    }
                }

                // Place le background juste après le plus à droite
                bg.position = new Vector3(rightmostX + backgroundWidth, bg.position.y, bg.position.z);
            }
        }
    }
}
