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

    private static bool bSineActive = false;
    public static IEnumerator StartVolumeFade(AudioSource audioSource, float duration, float targetVolume)
    {
        float currentTime = 0;
        float start = audioSource.volume;
        while (currentTime < duration)
        {
            currentTime += Time.unscaledTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
        yield break;
    }

    public static IEnumerator StartPitchSine(AudioSource audioSource, float targetMultiplier)
    {
        bSineActive = true;
        float currentTime = 0;
        float start = audioSource.pitch;
        while (bSineActive)
        {
            currentTime += Time.unscaledTime;
            audioSource.pitch = Mathf.Lerp(start, 1.0f + Mathf.Sin(currentTime * 10f) * targetMultiplier, currentTime);
            yield return null;
        }
        yield break;
    }

    private void StopPitchSine()
    {
        bSineActive = false;
    }

    public static IEnumerator StartPitchFade(AudioSource audioSource, float duration, float targetPitch)
    {
        float currentTime = 0;
        float start = audioSource.pitch;
        while (currentTime < duration)
        {
            currentTime += Time.unscaledTime;
            audioSource.pitch = Mathf.Lerp(start, targetPitch, currentTime / duration);
            yield return null;
        }
        yield break;
    }

    void Awake()
    {

        AudioClip clip = Resources.Load("Music/ingame_music") as AudioClip;
        soundList.Add("Music_Ingame", new Sound(clip, 1f, 1f, sources[0]));

        clip = Resources.Load("Sounds/snake_move") as AudioClip;
        soundList.Add("Sound_Snake_Move", new Sound(clip, 0.5f, 1f, sources[1]));

        clip = Resources.Load("Sounds/snake_move_ice") as AudioClip;
        soundList.Add("Sound_Snake_Move_Ice", new Sound(clip, 1f, 1f, sources[1]));

        clip = Resources.Load("Sounds/snake_move_water") as AudioClip;
        soundList.Add("Sound_Snake_Move_Water", new Sound(clip, 1f, 1f, sources[1]));

        clip = Resources.Load("Sounds/snake_fall") as AudioClip;
        soundList.Add("Sound_Snake_Fall", new Sound(clip, 1f, 1f, sources[1]));

        clip = Resources.Load("Sounds/snake_turn") as AudioClip;
        soundList.Add("Sound_Snake_Turn", new Sound(clip, 0.5f, 1f, sources[1]));

        clip = Resources.Load("Sounds/teleport") as AudioClip;
        soundList.Add("Sound_Portal", new Sound(clip, 1f, 1f, sources[2]));

        clip = Resources.Load("Sounds/apple_eat") as AudioClip;
        soundList.Add("Sound_Apple_Normal", new Sound(clip, 1f, 1f, sources[2]));

        clip = Resources.Load("Sounds/apple_burning_eat") as AudioClip;
        soundList.Add("Sound_Apple_Burn", new Sound(clip, 1f, 1f, sources[2]));

        clip = Resources.Load("Sounds/apple_frozen_eat") as AudioClip;
        soundList.Add("Sound_Apple_Freeze", new Sound(clip, 1f, 1f, sources[2]));

        clip = Resources.Load("Sounds/apple_golden_eat") as AudioClip;
        soundList.Add("Sound_Apple_Gold", new Sound(clip, 1f, 1f, sources[2]));

        clip = Resources.Load("Sounds/apple_ghost_eat") as AudioClip;
        soundList.Add("Sound_Apple_Ghost", new Sound(clip, 1f, 1f, sources[2]));

        clip = Resources.Load("Sounds/apple_drunk_eat") as AudioClip;
        soundList.Add("Sound_Apple_Drunk", new Sound(clip, 1f, 1f, sources[2]));

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

    public void SetMusicVolumeFade(float volume, float duration)
    {
        StartCoroutine(StartVolumeFade(sources[0], duration, volume));
    }

    public void SetMusicPitch(float pitch)
    {
        sources[0].pitch = pitch;
    }

    public void SetSoundPitch(float pitch, int src_n)
    {
        sources[src_n].pitch = pitch;
    }

    public void SetSoundVolume(float volume, int src_n)
    {
        sources[src_n].volume = volume;
    }

    public void SetMusicPitchFade(float pitch, float duration)
    {
        StopPitchSine();
        StartCoroutine(StartPitchFade(sources[0], duration, pitch));
    }

    public void SetMusicPitchSine(float pitch)
    {
        StartCoroutine(StartPitchSine(sources[0], pitch));
    }

    public void SetMusicDoppler(float doppler)
    {
        sources[0].dopplerLevel = doppler;
    }
}
