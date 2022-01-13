using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip jumpScare1;
    private AudioSource jumpscareAudio;

    private DemonBehaviour demon;

    // Start is called before the first frame update
    void Start()
    {
        demon = GameObject.FindGameObjectWithTag("Enemy").GetComponent<DemonBehaviour>();
        jumpscareAudio = this.GetComponent<AudioSource>();

        //events
        demon.OnPlayerKilled+=DisplayJumpscare1Audio;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayJumpscare1Audio()
    {
        jumpscareAudio.PlayOneShot(jumpScare1);
    }
}
