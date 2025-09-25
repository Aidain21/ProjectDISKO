using UnityEngine;
using TMPro;
using System.Collections;
public class PlayerScript : MonoBehaviour
{
    Rigidbody rb;
    public Camera cam;
    public float baseSpeed = 3f, curSpeed = 3f, maxSpeed = 8f, jumpForce, airTime;
    public bool canJump = true, boostOnCooldown, onGround, canDash = true, inputsEnabled = true;
    public TMP_Text speedTracker, ballerText, comboText;
    public bool baller, infiniteDash;
    public int combo;
    public string activeAbility;
    public GameObject testBall;
    public IEnumerator dash;
    public bool left;
    public Sprite[] sprites;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        activeAbility = "JumpToBeat";
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        //This is what checks if the player is on the ground
        RaycastHit hit;
        onGround = Physics.Raycast(transform.GetChild(1).position, Vector3.down, out hit, 0.3f);

        //Everything in here is disabled when var is false (useful for cutscenes)
        if (inputsEnabled)
        {
            //Moves player
            rb.linearVelocity = new Vector3(Input.GetAxisRaw("Horizontal") * curSpeed, rb.linearVelocity.y + -airTime * 0.07f, Input.GetAxisRaw("Vertical") * curSpeed);

            //Flips sprite with little animation based on movement
            if (Input.GetKeyDown(KeyCode.A) && !left)
            {
                StartCoroutine(Flip(left));
                left = !left;
            }
            if (Input.GetKeyDown(KeyCode.D) && left)
            {
                StartCoroutine(Flip(left));
                left = !left;
            }

            //Changes between front and back sprites
            if (Input.GetKeyDown(KeyCode.W))
            {
                GetComponent<SpriteRenderer>().sprite = sprites[1];
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                GetComponent<SpriteRenderer>().sprite = sprites[0];
            }



            //jump
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //default off ground jump
                if (onGround)
                {
                    rb.AddForce(Vector3.up * 7, ForceMode.Impulse);
                }

                //mid air twirl like mario when the right ability is active and on tempo, tracks the combo too.
                if (transform.GetChild(0).GetComponent<MusicScript>().onTempo && !boostOnCooldown && activeAbility == "JumpToBeat" && !onGround)
                {
                    ComboUp(combo);
                    boostOnCooldown = true;
                    airTime = 0f;
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                    rb.AddForce(Vector3.up * 7, ForceMode.Impulse);

                    StartCoroutine(Flip(!left, true));

                }
                else
                {
                    ComboUp(-1);
                }

            }

            //allows for small jumps
            if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity != Vector3.zero && rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f, rb.linearVelocity.z);
            }


            //Initiates dash if everything here is good.
            if (Input.GetKeyDown(KeyCode.LeftShift) && onGround == false && activeAbility == "DashOnBeat"
                && (transform.GetChild(0).GetComponent<MusicScript>().onTempo || infiniteDash) && canDash)
            {
                if (!Physics.Raycast(transform.position, new Vector3(Input.GetAxisRaw("Horizontal"), 0, 0), out hit, 0.3f) &&
                    !Physics.Raycast(transform.position, new Vector3(0, 0, Input.GetAxisRaw("Vertical")), out hit, 0.3f))
                {
                    dash = Dash(new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")));
                    StartCoroutine(dash);
                }
               

            }

            if (Input.GetKeyDown(KeyCode.E) && activeAbility == "BomberBunny")
            {
                var dir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
                Vector3 rot;
                if (dir == Vector3.back)
                {
                    
                }
                else if (dir == Vector3.forward)
                {

                }
                else if (dir == Vector3.left)
                {

                }
                else if (dir == Vector3.right)
                {
                    rot = Vector3.zero;
                }
                else
                {

                }


                Instantiate(testBall, transform.position + new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")), Quaternion.identity);

            }

            //Change between abilities (temporary possibly)
            if (Input.GetKeyDown(KeyCode.Alpha1)) { activeAbility = "JumpToBeat"; }
            if (Input.GetKeyDown(KeyCode.Alpha2)) { activeAbility = "Swing"; }
            if (Input.GetKeyDown(KeyCode.Alpha3)) { activeAbility = "DashOnBeat"; }
            if (Input.GetKeyDown(KeyCode.Alpha4)) { activeAbility = "BomberBunny"; }

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

        //Text that tracks the velocity of the player, and the active ability
        speedTracker.text = (Mathf.Round(rb.linearVelocity.x * 100) / 100).ToString("F2") +
            " " + (Mathf.Round(rb.linearVelocity.y * 100) / 100).ToString("F2") +
            " " + (Mathf.Round(rb.linearVelocity.z * 100) / 100).ToString("F2") +
            "\n Active Ability (Num Keys): " + activeAbility;

        //Moves camera with player, first for normal, second for the backside (backside needs a ton of work with controls being inverted due to camera angle)
        if (transform.position.z <= 2.5f)
        {
            cam.transform.position = new Vector3(transform.position.x, transform.position.y + 2.5f, -7);
            cam.transform.eulerAngles = new Vector3(25, 0, 0);
        }
        else if (transform.position.z >= 3.5f)
        {
            cam.transform.position = new Vector3(transform.position.x, transform.position.y + 2.5f, 12);
            cam.transform.eulerAngles = new Vector3(25, 180, 0);
        }

        //Tracks time in air. 
        airTime = !onGround ? airTime + Time.deltaTime : 0;

        //Speed up over time
        if (rb.linearVelocity != Vector3.zero)
        {
            //on ground movement speeds
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
            //in air movement speeds (still probably needs to be capped even more)
            else
            {
                float maxAirSpeed = maxSpeed / (1.2f + airTime * 3f);
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

    //Flip the character, left for if they are already facing left, full if you just want to spin instead of turn
    public IEnumerator Flip(bool left, bool full = false)
    {
        Vector3 start, end;
        float addTime = 0;
        if (full) { addTime = 0.1f; }
        if (left)
        {
            start = new Vector3(0,180,0);
            end = Vector3.zero;

        }
        else
        {
            end = new Vector3(0, 180, 0);
            start = Vector3.zero;
        }
        
        float elapsedTime = 0;
        while (elapsedTime < 0.15f + addTime)
        {
            Vector3 data = Vector3.Lerp(start, end, (elapsedTime / 0.15f));
            transform.eulerAngles = data;
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        if (full)
        {
            while (elapsedTime < 0.15f + addTime)
            {
                Vector3 data = Vector3.Lerp(end, start, (elapsedTime / 0.15f));
                transform.eulerAngles = data;
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        transform.eulerAngles = end;

    }

    //The dastardly dash, choose a direction to dash in, will change in future to make more customizable
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


            RaycastHit hit;
            bool noDash = Physics.Raycast(transform.position, dir, out hit, 0.3f);
            if (noDash)
            {
                elapsedTime = 100;
            }
        }
        rb.useGravity = true;
        canDash = true;
        inputsEnabled = true;
        GetComponent<SpriteRenderer>().color = Color.white;

    }

    //No dashing into walls please i beg  you
}
