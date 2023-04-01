using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SFB;

public class MenuManager : MonoBehaviour
{
    public int sceneNumber;
    public AudioController audioController;
    [SerializeField] private TMPro.TextMeshProUGUI musicText;
    private bool musicOn;

    public void StartGame()
    {
        SceneManager.LoadScene(this.sceneNumber);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void SelectLevel()
    {
        string pathLevelFolder = Application.dataPath + "/Resources/Levels";
        string pathLevel = StandaloneFileBrowser.OpenFilePanel("Select .wld file", pathLevelFolder, "wld", false)[0];
        if (pathLevel != "")
        {
            PlayerPrefs.SetString("LevelPath", pathLevel);
        }
    }

    void Start()
    {
        audioController.SetMusicPitch(1f);
        if (PlayerPrefs.HasKey("Music"))
        {
            musicOn = (PlayerPrefs.GetInt("Music") > 0) ? true : false;
        }
        if (musicOn == true)
        {
            audioController.PlayMusic();
            musicText.SetText("music: on");
        }
        else musicText.SetText("music: off");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            musicOn = !musicOn;
            PlayerPrefs.SetInt("Music", musicOn ? 1 : 0);
            if (musicOn == true)
            {
                audioController.PlayMusic();
                musicText.SetText("music: on");
            }
            else
            {
                audioController.StopMusic();
                musicText.SetText("music: off");
            }
        }
    }
}
