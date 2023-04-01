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
    [SerializeField] private GameObject menuPanel;

    public void UpdateCurrentScore(string score)
    {
        currentScore.text = "Score: " + score;
    }

    public void UpdateGoalScore(string score)
    {
        goalScore.text = "Record: " + score;
    }

    public IEnumerator PauseAnim()
    {
        while (true)
        {
            pause.alpha = 1f - pause.alpha;
            yield return new WaitForSecondsRealtime(0.5f);
        }
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
        pause.alpha = 1f;
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
        gameOver.alpha = 1f;
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
}
