using UnityEngine.Audio;
using UnityEngine;

// beispiel c# unity object von
// https://youtu.be/6OT43pvUyfY?t=406

// dieser teil des codes könnte wohl
// auch in python geschrieben werden

[System.Serializable]
public class Sound {
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;

    [Range(.1f, 3f)]
    public float pitch;

    public AudioSource source;
}

