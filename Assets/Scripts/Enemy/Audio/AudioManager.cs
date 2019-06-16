using UnityEngine.Audio;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public EnemySound[] sounds;
    private void Awake()
    {
        foreach (EnemySound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
        }
    }

    public void PlaySound(string name)
    {
        EnemySound s =Array.Find(sounds, EnemySound => EnemySound.name == name);
        s.source.Play();
        
    }
}
