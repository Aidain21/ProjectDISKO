using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;


public class PlayerScript : MonoBehaviour
{
    Rigidbody rb;
    public Camera cam;
    public float baseSpeed = 3f, curSpeed = 3f, maxSpeed = 8f, jumpForce, airTime;
    public bool canJump = true, onGround,inputsEnabled = true, spinning , left, mp3Collected = false;
    public TMP_Text abilityTracker, comboText, speedText, coinText, mp3Text;
    public int invert, playerHealth = 3;
    public static int coinCount = 0;
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
    public string[] abilitynames = {"None", "JumpToBeat", "Swing", "DashOnBeat", "BomberBunny" };
    public float[] cooldownTimes;
    public float[] cooldowns;
    public GameObject shadow;


    //basic animation storage
    public IEnumerator latestAnim;
    public IEnumerator breakableAnim;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        breakableAnim = null;
        rb = GetComponent<Rigidbody>();
        abilNum = 0;
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        shadow = transform.GetChild(3).gameObject;
        groundedSprites = new Sprite[] { sprites[0], sprites[1], sprites[11], sprites[12], sprites[13] };

    }

    // Update is called once per frame
    void Update()
    {
        //This is what checks if the player is on the ground
        onGround = Physics.Raycast(transform.GetChild(1).position, Vector3.down, out _, 0.2f);
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
        if (inputsEnabled && (latestAnim == null || (latestAnim == breakableAnim)))
        {
            //Moves player
            rb.linearVelocity = GetInput() * curSpeed + new Vector3(0, Mathf.Max(rb.linearVelocity.y + -airTime * 0.05f, -13f), 0);
            Vector3 Direction = GetInput();
            //Flips sprite with little animation based on movement
            if ((Direction.x * invert) < 0 && !left)
            {
                StartCoroutine(Flip(left));
                left = !left;
            }
            if ((Direction.x * invert) > 0 && left)
            {
                StartCoroutine(Flip(left));
                left = !left;
            }

            //Changes between front and back sprites
            if ((Direction.z * invert) > 0 && !spinning)
            {
                GetComponent<SpriteRenderer>().sprite = sprites[1];
            }
            if ((Direction.z * invert) < 0 && !spinning)
            {
                GetComponent<SpriteRenderer>().sprite = sprites[0];
            }

            if (rb.linearVelocity != Vector3.zero && breakableAnim != null)
            {
                StopCoroutine(breakableAnim);
                latestAnim = null;
                breakableAnim = null;
                GetComponent<SpriteRenderer>().sprite = sprites[0];
            }

            //jump
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                //default off ground jump
                //airTime adds coyote time effect (adds some forgiveness for the jump)
                if (onGround || airTime <= .12f)
                {
                    rb.AddForce(Vector3.up * 7, ForceMode.Impulse);
                }

                //mid air twirl like mario when the right ability is active and on tempo, tracks the combo too.
                if (abilitynames[abilNum] == "JumpToBeat" && airTime > 0.25f && cooldowns[1] == 0)
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
                    cooldowns[1] = cooldownTimes[1];
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
            if ((Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.JoystickButton0)) && rb.linearVelocity != Vector3.zero && rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f, rb.linearVelocity.z);
            }


            //Initiates dash if everything here is good.
            if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.JoystickButton1)) && onGround == false && abilitynames[abilNum] == "DashOnBeat"
                && (cooldowns[3] == 0 || infiniteDash) && canDash)
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
            if ((Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton1)) && abilitynames[abilNum] == "BomberBunny" && cooldowns[4] == 0)
            {
                Vector3 spawn = transform.position + GetInput();
                if (spawn != transform.position)
                {
                    Instantiate(testBall, spawn, Quaternion.identity);
                    cooldowns[4] = cooldownTimes[4];

                }
                

            }

            //Change between abilities (temporary possibly)
            if (Input.GetKeyDown(KeyCode.BackQuote)) { abilNum = 0; }
            if (Input.GetKeyDown(KeyCode.Alpha1)) { abilNum = 1; }
            if (Input.GetKeyDown(KeyCode.Alpha2)) { abilNum = 2; }
            if (Input.GetKeyDown(KeyCode.Alpha3)) { abilNum = 3; }
            if (Input.GetKeyDown(KeyCode.Alpha4)) { abilNum = 4; }

            if (Input.GetKeyDown(KeyCode.JoystickButton5)) { CycleAbility(); }

            //Change the music
            if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.JoystickButton4)) { transform.GetChild(0).GetComponent<MusicScript>().NextTrack(); }
            

            if (Input.GetKeyDown(KeyCode.M))
            {
                rb.linearVelocity = Vector3.zero;
                latestAnim = BasicAnim(new Sprite[] { sprites[11], sprites[12], sprites[13] }, 0.5f, true, -3 );
                StartCoroutine(latestAnim);
            }

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
            cam.transform.position = new Vector3(transform.position.x, transform.position.y + 3f, -7);
            cam.transform.eulerAngles = new Vector3(30, 0, 0);
            GetComponent<SpriteRenderer>().flipX = false;
        }
        else if (transform.position.z >= 3.5f)
        {
            cam.transform.position = new Vector3(transform.position.x, transform.position.y + 3f, 13);
            cam.transform.eulerAngles = new Vector3(30, 180, 0);
            GetComponent<SpriteRenderer>().flipX = true;
        }

        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity);
        shadow.transform.position = new Vector3(transform.position.x, hit.point.y + 0.011f, transform.position.z);

        //Tracks time in air. 
        airTime = !onGround ? airTime + Time.deltaTime : 0;

        //changes harmony sprite based on their velocity (jumping/falling)
        if (rb.linearVelocity.y != 0 && !spinning && !onGround)
        {
            if (rb.linearVelocity.y <= -13f && latestAnim == null)
            {
                latestAnim = BasicAnim(new Sprite[] { sprites[5], sprites[6], sprites[7] }, 0.06f, true, -1, true);
                StartCoroutine(latestAnim);
            }
            else if (latestAnim == null)
            {
                GetComponent<SpriteRenderer>().sprite = rb.linearVelocity.y switch
                {
                    > 2 => sprites[2],
                    > 1 => sprites[3],
                    > 0 => sprites[4],
                    < -5 => sprites[10],
                    < 0 => sprites[9],
                    _ => sprites[0]
                };

            }
            
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

        coinText.text = "Coins: " + coinCount;

        if (mp3Collected == false)
        {
            mp3Text.text = "0/1";
        }
        else {
            mp3Text.text = "1/1";
        }
    }

    public Vector3 GetInput()
    {
        if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.UpArrow))
        {
            if (transform.position.z >= 3.5f) { invert = -1; }
            if (transform.position.z <= 2.5f) { invert = 1; }

        }
        return new Vector3(invert*Input.GetAxisRaw("Horizontal"), 0, invert*Input.GetAxisRaw("Vertical"));
    }

    void ComboUp(int com)
    {
        combo = com + 1;
        string wow = new('!', combo);
        comboText.text = "Combo: " + combo + wow;
    }

    public void CycleAbility()
    {
        int index = -1;
        for (int i = 0; i < abilitynames.Length; i++)
        {
            index = abilitynames[i] == abilitynames[abilNum] ? i : -1;
            if (index != -1) { break; }
        }
        abilNum = index == abilitynames.Length - 1 ? 0 : index + 1;

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
        cooldowns[3] = cooldownTimes[3];
        GetComponent<SpriteRenderer>().color = Color.white;

    }

    public IEnumerator BasicAnim(Sprite[] frames, float timeBetween, bool backAndForth = false, int cycles = -1, bool stopOnGrounded = false)
    {
        IEnumerator self = latestAnim;
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
            if (Mathf.Abs(cycles) == curCycle) { breakableAnim = self; }
            if (stopOnGrounded && onGround) { break; }
        }
        latestAnim = null;

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

        if (other.CompareTag("HealthCollectable") && playerHealth < 3) {
            AddHealth(1);
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Teleport"))
        {
            transform.position = other.transform.GetChild(0).position;
        }

        if (other.CompareTag("Collectable")) {
            coinCount++;
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Collectable2")) {
            coinCount += 2;
            Destroy(other.gameObject);
        }

        if (other.CompareTag("MP3")) {
            mp3Collected = true;
            Destroy(other.gameObject);
        }

        
    }  

}
