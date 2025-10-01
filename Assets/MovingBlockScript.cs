using UnityEngine;

public class MovingBlockScript : MonoBehaviour
{
    public int beatsToActivate;
    public Vector3[] locations;
    public int curPosIndex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        curPosIndex = 0;
        locations = new Vector3[transform.childCount];
        locations[0] = transform.position;
        int count = transform.childCount;
        for (int i = 1; i < count; i++)
        {
            locations[i] = transform.GetChild(i).position;
            Destroy(transform.GetChild(i).gameObject);
        }
        transform.GetChild(0).position = locations[1];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BeatBlock(int beats)
    {
        if (beats % beatsToActivate == 0)
        {
            if (curPosIndex + 1 == locations.Length)
            {
                curPosIndex = 0;
            }
            else
            {
                curPosIndex++;
            }

            transform.position = locations[curPosIndex];
            transform.GetChild(0).position = curPosIndex + 1 == locations.Length ? locations[0] : locations[curPosIndex + 1];
        }
       
    }
}
