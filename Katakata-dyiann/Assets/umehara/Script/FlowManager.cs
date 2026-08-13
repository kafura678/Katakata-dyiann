using UnityEngine;
using UnityEngine.UI;

public class FlowManager : MonoBehaviour
{
    public static FlowManager Instance;

    [Header("フローゲージ")]
    public float flowGauge = 0f;
    public float maxFlowGauge = 100f;
    public float flowGainPerHit = 10f;

    [Header("フローモード")]
    public bool isFlowMode = false;
    public float flowDuration = 5f;

    private float flowTimer = 0f;

    [Header("超必殺")]
    public int requiredAttackCount = 8;

    private int attackCountInFlow = 0;

    public bool isUltimateReady = false;
    public bool isUltimatePlaying = false;

    [Header("敵スロー")]
    public float enemySlowRate = 0.3f;

    [Header("UI")]
    public Slider flowGaugeSlider;
    public Text flowText;

    private PlayerKeyMover player;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        player = FindFirstObjectByType<PlayerKeyMover>();
    }

    private void Update()
    {
        HandleFlowInput();
        UpdateFlowMode();
        UpdateUI();
    }

    private void HandleFlowInput()
    {
        // =========================================
        // 超必殺発動
        // =========================================

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

        // =========================================
        // フローモード開始
        // =========================================

        if (isFlowMode)
        {
            return;
        }

        bool shift =
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift);

        if (
            shift &&
            Input.GetKeyDown(KeyCode.A)
        )
        {
            StartFlowMode();
        }
    }

    public void AddFlowGauge(float amount)
    {
        if (isFlowMode)
        {
            return;
        }

        flowGauge += amount;

        flowGauge = Mathf.Clamp(
            flowGauge,
            0f,
            maxFlowGauge
        );
    }

    private void StartFlowMode()
    {
        if (flowGauge < maxFlowGauge)
        {
            return;
        }

        isFlowMode = true;

        flowTimer = flowDuration;

        attackCountInFlow = 0;

        isUltimateReady = false;
        isUltimatePlaying = false;

        if (player != null)
        {
            player.StartFlowRouteRecording();
        }

        Debug.Log("フローモード開始");
    }

    private void UpdateFlowMode()
    {
        if (!isFlowMode)
        {
            return;
        }

        // 超必殺中は時間を減らさない
        if (isUltimatePlaying)
        {
            return;
        }

        flowTimer -= Time.deltaTime;

        if (flowTimer <= 0f)
        {
            flowTimer = 0f;

            EndFlowMode();
        }
    }

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

        if (
            attackCountInFlow >= requiredAttackCount &&
            !isUltimateReady
        )
        {
            isUltimateReady = true;

            Debug.Log("超必殺 READY");
        }
    }

    private void StartUltimate()
    {
        if (player == null)
        {
            return;
        }

        isUltimatePlaying = true;

        Debug.Log("超必殺発動");

        player.StartUltimateReplay();
    }

    public void UltimateFinished()
    {
        isUltimatePlaying = false;

        EndFlowMode();
    }

    private void EndFlowMode()
    {
        isFlowMode = false;

        flowGauge = 0f;
        flowTimer = 0f;

        attackCountInFlow = 0;

        isUltimateReady = false;
        isUltimatePlaying = false;

        if (player != null)
        {
            player.ClearFlowRoute();
        }

        Debug.Log("フローモード終了");
    }

    public float GetEnemySpeedRate()
    {
        if (isFlowMode)
        {
            return enemySlowRate;
        }

        return 1f;
    }

    public bool CanEnemyShoot()
    {
        return !isFlowMode;
    }

    private void UpdateUI()
    {
        if (flowGaugeSlider != null)
        {
            flowGaugeSlider.maxValue =
                maxFlowGauge;

            flowGaugeSlider.value =
                flowGauge;
        }

        if (flowText == null)
        {
            return;
        }

        if (isUltimatePlaying)
        {
            flowText.text =
                "ULTIMATE";
        }
        else if (isUltimateReady)
        {
            flowText.text =
                "FINISH READY - ENTER";
        }
        else if (isFlowMode)
        {
            flowText.text =
                "FLOW " +
                flowTimer.ToString("F1") +
                "  HIT " +
                attackCountInFlow +
                "/" +
                requiredAttackCount;
        }
        else
        {
            flowText.text =
                "FLOW " +
                flowGauge.ToString("F0") +
                "%";
        }
    }
}