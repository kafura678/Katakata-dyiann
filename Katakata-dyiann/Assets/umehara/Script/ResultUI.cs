using UnityEngine;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    [Header("Texts")]
    public Text resultTitleText;
    public Text clearTimeText;
    public Text scoreText;
    public Text defeatedCountText;

    private void Start()
    {
        ShowResult();
    }

    private void ShowResult()
    {
        if (resultTitleText != null)
        {
            resultTitleText.text = ResultData.isClear ? "GAME CLEAR" : "GAME OVER";
        }

        if (clearTimeText != null)
        {
            clearTimeText.text = "TIME : " + ResultData.clearTime.ToString("F2");
        }

        if (scoreText != null)
        {
            scoreText.text = "SCORE : " + ResultData.score;
        }

        if (defeatedCountText != null)
        {
            defeatedCountText.text = "DEFEAT : " + ResultData.defeatedEnemyCount;
        }
    }
}