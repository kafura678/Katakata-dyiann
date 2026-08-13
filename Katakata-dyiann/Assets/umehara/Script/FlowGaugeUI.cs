using UnityEngine;
using UnityEngine.UI;

public class FlowGaugeUI : MonoBehaviour
{
    [Header("ゲージ表示")]
    [SerializeField]
    private Image gaugeImage;

    [Header("ゲージ画像")]
    [SerializeField]
    private Sprite[] gaugeSprites;

    [Header("FlowManager")]
    [SerializeField]
    private FlowManager flowManager;

    [Header("ゲージアニメーション")]
    [Tooltip("ゲージ画像が1段階変化するまでの時間")]
    [SerializeField]
    private float changeInterval = 0.08f;

    private int displayedIndex = 0;
    private float changeTimer = 0f;

    private void Start()
    {
        InitializeGauge();
    }

    private void Update()
    {
        UpdateGauge();
    }

    private void InitializeGauge()
    {
        if (gaugeImage == null)
        {
            return;
        }

        if (gaugeSprites == null || gaugeSprites.Length == 0)
        {
            return;
        }

        displayedIndex = 0;
        gaugeImage.sprite = gaugeSprites[displayedIndex];
    }

    private void UpdateGauge()
    {
        if (gaugeImage == null)
        {
            return;
        }

        if (flowManager == null)
        {
            return;
        }

        if (gaugeSprites == null || gaugeSprites.Length == 0)
        {
            return;
        }

        int maxIndex = gaugeSprites.Length - 1;

        float rate =
            flowManager.flowGauge /
            flowManager.maxFlowGauge;

        rate = Mathf.Clamp01(rate);

        int targetIndex =
            Mathf.FloorToInt(
                rate * maxIndex
            );

        // ============================================
        // 100%時
        // ============================================

        if (flowManager.flowGauge >= flowManager.maxFlowGauge)
        {
            // MAXの1つ前までは通常速度で進める
            int beforeMaxIndex = maxIndex - 1;

            if (displayedIndex < beforeMaxIndex)
            {
                targetIndex = beforeMaxIndex;
            }
            else
            {
                // 最後の1段階だけMAXへ一気に切り替える
                displayedIndex = maxIndex;

                gaugeImage.sprite =
                    gaugeSprites[displayedIndex];

                changeTimer = 0f;

                return;
            }
        }

        // ============================================
        // 目標と同じ
        // ============================================

        if (displayedIndex == targetIndex)
        {
            changeTimer = 0f;
            return;
        }

        // ============================================
        // 一定時間ごとに1段階
        // ============================================

        changeTimer += Time.deltaTime;

        if (changeTimer < changeInterval)
        {
            return;
        }

        changeTimer = 0f;

        if (displayedIndex < targetIndex)
        {
            displayedIndex++;
        }
        else if (displayedIndex > targetIndex)
        {
            displayedIndex--;
        }

        displayedIndex =
            Mathf.Clamp(
                displayedIndex,
                0,
                maxIndex
            );

        gaugeImage.sprite =
            gaugeSprites[displayedIndex];
    }
}