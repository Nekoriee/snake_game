using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] private List<AudioSource> sources;
    // sources[0] = music
    // sources[1] = snake
    // sources[2] = item
    // sources[3] = menu
    [SerializeField] public Dictionary<string, Sound> soundList = new Dictionary<string, Sound>();
    void Awake()
    {

        AudioClip clip = Resources.Load("Music/ingame_music") as AudioClip;
        soundList.Add("Music_Ingame", new Sound(clip, 1f, 1f, sources[0]));

        clip = Resources.Load("Sounds/snake_move") as AudioClip;
        soundList.Add("Sound_Snake_Move", new Sound(clip, 1f, 1f, sources[1]));

        clip = Resources.Load("Sounds/snake_turn") as AudioClip;
        soundList.Add("Sound_Snake_Turn", new Sound(clip, 1f, 1f, sources[1]));

        clip = Resources.Load("Sounds/apple_eat") as AudioClip;
        soundList.Add("Sound_Apple_Normal", new Sound(clip, 1f, 1f, sources[2]));

        clip = Resources.Load("Sounds/apple_burning_eat") as AudioClip;
        soundList.Add("Sound_Apple_Burn", new Sound(clip, 1f, 1f, sources[2]));

        clip = Resources.Load("Sounds/apple_frozen_eat") as AudioClip;
        soundList.Add("Sound_Apple_Freeze", new Sound(clip, 1f, 1f, sources[2]));

        clip = Resources.Load("Sounds/apple_golden_eat") as AudioClip;
        soundList.Add("Sound_Apple_Gold", new Sound(clip, 1f, 1f, sources[2]));

        clip = Resources.Load("Sounds/apple_frozen_eat") as AudioClip;
        soundList.Add("Sound_Apple_Ghost", new Sound(clip, 1f, 1f, sources[2]));

        clip = Resources.Load("Sounds/gameover") as AudioClip;
        soundList.Add("Sound_Menu_Gameover", new Sound(clip, 1f, 1f, sources[3]));
    }

    public void PlaySound(string soundIdentifier)
    {
        if (soundList.ContainsKey(soundIdentifier))
        {
            soundList[soundIdentifier].Play();
        }
    }

    public void SetMusic(string filename)
    {
        AudioClip clip = Resources.Load("Music/" + filename) as AudioClip;
        soundList.Add("Music_Ingame", new Sound(clip, 1f, 1f, sources[0]));
    }

    public void PauseMusic()
    {
        sources[0].Pause();
    }

    public void PlayMusic()
    {
        sources[0].Play();
    }

    public void StopMusic()
    {
        sources[0].Stop();
    }

    public void SetMusicVolume(float volume)
    {
        sources[0].volume = volume;
    }

    public void SetMusicPitch(float pitch)
    {
        sources[0].pitch = pitch;
    }
}
