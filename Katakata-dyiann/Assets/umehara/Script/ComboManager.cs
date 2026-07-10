using UnityEngine;
using UnityEngine.UI;

public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance;

    [Header("Combo")]
    public int comboCount = 0;
    public float comboResetTime = 2.0f;
    private float comboTimer = 0f;

    [Header("Damage Bonus")]
    public float damageBonusPerCombo = 0.03f;
    public float maxDamageMultiplier = 2.0f;

    [Header("Score")]
    public int baseScorePerHit = 100;

    [Header("UI")]
    public Text comboText;
    public Text comboTimerText;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        UpdateComboTimer();
        UpdateUI();
    }

    private void UpdateComboTimer()
    {
        if (comboCount <= 0) return;

        float timerSpeed = 1f;

        if (FlowManager.Instance != null && FlowManager.Instance.isFlowMode)
        {
            timerSpeed = 0.5f;
        }

        comboTimer -= Time.deltaTime * timerSpeed;

        if (comboTimer <= 0f)
        {
            ResetCombo();
        }
    }

    public void AddCombo()
    {
        comboCount++;
        comboTimer = comboResetTime;

        int score = Mathf.RoundToInt(baseScorePerHit * GetScoreMultiplier());

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(score);
        }

        Debug.Log("Combo: " + comboCount);
    }

    public float GetDamageMultiplier()
    {
        float multiplier = 1f + comboCount * damageBonusPerCombo;
        return Mathf.Clamp(multiplier, 1f, maxDamageMultiplier);
    }

    public float GetScoreMultiplier()
    {
        if (comboCount < 10) return 1f;
        if (comboCount < 20) return 1.5f;
        if (comboCount < 30) return 2f;

        return 3f;
    }

    public void ResetCombo()
    {
        comboCount = 0;
        comboTimer = 0f;

        Debug.Log("Combo Reset");
    }

    private void UpdateUI()
    {
        if (comboText != null)
        {
            if (comboCount <= 0)
            {
                comboText.text = "";
            }
            else
            {
                comboText.text = comboCount + " COMBO";
            }
        }

        if (comboTimerText != null)
        {
            if (comboCount <= 0)
            {
                comboTimerText.text = "";
            }
            else
            {
                comboTimerText.text = comboTimer.ToString("F1");
            }
        }
    }
}