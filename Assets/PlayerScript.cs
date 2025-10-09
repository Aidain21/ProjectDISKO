using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
public class PlayerScript : MonoBehaviour
{
    Rigidbody rb;
    public Camera cam;
    public float baseSpeed = 3f, curSpeed = 3f, maxSpeed = 8f, jumpForce, airTime;
    public bool canJump = true, onGround,inputsEnabled = true, spinning , left;
    public TMP_Text abilityTracker, comboText, speedText;
    public int invert, playerHealth = 3;
    public Sprite[] sprites, groundedSprites, healthBarSprites;
    public GameObject healthBar;

    [Header("Ability Variables")]
    //add to this list as more are made
    public int abilNum;
    public IEnumerator dash;
    public bool boostOnCooldown, canDash = true, infiniteDash;
    public int combo;
    public GameObject testBall;
    [Tooltip("When in lists the abilites go in this order: spinjump, swing, dash, bomb")]
    public string[] abilitynames = {"JumpToBeat", "Swing", "DashOnBeat", "BomberBunny" };
    public float[] cooldownTimes;
    public float[] cooldowns;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        abilNum = 0;
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        groundedSprites = new Sprite[] { sprites[0], sprites[1] };

    }

    // Update is called once per frame
    void Update()
    {
        //This is what checks if the player is on the ground
        onGround = Physics.Raycast(transform.GetChild(1).position, Vector3.down, out _, 0.15f);
        bool reset = onGround;
        foreach (Sprite sp in groundedSprites)
        {
            if (GetComponent<SpriteRenderer>().sprite == sp) { reset = false; break; }
        }
        if (reset) { GetComponent<SpriteRenderer>().sprite = sprites[0]; }

        for (int i = 0; i < cooldowns.Length; i++)
        {
            if (cooldowns[i] > 0)
            {
                cooldowns[i] -= Time.deltaTime;
            }
            else if (cooldowns[i] < 0)
            {
                cooldowns[i] = 0;
            }
        }



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
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //default off ground jump
                //airTime adds coyote time effect (adds some forgiveness for the jump)
                if (onGround || airTime <= .12f)
                {
                    rb.AddForce(Vector3.up * 7, ForceMode.Impulse);
                }

                //mid air twirl like mario when the right ability is active and on tempo, tracks the combo too.
                if (abilitynames[abilNum] == "JumpToBeat" && !onGround && cooldowns[0] == 0)
                {
                    airTime = 0f;
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                    if (transform.GetChild(0).GetComponent<MusicScript>().onTempo && !boostOnCooldown)
                    {
                        rb.AddForce(Vector3.up * 10, ForceMode.Impulse);
                        ComboUp(combo);
                        boostOnCooldown = true;
                    }
                    else
                    {
                        rb.AddForce(Vector3.up * 7, ForceMode.Impulse);
                        ComboUp(-1);
                    }
                    cooldowns[0] = cooldownTimes[0];
                    StartCoroutine(Flip(!left, true, true));
                }
                

            }
            
            //Press enter to test combo
            if (Input.GetKeyDown(KeyCode.Return))
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
            if (Input.GetKeyDown(KeyCode.LeftShift) && onGround == false && abilitynames[abilNum] == "DashOnBeat"
                && (cooldowns[2] == 0 || infiniteDash) && canDash)
            {
                if (!Physics.Raycast(transform.position, new Vector3(GetInput().x, 0, 0), out _, 0.3f) &&
                    !Physics.Raycast(transform.position, new Vector3(0, 0, GetInput().z), out _, 0.3f))
                {
                    if (transform.GetChild(0).GetComponent<MusicScript>().onTempo)
                    {
                        dash = Dash(GetInput(), 3.5f);
                        StartCoroutine(dash);
                        ComboUp(combo);
                        boostOnCooldown = true;
                    }
                    else
                    {
                        dash = Dash(GetInput());
                        StartCoroutine(dash);
                        ComboUp(-1);
                    }
                    
                }
               

            }

            //Throws bombs if player is moving in the direction they are moving
            if (Input.GetKeyDown(KeyCode.E) && abilitynames[abilNum] == "BomberBunny" && cooldowns[3] == 0)
            {
                Vector3 spawn = transform.position + GetInput();
                if (spawn != transform.position)
                {
                    Instantiate(testBall, spawn, Quaternion.identity);
                    cooldowns[3] = cooldownTimes[3];

                }
                

            }

            //Change between abilities (temporary possibly)
            if (Input.GetKeyDown(KeyCode.Alpha1)) { abilNum = 0; }
            if (Input.GetKeyDown(KeyCode.Alpha2)) { abilNum = 1; }
            if (Input.GetKeyDown(KeyCode.Alpha3)) { abilNum = 2; }
            if (Input.GetKeyDown(KeyCode.Alpha4)) { abilNum = 3; }

            //Change the music
            if (Input.GetKeyDown(KeyCode.P)) { transform.GetChild(0).GetComponent<MusicScript>().NextTrack(); }

        }

        //Text that tracks the velocity of the player, and the active ability
        abilityTracker.text = "Active Ability (Num Keys): " + abilitynames[abilNum] 
            + "\n Cooldown: " + (Mathf.Round(cooldowns[abilNum] * 100) / 100).ToString("F2");
        speedText.text = (Mathf.Round(rb.linearVelocity.x * 100) / 100).ToString("F2") +
            " " + (Mathf.Round(rb.linearVelocity.y * 100) / 100).ToString("F2") +
            " " + (Mathf.Round(rb.linearVelocity.z * 100) / 100).ToString("F2");


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

        //changes harmony sprite based on their velocity (jumping/falling)
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

    public void AddHealth(int hp, bool set = false)
    {
        //This line is saying health equals the hp value if set is true, otherwise add hp to health.
        playerHealth = set ? hp : playerHealth + hp;

        //This is a shorthand to saying 3 different if statements for hp
        switch (playerHealth)
        {
            case 3:
                healthBar.GetComponent<Image>().sprite = healthBarSprites[2];
                healthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(390, 150);
                break;
            case 2:
                healthBar.GetComponent<Image>().sprite = healthBarSprites[1];
                healthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(420 * 0.75f, 150);
                break;
            case 1:
                healthBar.GetComponent<Image>().sprite = healthBarSprites[0];
                healthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(320 * 0.75f, 150);
                break;
            case int n when (n <= 0):
                Destroy(gameObject);
                break;
        }

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
    public IEnumerator Dash(Vector3 dir, float distance = 3f)
    {
        GetComponent<SpriteRenderer>().color = Color.cyan;
        inputsEnabled = false;
        canDash = false;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        Vector3 start = transform.position;
        Vector3 end = transform.position + dir * distance;
        float elapsedTime = 0;
        while (elapsedTime < 0.15f)
        {
            Vector3 data = Vector3.Lerp(start, end, (elapsedTime / 0.15f));
            rb.MovePosition(new Vector3(data.x, transform.position.y, data.z));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
            bool noDash = Physics.Raycast(transform.position, dir, out _, 0.3f);
            if (noDash)
            {
                elapsedTime = 100;
            }
        }
        rb.useGravity = true;
        canDash = true;
        inputsEnabled = true;
        cooldowns[2] = cooldownTimes[2];
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
            AddHealth(-1);
        }
    }

}
