using UnityEngine;
using TMPro;
using System.Collections;
public class PlayerScript : MonoBehaviour
{
    Rigidbody rb;
    public Camera cam;
    public float baseSpeed = 3f, curSpeed = 3f, maxSpeed = 8f, jumpForce, airTime;
    public bool canJump = true, boostOnCooldown, onGround, canDash = true, inputsEnabled = true, spinning;
    public TMP_Text speedTracker, ballerText, comboText, healthText;
    public bool baller, infiniteDash;
    public int combo, invert;
    public string activeAbility;
    public GameObject testBall;
    public IEnumerator dash;
    public bool left;
    public int playerHealth = 3;
    public Sprite[] sprites, groundedSprites;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        activeAbility = "JumpToBeat";
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        groundedSprites = new Sprite[] { sprites[0], sprites[1] };

    }

    // Update is called once per frame
    void Update()
    {
        //This is what checks if the player is on the ground
        RaycastHit hit;
        onGround = Physics.Raycast(transform.GetChild(1).position, Vector3.down, out hit, 0.15f);
        bool reset = onGround;
        foreach (Sprite sp in groundedSprites)
        {
            if (GetComponent<SpriteRenderer>().sprite == sp) { reset = false; break; }
        }
        if (reset) { GetComponent<SpriteRenderer>().sprite = sprites[0]; }

        //Everything in here is disabled when var is false (useful for cutscenes)
        if (inputsEnabled)
        {
            //Moves player
            rb.linearVelocity = GetInput() * curSpeed + new Vector3(0, Mathf.Max(rb.linearVelocity.y + -airTime * 0.07f, -13f), 0);
            Vector3 Direction = GetInput();
            //Flips sprite with little animation based on movement
            if (Direction.x == -1 && !left)
            {
                StartCoroutine(Flip(left));
                left = !left;
            }
            if (Direction.x == 1 && left)
            {
                StartCoroutine(Flip(left));
                left = !left;
            }

            //Changes between front and back sprites
            if (Direction.z == 1 && !spinning)
            {
                GetComponent<SpriteRenderer>().sprite = sprites[1];
            }
            if (Direction.z == -1 && !spinning)
            {
                GetComponent<SpriteRenderer>().sprite = sprites[0];
            }



            //jump
            if (Input.GetKeyDown(KeyCode.Space) && activeAbility != "MusicComboTest")
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

                    StartCoroutine(Flip(!left, true, true));

                }
                else
                {
                    ComboUp(-1);
                }

            }
            else if (Input.GetKeyDown(KeyCode.Space) && activeAbility == "MusicComboTest")
            {
                if (transform.GetChild(0).GetComponent<MusicScript>().onTempo && !boostOnCooldown)
                {
                    ComboUp(combo);
                    boostOnCooldown = true;
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
                if (!Physics.Raycast(transform.position, new Vector3(GetInput().x, 0, 0), out hit, 0.3f) &&
                    !Physics.Raycast(transform.position, new Vector3(0, 0, GetInput().z), out hit, 0.3f))
                {
                    dash = Dash(GetInput());
                    StartCoroutine(dash);
                }
               

            }

            if (Input.GetKeyDown(KeyCode.E) && activeAbility == "BomberBunny")
            {
                Vector3 spawn = transform.position + GetInput();
                if (spawn != transform.position)
                {
                    Instantiate(testBall, spawn, Quaternion.identity);

                }
                

            }

            //Change between abilities (temporary possibly)
            if (Input.GetKeyDown(KeyCode.BackQuote)) { activeAbility = "MusicComboTest"; }
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

            if (Input.GetKeyDown(KeyCode.P)) { transform.GetChild(0).GetComponent<MusicScript>().NextTrack(); }

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
            GetComponent<SpriteRenderer>().flipX = false;
        }
        else if (transform.position.z >= 3.5f)
        {
            cam.transform.position = new Vector3(transform.position.x, transform.position.y + 2.5f, 12);
            cam.transform.eulerAngles = new Vector3(25, 180, 0);
            GetComponent<SpriteRenderer>().flipX = true;
        }

        //Tracks time in air. 
        airTime = !onGround ? airTime + Time.deltaTime : 0;

        if (rb.linearVelocity.y != 0 && !spinning && !onGround)
        {
            GetComponent<SpriteRenderer>().sprite = rb.linearVelocity.y switch
            {
                > 4 => sprites[2],
                > 2 => sprites[3],
                > 0 => sprites[4],
                < -10 => sprites[5],
                < -5 => sprites[6],
                < 0 => sprites[7],
                _ => sprites[0]
            };
        }

        

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
        healthText.text = "Health: " + playerHealth;
        if (playerHealth <= 0) { 
            Destroy(gameObject);
        }
    }

    public Vector3 GetInput()
    {
        if (transform.position.z >= 3.5f) { invert = -1; }
        if (transform.position.z <= 2.5f) { invert = 1; }

        return new Vector3(invert*Input.GetAxisRaw("Horizontal"), 0, invert*Input.GetAxisRaw("Vertical"));
    }

    void ComboUp(int com)
    {
        combo = com + 1;
        string wow = new('!', combo);
        comboText.text = "Combo: " + combo + wow;
    }

    //Flip the character, left for if they are already facing left, full if you just want to spin instead of turn
    public IEnumerator Flip(bool left, bool full = false, bool harmony = false)
    {
        Vector3 start, end;
        float addTime = 0;
        Sprite temp = null;
        if (harmony) 
        {
            temp = GetComponent<SpriteRenderer>().sprite;
            GetComponent<SpriteRenderer>().sprite = sprites[8];
            spinning = true;
        }
        if (full) { addTime = 0.1f;  }
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
        if (harmony) { GetComponent<SpriteRenderer>().sprite = temp; spinning = false; }

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
    /*
     public void OnCollisionEnter(Collision collision)
     {
         if (collision.gameObject.CompareTag("Spike")){
             playerHealth--;
         }
     }
    */

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Spike")) {
            playerHealth--;
        }
    }

}
