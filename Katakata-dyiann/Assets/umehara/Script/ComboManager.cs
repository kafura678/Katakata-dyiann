using UnityEngine;
using UnityEngine.UI;

public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance;


    // ==================================================
    // コンボ
    // ==================================================

    [Header("Combo")]

    [Tooltip("現在のコンボ数")]
    public int comboCount = 0;

    [Tooltip("コンボが途切れるまでの時間")]
    public float comboResetTime = 2.0f;

    private float comboTimer = 0f;


    // ==================================================
    // ダメージボーナス
    // ==================================================

    [Header("Damage Bonus")]

    [Tooltip("1コンボごとに増加するダメージ倍率")]
    public float damageBonusPerCombo = 0.03f;

    [Tooltip("コンボによる最大ダメージ倍率")]
    public float maxDamageMultiplier = 2.0f;

    [Tooltip("1回の攻撃で与えられる最大ダメージ")]
    public float maxDamage = 100f;


    // ==================================================
    // スコア
    // ==================================================

    [Header("Score")]

    [Tooltip("1ヒットあたりの基本スコア")]
    public int baseScorePerHit = 100;


    // ==================================================
    // UI
    // ==================================================

    [Header("UI")]

    [Tooltip("コンボ数表示")]
    public Text comboText;

    [Tooltip("コンボ残り時間表示")]
    public Text comboTimerText;


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


    private void Update()
    {
        UpdateComboTimer();

        UpdateUI();
    }


    // ==================================================
    // コンボタイマー
    // ==================================================

    private void UpdateComboTimer()
    {
        if (comboCount <= 0)
        {
            return;
        }


        float timerSpeed =
            1f;


        // ==========================================
        // FLOW中はコンボタイマーの減少速度を半分
        // ==========================================

        if (
            FlowManager.Instance != null &&
            FlowManager.Instance.isFlowMode
        )
        {
            timerSpeed =
                0.5f;
        }


        comboTimer -=
            Time.deltaTime *
            timerSpeed;


        if (comboTimer <= 0f)
        {
            ResetCombo();
        }
    }


    // ==================================================
    // コンボ追加
    // ==================================================

    public void AddCombo()
    {
        comboCount++;


        // ==========================================
        // コンボタイマーリセット
        // ==========================================

        comboTimer =
            comboResetTime;


        // ==========================================
        // スコア計算
        // ==========================================

        int score =
            Mathf.RoundToInt(
                baseScorePerHit *
                GetScoreMultiplier()
            );


        // ==========================================
        // スコア追加
        // ==========================================

        if (GameManager.Instance != null)
        {
            GameManager.Instance
                .AddScore(
                    score
                );
        }


        Debug.Log(
            "Combo: " +
            comboCount +
            " / Damage x" +
            GetDamageMultiplier()
        );
    }


    // ==================================================
    // ダメージ倍率
    // ==================================================

    public float GetDamageMultiplier()
    {
        float multiplier =
            1f +
            comboCount *
            damageBonusPerCombo;


        return Mathf.Clamp(
            multiplier,
            1f,
            maxDamageMultiplier
        );
    }


    // ==================================================
    // 最大ダメージ取得
    // ==================================================

    public float GetMaxDamage()
    {
        return maxDamage;
    }


    // ==================================================
    // ダメージを最大値以内に制限
    // ==================================================

    public float ClampDamage(
        float damage
    )
    {
        return Mathf.Clamp(
            damage,
            0f,
            maxDamage
        );
    }


    // ==================================================
    // スコア倍率
    // ==================================================

    public float GetScoreMultiplier()
    {
        if (comboCount < 10)
        {
            return 1f;
        }


        if (comboCount < 20)
        {
            return 1.5f;
        }


        if (comboCount < 30)
        {
            return 2f;
        }


        return 3f;
    }


    // ==================================================
    // コンボリセット
    // ==================================================

    public void ResetCombo()
    {
        comboCount =
            0;


        comboTimer =
            0f;


        Debug.Log(
            "Combo Reset"
        );
    }


    // ==================================================
    // UI
    // ==================================================

    private void UpdateUI()
    {
        // ==========================================
        // コンボ数
        // ==========================================

        if (comboText != null)
        {
            if (comboCount <= 0)
            {
                comboText.text =
                    "";
            }
            else
            {
                comboText.text =
                    comboCount +
                    " COMBO";
            }
        }


        // ==========================================
        // コンボ残り時間
        // ==========================================

        if (comboTimerText != null)
        {
            if (comboCount <= 0)
            {
                comboTimerText.text =
                    "";
            }
            else
            {
                comboTimerText.text =
                    comboTimer.ToString(
                        "F1"
                    );
            }
        }
    }
}