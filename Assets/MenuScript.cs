using TMPro;
using UnityEditor.Build.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class MenuScript : MonoBehaviour
{
    public Canvas gameUI;
    public GameObject start, pause;
    public PlayerScript player;
    public TMP_Text music;

    public string levelIntro;
    public TMP_Text levelIntroText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = transform.parent.GetComponent<PlayerScript>();
        gameUI.gameObject.SetActive(false);
        levelIntroText.text = levelIntro;
        start.SetActive(true);
        pause.SetActive(false);
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
