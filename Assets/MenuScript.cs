using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class MenuScript : MonoBehaviour
{
    public Canvas gameUI;
    public Canvas menus;
    public GameObject start, pause, levels;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameUI.gameObject.SetActive(false);
        menus.gameObject.SetActive(true);
        start.SetActive(true);
        pause.SetActive(false);
        levels.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
