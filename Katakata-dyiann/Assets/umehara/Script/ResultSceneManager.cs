using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultSceneManager : MonoBehaviour
{
    [Header("シーン")]
    [SerializeField]
    private string gameSceneName = "Game";


    // ==================================================
    // リトライ
    // ==================================================

    public void Retry()
    {
        // ==========================================
        // GameManagerを新しいゲーム状態へ戻す
        // ==========================================

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGameData();
        }


        // ==========================================
        // ゲームシーンへ
        // ==========================================

        SceneManager.LoadScene(
            gameSceneName
        );
    }
}