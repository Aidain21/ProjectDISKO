using UnityEngine;
using TMPro;
public class PlayerScript : MonoBehaviour
{
    Rigidbody rb;
    public Camera cam;
    public float baseSpeed = 3f, curSpeed = 3f, maxSpeed = 10f, jumpForce;
    public bool canJump = true;
    public TMP_Text speedTracker;
    public bool baller;
    public TMP_Text ballerText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        speedTracker.text = (Mathf.Round(rb.linearVelocity.x * 100)/100) + " " + (Mathf.Round(rb.linearVelocity.y * 100) / 100) + " " + (Mathf.Round(rb.linearVelocity.z * 100) / 100);
        //Moves camera with player
        cam.transform.position = new Vector3(transform.position.x, transform.position.y + 2.5f, cam.transform.position.z);
        //Moves player
        rb.linearVelocity = new Vector3(Input.GetAxisRaw("Horizontal") *curSpeed, rb.linearVelocity.y, Input.GetAxisRaw("Vertical") * curSpeed);
        //Speed up over time
        if (rb.linearVelocity != Vector3.zero)
        {
            if (curSpeed < maxSpeed)
            {
                curSpeed += Time.deltaTime * 5;
            }
            if (curSpeed > maxSpeed)
            {
                curSpeed = maxSpeed;
            }
        }
        else
        {
            curSpeed = baseSpeed;   
        }

        if (Input.GetKeyDown(KeyCode.Space) && canJump)
        {
            rb.AddForce(Vector3.up * 7, ForceMode.Impulse);
            canJump = false;
        }

        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity != Vector3.zero)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f, rb.linearVelocity.z);
        }

        if (transform.position.y < -25)
        {
            transform.position = Vector3.up;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (baller)
            {
                rb.freezeRotation = true;
                transform.rotation = Quaternion.identity;
                baller = false;
                ballerText.text = "Press B to become B A L L \nBall Status: Inactive";
            }
            else
            {
                rb.freezeRotation = false;
                baller = true;
                ballerText.text = "Press B to become B A L L \nBall Status: It's Roll Time";
            }
        }
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            canJump = true;
        }
    }
}
