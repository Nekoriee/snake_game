using System.Collections;
using UnityEngine;

public class UIController : MonoBehaviour
{

    [SerializeField] private GameDirector gameDirector;
    [SerializeField] private TMPro.TextMeshProUGUI currentScore;
    [SerializeField] private TMPro.TextMeshProUGUI goalScore;
    [SerializeField] private TMPro.TextMeshProUGUI gameOver;
    [SerializeField] private TMPro.TextMeshProUGUI pause;
    [SerializeField] private TMPro.TextMeshProUGUI musicText;
    [SerializeField] private TMPro.TextMeshProUGUI modifierText;
    [SerializeField] private TMPro.TextMeshProUGUI speedText;
    [SerializeField] private TMPro.TextMeshProUGUI controlTimer;
    [SerializeField] private GameObject menuPanel;

    public void UpdateControlTimer(string timer)
    {
        controlTimer.text = timer;
    }

    public void UpdateCurrentScore(string score)
    {
        currentScore.text = "Score: " + score;
    }

    public void UpdateGoalScore(string score)
    {
        goalScore.text = "Record: " + score;
    }

    private void Awake()
    {
        gameOver.alpha = 0f;
        pause.alpha = 0f;
        menuPanel.SetActive(false);
    }

    public void StartPause()
    {
        //StartCoroutine(PauseAnim());
        pause.alpha = 0.52f;
        menuPanel.SetActive(true);
    }

    public void StopPause()
    {
        //StopAllCoroutines();
        pause.alpha = 0f;
        menuPanel.SetActive(false);
    }

    public void StartGameOver()
    {
        gameOver.alpha = 0.52f;
        menuPanel.SetActive(true);
    }

    public void StopGameOver()
    {
        gameOver.alpha = 0f;
        menuPanel.SetActive(false);
    }

    public void UpdateMusicText(bool isPlaying)
    {
        if (isPlaying)
        {
            musicText.SetText("music: on");
        }
        else
        {
            musicText.SetText("music: off");
        }
    }

    public void UpdateModifierText(string modifier)
    {
        switch (modifier)
        {
            case "classic":
                modifierText.text = "modifier: Classic";
                break;
            case "chill":
                modifierText.text = "modifier: Chill Out";
                break;
            case "chili_pepper":
                modifierText.text = "modifier: Hot Chili Apple";
                break;
            case "gold":
                modifierText.text = "modifier: Goldlike";
                break;
            case "hungry":
                modifierText.text = "modifier: No Growth";
                break;
            case "spaghetti":
                modifierText.text = "modifier: Spaghetti";
                break;
            case "control":
                modifierText.text = "modifier: Manual";
                break;
            case "double":
                modifierText.text = "modifier: Double Food";
                break;
            default:
                modifierText.text = "modifier: None";
                break;
        }
    }

    public void UpdateSpeedText(float speed)
    {
        switch (speed)
        {
            case 0.75f:
                speedText.text = "speed: Slow";
                break;
            case 1.5f:
                speedText.text = "speed: Fast";
                break;
            default:
                speedText.text = "speed: Normal";
                break;
        }
    }
}
