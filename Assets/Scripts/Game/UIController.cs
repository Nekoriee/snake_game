using System.Collections;
using UnityEngine;

public class UIController : MonoBehaviour
{

    [SerializeField] private GameDirector gameDirector;
    [SerializeField] private TMPro.TextMeshProUGUI currentScore;
    [SerializeField] private TMPro.TextMeshProUGUI goalScore;
    [SerializeField] private TMPro.TextMeshProUGUI gameOver;
    [SerializeField] private TMPro.TextMeshProUGUI pause;

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
    }

    public void StartPause()
    {
        StartCoroutine(PauseAnim());
    }

    public void StopPause()
    {
        StopAllCoroutines();
        pause.alpha = 0f;
    }

    public void StartGameOver()
    {
        gameOver.alpha = 1f;
    }

    public void StopGameOver()
    {
        gameOver.alpha = 0f;
    }
}
