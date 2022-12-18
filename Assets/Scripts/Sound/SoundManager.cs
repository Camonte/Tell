using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all audio.
/// </summary>
public class SoundManager : Singleton<SoundManager>
{
    AudioSource player;
    Dictionary<string, AudioClip> sounds = new Dictionary<string, AudioClip>();
    AudioClip success;
    AudioClip failure;

    private static string Translate(string name)
    {
        // windows has a case-insensitive file manager, so files named "e" and "E" are considered the same.
        // we follow the convention of adding a "+" after a letter that should be capitalized
        return name.Replace("o+", "O").Replace("e+", "E").Replace("z+", "Z").Replace("s+", "S");
    }

    public override void Awake()
    {
        // load all sounds into dictionary
        player = GetComponent<AudioSource>();
        success = Resources.Load<AudioClip>("Sounds/success");
        failure = Resources.Load<AudioClip>("Sounds/failure");
        var clips = Resources.LoadAll<AudioClip>("Sounds/Phonemes");
        foreach (var clip in clips) sounds.Add(Translate(clip.name), clip);
    }

    /// <summary>
    /// Plays a sound for the given Phoneme, return the lenght of the audio.
    /// </summary>
    internal float Play(string name)
    {
        // try to play single sound
        if (sounds.ContainsKey(name))
        {
            player.PlayOneShot(sounds[name]);
            return sounds[name].length;
        }

        // otherwise play them in succession
        var parts = name.Split(' ');
        if (parts.Length == 2)
        {
            var clips = new List<AudioClip>();
            var length = 0f;
            foreach (var part in parts)
            {
                if (!sounds.ContainsKey(part)) return 0;
                var clip = sounds[part];
                clips.Add(clip);
                length += clip.length;
            }
            StartCoroutine(PlaySequential(clips));
            return length;
        }
        return 0;
    }

    private IEnumerator PlaySequential(List<AudioClip> clips)
    {
        player.Stop();
        foreach(var clip in clips)
        {
            player.clip = clip;
            player.Play();

            while (player.isPlaying) yield return null;
        }
    }

    /// <summary>
    /// Plays a success sound. yeyyyy
    /// </summary>
    internal void Success()
    {
        player.PlayOneShot(success, 0.08f);
    }

    /// <summary>
    /// Plays a failure sound. neyyyy
    /// </summary>
    internal void Failure()
    {
        player.PlayOneShot(failure, 0.08f);
    }
}