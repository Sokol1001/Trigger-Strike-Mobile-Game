using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionDieAfterTime : MonoBehaviour
{
    float waitTimer = 5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        waitTimer -= Time.deltaTime;
        if(waitTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
