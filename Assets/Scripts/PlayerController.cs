using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    // ASSIGNMENTS
    private Animator animator;
    private CharacterController characterController;
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

    // PARAMETERS
    private float health;
    private float rotationSpeed = 5f;
    private float forwardSpeed = 0.3f;
    private float upwardSpeed = 0.3f;
    private float shootDelay = 0.5f;
    private float aimDelay = 1f;
    private float launchForce = 15f;
    private float autoaimSpeed = 5f;
    private int bulletsToShoot = 1;
    public float enemyDamage = 0;
    public float bulletDamage = 1;
    private bool freeShooting = false;

    // BOUNDERS
    private float maxRotation = 75;
    private float maxX = 8.5f;
    private float maxY = 4.8f;

    // AIMS
    private Vector3 targetDirection;
    private Vector3 aim;
    private bool aimed = false;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();        
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
        movX = inputHorizontal * forwardSpeed;
        movY = inputVertical * upwardSpeed;

        characterController.Move(new Vector2(movX, 0));
        characterController.Move(new Vector2(0, movY));

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
        /*
        if (inputHorizontal*last_dir < 0) // not of the same sign -> switch side
        {
            transform.localScale = Vector3.Scale(transform.localScale, new Vector3(-1, 1, 1)); // Pointwise product just to flip the x of localScale
            totalRotation *= -1;
            last_dir *= -1;
        }
        */


        // Check boundaries
        if (Mathf.Abs(transform.position.x) > maxX || Mathf.Abs(transform.position.y) > maxY) {

            gameObject.GetComponent<CharacterController>().enabled = false;

            if (transform.position.x > maxX)
            {
                transform.position = new Vector3(maxX, transform.position.y, 0f);
            }
            if (transform.position.x < -maxX)
            {
                transform.position = new Vector3(-maxX, transform.position.y, 0f);
            }
            if (transform.position.y > maxY)
            {
                transform.position = new Vector3(transform.position.x, maxY, 0f);
            }
            if (transform.position.y < -1*maxY)
            {
                transform.position = new Vector3(transform.position.x, -1*maxY, 0f);
            }

            gameObject.GetComponent<CharacterController>().enabled = true;
        }


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
        bullet_obj = Instantiate(bullet, transform.position, transform.rotation);
        for (int i=1; i < amount; i++)
            bullet_obj = Instantiate(bullet, transform.position, transform.rotation);
            bullet_obj.GetComponent<Rigidbody>().AddForce(aim * launchForce, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(gameManager.GetWonOrLost() == 0) // while still playing
        {
            if (other.gameObject.CompareTag("Enemy"))
            {
                health -= enemyDamage;
            }

            if (other.gameObject.CompareTag("Bullet"))
            {
                other.gameObject.tag = "Untagged";
                health -= bulletDamage;
                Instantiate(explosion, transform.position, Quaternion.identity);
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
