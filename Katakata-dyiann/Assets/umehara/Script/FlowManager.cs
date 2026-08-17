using UnityEngine;

public class FlowManager : MonoBehaviour
{
    public static FlowManager Instance;


    // ==================================================
    // フローゲージ
    // ==================================================

    [Header("フローゲージ")]

    [Tooltip("現在のフローゲージ")]
    public float flowGauge = 0f;

    [Tooltip("フローゲージ最大値")]
    public float maxFlowGauge = 100f;

    [Tooltip("攻撃1回で増えるフローゲージ")]
    public float flowGainPerHit = 10f;


    // ==================================================
    // フローモード
    // ==================================================

    [Header("フローモード")]

    [Tooltip("現在フローモード中か")]
    public bool isFlowMode = false;

    [Tooltip("フローモード継続時間")]
    [SerializeField]
    private float flowDuration = 5f;

    private float flowTimer = 0f;


    // ==================================================
    // 超必殺
    // ==================================================

    [Header("超必殺")]

    [Tooltip("超必殺READYに必要な攻撃回数")]
    [SerializeField]
    private int requiredAttackCount = 8;

    private int attackCountInFlow = 0;

    [Tooltip("超必殺が使用可能か")]
    public bool isUltimateReady = false;

    [Tooltip("超必殺が現在再生中か")]
    public bool isUltimatePlaying = false;


    // ==================================================
    // 敵への影響
    // ==================================================

    [Header("フロー中の敵")]

    [Tooltip("フロー中の敵の速度倍率")]
    [SerializeField, Range(0f, 1f)]
    private float enemySlowRate = 0.3f;

    [Tooltip("フロー中に敵の射撃を止める")]
    [SerializeField]
    private bool stopEnemyShooting = true;


    // ==================================================
    // Player
    // ==================================================

    [Header("Player")]

    [SerializeField]
    private PlayerKeyMover player;


    // ==================================================
    // プロパティ
    // ==================================================

    public float FlowTimer
    {
        get
        {
            return flowTimer;
        }
    }

    public float FlowDuration
    {
        get
        {
            return flowDuration;
        }
    }

    public int AttackCountInFlow
    {
        get
        {
            return attackCountInFlow;
        }
    }

    public int RequiredAttackCount
    {
        get
        {
            return requiredAttackCount;
        }
    }

    public float FlowRate
    {
        get
        {
            if (maxFlowGauge <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(
                flowGauge /
                maxFlowGauge
            );
        }
    }


    // ==================================================
    // Unity
    // ==================================================

    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Inspectorで未設定なら自動取得
        if (player == null)
        {
            player =
                FindFirstObjectByType<PlayerKeyMover>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log(
                "Enter押下 " +
                " Flow=" + isFlowMode +
                " Ready=" + isUltimateReady +
                " Playing=" + isUltimatePlaying +
                " Attack=" + attackCountInFlow
            );
        }

        HandleInput();
        UpdateFlowMode();
    }


    // ==================================================
    // 入力
    // ==================================================

    private void HandleInput()
    {
        // ------------------------------------------
        // 超必殺発動
        // ------------------------------------------

        if (
            isFlowMode &&
            isUltimateReady &&
            !isUltimatePlaying &&
            Input.GetKeyDown(KeyCode.Return)
        )
        {
            StartUltimate();

            return;
        }


        // ------------------------------------------
        // フロー中は再発動不可
        // ------------------------------------------

        if (isFlowMode)
        {
            return;
        }


        // ------------------------------------------
        // Shift + A
        // ------------------------------------------

        bool shiftPressed =
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift);


        if (
            shiftPressed &&
            Input.GetKeyDown(KeyCode.A)
        )
        {
            StartFlowMode();
        }
    }


    // ==================================================
    // フローゲージ追加
    // ==================================================

    public void AddFlowGauge(
        float amount
    )
    {
        // フロー中はゲージを増やさない
        if (isFlowMode)
        {
            return;
        }

        if (amount <= 0f)
        {
            return;
        }


        flowGauge += amount;


        flowGauge =
            Mathf.Clamp(
                flowGauge,
                0f,
                maxFlowGauge
            );
    }


    // ==================================================
    // フローモード開始
    // ==================================================

    public void StartFlowMode()
    {
        // すでにフロー中
        if (isFlowMode)
        {
            return;
        }


        // ゲージ不足
        if (flowGauge < maxFlowGauge)
        {
            return;
        }


        isFlowMode = true;

        flowTimer =
            flowDuration;

        attackCountInFlow = 0;

        isUltimateReady = false;

        isUltimatePlaying = false;


        Debug.Log(
            "フローモード開始"
        );
    }


    // ==================================================
    // フローモード更新
    // ==================================================

    private void UpdateFlowMode()
    {
        if (!isFlowMode)
        {
            return;
        }


        // ------------------------------------------
        // 超必殺中はフロー時間停止
        // ------------------------------------------

        if (isUltimatePlaying)
        {
            return;
        }


        flowTimer -=
            Time.deltaTime;


        if (flowTimer <= 0f)
        {
            flowTimer = 0f;

            EndFlowMode();
        }
    }


    // ==================================================
    // フロー中の攻撃成功
    // PlayerKeyMoverから呼ぶ
    // ==================================================

    public void CountFlowAttack()
    {
        if (!isFlowMode)
        {
            return;
        }


        if (isUltimatePlaying)
        {
            return;
        }


        attackCountInFlow++;


        // ------------------------------------------
        // 超必殺READY
        // ------------------------------------------

        if (
            attackCountInFlow >=
            requiredAttackCount
        )
        {
            if (!isUltimateReady)
            {
                isUltimateReady = true;

                Debug.Log(
                    "ULTIMATE READY"
                );
            }
        }
    }


    // ==================================================
    // 超必殺開始
    // ==================================================

    private void StartUltimate()
    {
        if (!isFlowMode)
        {
            return;
        }

        if (!isUltimateReady)
        {
            return;
        }

        if (isUltimatePlaying)
        {
            return;
        }


        if (player == null)
        {
            player =
                FindFirstObjectByType<PlayerKeyMover>();
        }


        if (player == null)
        {
            Debug.LogWarning(
                "PlayerKeyMoverが見つかりません"
            );

            return;
        }


        isUltimatePlaying = true;

        isUltimateReady = false;


        Debug.Log(
            "超必殺発動"
        );


        player.StartUltimateAttack();
    }


    // ==================================================
    // 超必殺終了
    // PlayerKeyMoverから呼ぶ
    // ==================================================

    public void UltimateFinished()
    {
        if (!isUltimatePlaying)
        {
            return;
        }


        isUltimatePlaying = false;


        Debug.Log(
            "超必殺終了"
        );


        EndFlowMode();
    }


    // ==================================================
    // フローモード終了
    // ==================================================

    private void EndFlowMode()
    {
        isFlowMode = false;

        isUltimateReady = false;

        isUltimatePlaying = false;


        flowGauge = 0f;

        flowTimer = 0f;

        attackCountInFlow = 0;


        Debug.Log(
            "フローモード終了"
        );
    }


    // ==================================================
    // 敵の速度倍率
    // Enemy側から取得
    // ==================================================

    public float GetEnemySpeedRate()
    {
        if (isFlowMode)
        {
            return enemySlowRate;
        }


        return 1f;
    }


    // ==================================================
    // 敵が射撃可能か
    // Enemy / Boss側から取得
    // ==================================================

    public bool CanEnemyShoot()
    {
        if (
            isFlowMode &&
            stopEnemyShooting
        )
        {
            return false;
        }


        return true;
    }


    // ==================================================
    // フローゲージを最大にする
    // デバッグや特殊効果用
    // ==================================================

    public void SetFlowGaugeMax()
    {
        flowGauge =
            maxFlowGauge;
    }


    // ==================================================
    // フローゲージをリセット
    // ==================================================

    public void ResetFlowGauge()
    {
        if (isFlowMode)
        {
            return;
        }


        flowGauge = 0f;
    }
}