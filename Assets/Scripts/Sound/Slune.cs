using UnityEngine.Audio;
using UnityEngine;
using System;

// found at:
// https://youtu.be/6OT43pvUyfY?t=582

// dieser Teil des codes könnte wohl auch in python geschrieben werden

// hier sollten wir aber keine berechnungen machen

public class Slune: MonoBehaviour {
    // SoundManager

    public Sound[] sounds;

    // in der C# unity API werden folgende Funktionen gecallt:

    void Update () {
        // called once per frame
    }

    void Awake () {
        // called before first frame

        // beispiel implementation:
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;

        }
    }
    public void Play (string name)
    {
        Array.Find(sounds, sound=>sound.name==name);
    }



}