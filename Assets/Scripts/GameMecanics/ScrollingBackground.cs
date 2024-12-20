using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    private float scrollSpeed = 2f;

    private Vector3 scrollingDirection = new Vector3(1, 0, 0);


    private float backgroundWidth;
    private Transform[] backgrounds;
    private float leftBound;

    // private StartGame startGame;

    public bool isScrolling = false;

    void Start()
    {

        // startGame = FindObjectOfType<StartGame>();

        // // Récupère tous les enfants pour le scrolling
        // backgrounds = new Transform[transform.childCount];
        // for (int i = 0; i < transform.childCount; i++)
        // {
        //     backgrounds[i] = transform.GetChild(i);
        // }

        // Calcule la largeur du background en utilisant le SpriteRenderer
        // if (backgrounds[0].GetComponent<SpriteRenderer>() != null)
        // {
        //     backgroundWidth = backgrounds[0].GetComponent<SpriteRenderer>().bounds.size.x;
        // }

        // // Calcule les limites
        // leftBound = -backgroundWidth;
        // rightBound = backgroundWidth * 2; // Position du dernier background
    }


    public void StartScrolling(float speed, Vector3 direction, GameObject initialBackground){
        isScrolling = true;
        scrollSpeed = speed;
        scrollingDirection = direction;


        // Initialize background copies

        backgroundWidth = initialBackground.GetComponent<SpriteRenderer>().bounds.size.x;
        leftBound = -backgroundWidth;

        // Initialize backgrounds array with 3 copies
        backgrounds = new Transform[3];
        
        // Add initial background
        backgrounds[0] = initialBackground.transform;
        
        // Create and position two duplicates
        for (int i = 1; i < 3; i++) {
            GameObject duplicate = Instantiate(initialBackground, transform);
            duplicate.transform.position = backgrounds[i-1].position + new Vector3(backgroundWidth, 0, 0);
            backgrounds[i] = duplicate.transform;
        }
    }

    public void StopScrolling(){
        isScrolling = false;
    }

    public void UpdateScrollingSpeed(float speed){
        scrollSpeed = speed;
    }

    public void UpdateScrollingDirection(Vector3 direction){
        scrollingDirection = direction;
    }

    void Update()
    {

        // if (!startGame.startGame)
        // {
        //     return;
        // }

        if (isScrolling)
        {
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
                    bg.position = new Vector3(rightmostX + backgroundWidth - 0.1f, bg.position.y, bg.position.z + 0.1f);
                }
            }
        }
    }
}
