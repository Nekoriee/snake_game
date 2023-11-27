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
    [SerializeField] private GameObject PanelMain;
    [SerializeField] private GameObject PanelOptions;
    [SerializeField] private TMPro.TextMeshProUGUI speedText;
    [SerializeField] private TMPro.TextMeshProUGUI modifierText;
    private bool musicOn;
    private float prefsSpeed;
    private string prefsModifier;
    private float newPrefsSpeed;
    private string newPrefsModifier;
    private List<float> speedList;
    private List<string> modifierList;

    public void StartGame()
    {
        SceneManager.LoadScene(this.sceneNumber);
    }

    public void StartEditor()
    {
        SceneManager.LoadScene(2);
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

    public void OpenOptions(bool shouldOpen)
    {
        newPrefsModifier = prefsModifier;
        newPrefsSpeed = prefsSpeed;
        UpdateModifierText(prefsModifier);
        UpdateSpeedText(prefsSpeed);
        PanelMain.SetActive(!shouldOpen);
        PanelOptions.SetActive(shouldOpen);
    }

    void Start()
    {
        Screen.SetResolution(800, 800, false);
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
        //PlayerPrefs.SetFloat("Speed", 1f); // 0.75; 1; 1.5
        //PlayerPrefs.SetString("Modifier", "none");
        // none
        // classic
        // chill
        // chili_pepper
        // gold
        // hungry
        // spaghetti
        // control
    }

    public void GetPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("Speed") && PlayerPrefs.GetFloat("Speed") > 0f) prefsSpeed = PlayerPrefs.GetFloat("Speed");
        else prefsSpeed = 1f;
        if (PlayerPrefs.HasKey("Modifier")) prefsModifier = PlayerPrefs.GetString("Modifier");
        else prefsModifier = "none";
    }

    private void FillLists()
    {
        speedList = new List<float>();
        speedList.Add(0.75f);
        speedList.Add(1f);
        speedList.Add(1.5f);
        modifierList = new List<string>();
        modifierList.Add("none");
        modifierList.Add("classic");
        modifierList.Add("chill");
        modifierList.Add("chili_pepper");
        modifierList.Add("gold");
        modifierList.Add("drunk");
        modifierList.Add("hungry");
        modifierList.Add("spaghetti");
        modifierList.Add("control");
        modifierList.Add("double");
    }

    private void UpdateModifierText(string modifier)
    {
        switch (modifier)
        {
            case "classic":
                modifierText.text = "Classic";
                break;
            case "chill":
                modifierText.text = "Chill Out";
                break;
            case "chili_pepper":
                modifierText.text = "Hot Chili Apple";
                break;
            case "gold":
                modifierText.text = "Goldlike";
                break;
            case "hungry":
                modifierText.text = "Hungry";
                break;
            case "spaghetti":
                modifierText.text = "Spaghetti";
                break;
            case "control":
                modifierText.text = "Control";
                break;
            case "double":
                modifierText.text = "Double";
                break;
            case "drunk":
                modifierText.text = "Tipsy";
                break;
            default:
                modifierText.text = "None";
                break;
        }
    }

    private void UpdateSpeedText(float speed)
    {
        switch (speed)
        {
            case 0.75f:
                speedText.text = "Slow";
                break;
            case 1.5f:
                speedText.text = "Fast";
                break;
            default:
                speedText.text = "Normal";
                break;
        }
    }

    public void UpdateCurModifier(int step)
    {
        if (newPrefsModifier != null && modifierList.Count > 0) {
            int modifierIndex = modifierList.IndexOf(newPrefsModifier);
            if (modifierIndex > -1)
            {
                modifierIndex += step;
                if (modifierIndex < 0) modifierIndex = modifierList.Count - 1;
                else if (modifierIndex >= modifierList.Count) modifierIndex = 0;
                newPrefsModifier = modifierList[modifierIndex];
                UpdateModifierText(newPrefsModifier);
            }
        }
    }

    public void UpdateCurSpeed(int step)
    {
        if (speedList.Count > 0)
        {
            int speedIndex = speedList.IndexOf(newPrefsSpeed);
            if (speedIndex > -1)
            {
                speedIndex += step;
                if (speedIndex < 0) speedIndex = speedList.Count - 1;
                else if (speedIndex >= speedList.Count) speedIndex = 0;
                newPrefsSpeed = speedList[speedIndex];
                UpdateSpeedText(newPrefsSpeed);
            }
        }
    }

    public void SaveOptions()
    {
        PlayerPrefs.SetString("Modifier", newPrefsModifier);
        PlayerPrefs.SetFloat("Speed", newPrefsSpeed);
        prefsModifier = newPrefsModifier;
        prefsSpeed = newPrefsSpeed;
    }

    public void DeleteData()
    {
        PlayerPrefs.DeleteAll();
        prefsModifier = "none";
        prefsSpeed = 1f;
        musicOn = true;
        PlayerPrefs.SetInt("Music", 1);
        audioController.PlayMusic();
        musicText.SetText("music: on");
        UpdateModifierText(prefsModifier);
        UpdateSpeedText(prefsSpeed);
    }

    private void Awake()
    {
        GetPlayerPrefs();
        FillLists();
        UpdateModifierText(prefsModifier);
        UpdateSpeedText(prefsSpeed);
        OpenOptions(false);
        Cursor.visible = true;
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
