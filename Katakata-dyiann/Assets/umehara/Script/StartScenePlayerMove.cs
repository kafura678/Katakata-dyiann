using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScenePlayerMove : MonoBehaviour
{
    // ==================================================
    // プレイヤー
    // ==================================================

    [Header("プレイヤー")]

    [Tooltip("スタート画面に表示しているプレイヤー")]
    [SerializeField]
    private Transform player;

    [Tooltip("STARTを押した後に移動する目的地")]
    [SerializeField]
    private Transform targetPosition;


    // ==================================================
    // 移動
    // ==================================================

    [Header("移動設定")]

    [Tooltip("目的地まで移動する時間")]
    [SerializeField]
    private float moveDuration = 1f;

    [Tooltip("移動終了時のサイズ倍率")]
    [Range(0f, 1f)]
    [SerializeField]
    private float endScaleRate = 0.1f;


    // ==================================================
    // 残像
    // ==================================================

    [Header("残像")]

    [Tooltip("移動中に生成する残像Prefab")]
    [SerializeField]
    private GameObject afterImagePrefab;

    [Tooltip("残像を生成する間隔")]
    [SerializeField]
    private float afterImageInterval = 0.05f;

    [Tooltip("残像が消えるまでの時間")]
    [SerializeField]
    private float afterImageLifeTime = 0.2f;


    // ==================================================
    // SE
    // ==================================================

    [Header("SE")]

    [Tooltip("SE再生用AudioSource")]
    [SerializeField]
    private AudioSource audioSource;

    [Tooltip("移動開始時に鳴らすSE")]
    [SerializeField]
    private AudioClip moveSE;

    [Range(0f, 1f)]
    [SerializeField]
    private float moveSEVolume = 1f;


    // ==================================================
    // UI
    // ==================================================

    [Header("UI")]

    [Tooltip("STARTボタン")]
    [SerializeField]
    private Button startButton;


    // ==================================================
    // シーン
    // ==================================================

    [Header("シーン")]

    [Tooltip("移動終了後に読み込むゲームシーン名")]
    [SerializeField]
    private string gameSceneName = "Game";


    // ==================================================
    // 状態
    // ==================================================

    private bool isStarting = false;


    // ==================================================
    // Unity
    // ==================================================

    private void Awake()
    {
        // ==========================================
        // AudioSource自動取得
        // ==========================================

        if (audioSource == null)
        {
            audioSource =
                GetComponent<AudioSource>();
        }
    }


    // ==================================================
    // STARTボタン
    //
    // ButtonのOnClickから呼ぶ
    // ==================================================

    public void StartGame()
    {
        // ==========================================
        // 二重実行防止
        // ==========================================

        if (isStarting)
        {
            return;
        }


        // ==========================================
        // Playerチェック
        // ==========================================

        if (player == null)
        {
            Debug.LogError(
                "StartScenePlayerMove：Playerが設定されていません"
            );

            return;
        }


        // ==========================================
        // Target Positionチェック
        // ==========================================

        if (targetPosition == null)
        {
            Debug.LogError(
                "StartScenePlayerMove：Target Positionが設定されていません"
            );

            return;
        }


        // ==========================================
        // 開始演出
        // ==========================================

        StartCoroutine(
            StartGameRoutine()
        );
    }


    // ==================================================
    // ゲーム開始演出
    // ==================================================

    private IEnumerator StartGameRoutine()
    {
        isStarting =
            true;


        // ==========================================
        // STARTボタンを無効化
        // ==========================================

        if (startButton != null)
        {
            startButton.interactable =
                false;
        }


        // ==========================================
        // 移動SE
        // ==========================================

        PlayMoveSE();


        // ==========================================
        // プレイヤー
        //
        // 移動
        // 縮小
        // 残像
        // ==========================================

        yield return StartCoroutine(
            MoveAndShrinkPlayer()
        );


        // ==========================================
        // 新しいゲームとして初期化
        //
        // Result → Title → Game
        // の場合にも前回の状態を残さない
        // ==========================================

        ResetGameData();


        // ==========================================
        // Gameシーンへ
        // ==========================================

        LoadGameScene();
    }


    // ==================================================
    // プレイヤー移動
    // ==================================================

    private IEnumerator MoveAndShrinkPlayer()
    {
        Vector3 startPosition =
            player.position;


        Vector3 endPosition =
            targetPosition.position;


        Vector3 startScale =
            player.localScale;


        Vector3 endScale =
            startScale *
            endScaleRate;


        // ==========================================
        // 移動時間0以下
        // ==========================================

        if (moveDuration <= 0f)
        {
            player.position =
                endPosition;


            player.localScale =
                endScale;


            yield break;
        }


        float timer =
            0f;


        float afterImageTimer =
            0f;


        // ==========================================
        // 移動開始地点にも残像
        // ==========================================

        SpawnAfterImage();


        // ==========================================
        // 移動
        // ==========================================

        while (timer < moveDuration)
        {
            float deltaTime =
                Time.deltaTime;


            timer +=
                deltaTime;


            afterImageTimer +=
                deltaTime;


            float t =
                Mathf.Clamp01(
                    timer /
                    moveDuration
                );


            // ======================================
            // 位置
            // ======================================

            player.position =
                Vector3.Lerp(
                    startPosition,
                    endPosition,
                    t
                );


            // ======================================
            // 徐々に縮小
            // ======================================

            player.localScale =
                Vector3.Lerp(
                    startScale,
                    endScale,
                    t
                );


            // ======================================
            // 残像
            // ======================================

            if (
                afterImageInterval > 0f &&
                afterImageTimer >= afterImageInterval
            )
            {
                afterImageTimer =
                    0f;


                SpawnAfterImage();
            }


            yield return null;
        }


        // ==========================================
        // 最終位置補正
        // ==========================================

        player.position =
            endPosition;


        player.localScale =
            endScale;
    }


    // ==================================================
    // 残像生成
    // ==================================================

    private void SpawnAfterImage()
    {
        if (afterImagePrefab == null)
        {
            return;
        }


        if (player == null)
        {
            return;
        }


        GameObject afterImage =
            Instantiate(
                afterImagePrefab,
                player.position,
                player.rotation
            );


        // ==========================================
        // 現在のPlayerサイズをコピー
        //
        // Playerが小さくなるほど
        // 残像も小さくなる
        // ==========================================

        afterImage.transform.localScale =
            player.localScale;


        // ==========================================
        // 残像削除
        // ==========================================

        if (afterImageLifeTime > 0f)
        {
            Destroy(
                afterImage,
                afterImageLifeTime
            );
        }
        else
        {
            Destroy(
                afterImage
            );
        }
    }


    // ==================================================
    // 移動SE
    // ==================================================

    private void PlayMoveSE()
    {
        if (audioSource == null)
        {
            return;
        }


        if (moveSE == null)
        {
            return;
        }


        audioSource.PlayOneShot(
            moveSE,
            moveSEVolume
        );
    }


    // ==================================================
    // GameManager初期化
    // ==================================================

    private void ResetGameData()
    {
        if (GameManager.Instance == null)
        {
            return;
        }


        GameManager.Instance.ResetGameData();
    }


    // ==================================================
    // Gameシーン読み込み
    // ==================================================

    private void LoadGameScene()
    {
        // ==========================================
        // シーン名チェック
        // ==========================================

        if (
            string.IsNullOrWhiteSpace(
                gameSceneName
            )
        )
        {
            Debug.LogError(
                "StartScenePlayerMove：Game Scene Nameが設定されていません"
            );


            ResetStartState();


            return;
        }


        // ==========================================
        // Build Profilesチェック
        // ==========================================

        if (
            !Application.CanStreamedLevelBeLoaded(
                gameSceneName
            )
        )
        {
            Debug.LogError(
                "シーン「" +
                gameSceneName +
                "」を読み込めません。\n" +
                "File → Build Profiles の Scene List に追加されているか確認してください。"
            );


            ResetStartState();


            return;
        }


        // ==========================================
        // Gameシーンへ
        // ==========================================

        SceneManager.LoadScene(
            gameSceneName
        );
    }


    // ==================================================
    // 開始失敗時の復帰
    // ==================================================

    private void ResetStartState()
    {
        isStarting =
            false;


        if (startButton != null)
        {
            startButton.interactable =
                true;
        }
    }
}