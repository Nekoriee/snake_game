using UnityEngine;

public class Sound
{
    private AudioClip clip;
    private float volume = 1;
    private float pitch = 1;
    private AudioSource source;

    public Sound(AudioClip clip, float volume, float pitch, AudioSource source)
    {
        this.clip = clip;
        this.volume = volume;
        this.pitch = pitch;
        this.source = source;
    }

    public void SetVolume(float volume)
    {
        this.volume = volume;
    }

    public void SetPitch(float pitch)
    {
        this.pitch = pitch;
    }

    public void Play()
    {
        source.PlayOneShot(clip);
    }

    public void PlayLooping()
    {
        source.Stop();
        source.loop = true;
        source.clip = clip;
        source.Play();
    }

    public void Pause()
    {
        source.Pause();
    }

    public void Stop()
    {
        source.Stop();
    }
}
