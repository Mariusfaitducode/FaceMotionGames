using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum TileType
{
    Normal,
    Long
}

public class TileObject : MonoBehaviour
{
    private float speed = 0f;
    private TileType tileType;
    private float width;

    public float Width => width;
    public TileType Type => tileType;

    public void Initialize(TileType type, float speed)
    {
        tileType = type;
        this.speed = speed;
        if(type == TileType.Long)
        {
            width = Random.Range(3f, 6f);
            transform.localScale = new Vector3(width, 0.6f, 1f);
        }
        else
        {
            width = 1f;
        }
    }
    
    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);
        
        if (transform.position.x < -12f)
        {
            Destroy(gameObject);
        }
    }
}
