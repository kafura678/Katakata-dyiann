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

    [Header("敵スロー")]
    public float enemySlowRate = 0.3f;

    [Header("超必殺ダメージ")]
    public float ultimateDamage = 999f;

    [Header("UI")]
    public Slider flowGaugeSlider;
    public Text flowText;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        HandleFlowInput();
        UpdateFlowMode();
        UpdateUI();
    }

    private void HandleFlowInput()
    {
        if (isFlowMode) return;

        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (shift && Input.GetKeyDown(KeyCode.A))
        {
            StartFlowMode();
        }
    }

    public void AddFlowGauge(float amount)
    {
        if (isFlowMode) return;

        flowGauge += amount;
        flowGauge = Mathf.Clamp(flowGauge, 0f, maxFlowGauge);
    }

    private void StartFlowMode()
    {
        if (flowGauge < maxFlowGauge) return;

        isFlowMode = true;
        flowTimer = flowDuration;
        attackCountInFlow = 0;

        Debug.Log("フローモード開始");
    }

    private void UpdateFlowMode()
    {
        if (!isFlowMode) return;

        flowTimer -= Time.deltaTime;

        if (flowTimer <= 0f)
        {
            EndFlowMode();
        }
    }

    public void CountFlowAttack()
    {
        if (!isFlowMode) return;

        attackCountInFlow++;

        Debug.Log("フロー中攻撃回数: " + attackCountInFlow);
    }

    private void EndFlowMode()
    {
        isFlowMode = false;
        flowGauge = 0f;

        Debug.Log("フローモード終了");

        if (attackCountInFlow >= requiredAttackCount)
        {
            ActivateUltimate();
        }
    }

    private void ActivateUltimate()
    {
        Debug.Log("超必殺発動");

        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy enemy in enemies)
        {
            enemy.TakeDamage(ultimateDamage);
        }
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
            flowGaugeSlider.maxValue = maxFlowGauge;
            flowGaugeSlider.value = flowGauge;
        }

        if (flowText != null)
        {
            if (isFlowMode)
            {
                flowText.text = "FLOW " + flowTimer.ToString("F1") + " / HIT " + attackCountInFlow;
            }
            else
            {
                flowText.text = "FLOW " + flowGauge.ToString("F0") + "%";
            }
        }
    }
}