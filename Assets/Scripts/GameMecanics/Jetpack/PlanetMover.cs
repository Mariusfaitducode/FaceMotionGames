using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetMover : MonoBehaviour
{
    public float speed;
    private float destroyXPosition;
    private float amplitude;
    private float frequency;
    private float startY;
    private float timeOffset;

    public void Initialize(float speed, float destroyXPosition, float amplitude, float frequency)
    {
        this.speed = speed;
        this.destroyXPosition = destroyXPosition;
        this.amplitude = amplitude;
        this.frequency = frequency;
        this.startY = transform.position.y;
        this.timeOffset = Random.Range(0f, 2f * Mathf.PI); // Offset aléatoire pour varier le mouvement
    }

    private void Update()
    {
        // Mouvement horizontal
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        // Mouvement oscillatoire vertical
        float newY = startY + amplitude * Mathf.Sin((Time.time + timeOffset) * frequency);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        if (transform.position.x < destroyXPosition)
        {
            Destroy(gameObject);
        }
    }
}
