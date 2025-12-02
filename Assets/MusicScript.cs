using UnityEngine;
using System.Collections;


public class MusicScript : MonoBehaviour
{
    public int startTrack;
    public AudioSource music;
    public AudioClip[] tracks;
    public int[] bpms;
    public bool onTempo;
    public int bpm;
    public float timer;
    public int totalBeats;
    public RectTransform left, middle, right;
    public string trackName;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        music = GetComponent<AudioSource>();    
        ChangeMusic(startTrack);

    }

    // Update is called once per frame
    void Update()
    {
        if (music.clip.loadState == AudioDataLoadState.Loaded)
        {
            timer += Time.deltaTime;
            if (timer >= 60f / bpm - 0.02f)
            {
                totalBeats++;
                StartCoroutine(OnBeat());
                MovingBlockScript[] blocks = FindObjectsByType<MovingBlockScript>(FindObjectsSortMode.None);
                foreach (MovingBlockScript block in blocks)
                {
                    block.BeatBlock(totalBeats);
                }
                timer = 0;
            }
        }
        
    }

    public IEnumerator OnBeat()
    {
        onTempo = true;
        if (left.anchoredPosition.y == 100)
        {
            left.anchoredPosition = new Vector2(-140, 50);
            middle.anchoredPosition = new Vector2(0, 100);
            right.anchoredPosition = new Vector2(140, 50);
        }
        else
        {
            left.anchoredPosition = new Vector2(-140, 100);
            middle.anchoredPosition = new Vector2(0, 50);
            right.anchoredPosition = new Vector2(140, 100);
        }
       
        yield return new WaitForSeconds(0.2f);
        onTempo = false;
        yield return new WaitForSeconds(0.1f);
        transform.parent.GetComponent<PlayerScript>().boostOnCooldown = false;
    }

    //track is index of the music in the tracks array, bpm for the bpm (may need to be worked on)
    public void ChangeMusic(int track)
    {
        music.clip = tracks[track];
        music.Play();
        bpm = bpms[track];
        trackName = music.clip.name;
    }

    public void NextTrack()
    {
        
        int index = -1;
        for (int i = 0; i < bpms.Length; i++) 
        {
            index = bpms[i] == bpm ? i : -1;
            if (index != -1) { break; }
        }
        Debug.Log(bpms.Length);
        Debug.Log(index == bpms.Length - 1 ? 0 : index + 1);
        ChangeMusic(index == bpms.Length - 1 ? 0 : index + 1);

    }
}


