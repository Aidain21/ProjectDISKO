using UnityEngine;
using System.Collections;

public class MusicScript : MonoBehaviour
{
    public AudioSource music;
    public AudioClip[] tracks;
    public bool onTempo;
    public int bpm;
    public float timer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        music = GetComponent<AudioSource>();
        ChangeMusic(0, 70);

    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 60f/bpm)
        {
            StartCoroutine(OnBeat());
            timer = 0;
        }
    }

    public IEnumerator OnBeat()
    {
        GetComponent<SpriteRenderer>().enabled = true;
        onTempo = true;
        yield return new WaitForSeconds(0.2f);
        GetComponent<SpriteRenderer>().enabled = false;
        onTempo = false;
        yield return new WaitForSeconds(0.1f);
        transform.parent.GetComponent<PlayerScript>().boostOnCooldown = false;
    }

    //track is index of the music in the tracks array, bpm for the bpm (may need to be worked on)
    public void ChangeMusic(int track, int _bpm)
    {
        music.clip = tracks[track];
        music.Play();
        bpm = _bpm;
    }
}


