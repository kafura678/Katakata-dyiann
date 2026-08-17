using UnityEngine;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    // ==================================================
    // UI
    // ==================================================

    [Header("Texts")]

    [SerializeField]
    private Text resultTitleText;

    [SerializeField]
    private Text scoreText;

    [SerializeField]
    private Text maxComboText;


    // ==================================================
    // Unity
    // ==================================================

    private void Start()
    {
        ShowResult();
    }


    // ==================================================
    // リザルト表示
    // ==================================================

    private void ShowResult()
    {
        // ==========================================
        // GameManager確認
        // ==========================================

        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "GameManagerが見つかりません"
            );

            return;
        }


        // ==========================================
        // GAME CLEAR / GAME OVER
        // ==========================================

        if (resultTitleText != null)
        {
            resultTitleText.text =
                GameManager.Instance.ResultIsClear
                ?
                "GAME CLEAR"
                :
                "GAME OVER";
        }


        // ==========================================
        // SCORE
        // ==========================================

        if (scoreText != null)
        {
            scoreText.text =
                "SCORE : " +
                GameManager.Instance.ResultScore;
        }


        // ==========================================
        // MAX COMBO
        // ==========================================

        if (maxComboText != null)
        {
            maxComboText.text =
                "MAX COMBO : " +
                GameManager.Instance.ResultMaxCombo;
        }
    }
}