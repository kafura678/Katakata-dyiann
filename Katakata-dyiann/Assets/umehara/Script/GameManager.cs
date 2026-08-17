using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // ==================================================
    // Singleton
    // ==================================================

    public static GameManager Instance;


    // ==================================================
    // スコア
    // ==================================================

    [Header("スコア")]

    [SerializeField]
    private int score = 0;


    // ==================================================
    // リザルト移行
    // ==================================================

    [Header("リザルト移行")]

    [Tooltip("ゲーム終了からResultシーンへ移動するまでの時間")]
    [SerializeField]
    private float resultDelay = 3f;

    [Tooltip("リザルトシーン名")]
    [SerializeField]
    private string resultSceneName = "Result";


    // ==================================================
    // ゲーム状態
    // ==================================================

    private bool isGameFinished = false;

    private bool isGameClear = false;

    private bool isLoadingResult = false;


    // ==================================================
    // リザルト保存データ
    // ==================================================

    private int resultScore = 0;

    private int resultMaxCombo = 0;

    private bool resultIsClear = false;


    // ==================================================
    // 外部参照
    // ==================================================

    public int Score => score;

    public bool IsGameFinished => isGameFinished;

    public bool IsGameClear => isGameClear;


    public int ResultScore => resultScore;

    public int ResultMaxCombo => resultMaxCombo;

    public bool ResultIsClear => resultIsClear;


    // ==================================================
    // Unity
    // ==================================================

    private void Awake()
    {
        // ==========================================
        // Singleton
        // ==========================================

        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(gameObject);

            return;
        }


        Instance = this;


        // ==========================================
        // Resultシーンまで保持
        // ==========================================

        DontDestroyOnLoad(
            gameObject
        );
    }


    // ==================================================
    // スコア加算
    // ==================================================

    public void AddScore(
        int amount
    )
    {
        // ==========================================
        // 0以下は加算しない
        // ==========================================

        if (amount <= 0)
        {
            return;
        }


        // ==========================================
        // ゲーム終了後はスコア変更しない
        // ==========================================

        if (isGameFinished)
        {
            return;
        }


        score += amount;


        Debug.Log(
            "SCORE : " +
            score
        );
    }


    // ==================================================
    // GAME CLEAR
    //
    // 敵撃破時に呼ぶ
    // ==================================================

    public void GameClear()
    {
        // ==========================================
        // 二重実行防止
        // ==========================================

        if (isGameFinished)
        {
            return;
        }


        isGameFinished =
            true;


        isGameClear =
            true;


        // ==========================================
        // リザルトデータ保存
        // ==========================================

        SaveResultData();


        Debug.Log(
            "GAME CLEAR"
        );


        // ==========================================
        // 数秒後にResultへ
        // ==========================================

        StartCoroutine(
            ResultTransitionRoutine()
        );
    }


    // ==================================================
    // GAME OVER
    //
    // プレイヤー死亡時に呼ぶ
    // ==================================================

    public void GameOver()
    {
        // ==========================================
        // 二重実行防止
        // ==========================================

        if (isGameFinished)
        {
            return;
        }


        isGameFinished =
            true;


        isGameClear =
            false;


        // ==========================================
        // リザルトデータ保存
        // ==========================================

        SaveResultData();


        Debug.Log(
            "GAME OVER"
        );


        // ==========================================
        // 数秒後にResultへ
        // ==========================================

        StartCoroutine(
            ResultTransitionRoutine()
        );
    }


    // ==================================================
    // リザルトデータ保存
    // ==================================================

    private void SaveResultData()
    {
        // ==========================================
        // スコア
        // ==========================================

        resultScore =
            score;


        // ==========================================
        // CLEAR / OVER
        // ==========================================

        resultIsClear =
            isGameClear;


        // ==========================================
        // 最大コンボ
        // ==========================================

        if (ComboManager.Instance != null)
        {
            resultMaxCombo =
                ComboManager.Instance
                    .MaxComboCount;
        }
        else
        {
            resultMaxCombo =
                0;
        }


        // ==========================================
        // Debug
        // ==========================================

        Debug.Log(
            "===== RESULT DATA ====="
        );


        Debug.Log(
            "RESULT : " +
            (
                resultIsClear
                    ? "GAME CLEAR"
                    : "GAME OVER"
            )
        );


        Debug.Log(
            "SCORE : " +
            resultScore
        );


        Debug.Log(
            "MAX COMBO : " +
            resultMaxCombo
        );
    }


    // ==================================================
    // Result移行待機
    // ==================================================

    private IEnumerator ResultTransitionRoutine()
    {
        // ==========================================
        // 二重読み込み防止
        // ==========================================

        if (isLoadingResult)
        {
            yield break;
        }


        isLoadingResult =
            true;


        // ==========================================
        // 待機時間
        // ==========================================

        float delay =
            Mathf.Max(
                0f,
                resultDelay
            );


        /*
         * Time.timeScale が0になっていても
         * Resultへ移行できるようにRealtimeを使用
         */

        if (delay > 0f)
        {
            yield return new WaitForSecondsRealtime(
                delay
            );
        }


        // ==========================================
        // Resultシーンへ
        // ==========================================

        GoToResultScene();
    }


    // ==================================================
    // Resultシーン読み込み
    // ==================================================

    private void GoToResultScene()
    {
        // ==========================================
        // シーン名チェック
        // ==========================================

        if (
            string.IsNullOrWhiteSpace(
                resultSceneName
            )
        )
        {
            Debug.LogError(
                "GameManager：Result Scene Nameが設定されていません"
            );


            isLoadingResult =
                false;


            return;
        }


        // ==========================================
        // Build Profilesチェック
        // ==========================================

        if (
            !Application.CanStreamedLevelBeLoaded(
                resultSceneName
            )
        )
        {
            Debug.LogError(
                "Resultシーン「" +
                resultSceneName +
                "」を読み込めません。\n" +
                "File → Build Profiles の Scene List に追加されているか確認してください。"
            );


            isLoadingResult =
                false;


            return;
        }


        // ==========================================
        // シーン移動
        // ==========================================

        SceneManager.LoadScene(
            resultSceneName
        );
    }


    // ==================================================
    // 新しいゲーム用リセット
    //
    // タイトル → ゲーム開始時などに使用
    // ==================================================

    public void ResetGameData()
    {
        // ==========================================
        // 動いているResult移行Coroutineを停止
        // ==========================================

        StopAllCoroutines();


        // ==========================================
        // ゲームデータ
        // ==========================================

        score =
            0;


        isGameFinished =
            false;


        isGameClear =
            false;


        isLoadingResult =
            false;


        // ==========================================
        // リザルトデータ
        // ==========================================

        resultScore =
            0;


        resultMaxCombo =
            0;


        resultIsClear =
            false;


        Debug.Log(
            "GameManager：ゲームデータをリセットしました"
        );
    }
}