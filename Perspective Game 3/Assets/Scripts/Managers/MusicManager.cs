using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public int trackIndex = 0;
    public bool playOnAwake = true;
    public float musicVolume = 0.75f;
    public bool loop;
    public bool shuffle;
    public Sound[] sounds;


    private bool playing = false;
    private void Awake()
    {
        if (shuffle)
        {
            trackIndex = UnityEngine.Random.Range(0, sounds.Length);
        }
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.volume = musicVolume;
            s.source.volume = s.volume;
            s.source.pitch = 1f;
            s.source.loop = loop;
        }
        if (playOnAwake)
        {
            startPlaying();
        }
    }

    public void setMusicVolume(float val)
    {
        musicVolume = val;
        foreach (Sound s in sounds)
        {
            s.volume = musicVolume;
            s.source.volume = s.volume;


        }
    }

    public void startPlaying()
    {
        playing = true;
        playSongIndex(trackIndex);
    }
    private void Update()
    {
        if (sounds[trackIndex].source.isPlaying == false && playing == true)
        {
            nextTrack();
        }
    }
    public void pauseMusic()
    {
        sounds[trackIndex].source.Pause();
        playing = false;
    }

    public void unpauseMusic()
    {
        sounds[trackIndex].source.UnPause();
        playing = true;
    }

    public void setLoop(bool val)
    {
        foreach  (Sound s in sounds)
        {
            s.source.loop = val;
        }
    }

    public void nextTrack()
    {
        sounds[trackIndex].source.Stop();
        if (shuffle)
        {
            trackIndex = UnityEngine.Random.Range(0, sounds.Length);
        }
        else
        {
            if (trackIndex == sounds.Length)
            {
                trackIndex = 0;
            }
            else
            {
                trackIndex++;
            }
        }

        sounds[trackIndex].source.Play();
    }

    public void playSongIndex(int i)
    {
        sounds[i].source.Play();
        trackIndex = i;
    }
    public void PlaySongName(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Could not find sound with name: " + name);
            return;
        }
        s.source.Play();
    }
}
