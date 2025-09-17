using UnityEngine;
using TMPro;
using System.Collections;
public class PlayerScript : MonoBehaviour
{
    Rigidbody rb;
    public Camera cam;
    public float baseSpeed = 3f, curSpeed = 3f, maxSpeed = 10f, jumpForce, airTime;
    public bool canJump = true, alreadyBoosted, onGround, canDash = true, inputsEnabled = true;
    public TMP_Text speedTracker, ballerText, comboText;
    public bool baller, infiniteDash;
    public int invert, combo;
    public string activeAbility;
    public GameObject testBall, dashCheckBall;
    public IEnumerator dash;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        activeAbility = "JumpToBeat";
        dashCheckBall = transform.GetChild(2).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (inputsEnabled)
        {
            //Moves player
            rb.linearVelocity = new Vector3(Input.GetAxisRaw("Horizontal") * curSpeed * invert, rb.linearVelocity.y + -airTime * 0.2f, Input.GetAxisRaw("Vertical") * curSpeed * invert);

            if (activeAbility == "DashOnBeat")
            {
               dashCheckBall.transform.localPosition = new Vector3(Input.GetAxisRaw("Horizontal") * 0.25f, 0, Input.GetAxisRaw("Vertical") * 0.25f);
            }

            //jump
            if (Input.GetKeyDown(KeyCode.Space) && canJump)
            {
                rb.AddForce(Vector3.up * 7, ForceMode.Impulse);
                if (transform.GetChild(0).GetComponent<MusicScript>().onTempo && !alreadyBoosted && activeAbility == "JumpToBeat")
                {
                    ComboUp(combo);
                    alreadyBoosted = true;
                    rb.AddForce(Vector3.up * 3, ForceMode.Impulse);
                }
                else
                {
                    ComboUp(-1);
                    canJump = false;
                }

            }
            //allows for small jumps
            if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity != Vector3.zero && rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f, rb.linearVelocity.z);
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) && onGround == false && activeAbility == "DashOnBeat" 
                && (transform.GetChild(0).GetComponent<MusicScript>().onTempo||infiniteDash) && canDash )
            {
                dash = Dash(new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")));
                StartCoroutine(dash);

            }
            if (Input.GetKeyDown(KeyCode.Alpha1)) { activeAbility = "JumpToBeat"; }
            if (Input.GetKeyDown(KeyCode.Alpha2)) { activeAbility = "Swing"; }
            if (Input.GetKeyDown(KeyCode.Alpha3)) { activeAbility = "DashOnBeat"; }

            //B A L L E R
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


        speedTracker.text = (Mathf.Round(rb.linearVelocity.x * 100) / 100).ToString("F2") +
            " " + (Mathf.Round(rb.linearVelocity.y * 100) / 100).ToString("F2") +
            " " + (Mathf.Round(rb.linearVelocity.z * 100) / 100).ToString("F2") +
            "\n Active Ability (Num Keys): " + activeAbility;

        //Moves camera with player
        if (transform.position.z <= 2.5f)
        {
            invert = 1;
            cam.transform.position = new Vector3(transform.position.x, transform.position.y + 2.5f, -5);
            cam.transform.eulerAngles = new Vector3(25, 0, 0);
        }
        else if (transform.position.z >= 3.5f)
        {
            invert = -1;
            cam.transform.position = new Vector3(transform.position.x, transform.position.y + 2.5f, 10);
            cam.transform.eulerAngles = new Vector3(25, 180, 0);
        }

        airTime = !onGround ? airTime + Time.deltaTime : 0;

        //Speed up over time
        if (rb.linearVelocity != Vector3.zero)
        {
            if (onGround)
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
            //probably couldve wrote this better but oh well
            else
            {
                float maxAirSpeed = maxSpeed / (2.5f + airTime);
                if (curSpeed < maxAirSpeed)
                {
                    curSpeed += Time.deltaTime * 5;
                }
                if (curSpeed > maxAirSpeed)
                {
                    curSpeed = maxAirSpeed;
                }
            }
            
        }
        else
        {
            curSpeed = baseSpeed;
        }
       
        //repsawn
        if (transform.position.y < -25)
        {
            transform.position = Vector3.up;
        }
        

        

    }

    void ComboUp(int com)
    {
        combo = com + 1;
        string wow = new('!', combo);
        comboText.text = "Combo: " + combo + wow;
    }

    public IEnumerator Dash(Vector3 dir)
    {
        GetComponent<SpriteRenderer>().color = Color.cyan;
        inputsEnabled = false;
        canDash = false;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        Vector3 start = transform.position;
        Vector3 end = transform.position + dir * 3f;
        float elapsedTime = 0;
        while (elapsedTime < 0.15f)
        {
            Vector3 data = Vector3.Lerp(start, end, (elapsedTime / 0.15f));
            rb.MovePosition(new Vector3(data.x, transform.position.y, data.z));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        rb.useGravity = true;
        canDash = true;
        inputsEnabled = true;
        GetComponent<SpriteRenderer>().color = Color.white;

    }

    private void OnTriggerStay(Collider other)
    {
        onGround = true;
        canJump = true;
    }

    private void OnTriggerExit(Collider other)
    {
        onGround = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!canDash)
        {
            StopCoroutine(dash);
            rb.useGravity = true;
            canDash = true;
            inputsEnabled = true;
            GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
}
