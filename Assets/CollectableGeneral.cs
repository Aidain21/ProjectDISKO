using UnityEngine;
using System.Collections;

public class CollectableGeneral : MonoBehaviour
{
    public Sprite[] sprites;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        StartCoroutine(BasicAnim(sprites, 0.15f, false));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator BasicAnim(Sprite[] frames, float timeBetween, bool backAndForth = false, int cycles = -1, bool stopOnGrounded = false)
    {
        int curFrame = 0;
        int curCycle = 0;
        int step = 1;
        while (curCycle < cycles || cycles < 0)
        {
            GetComponent<SpriteRenderer>().sprite = frames[curFrame];
            curFrame += step;
            yield return new WaitForSeconds(timeBetween);

            if (curFrame == frames.Length)
            {
                if (backAndForth) { step = -1; curFrame -= 2; }
                else { curFrame = 0; }
                curCycle++;
            }
            if (curFrame == -1) { step = 1; curFrame += 2; curCycle++; }
        }

    }
}
