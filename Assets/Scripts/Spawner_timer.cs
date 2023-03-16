using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner_timer : MonoBehaviour
{
    public GameObject enemy;
    private GameObject enemy_spawned;

    // PARAMS
    public int quantity = 5;   // number of enemies to spawn
    public float interval_spawning = 3;  // time interval between spawns
    public int enemies = 0;    // number of enemies spawned
    // ENEMY PARAMS
    private float enemyHealth;
    private float enemySize;
    private float enemySpeed;
    public bool active = false;
    private bool started = false;
    


    IEnumerator spawn_coroutine;

    private void Start()
    {
        active = false;
        started = false;
    }

    private void Update()
    {
        if (active && !started)
        {
            started = true;
            spawn_coroutine = spawn_wait(interval_spawning);
            StartCoroutine(spawn_coroutine);
        }
        if (enemies >= quantity && active)
        {
            active = false;
            StopCoroutine(spawn_coroutine);
            //Debug.Log("Spawner stopped");
        }

    }

    void EnemySpawn()
    {
        enemy_spawned = Instantiate(enemy, transform);
        enemy_spawned.GetComponent<EnemyController>().SetEnemyParams(enemyHealth, enemySpeed, enemySize);
    }

    public void SetParams(float en_health = 1, float en_speed = 1.5f, float en_size = 1, int en_quantity = 5, float en_interval = 3)
    {
        quantity = en_quantity;
        interval_spawning = en_interval;
        SetEnemyParams(en_health, en_speed, en_size);
    }

    public void StartSpawner()
    {
        active = true;
    }

    public void SetEnemyParams(float en_health, float en_speed, float en_size)
    {
        enemyHealth = en_health;
        enemySpeed = en_speed;
        enemySize = en_size;
    }

    IEnumerator spawn_wait(float interval_s)
    {
        while (active)
        {
            yield return new WaitForSeconds(interval_s);
            EnemySpawn();
            enemies++;
        }
    }
}