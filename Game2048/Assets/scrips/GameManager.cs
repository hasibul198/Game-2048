using UnityEngine;
using System.Collections;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public TileBoard board;
    public CanvasGroup gameOver;
    public GameObject gameOverPanel;
    public GameObject ResetButton;
    public GameObject PlayAgainButton;
    public RectTransform _2048;
    public GameObject gameOverTitleText;

    [Header("Score Containers (RectTransform)")]
    public RectTransform scoreGroupRect;
    public RectTransform bestGroupRect;

    [Header("Animation Settings")]
    public float animationDuration = 0.8f;
    public Vector3 targetScale = new Vector3(1.2f, 1.2f, 1.2f); 

    [Header("Score Texts (Values)")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI hiscoreText;
    public TextMeshProUGUI currentScoreText;

    [Header("Best Score Texts")]
    public TextMeshProUGUI todayBestText;
    public TextMeshProUGUI weekBestText;
    public TextMeshProUGUI allTimeBestText;

    private Vector2 scoreGroupOriginalPos;
    private Vector2 bestGroupOriginalPos;
    private Vector3 scoreGroupOriginalScale;
    private Vector3 bestGroupOriginalScale;
    public int score;

    private void Awake()
    {
        // position and sale save kore thakbe
        scoreGroupOriginalPos = scoreGroupRect.anchoredPosition;
        bestGroupOriginalPos = bestGroupRect.anchoredPosition;
        scoreGroupOriginalScale = scoreGroupRect.localScale;
        bestGroupOriginalScale = bestGroupRect.localScale;
    }

    private void Start()
    {
        NewGame();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NewGame()
    {
        SetScore(0);
        hiscoreText.text = LoadAllTimeBest().ToString();

        // ager jagai fire jabe, scale o reset hobe
        StopAllCoroutines(); 
        scoreGroupRect.anchoredPosition = scoreGroupOriginalPos;
        bestGroupRect.anchoredPosition = bestGroupOriginalPos;
        scoreGroupRect.localScale = scoreGroupOriginalScale;
        bestGroupRect.localScale = bestGroupOriginalScale;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        gameOver.alpha = 0f;
        gameOver.interactable = false;
        gameOver.blocksRaycasts = false;

        board.ClearBoard();
        Canvas.ForceUpdateCanvases();

        board.CreateTile();
        board.CreateTile();
        board.enabled = true;

        if (ResetButton != null) ResetButton.SetActive(true);
        if (PlayAgainButton != null) PlayAgainButton.SetActive(false);

        // game suru hole "2048" lekha dekhabe
        if (_2048 != null) _2048.gameObject.SetActive(true);
        if (gameOverTitleText != null) gameOverTitleText.SetActive(false);
    }

    public void GameOver()
    {
        if (ResetButton != null) ResetButton.SetActive(false);
        board.enabled = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        gameOver.interactable = true;
        gameOver.blocksRaycasts = true;

        if (PlayAgainButton != null) PlayAgainButton.SetActive(true);

        HandleScores();

        // smothly move
        if (_2048 != null)
        {
            Vector2 logoPos = _2048.anchoredPosition;
            Vector2 targetBestPos = new Vector2(0f, logoPos.y - 0f);
            Vector2 targetScorePos = new Vector2(0f, logoPos.y - 150f);

            StartCoroutine(AnimateGameOverUI(targetBestPos, targetScorePos));
        }

        // game over hole "game over lekha dekhabe"
        if (_2048 != null) _2048.gameObject.SetActive(false);
        if (gameOverTitleText != null) gameOverTitleText.SetActive(true);

        StartCoroutine(Fade(gameOver, 1f, 0.5f));
    }

    // smoth position sorbe
    private IEnumerator AnimateGameOverUI(Vector2 targetBestPos, Vector2 targetScorePos)
    {
        float elapsed = 0f;

        Vector2 startBestPos = bestGroupRect.anchoredPosition;
        Vector2 startScorePos = scoreGroupRect.anchoredPosition;
        Vector3 startBestScale = bestGroupRect.localScale;
        Vector3 startScoreScale = scoreGroupRect.localScale;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animationDuration);

            // position sorbe
            bestGroupRect.anchoredPosition = Vector2.Lerp(startBestPos, targetBestPos, t);
            scoreGroupRect.anchoredPosition = Vector2.Lerp(startScorePos, targetScorePos, t);

            // scale boro korbe
            bestGroupRect.localScale = Vector3.Lerp(startBestScale, targetScale, t);
            scoreGroupRect.localScale = Vector3.Lerp(startScoreScale, targetScale, t);

            yield return null;
        }

        // final position and scale set kore deya
        bestGroupRect.anchoredPosition = targetBestPos;
        scoreGroupRect.anchoredPosition = targetScorePos;
        bestGroupRect.localScale = targetScale;
        scoreGroupRect.localScale = targetScale;
    }

    private void HandleScores()
    {
        int allTimeBest = LoadAllTimeBest();
        int todayBest = PlayerPrefs.GetInt("TodayBest", 0);
        int weekBest = PlayerPrefs.GetInt("WeekBest", 0);

        if (score > allTimeBest)
        {
            PlayerPrefs.SetInt("AllTimeBest", score);
            allTimeBest = score;
        }

        if (score > todayBest) PlayerPrefs.SetInt("TodayBest", score);
        if (score > weekBest) PlayerPrefs.SetInt("WeekBest", score);

        PlayerPrefs.Save();

        todayBestText.text = PlayerPrefs.GetInt("TodayBest", 0).ToString();
        weekBestText.text = PlayerPrefs.GetInt("WeekBest", 0).ToString();
        allTimeBestText.text = allTimeBest.ToString();

        currentScoreText.text = score.ToString();
    }

    private void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString();
    }

    private int LoadAllTimeBest()
    {
        return PlayerPrefs.GetInt("AllTimeBest", 0);
    }

    public void IncreaseScore(int points)
    {
        SetScore(score + points);
    }

    private IEnumerator Fade(CanvasGroup canvasGroup, float to, float delay)
    {
        yield return new WaitForSeconds(delay);
        float elapsed = 0f;
        float duration = 0.5f;
        float from = canvasGroup.alpha;

        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}