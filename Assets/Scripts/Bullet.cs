using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float timeLife = 10f; // Time life of bullet
    private bool coll; // If collision happened
    private float timerLife; // Timer for calculate current time life of bullet

    public float launchForce = 5f;
    private bool bulletActive = false; // Wait to leave the player's collider before being active

    private Transform player_transform;
    //private Vector2 direction;
    public Vector3 velocity;

    // BOUNDERS
    private float maxX = 8.5f;
    private float maxY = 4.8f;

    // AUX 
    private int count_updates = 0;
    private bool changed_traj = false;

    // Start is called before the first frame update
    void Start()
    {
        player_transform = GameObject.FindGameObjectWithTag("Player").transform;
        bulletActive = false;
        gameObject.tag = "Untagged";
        count_updates = 0;
        changed_traj = false;
    }

    void FixedUpdate()
    {
        count_updates++;
        if(count_updates == 10) // to have a treshold before checking bounces again
        {
            count_updates = 0;
            changed_traj = false; 
        }

        velocity = gameObject.GetComponent<Rigidbody>().velocity;
        if (velocity != Vector3.zero)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        
        // bouncing
        if (Mathf.Abs(transform.position.x) > maxX || Mathf.Abs(transform.position.y) > maxY && !changed_traj)
        {
            if (Mathf.Abs(transform.position.x) > maxX)
            {
                gameObject.GetComponent<Rigidbody>().velocity = new Vector3(-velocity.x, velocity.y, 0f);
            }
            if (Mathf.Abs(transform.position.y) > maxY)
            {
                gameObject.GetComponent<Rigidbody>().velocity = new Vector3(velocity.x, -velocity.y, 0f);
            }
            changed_traj = true;
        }
        

        if (coll == false) // Until collision
        {
            timerLife += Time.deltaTime; // Timer to autodestruct bullet

            if (timerLife >= timeLife) // If timer is ended
            {
                Destroy(gameObject);
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if(bulletActive)
        {
            //Debug.Log("Bullet on target!");

            if (other.gameObject.CompareTag("Enemy"))
            {
                coll = true; //Collision happened
                Destroy(gameObject);
            }

            if (other.gameObject.CompareTag("Player"))
            {
                coll = true; // Collision happened
                Destroy(gameObject);
            }            
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            bulletActive = true; // We activate the bullet after it leaves player's collider
            gameObject.tag = "Bullet";
        }
    }

}
