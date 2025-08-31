using UnityEngine;

public class WallScript : MonoBehaviour
{
    Color clr;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        clr = transform.parent.GetComponent<MeshRenderer>().material.color;
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("rrfe");
        if (other.CompareTag("Player"))
        {
            Debug.Log("R");
            clr = new Color(clr.r,clr.g,clr.b,0.2f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            clr = new Color(clr.r, clr.g, clr.b, 1);
        }
    }
}


