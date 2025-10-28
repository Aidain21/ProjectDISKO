using TMPro;
using UnityEngine;

public class MoneyWallScript : MonoBehaviour
{
    public bool spendingWall;
    public int coinsRequired;
    public TMP_Text wallText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        wallText.text = "Requires" + "\n" + coinsRequired + " Coins";
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (PlayerScript.coinCount >= coinsRequired)
            {
                Destroy(gameObject);
                if (spendingWall == true)
                {
                    PlayerScript.coinCount -= coinsRequired;
                }
            }
        
    }
}
