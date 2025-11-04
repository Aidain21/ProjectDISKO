using UnityEngine;

public class BombScript : MonoBehaviour
{
    public int deathTick;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.LookAt(GameObject.Find("Player").transform, Vector3.up);
        transform.Rotate(new Vector3(0, 180, 0));
        GetComponent<Rigidbody>().AddForce(transform.forward * 150 + Vector3.up * 300);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bombable"))
        {
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
        else
        {
            deathTick++;
            if (deathTick > 3)
            {
                Destroy(gameObject);
            }
        }
    }
}
