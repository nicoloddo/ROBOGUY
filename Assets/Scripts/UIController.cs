using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    private GameManager gameManager;
    public GameObject heart, heart1, heart2, heart3, heart4;
    private GameObject[] hearts;

    public TextMeshProUGUI e1_n, b2_n, e2_n, e3_n, e4_n, b1_n;
    private TextMeshProUGUI[] enemies_n_text;

    public GameObject title_t, welcome_t, difficulty_t;
    public bool restart_bool;
    public GameObject continue_b, reset_b, difficulty_s, autodifficulty_toggle;
    public GameObject pause_m, gameover, youwon;
    public TextMeshProUGUI enemies_t, score_t, score_t2, record_t, difficulty_modifier_t, fictious_diff_modifier_t;
    private bool first_time = false;
    public bool first_time_override = false;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        if (first_time_override)
        {
            first_time = false;
            Debug.Log("Overriding first_time!");
        } else if(!PlayerPrefs.HasKey("FirstTime"))
        {
            first_time = true;
        } else
        {
            first_time = false;
        }

        UpdateRecordLabel();

        gameManager = FindObjectOfType<GameManager>();

        hearts = new[] { heart, heart1, heart2, heart3, heart4 };
        enemies_n_text = new[] { e1_n, b2_n, e2_n, e3_n, e4_n, b1_n };

        float difficultyLevel;
        if (PlayerPrefs.HasKey("Difficulty"))
        {
            difficultyLevel = PlayerPrefs.GetFloat("Difficulty");
        } else
        {
            difficultyLevel = gameManager.GetDifficultyLevel();
        }
        switch (difficultyLevel)
        {
            case 1:
                difficulty_s.GetComponent<Slider>().value = 1;
                break;
            case 2:
                difficulty_s.GetComponent<Slider>().value = 2.45f;
                break;
            case 3:
                difficulty_s.GetComponent<Slider>().value = 3.9f;
                break;
        }
        if (PlayerPrefs.HasKey("AutoDifficultyIsOn"))
        {
            switch(PlayerPrefs.GetInt("AutoDifficultyIsOn"))
            {
                case 0:
                    autodifficulty_toggle.GetComponent<Toggle>().isOn = false;
                    break;
                case 1:
                    autodifficulty_toggle.GetComponent<Toggle>().isOn = true;
                    break;
            }
        }
        else
        {
            autodifficulty_toggle.GetComponent<Toggle>().isOn = true;
        }

        if (autodifficulty_toggle.GetComponent<Toggle>().isOn)
            difficulty_s.GetComponent<SliderController>().SetDifficultyBaseText("Auto Difficulty: ");
        else
            difficulty_s.GetComponent<SliderController>().SetDifficultyBaseText("Difficulty: ");

        pause_m.SetActive(false);

        gameover.SetActive(false);
        youwon.SetActive(false);

        title_t.SetActive(false);
        difficulty_t.SetActive(false);
        welcome_t.SetActive(false);

        if(first_time)
        {
            StartCoroutine(wait_and_display(3, title_t, difficulty_t, welcome_t));
            PlayerPrefs.SetInt("FirstTime", 1);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        Cursor.visible = false;

        // WIN OR LOSE DISPLAY
        if (gameManager.GetWonOrLost() != 0)
        {
            int outcome = gameManager.GetWonOrLost();
            if(outcome == -1) // lost
            {
                gameover.SetActive(true);
            }
            else if (outcome == 1) // won
            {
                difficulty_modifier_t.text = "Difficulty modifier: x" + gameManager.GetInstantDifficultyModifier().ToString("F2");
                UpdateRecordLabel();

                int[] remaining_enemies_distribution = gameManager.GetRemainingEnemiesDistribution();
                int[] enemies_value = gameManager.GetEnemiesValues();

                youwon.SetActive(true);
                score_t.text = "Total score: " + gameManager.GetScore()[1].ToString();
                score_t2.text = gameManager.GetScore()[0].ToString();
                for (int i = 0; i < remaining_enemies_distribution.Length; i++)
                {
                    enemies_n_text[i].text = enemies_value[i] + " (x" + remaining_enemies_distribution[i] + ")";
                    if (remaining_enemies_distribution[i] == 0)
                    {
                        GameObject uienemy_sprite = enemies_n_text[i].transform.parent.gameObject;
                        uienemy_sprite.GetComponent<SpriteRenderer>().color = Color.black;

                        if(uienemy_sprite.name[0] == 'e') // if the enemy is a simple one
                        {
                            enemies_n_text[i].text += "\n\nEXTINCT";
                        }
                        else // if it is a big boss
                        {
                            switch(uienemy_sprite.name)
                            {
                                case "b1":
                                    enemies_n_text[i].text += "\n\n\nEXTINCT";
                                    break;
                                case "b2":
                                    enemies_n_text[i].text += "\n\n\n\n\n\nEXTINCT";
                                    break;
                            }
                        }
                    }                    
                }
            }
        }

        // HEALTH SPRITE
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].SetActive(false);
        }
        for (int i = 0; i < gameManager.GetMaxPlayerHealth(); i++)
        {
            hearts[i].SetActive(true);
            hearts[i].GetComponent<SpriteController>().AltSprite();
        }
        for (int i = 0; i < gameManager.GetPlayerHealth(); i++)
        {
            hearts[i].SetActive(true);
            hearts[i].GetComponent<SpriteController>().DefaultSprite();
        }

        // ENEMIES COUNTER
        enemies_t.text = "Enemies: " + gameManager.GetEnemiesCount().ToString() + "/" + gameManager.GetEnemiesTotal().ToString();
        fictious_diff_modifier_t.text = "Diff. Bonus: " + gameManager.GetInstantDifficultyModifier().ToString("F2") + "x";

        // PAUSE MENU
        if (Input.GetKeyDown(KeyCode.Escape) && Time.timeScale != 0 && gameManager.GetWonOrLost() == 0)
        {
            Time.timeScale = 0;
            pause_m.SetActive(true);
            difficulty_s.GetComponent<SliderController>().UpdateText();
        }

        if (continue_b.GetComponent<ButtonController>().continue_click)
        {
            pause_m.SetActive(false);
            Time.timeScale = 1;
            continue_b.GetComponent<ButtonController>().continue_click = false;
        }

        if (reset_b.GetComponent<ButtonController>().reset_click)
        {
            gameManager.ResetGame();
            reset_b.GetComponent<ButtonController>().reset_click = false;
        }

        if (restart_bool)
        {
            restart_bool = false;
            gameManager.RestartGame();
            Time.timeScale = 1;
            pause_m.SetActive(false);
        }

        if (difficulty_s.GetComponent<SliderController>().changed_difficulty)
        {
            difficulty_s.GetComponent<SliderController>().changed_difficulty = false;
            if (!autodifficulty_toggle.GetComponent<Toggle>().isOn)
                gameManager.SetDifficulty(difficulty_s.GetComponent<SliderController>().GetDifficultyValue());
        }

        if (autodifficulty_toggle.GetComponent<AutoDifficultyController>().GetChangedValue())
        {
            gameManager.SetAutoDifficulty(autodifficulty_toggle.GetComponent<Toggle>().isOn);
            autodifficulty_toggle.GetComponent<AutoDifficultyController>().NotChangedValue();

            if (autodifficulty_toggle.GetComponent<Toggle>().isOn)
            {
                difficulty_s.GetComponent<SliderController>().SetDifficultyBaseText("Auto Difficulty: ");
            }                
            else
            {
                difficulty_s.GetComponent<SliderController>().SetDifficultyBaseText("Difficulty: ");
                gameManager.SetDifficulty(difficulty_s.GetComponent<SliderController>().GetDifficultyValue());
            } 
        }

        if (gameManager.GetAutoDifficultyIsOn())
        {
            switch (gameManager.GetDifficultyLevel())
            {
                case 1:
                    difficulty_s.GetComponent<Slider>().value = 1;
                    break;
                case 1.5f:
                    difficulty_s.GetComponent<Slider>().value = 1.73f;
                    break;
                case 2:
                    difficulty_s.GetComponent<Slider>().value = 2.45f;
                    break;
                case 2.5f:
                    difficulty_s.GetComponent<Slider>().value = 3.17f;
                    break;
                case 3:
                    difficulty_s.GetComponent<Slider>().value = 3.9f;
                    break;
            }
        }

        // MANAGE THE CURSOR
        if (pause_m.activeSelf || youwon.activeSelf || gameover.activeSelf)
        {
            Cursor.visible = true;
        } else
        {
            Cursor.visible = false;
        }
    }

    private void UpdateRecordLabel()
    {
        if (PlayerPrefs.HasKey("Record"))
        {
            int record = PlayerPrefs.GetInt("Record");
            record_t.text = "Record: " + record;
        }
        else
        {
            record_t.text = "Record: 0";
        }
    }
    IEnumerator wait_and_display(float interval_s, GameObject to_display, GameObject to_display2, GameObject to_display3)
    {
        Time.timeScale = 0;
        to_display.SetActive(true);
        yield return new WaitForSecondsRealtime(interval_s); // Normal wait for seconds freezes with timescale = 0
        to_display.SetActive(false);
        yield return new WaitForSecondsRealtime(0.3f);
        to_display2.SetActive(true);
        yield return new WaitForSecondsRealtime(interval_s);
        to_display2.SetActive(false);
        yield return new WaitForSecondsRealtime(0.3f);
        to_display3.SetActive(true);
        yield return new WaitForSecondsRealtime(interval_s);
        to_display3.SetActive(false);
        Time.timeScale = 1;
    }
}
