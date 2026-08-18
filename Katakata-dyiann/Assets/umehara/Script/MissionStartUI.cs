using System.Collections;
using TMPro;
using UnityEngine;

public class MissionStartUI : MonoBehaviour
{
    // ==================================================
    // MISSION UI
    // ==================================================

    [Header("MISSION UI")]

    [Tooltip("MISSION：敵機の破壊 を表示するText")]
    [SerializeField]
    private TMP_Text missionText;

    [Tooltip("MISSION START を表示するText")]
    [SerializeField]
    private TMP_Text missionStartText;


    // ==================================================
    // 表示内容
    // ==================================================

    [Header("表示内容")]

    [SerializeField]
    private string missionMessage =
        "MISSION：敵機の破壊";

    [SerializeField]
    private string missionStartMessage =
        "MISSION START";


    // ==================================================
    // 表示時間
    // ==================================================

    [Header("表示時間")]

    [Tooltip("MISSION表示からMISSION START表示までの時間")]
    [SerializeField]
    private float missionDisplayTime = 3f;

    [Tooltip("MISSION STARTを表示している時間")]
    [SerializeField]
    private float missionStartDisplayTime = 1f;


    // ==================================================
    // プレイヤー
    // ==================================================

    [Header("プレイヤー登場")]

    [Tooltip("プレイヤー")]
    [SerializeField]
    private Transform player;

    [Tooltip("登場時に移動する目的地")]
    [SerializeField]
    private Transform playerStartTarget;

    [Tooltip("登場移動にかける時間")]
    [SerializeField]
    private float playerEntryMoveDuration = 0.5f;


    // ==================================================
    // 敵
    // ==================================================

    [Header("敵の切り替え")]

    [Tooltip("開始演出中に表示するハリボテ敵")]
    [SerializeField]
    private GameObject dummyEnemy;

    [Tooltip("実際に戦う敵")]
    [SerializeField]
    private GameObject realEnemy;


    // ==================================================
    // 他スクリプト
    // ==================================================

    private PlayerKeyMover playerKeyMover;


    // ==================================================
    // 状態
    // ==================================================

    private bool isMissionStarting = false;


    // ==================================================
    // Unity
    // ==================================================

    private void Awake()
    {
        // ==========================================
        // PlayerKeyMover取得
        // ==========================================

        if (player != null)
        {
            playerKeyMover =
                player.GetComponent<PlayerKeyMover>();
        }


        // ==========================================
        // UI初期状態
        // ==========================================

        SetMissionUIActive(
            false
        );


        // ==========================================
        // 敵初期状態
        // ==========================================

        PrepareEnemy();
    }


    private void Start()
    {
        StartCoroutine(
            MissionStartRoutine()
        );
    }


    // ==================================================
    // ゲーム開始演出
    // ==================================================

    private IEnumerator MissionStartRoutine()
    {
        if (isMissionStarting)
        {
            yield break;
        }


        isMissionStarting =
            true;


        // ==========================================
        // プレイヤー操作禁止
        // ==========================================

        SetPlayerInput(
            false
        );


        // ==========================================
        // FLOW ON
        //
        // 開始演出中の停止状態
        // ==========================================

        SetFlowMode(
            true
        );


        // ==========================================
        // ハリボテ敵を表示
        // 本物の敵を非表示
        // ==========================================

        PrepareEnemy();


        // ==========================================
        // プレイヤー登場移動
        //
        // キーを押した時と同じ移動演出
        // ==========================================

        yield return StartCoroutine(
            MovePlayerToStartPosition()
        );


        // ==========================================
        // MISSION表示
        // ==========================================

        ShowMission();


        // ==========================================
        // MISSION STARTまで待つ
        // ==========================================

        if (missionDisplayTime > 0f)
        {
            yield return new WaitForSeconds(
                missionDisplayTime
            );
        }


        // ==========================================
        // MISSION START表示
        //
        // MISSION：敵機の破壊 はそのまま表示
        // ==========================================

        ShowMissionStart();


        // ==========================================
        // MISSION START演出時間
        // ==========================================

        if (missionStartDisplayTime > 0f)
        {
            yield return new WaitForSeconds(
                missionStartDisplayTime
            );
        }


        // ==========================================
        // MISSION UIを両方消す
        // ==========================================

        SetMissionUIActive(
            false
        );


        // ==========================================
        // ハリボテ敵 → 本物の敵
        // ==========================================

        ReplaceEnemy();


        // ==========================================
        // FLOW OFF
        // ==========================================

        SetFlowMode(
            false
        );


        // ==========================================
        // プレイヤー操作解禁
        //
        // MISSION START終了と同時
        // ==========================================

        SetPlayerInput(
            true
        );


        isMissionStarting =
            false;
    }


    // ==================================================
    // プレイヤー登場移動
    // ==================================================

    private IEnumerator MovePlayerToStartPosition()
    {
        if (
            player == null ||
            playerStartTarget == null
        )
        {
            yield break;
        }


        // ==========================================
        // PlayerKeyMoverを再取得
        // ==========================================

        if (playerKeyMover == null)
        {
            playerKeyMover =
                player.GetComponent<PlayerKeyMover>();
        }


        // ==========================================
        // PlayerKeyMoverがある場合
        //
        // 通常キー移動と同じ演出で移動
        // ==========================================

        if (playerKeyMover != null)
        {
            yield return StartCoroutine(
                playerKeyMover.MoveForMissionIntro(
    playerStartTarget.position
)
            );


            yield break;
        }


        // ==========================================
        // PlayerKeyMoverが無い場合
        // 念のため瞬間移動
        // ==========================================

        player.position =
            playerStartTarget.position;
    }


    // ==================================================
    // 敵初期状態
    // ==================================================

    private void PrepareEnemy()
    {
        // ==========================================
        // ハリボテ敵 ON
        // ==========================================

        if (dummyEnemy != null)
        {
            dummyEnemy.SetActive(
                true
            );
        }


        // ==========================================
        // 本物の敵 OFF
        // ==========================================

        if (realEnemy != null)
        {
            realEnemy.SetActive(
                false
            );
        }
    }


    // ==================================================
    // ハリボテ → 本物
    // ==================================================

    private void ReplaceEnemy()
    {
        // ==========================================
        // ハリボテ敵 OFF
        // ==========================================

        if (dummyEnemy != null)
        {
            dummyEnemy.SetActive(
                false
            );
        }


        // ==========================================
        // 本物の敵 ON
        // ==========================================

        if (realEnemy != null)
        {
            realEnemy.SetActive(
                true
            );
        }
    }


    // ==================================================
    // MISSION表示
    // ==================================================

    private void ShowMission()
    {
        if (missionText != null)
        {
            missionText.text =
                missionMessage;


            missionText.gameObject.SetActive(
                true
            );
        }


        // ==========================================
        // MISSION STARTはまだ表示しない
        // ==========================================

        if (missionStartText != null)
        {
            missionStartText.gameObject.SetActive(
                false
            );
        }
    }


    // ==================================================
    // MISSION START表示
    // ==================================================

    private void ShowMissionStart()
    {
        if (missionStartText == null)
        {
            return;
        }


        missionStartText.text =
            missionStartMessage;


        missionStartText.gameObject.SetActive(
            true
        );
    }


    // ==================================================
    // MISSION UI表示切り替え
    // ==================================================

    private void SetMissionUIActive(
        bool active
    )
    {
        if (missionText != null)
        {
            missionText.gameObject.SetActive(
                active
            );
        }


        if (missionStartText != null)
        {
            missionStartText.gameObject.SetActive(
                active
            );
        }
    }


    // ==================================================
    // プレイヤー入力
    // ==================================================

    private void SetPlayerInput(
        bool enabled
    )
    {
        if (playerKeyMover == null)
        {
            return;
        }


        playerKeyMover.SetInputEnabled(
            enabled
        );
    }


    // ==================================================
    // FLOWモード
    // ==================================================

    private void SetFlowMode(
        bool enabled
    )
    {
        if (FlowManager.Instance == null)
        {
            return;
        }


        FlowManager.Instance.isFlowMode =
            enabled;
    }
}