using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // ASSIGNATIONS
    private Renderer objectRenderer;
    private GameManager gameManager;
    private GameObject playerObj;
    private GameObject belonging_uienemy;
    public GameObject explosion;
    public Vector3 default_scale;
    private bool instantiation = true; // to check if instantiation already happened and we are only modifying parameters

    // PARAMETERS BASELINE (WILL BE MULTIPLIED AND ACTUATED IN THE SETTING PARAMETERS FUNCTION
    private float health = 1;
    private float speed = 1;
    private float enemySize = 1;
    
    // BOUNDERS
    public bool insidebound = false;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer.IsVisibleFrom(Camera.main))
            {
                gameObject.tag = "Enemy";
            } else
            {
                gameObject.tag = "OutboundEnemy";
            }
    }

    // Update is called once per frame
    void Update()
    {
        playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, playerObj.transform.position, speed * Time.deltaTime);

            /*
            if (Mathf.Abs(transform.position.x) > maxX || Mathf.Abs(transform.position.y) > maxY)
            {
                insidebound = false;
                gameObject.tag = "OutboundEnemy";
            }
            else
            {
                insidebound = true;
                gameObject.tag = "Enemy";
            }
            */

            if (objectRenderer.IsVisibleFrom(Camera.main))
            {
                gameObject.tag = "Enemy";
            } else
            {
                gameObject.tag = "OutboundEnemy";
            }

            // DEATH
            if (health <= 0)
            {
                Instantiate(explosion, transform.position, Quaternion.identity);
                Die("dunno");
            }
        }
        else
        {
            SetEnemyParams();

            GameObject[] uienemies = GameObject.FindGameObjectsWithTag("GUIEnemy");
            for (int i = 0; i < uienemies.Length; i++)
                if (uienemies[i].name == gameObject.name.Substring(0, 2)) // The name in game will be for example "e1(Clone)". The actual enemy name is only the first two char.
                {
                    belonging_uienemy = uienemies[i];
                }
            transform.position = Vector2.MoveTowards(transform.position, belonging_uienemy.transform.position, speed * Time.deltaTime);
        }
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (gameObject.tag == "Enemy") // If enemy is inside bounds and game is not finished (SavedEnemy tag)
        {
            if (other.gameObject.CompareTag("Bullet"))
            {
                //Debug.Log("Hit by bullet!");
                Instantiate(explosion, transform.position, Quaternion.identity);
                other.gameObject.tag = "Untagged";
                health -= 1;
                Destroy(other.gameObject);
                if (health <= 0)
                {
                    Die("bullet");
                }
            }

            if (other.gameObject.CompareTag("Player"))
            {
                Instantiate(explosion, transform.position, Quaternion.identity);
                Die("player_contact");   
                gameObject.tag = "DeadEnemy";             
            }
        }        
    }

    public void SetEnemyParams(float en_health = 1, float en_speed = 1.5f, float en_size = 1)  // (Default = 1, 1.5, 1)
    {
        if (instantiation) // If it's the first time we are setting them
        {
            default_scale = transform.localScale;
            instantiation = false;
        }

        health = en_health;

        speed = en_speed;

        enemySize = en_size;
        transform.localScale = default_scale * enemySize;
    }

    private void Die(string how)
    {
        gameManager.IDied(gameObject.name, gameObject.tag, how);
        Destroy(gameObject);
    }
}
