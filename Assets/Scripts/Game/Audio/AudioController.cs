using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class AudioController : MonoBehaviour
{
    [SerializeField] private List<AudioSource> sources;
    // sources[0] = music
    // sources[1] = snake
    // sources[2] = item
    // sources[3] = menu
    [SerializeField] public Dictionary<string, Sound> soundList = new Dictionary<string, Sound>();

    private static bool bSineActive = false;

    static List<AudioClip> musicListGame;
    static List<AudioClip> musicListMenu;
    static List<string> musicNamesGame;
    static List<string> musicNamesMenu;
    static int musicCurGame = 0;
    static int musicCurMenu = 0;

    private void GetMusicFiles()
    {
        if (musicListGame == null || musicListGame.Count <= 0)
        {
            musicListGame = new List<AudioClip>();
            string pathGame = Application.dataPath + "/Resources/Music/Game";
            if (!Directory.Exists(pathGame)) Directory.CreateDirectory(pathGame);
            DirectoryInfo directoryInfo = new DirectoryInfo(Application.dataPath + "/Resources/Music/Game");
            FileInfo[] songFiles = directoryInfo.GetFiles("*.wav");
            foreach (FileInfo songFile in songFiles)
            {
                StartCoroutine(ConvertAudioClip(songFile, "game"));
            }
        }
        if (musicListMenu == null || musicListMenu.Count <= 0)
        {
            musicListMenu = new List<AudioClip>();
            string pathMenu = Application.dataPath + "/Resources/Music/Menu";
            if (!Directory.Exists(pathMenu)) Directory.CreateDirectory(pathMenu);
            DirectoryInfo directoryInfo = new DirectoryInfo(Application.dataPath + "/Resources/Music/Menu");
            FileInfo[] songFiles = directoryInfo.GetFiles("*.wav");
            foreach (FileInfo songFile in songFiles)
            {
                StartCoroutine(ConvertAudioClip(songFile, "menu"));
            }
        }
    }

    private IEnumerator ConvertAudioClip(FileInfo songFile, string musicType)
    {
        Debug.Log("Music: " + songFile.Name);
        if (songFile.Name.Contains("meta"))
            yield break;
        else
        {
            string songName = songFile.FullName.ToString();
            string url = string.Format("file://{0}", songName);
            Debug.Log("Music URL: " + url);
            using (UnityWebRequest web = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
            {
                yield return web.SendWebRequest();
                if (!web.isNetworkError && !web.isHttpError)
                {
                    var clip = DownloadHandlerAudioClip.GetContent(web);
                    if (clip != null)
                    {
                        Debug.Log(songFile + " added to " + musicType);
                        if (musicType == "menu") musicListMenu.Add(clip);
                        else musicListGame.Add(clip);
                    }
                }
            }
        }
    }

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

        GetMusicFiles();

        AudioClip clip = Resources.Load("Sounds/snake_move") as AudioClip;
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

        SetRandomMusic();
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
	
	private void SetNextMusicIndex() 
	{
		musicCurGame += 1;
		if (musicCurGame > musicListGame.Count-1) musicCurGame = 0;
		musicCurMenu += 1;
		if (musicCurMenu > musicListMenu.Count-1) musicCurMenu = 0;
	}

    public void SetRandomMusic()
    {
        AudioClip clip;
        if (soundList.ContainsKey("Music_Ingame")) soundList.Remove("Music_Ingame");
        if (soundList.ContainsKey("Music_Menu")) soundList.Remove("Music_Menu");
		SetNextMusicIndex();
        if (musicListGame != null && musicListGame.Count > 0)
        {

            soundList.Add("Music_Ingame", new Sound(musicListGame[musicCurGame], 1f, 1f, sources[0]));
            Debug.LogWarning("Loaded custom game music!");
        }
        else
        {
			Debug.Log("Using default ingame music...");
            clip = Resources.Load("Music/ingame_music") as AudioClip;
            soundList.Add("Music_Ingame", new Sound(clip, 1f, 1f, sources[0]));
        }

        if (musicListMenu != null && musicListMenu.Count > 0)
        {
            soundList.Add("Music_Menu", new Sound(musicListMenu[musicCurMenu], 1f, 1f, sources[0]));
            Debug.LogWarning("Loaded custom menu music!");
        }
        else
        {
			Debug.Log("Using default menu music...");
            clip = Resources.Load("Music/menu_music") as AudioClip;
            soundList.Add("Music_Menu", new Sound(clip, 1f, 1f, sources[0]));
        }
    }

    public void PauseMusic()
    {
        sources[0].Pause();
    }

    public void PlayMusic(string type)
    {
		if (type == "Game") soundList["Music_Ingame"].PlayLooping();
		else soundList["Music_Menu"].PlayLooping();
    }
	
	public void PlayRandomMusic(string type)
    {
		SetRandomMusic();
		PlayMusic(type);
		
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
