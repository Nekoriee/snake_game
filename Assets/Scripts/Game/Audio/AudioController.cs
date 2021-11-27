using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] private List<AudioSource> sources;
    [SerializeField] public Dictionary<string, Sound> soundList = new Dictionary<string, Sound>();
    void Awake()
    {
        AudioClip clip = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Music/ingame_music.wav", typeof(Object)) as AudioClip;
        soundList.Add("Music_Ingame", new Sound(clip, 1f, 1f, sources[0]));

        clip = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Sounds/snake_move.wav", typeof(Object)) as AudioClip;
        soundList.Add("Sound_Snake_Move", new Sound(clip, 1f, 1f, sources[1]));

        clip = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Sounds/snake_turn.wav", typeof(Object)) as AudioClip;
        soundList.Add("Sound_Snake_Turn", new Sound(clip, 1f, 1f, sources[1]));

        clip = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Sounds/apple_eat.wav", typeof(Object)) as AudioClip;
        soundList.Add("Sound_Apple_Normal", new Sound(clip, 1f, 1f, sources[2]));

        clip = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Sounds/apple_burning_eat.wav", typeof(Object)) as AudioClip;
        soundList.Add("Sound_Apple_Burn", new Sound(clip, 1f, 1f, sources[2]));

        clip = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Sounds/apple_frozen_eat.wav", typeof(Object)) as AudioClip;
        soundList.Add("Sound_Apple_Freeze", new Sound(clip, 1f, 1f, sources[2]));

        clip = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Sounds/apple_golden_eat.wav", typeof(Object)) as AudioClip;
        soundList.Add("Sound_Apple_Gold", new Sound(clip, 1f, 1f, sources[2]));

        clip = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Sounds/apple_golden_eat.wav", typeof(Object)) as AudioClip;
        soundList.Add("Sound_Apple_Ghost", new Sound(clip, 1f, 1f, sources[2]));

        clip = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Sounds/gameover.wav", typeof(Object)) as AudioClip;
        soundList.Add("Sound_Menu_Gameover", new Sound(clip, 1f, 1f, sources[3]));
    }

    public void PlaySound(string soundIdentifier)
    {
        if (soundList.ContainsKey(soundIdentifier))
        {
            soundList[soundIdentifier].Play();
        }
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
