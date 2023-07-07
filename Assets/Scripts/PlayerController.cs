using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    // ASSIGNMENTS
    private Animator animator;
    //private CharacterController characterController;
    private Rigidbody2D rigidBody;
    private GameManager gameManager;
    public Animator engine_animator;
    public GameObject explosion;
    public GameObject bullet;
    private GameObject target;

    // INPUTS
    private float inputRotation;
    private float inputVertical;
    private float inputHorizontal;

    // AUX
    private float totalRotation = 0;
    private int last_dir = 1; // 1 = right, -1 = left
    private float movX, movY;
    private float forceAmount;

    // PARAMETERS
    private float health;
    private float rotationSpeed = 5f;
    private float playerSpeed = 5f;
    private float forceMultiplier = 100f;
    private float shootDelay = 0.5f;
    private float aimDelay = 1f;
    private float launchForce = 15f;
    private float autoaimSpeed = 5f;
    public int bulletsToShoot = 1;
    public float enemyDamage = 0;
    public float bulletDamage = 1;
    private bool freeShooting = false;
    private float bullet_offset = 0.5f; // offset to spawn the bullet where the weapon is

    // BOUNDERS
    private float maxRotation = 75;

    // AIMS
    private Vector2 targetDirection;
    private Vector2 aim;
    private bool aimed = false;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        animator = GetComponent<Animator>();
        //characterController = GetComponent<CharacterController>();      
        rigidBody = GetComponent<Rigidbody2D>();     

        forceAmount = playerSpeed * forceMultiplier;
    }

    // Update is called once per frame
    void Update()
    {
        inputVertical = Input.GetAxis("Vertical");
        inputHorizontal = Input.GetAxis("Horizontal");
        inputRotation = Input.GetAxis("Rotation");

        // SHOOTING
        if(Input.GetKeyDown(KeyCode.Space) && freeShooting)
        {
            Fire(bulletsToShoot);
        }
    }

    private void FixedUpdate()
    {
        // MOVEMENT
        movX = inputHorizontal;
        movY = inputVertical;

        rigidBody.velocity = Vector2.zero; 
        rigidBody.angularVelocity = 0; 
        Vector2 force = new Vector2(movX, movY) * forceAmount;
        rigidBody.AddForce(force);

        // Calculate and update new orientation
        if(last_dir < 0)
        {
            //inputRotation = -inputRotation;
        }
        totalRotation += last_dir*(inputRotation * rotationSpeed);

        if (totalRotation > maxRotation)
        {
            totalRotation = maxRotation;
        }
        else if (totalRotation < -maxRotation)
        {
            totalRotation = -maxRotation;
        }
        transform.rotation = Quaternion.Euler(0f, 0f, totalRotation);

        // AIM
        if (last_dir == 1)
        {
            aim = transform.right;
        }
        else if (last_dir == -1)
        {
            aim = -transform.right;
        }

        // AUTODIRECTION
        if (target == null)
        {
            target = FindCloseDistance();
        } else
        {
            Debug.DrawRay(transform.position, target.transform.position - transform.position, Color.red);
            targetDirection = (target.transform.position - transform.position).normalized;

            if (targetDirection.x * last_dir < 0) // If target is on the other direction
            {
                transform.localScale = Vector3.Scale(transform.localScale, new Vector3(-1, 1, 1)); // Pointwise product just to flip the x of localScale
                totalRotation *= -1;
                last_dir *= -1;
            }

            // AUTOAIM AND FIRE
            if (aimed == false)
            {
                if (targetDirection.y - aim.y > 0)
                {
                    totalRotation += last_dir * (0.5f * autoaimSpeed);
                }
                else if (targetDirection.y - aim.y < 0)
                {
                    totalRotation += last_dir * (-0.5f * autoaimSpeed);
                }

                if (Mathf.Abs(targetDirection.y - aim.y) < 0.1)
                {
                    StartCoroutine(wait_and_fire(shootDelay, aimDelay, bulletsToShoot));
                    aimed = true;
                }
            }
        }
               

        // DEATH
        if (health <= 0)
        {
            gameManager.IDied(gameObject.name, gameObject.tag, "zero_health");
            Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
        

    }

    public void SetParams(float playerHealth, float playerShootDelay, float playerAimDelay, float playerLaunchForce, float playerAutoaimSpeed, int playerBulletsToShoot, bool playerFreeShooting)
    {
        health = playerHealth;
        shootDelay = playerShootDelay;
        aimDelay = playerAimDelay;
        launchForce = playerLaunchForce;
        autoaimSpeed = playerAutoaimSpeed;
        bulletsToShoot = playerBulletsToShoot;
        freeShooting = playerFreeShooting;
    }

    public float GetHealth()
    {
        return health;
    }

    public GameObject FindCloseDistance()
    {
        GameObject closestEnemy = null;
        float closestDistance;
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag("Enemy");

        if (taggedObjects.Length != 0)
        {
            closestDistance = Vector3.Distance(transform.position, taggedObjects[0].transform.position);
            closestEnemy = taggedObjects[0];
            for (int i = 0; i < taggedObjects.Length; i++)
            {
                if (Vector3.Distance(transform.position, taggedObjects[i].transform.position) <= closestDistance)
                {
                    closestEnemy = taggedObjects[i];
                }
            }
        }
        
        return closestEnemy;
    }

    private void Fire(int amount)
    {
        animator.Play("shoot", 0, 0.25f);
        GameObject bullet_obj;
        Vector2 spawnPosition = transform.position + bullet_offset * transform.right;

        if(amount > 0)
        {
            bullet_obj = Instantiate(bullet, spawnPosition, transform.rotation);
            for (int i=1; i < amount-1; i++)
                bullet_obj = Instantiate(bullet, spawnPosition, transform.rotation);
                bullet_obj.GetComponent<Rigidbody2D>().AddForce(aim * launchForce, ForceMode2D.Impulse);
        }
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(gameManager.GetWonOrLost() == 0) // while still playing
        {
            if (other.gameObject.CompareTag("BulletPlayerDamage"))
            {
                // the collider of the projectile is set to not collide with the player to not create uncontrolled forces on the player
                // instead, a child object of the projectile handles the collisions with the player, therefore the collider is of a child of the bullet
                GameObject bullet = other.transform.parent.gameObject;
                if (bullet.GetComponent<Bullet>().GetLife() < 1) // if the bullet exists since less than 1 second
                {
                    return;
                }
                else
                {
                    other.gameObject.tag = "Untagged";
                    health -= bulletDamage;
                    Instantiate(explosion, transform.position, Quaternion.identity);

                    Destroy(bullet); 
                }
            }

            if (other.gameObject.CompareTag("Enemy"))
            {
                health -= enemyDamage;
            }
        }        
    }

    IEnumerator wait_and_fire(float shoot_delay, float aim_delay, int amount)
    {
        yield return new WaitForSeconds(shoot_delay);
        Fire(amount);
        yield return new WaitForSeconds(aim_delay);
        aimed = false;
    }
}
