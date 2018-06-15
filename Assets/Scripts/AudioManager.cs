using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public Sound[] sounds;

    void Awake() {
        
        foreach (Sound s in sounds) {
            s.src = gameObject.AddComponent<AudioSource>();
            s.src.clip = s.clip;

            s.src.volume = s.volume;
            s.src.pitch = s.pitch;
        }
    }

    public void Play(string name) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.src.Play();
        //Debug.Log("Played Sound");
    }

    public void PlayFor(string name, float time) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.src.PlayScheduled(time);
    }
}
