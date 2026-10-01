using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyadd : MonoBehaviour
{
    public GameObject enemy;
    public float spawnRate = 2;
    private float timer = 0;


    // Start is called before the first frame update
    void Start()
    {
        shengchengh();
    }

    // Update is called once per frame
    void Update()
    {
        if (timer < spawnRate)
        {
            timer = timer + Time.deltaTime;
        }
        else
        {
            shengchengh();
            timer = 0;
        }
        
    }
    void shengchengh()
    {
        Instantiate(enemy, transform.position, Quaternion.identity);
        
    }
    
}


