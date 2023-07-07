using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // ASSIGNMENTS
    public GameObject spawner, spawner1, spawner2, spawner3, spawner4, spawner5;
    public GameObject player;
    private GameObject[] spawners;
    public int enemies_count;
    private float player_current_health;
    public float game_time;
    public float difficulty_time;
    public bool auto_difficulty;
    public PlayerModel playerModel;


    // PARAMETERS (The ones below are the DEFAULT. CHANGE THEM ONLY IN THE EDITOR)
    private string[] spawners_type = { "e1", "b2", "e2", "e3", "e4", "b1" }; // enemies type ordered by belonging spawner
    private int[] enemies_distribution = { 4, 2, 2, 2, 2, 3 }; // amount of enemies to be spawned ordered by type (and therefore by belonging spawner)
    private float[] spawning_interval_offset = { -0.15f, 0, 0, 0, 0, 0 }; // if you want a specific spawner to be relatively faster specify it here in order of spawner

    private int[] enemies_value = { 4, 15, 6, 2, 1, 12 }; // score awarded when you make one specific enemy survive
    public int[] remaining_enemies_distribution = { 0, 0, 0, 0, 0, 0 };
    
    private float enemyHealth = 1f;
    public float enemySize = 1f;
    public float enemySpeed = 1.5f;
    public int enemiesAmount = 15;  // This to be presented in settings as [few, some, a lot, too many] because it will not always be accurate: depends on the distribution
    public float generalSpawningLevel = 45; // Spawning level divided by the enemies amount gives the interval seconds of spawn.
    // For example 45/15 means one enemy spawned each 3 seconds. If we have 30 enemies, spawning will happen faster 1 each 1.5 seconds.

    private float playerHealth = 4; // Total health of player
    public float playerAutoaimSpeed = 5f; // Autoaim speed and shoot delay both manipulate the fastness of aiming and shooting. Should manipulated together.
    public float playerShootDelay = 0.5f; // Amount of time to shoot after aiming.
    public float playerAimDelay = 1; // Amount of time to pass before aiming and shooting again. Making it small make the character to shoot a lot because it's fastly reaiming and shooting.
    public float playerLaunchForce = 15f;
    
    private int playerBulletsToShoot = 1; // !!! Needs to be fixed before being manipulable: KEEP = 1.
    public bool playerFreeShooting = false;

    // SETTINGS TO MANIPULATE PARAMETERS (some are not manipulated directly)
    public int robotAttackLevel = 3; // this manipulates autoaim speed, shoot delay, aim delay and launch force
    // NOT COMBINATORY PARAMETERS (ALREADY DECLARED):
    // enemy size
    // enemy speed
    // enemy amount

    // DIFFICULTY PARAMETER
    public float difficultyLevel = 1.5f;
    private float max_difficulty = 3;
    private float min_difficulty = 1;
    private float difficultyModifier = 0;
    public float instant_difficulty_modifier = 0; // The modifier if the game finished now
    public int increase_threshold = 2; // After how many good games we should increase difficulty
    public int decrease_threshold = 2; // After how many bad games we should decrease difficulty

    // AUX
    private int actualEnemiesTotal = 0; // see the comment on the enemiesAmount
    private int score = 0;
    private int prescore = 0;
    private bool won = false;
    private bool lost = false;

    // Start is called before the first frame update
    void Start()
    {
        playerModel = FindObjectOfType<PlayerModel>();
        int max_prescore = 100;
        playerModel.SetMax((int)playerHealth, max_prescore);

        if (PlayerPrefs.HasKey("Difficulty"))
        {
            difficultyLevel = PlayerPrefs.GetFloat("Difficulty");
        }
        else
        {
            difficultyLevel = 1.5f;
        }

        if (PlayerPrefs.HasKey("AutoDifficultyIsOn"))
        {
            switch (PlayerPrefs.GetInt("AutoDifficultyIsOn"))
            {
                case 0:
                    auto_difficulty = false;
                    break;
                case 1:
                    auto_difficulty = true;
                    break;
            }
        }
        else
        {
            auto_difficulty = true;
        }

        won = false;
        lost = false;

        spawners = new[]{spawner, spawner1, spawner2, spawner3, spawner4, spawner5};

        // Setting default parameters
        playerHealth = 4;
        player_current_health = playerHealth;

        // Setting difficulty
        difficultyModifier = 0;
        SetDifficultyParams();
        playerModel.SetSuggestedLevelIfNot(difficultyLevel);

        if (player != null)
            player.GetComponent<PlayerController>().SetParams(player_current_health, playerShootDelay, playerAimDelay, playerLaunchForce, playerAutoaimSpeed, playerBulletsToShoot, playerFreeShooting);

        enemies_count = 0;
        int distribution_sum = 0;
        foreach (int en_n in enemies_distribution) distribution_sum += en_n;

        actualEnemiesTotal = 0;
        for (int i = 0; i < spawners.Length; i++)
        {
            int amount = (int)Mathf.Round((float)enemies_distribution[i] / (float)distribution_sum * enemiesAmount);
            float general_interval = (generalSpawningLevel / enemiesAmount);
            float interval = general_interval + (spawning_interval_offset[i] * general_interval);
            spawners[i].GetComponent<Spawner_timer>().SetParams(enemyHealth, enemySpeed, enemySize, amount, interval);
            spawners[i].GetComponent<Spawner_timer>().StartSpawner();
            //Debug.Log("Starting spawner " + i.ToString() + " with " + amount.ToString() +  " enemies to spawn and " + interval.ToString() + " seconds of interval.");
            actualEnemiesTotal += amount;
        }
        //Debug.Log("Total enemies to be spawned: " + actualEnemiesTotal.ToString());
        enemies_count = actualEnemiesTotal;
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
            player_current_health = player.GetComponent<PlayerController>().GetHealth();
        else
            player_current_health = -1; // if player is null send error number

        game_time += Time.deltaTime;
        difficulty_time += Time.deltaTime;

        if (GetWonOrLost() == 0) // If game is not ended
            instant_difficulty_modifier = (difficultyModifier + (difficultyLevel * difficulty_time)) / game_time; // The modifier if the game finished now
    }

    public float GetPlayerHealth()
    {
        return player_current_health;
    }
    public float GetMaxPlayerHealth()
    {
        return playerHealth;
    }

    public int GetEnemiesCount()
    {
        return enemies_count;
    }
    public int GetEnemiesTotal()
    {
        return actualEnemiesTotal;
    }
    public int[] GetRemainingEnemiesDistribution()
    {
        return remaining_enemies_distribution;
    }
    public int[] GetEnemiesValues()
    {
        return enemies_value;
    }

    public float GetDifficultyLevel()
    {
        return difficultyLevel;
    }

    public float GetDifficultyModifier()
    {
        return difficultyModifier;
    }

    public float GetInstantDifficultyModifier()
    {
        return instant_difficulty_modifier;
    }

    public bool GetAutoDifficultyIsOn()
    {
        return auto_difficulty;
    }

    public int GetWonOrLost()
    {
        if (won)
            return 1;
        if (lost)
            return -1;
        return 0;
    }
    public int[] GetScore()
    {
        return new[] { prescore, score };
    }

    private void SetDifficultyParams()
    {
        switch(difficultyLevel)
        {
            case 1: // easy
                robotAttackLevel = 1;
                SetRobotLevelParams();
                enemySize = 0.7f;
                enemySpeed = 0.8f;
                break;
            case 1.5f:
                robotAttackLevel = 2;
                SetRobotLevelParams();
                enemySize = 0.7f; //0.7
                enemySpeed = 1f; //1
                break;
            case 2: // medium
                robotAttackLevel = 3;
                SetRobotLevelParams();
                enemySize = 1f; //1
                enemySpeed = 1.5f; //1.5
                break;
            case 2.5f:
                robotAttackLevel = 3;
                SetRobotLevelParams();
                enemySize = 1.1f;
                enemySpeed = 1.65f;
                break;
            case 3: //hard
                robotAttackLevel = 4;
                SetRobotLevelParams();
                enemySize = 1.2f; //1.2
                enemySpeed = 1.8f; //1.8
                break;
        }
    }

    public void SetDifficulty(float difficulty)
    {
        difficultyModifier += difficultyLevel * difficulty_time;
        difficulty_time = 0;

        difficultyLevel = difficulty;
        PlayerPrefs.SetFloat("Difficulty", difficulty);
        SetDifficultyParams();
        
        // update player
        if (player != null)
            player.GetComponent<PlayerController>().SetParams(player_current_health, playerShootDelay, playerAimDelay, playerLaunchForce, playerAutoaimSpeed, playerBulletsToShoot, playerFreeShooting);

        // update spawners
        for (int i = 0; i < spawners.Length; i++)
        {
            spawners[i].GetComponent<Spawner_timer>().SetEnemyParams(enemyHealth, enemySpeed, enemySize);
        }

        // update enemies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].GetComponent<EnemyController>().SetEnemyParams(enemyHealth, enemySpeed, enemySize);
        }
        enemies = GameObject.FindGameObjectsWithTag("OutboundEnemy");
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].GetComponent<EnemyController>().SetEnemyParams(enemyHealth, enemySpeed, enemySize);
        }
    }

    private void SetRobotLevelParams()
    {
        switch(robotAttackLevel)
        {
            case 1: // rookie
                playerAutoaimSpeed = 2f; 
                playerShootDelay = 1f; 
                playerAimDelay = 1f;
                playerLaunchForce = 8f;
                break;
            case 2: // ok
                playerAutoaimSpeed = 4f;
                playerShootDelay = 0.8f;
                playerAimDelay = 1f;
                playerLaunchForce = 12f;
                break;
            case 3: // default
                playerAutoaimSpeed = 5f;
                playerShootDelay = 0.5f;
                playerAimDelay = 1f;
                playerLaunchForce = 15f;
                break;
            case 4: // powerful
                playerAutoaimSpeed = 10f;
                playerShootDelay = 0.3f;
                playerAimDelay = 1f;
                playerLaunchForce = 16f;
                break;
            case 5: // mayhem
                playerAutoaimSpeed = 15f;
                playerShootDelay = 0.1f;
                playerAimDelay = 1f;
                playerLaunchForce = 20f;
                break;
        }
    }

    public void SetAutoDifficulty(bool isOn)
    {
        auto_difficulty = isOn;
        if (auto_difficulty)
        {
            PlayerPrefs.SetInt("AutoDifficultyIsOn", 1);
            SetDifficulty(playerModel.GetSuggestedLevel());
        }
        else
        {
            PlayerPrefs.SetInt("AutoDifficultyIsOn", 0);
        }
    }

    public void IDied(string who, string tag, string how)
    {
        switch (tag)
        {
            case "Enemy":
                enemies_count--;
                if(enemies_count == 0)
                {
                    GameOver();
                    EndGame(false);
                }
                break;
            case "Player":
                YouWon();
                EndGame(true);
                break;
            case "OutboundEnemy":
                Debug.Log("An outbound enemy died?");
                break;
        }
    }
    private void AdaptDifficulty()
    {
        float average_threshold = (increase_threshold + decrease_threshold) / 2;
        int count_diff = Mathf.Abs(playerModel.GetIncreaseCount() - playerModel.GetDecreaseCount());

        if (count_diff < average_threshold) // If the player model is undecided, nothing changes
        {
            return;
        }

        if (playerModel.GetIncreaseCount() >= increase_threshold)
        {
            float new_difficulty = RoundToDot5(playerModel.GetAverageDiffModifier() + 0.5f);
            if (new_difficulty > max_difficulty)
            {
                new_difficulty = max_difficulty;
            }

            playerModel.ResetSessionData();
            playerModel.SetSuggestedLevel(new_difficulty);
            if (auto_difficulty)
            {
                difficultyLevel = new_difficulty;
                PlayerPrefs.SetFloat("Difficulty", difficultyLevel);
            } 
        }
        if (playerModel.GetDecreaseCount() >= decrease_threshold)
        {
            float new_difficulty = RoundToDot5(playerModel.GetAverageDiffModifier() - 0.5f);
            if (new_difficulty < min_difficulty)
            {
                new_difficulty = min_difficulty;
            }

            playerModel.ResetSessionData();
            playerModel.SetSuggestedLevel(new_difficulty);
            if (auto_difficulty)
            {
                difficultyLevel = new_difficulty;
                PlayerPrefs.SetFloat("Difficulty", difficultyLevel);
            }
            
        }
    }
    private float RoundToDot5(float n)
    {
        int aux = (int)(n * 2 + 0.5f);
        return (float)aux / 2;
    }

    private void CalculateScore()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("SavedEnemy");
        for (int i = 0; i < enemies.Length; i++)
        {
            int enemy_index = System.Array.IndexOf(spawners_type, enemies[i].name.Substring(0, 2)); // The name in game will be for example "e1(Clone)". The actual enemy name is only the first two char.
            score += enemies_value[enemy_index];
            remaining_enemies_distribution[enemy_index] += 1;
        }
        enemies = GameObject.FindGameObjectsWithTag("OutboundEnemy");
        for (int i = 0; i < enemies.Length; i++)
        {
            int enemy_index = System.Array.IndexOf(spawners_type, enemies[i].name.Substring(0, 2)); // The name in game will be for example "e1(Clone)". The actual enemy name is only the first two char.
            score += enemies_value[enemy_index];
            remaining_enemies_distribution[enemy_index] += 1;
        }
        prescore = score;
        score = (int)System.Math.Ceiling(score * difficultyModifier); // Round up the score to the greater number
    }

    private void Save()
    {
        int record = PlayerPrefs.GetInt("Record");
        if (score > record)
        {
            PlayerPrefs.SetInt("Record", score);            
        }

        PlayerPrefs.SetInt("FirstTime", 0);
        
        if(auto_difficulty)
            PlayerPrefs.SetInt("AutoDifficultyIsOn", 1);
        else
            PlayerPrefs.SetInt("AutoDifficultyIsOn", 0);

        PlayerPrefs.Save();
    }
    public void YouWon()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].tag = "SavedEnemy";
        }

        Debug.Log("You won!");
    }

    public void GameOver()
    {
        Debug.Log("Game Over.");
    }

    private void EndGame(bool won_bool)
    {
        difficultyModifier += difficultyLevel * difficulty_time;
        difficultyModifier /= game_time;

        if (won_bool)
            CalculateScore();
        else
        {
            prescore = 0;
            score = 0;
        }
        playerModel.AddSessionData((int)player_current_health, prescore, instant_difficulty_modifier);
        Save();

        won = won_bool;
        lost = !won_bool;

        AdaptDifficulty();
    }

    public void RestartGame()
    {
        game_time = 0;
        difficulty_time = 0;
        
        AdaptDifficulty();
        SceneManager.LoadScene("Scene1");
    }

    public void ResetGame()
    {
        playerModel.ResetSessionData();
        playerModel.ResetSkillHistory();
        PlayerPrefs.DeleteAll();
        RestartGame();
    }
}
