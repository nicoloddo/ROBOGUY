using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float timeLife = 10f; // Time life of bullet
    private float timerLife; // Timer for calculate current time life of bullet

    private Vector2 velocity;

    // Start is called before the first frame update
    void Start()
    {

    }

    void FixedUpdate()
    {    
        // Always point forward
        velocity = gameObject.GetComponent<Rigidbody2D>().velocity;
        if (velocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }        
        

        timerLife += Time.deltaTime; // Timer to autodestruct bullet

        if (timerLife >= timeLife) // If timer is ended
        {
            Destroy(gameObject);
        }
    }

    public float GetLife()
    {
        return timerLife;
    }
}

