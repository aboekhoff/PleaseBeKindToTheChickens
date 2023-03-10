using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * foreach (AudioClip ac in Resources.FindObjectsOfTypeAll(typeof(AudioClip)) as AudioClip[])
        {
            Debug.Log(ac);
        }
 *  may want to do something like this and compute the counts based on prefix automatically
 *  then we don't have to do anything more than add an audio file and name it correctly to get it integrated
 *  same goes for removing audio clips
 */

public class AudioManager
{
    static AudioManager instance;
    public static AudioManager GetInstance() { 
        if (instance == null)
        {
            instance = new AudioManager();
        }
        return instance;
    }

    public enum SoundType
    {
        Click,
        Cluck,
        Squawk,
        Slide,
        Door,
        Success,
    } 

    Dictionary<SoundType, AudioClip[]> audioClips;

    public AudioClip GetClip(SoundType type)
    {
        AudioClip[] clips = audioClips[type];
        int index = (int)(Random.value * clips.Length);
        return clips[index]; 
    }

    public void PlayClip(SoundType type, AudioSource audioSource)
    {
        audioSource.PlayOneShot(GetClip(type));
    }

    public AudioManager()
    {
        audioClips = new Dictionary<SoundType, AudioClip[]>();
        SetupAudioClips();
    }

    void SetupAudioClips()
    {
        SoundType[] types = { SoundType.Click, SoundType.Cluck, SoundType.Squawk, SoundType.Slide, SoundType.Door, SoundType.Success };
        string[] prefixes = { "click", "cluck", "squawk", "slide", "mechanical-door", "success" };
        int[] counts = { 2, 5, 2, 4, 1, 1 };

        for (int i = 0; i < types.Length; i++)
        {
            SoundType type = types[i];
            string prefix = prefixes[i];
            int count = counts[i];
            AudioClip[] clips = new AudioClip[count];
            audioClips.Add(type, clips);

            for (int j = 1; j <= count; j++)
            {
                string assetName = prefix + j.ToString();
                AudioClip clip = Resources.Load<AudioClip>(assetName);
                clips[j - 1] = clip;
            }
        }
    }
}
