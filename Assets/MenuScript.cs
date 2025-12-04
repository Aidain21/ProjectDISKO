using TMPro;
using UnityEditor.Build.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MenuScript : MonoBehaviour
{
    public Canvas gameUI;
    public GameObject start, pause, finish;
    public PlayerScript player;
    public TMP_Text music;

    public string levelIntro;
    public TMP_Text levelIntroText;
    public TMP_Text finishText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        player = transform.parent.GetComponent<PlayerScript>();
        player.inputsEnabled = false;
        gameUI.gameObject.SetActive(false);
        levelIntroText.text = levelIntro;
        start.SetActive(true);
        pause.SetActive(false);
        finish.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (start.activeSelf)
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame)
            {
                start.SetActive(false);
                gameUI.gameObject.SetActive(true);
                player.inputsEnabled = true;
            }
        }

        if (finish.activeSelf)
        {
            player.inputsEnabled = false;
            gameUI.gameObject.SetActive(false);
            finishText.text = "You beat the level!\r\n\r\nTime: " + (Mathf.Round(player.levelTimer * 100) / 100).ToString("F2") + " seconds" + "\r\nDeaths: " + player.deaths + "\r\n\r\nPress any key to continue...";
            if (Keyboard.current.anyKey.wasPressedThisFrame)
            {
                SceneManager.LoadScene(player.nextLevel);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pause.activeSelf)
            {
                player.inputsEnabled = true;
                gameUI.gameObject.SetActive(true);
                pause.SetActive(false);
            }
            else
            {
                player.inputsEnabled = false;
                gameUI.gameObject.SetActive(false);
                pause.SetActive(true);
                music.text = "Track Name: " + player.transform.GetChild(0).GetComponent<MusicScript>().trackName;
            }
            

        }
    }

}
