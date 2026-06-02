using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Gauge Settings")]
    public float shotGauge = 0f;
    public float maxShotGauge = 100f;
    public float gaugeGainPerKey = 5f;
    public float gaugeDrainPerSecond = 15f;

    [Header("Flow Settings")]
    public float flowGauge = 0f;
    public float maxFlowGauge = 100f;
    public float flowGainPerEnemy = 20f;
    public float flowDrainPerSecond = 20f;

    [Header("State")]
    public bool isShooting = false;
    public bool isFlowMode = false;

    [Header("UI")]
    public Text textShotGauge;
    public Text textFlowGauge;
    public Text textStatus;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        UpdateShootingGauge();
        UpdateFlowMode();
        UpdateUI();
    }

    private void UpdateShootingGauge()
    {
        if (isShooting)
        {
            shotGauge -= gaugeDrainPerSecond * Time.deltaTime;

            if (shotGauge <= 0f)
            {
                shotGauge = 0f;
                isShooting = false;
            }
        }
    }

    private void UpdateFlowMode()
    {
        if (isFlowMode)
        {
            flowGauge -= flowDrainPerSecond * Time.deltaTime;

            if (flowGauge <= 0f)
            {
                flowGauge = 0f;
                isFlowMode = false;
            }
        }
    }

    public void AddShotGauge()
    {
        if (isShooting) return;

        shotGauge += gaugeGainPerKey;
        shotGauge = Mathf.Clamp(shotGauge, 0f, maxShotGauge);
    }

    public void StartShooting()
    {
        if (shotGauge > 0f)
        {
            isShooting = true;
        }
    }

    public void AddFlowGauge()
    {
        if (isFlowMode) return;

        flowGauge += flowGainPerEnemy;
        flowGauge = Mathf.Clamp(flowGauge, 0f, maxFlowGauge);
    }

    public void StartFlowMode()
    {
        if (flowGauge >= maxFlowGauge)
        {
            isFlowMode = true;
        }
    }

    private void UpdateUI()
    {
        if (textShotGauge != null)
            textShotGauge.text = "射撃ゲージ：" + shotGauge.ToString("F0") + "%";

        if (textFlowGauge != null)
            textFlowGauge.text = "フローゲージ：" + flowGauge.ToString("F0") + "%";

        if (textStatus != null)
        {
            if (isFlowMode)
                textStatus.text = "フローモード";
            else if (isShooting)
                textStatus.text = "射撃中";
            else
                textStatus.text = "チャージ中";
        }
    }
}