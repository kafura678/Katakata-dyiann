using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public bool isGameOver = false;
    public bool isClear = false;

    [Header("Timer")]
    public float gameTime = 0f;

    [Header("Score")]
    public int score = 0;
    public int defeatedEnemyCount = 0;

    [Header("Result")]
    public float resultDelay = 1.5f;

    private void Awake()
    {
        Instance = this;
        ResultData.Reset();
    }

    private void Update()
    {
        if (isGameOver) return;

        gameTime += Time.deltaTime;
    }

    public void AddScore(int amount)
    {
        score += amount;
    }

    public void AddDefeatedEnemyCount()
    {
        defeatedEnemyCount++;
    }

    public void GameClear()
    {
        if (isGameOver) return;

        isGameOver = true;
        isClear = true;

        SaveResultData();

        Invoke(nameof(LoadResultScene), resultDelay);
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        isClear = false;

        SaveResultData();

        Invoke(nameof(LoadResultScene), resultDelay);
    }

    private void SaveResultData()
    {
        ResultData.isClear = isClear;
        ResultData.clearTime = gameTime;
        ResultData.score = score;
        ResultData.defeatedEnemyCount = defeatedEnemyCount;
    }

    private void LoadResultScene()
    {
        SceneManager.LoadScene("ResultScene");
    }
}