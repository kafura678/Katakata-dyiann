using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    // ==================================================
    // HP
    // ==================================================

    [Header("HP")]

    [Tooltip("最大ハート数")]
    [SerializeField]
    private int maxHearts = 3;

    private int currentHearts;


    // ==================================================
    // 被弾後の無敵
    // ==================================================

    [Header("被弾後の無敵")]

    [Tooltip("被弾後の無敵時間")]
    [SerializeField]
    private float invincibleTime = 1f;

    [Tooltip("点滅する間隔")]
    [SerializeField]
    private float blinkInterval = 0.1f;

    [Tooltip("点滅させるSpriteRenderer")]
    [SerializeField]
    private SpriteRenderer playerRenderer;

    private bool isInvincible = false;


    // ==================================================
    // SHIELD
    // ==================================================

    [Header("シールド")]

    [Tooltip("シールドの最大耐久")]
    [SerializeField]
    private int maxShieldHealth = 3;

    private int shieldHealth = 0;


    // ==================================================
    // シールド画像
    // ==================================================

    [Header("シールド画像")]

    [Tooltip("シールド画像1")]
    [SerializeField]
    private SpriteRenderer shieldRenderer1;

    [Tooltip("シールド画像2")]
    [SerializeField]
    private SpriteRenderer shieldRenderer2;


    // ==================================================
    // シールド色
    // ==================================================

    [Header("シールド色")]

    [Tooltip("耐久3の色")]
    [SerializeField]
    private Color shieldBlue = Color.blue;

    [Tooltip("耐久2の色")]
    [SerializeField]
    private Color shieldYellow = Color.yellow;

    [Tooltip("耐久1の色")]
    [SerializeField]
    private Color shieldRed = Color.red;


    // ==================================================
    // シールド透明度
    // ==================================================

    [Header("シールド透明度")]

    [Tooltip("シールド画像1の透明度")]
    [SerializeField, Range(0f, 1f)]
    private float shieldAlpha1 = 1f;

    [Tooltip("シールド画像2の透明度")]
    [SerializeField, Range(0f, 1f)]
    private float shieldAlpha2 = 0.5f;


    // ==================================================
    // ダメージSE
    // ==================================================

    [Header("ダメージSE")]

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip damageSE;

    [SerializeField, Range(0f, 1f)]
    private float damageSEVolume = 1f;


    // ==================================================
    // 状態
    // ==================================================

    private bool isDead = false;


    // ==================================================
    // 他スクリプト
    // ==================================================

    private PlayerKeyMover playerKeyMover;


    // ==================================================
    // プロパティ
    // ==================================================

    public int MaxHearts
    {
        get
        {
            return maxHearts;
        }
    }


    public int CurrentHearts
    {
        get
        {
            return currentHearts;
        }
    }


    public bool IsDead
    {
        get
        {
            return isDead;
        }
    }


    public bool IsInvincible
    {
        get
        {
            return isInvincible;
        }
    }


    public bool HasShield
    {
        get
        {
            return shieldHealth > 0;
        }
    }


    public int ShieldHealth
    {
        get
        {
            return shieldHealth;
        }
    }


    // ==================================================
    // Unity
    // ==================================================

    private void Awake()
    {
        currentHearts =
            maxHearts;


        shieldHealth =
            0;


        playerKeyMover =
            GetComponent<PlayerKeyMover>();


        if (playerRenderer == null)
        {
            playerRenderer =
                GetComponent<SpriteRenderer>();
        }


        UpdateShieldVisual();
    }


    private void Start()
    {
        UpdateHeartUI();
    }


    // ==================================================
    // ダメージ
    // ==================================================

    public void TakeDamage(
        int damage
    )
    {
        // ==========================================
        // 死亡中
        // ==========================================

        if (isDead)
        {
            return;
        }


        // ==========================================
        // 移動中
        // ==========================================

        if (
            playerKeyMover != null &&
            playerKeyMover.isMoving
        )
        {
            return;
        }


        // ==========================================
        // 無敵中
        // ==========================================

        if (isInvincible)
        {
            return;
        }


        // ==========================================
        // 無効なダメージ
        // ==========================================

        if (damage <= 0)
        {
            return;
        }


        // ==========================================
        // SHIELD
        // ==========================================

        if (HasShield)
        {
            UseShield();

            return;
        }


        // ==========================================
        // HP減少
        // ==========================================

        currentHearts -=
            damage;


        currentHearts =
            Mathf.Clamp(
                currentHearts,
                0,
                maxHearts
            );


        // ==========================================
        // ダメージSE
        // ==========================================

        PlayDamageSE();


        // ==========================================
        // カメラシェイク
        // ==========================================

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance
                .Shake();
        }


        // ==========================================
        // コンボリセット
        // ==========================================

        if (ComboManager.Instance != null)
        {
            ComboManager.Instance
                .ResetCombo();
        }


        // ==========================================
        // HP UI更新
        // ==========================================

        UpdateHeartUI();


        Debug.Log(
            "プレイヤー被弾：" +
            currentHearts +
            " / " +
            maxHearts
        );


        // ==========================================
        // 死亡判定
        // ==========================================

        if (currentHearts <= 0)
        {
            Die();

            return;
        }


        // ==========================================
        // 被弾後無敵
        // ==========================================

        StartCoroutine(
            InvincibleRoutine()
        );
    }


    // ==================================================
    // HEAL
    // PlayerKeyMoverから呼ばれる
    // ==================================================

    public void Heal(
        int amount
    )
    {
        if (isDead)
        {
            return;
        }


        if (amount <= 0)
        {
            return;
        }


        if (currentHearts >= maxHearts)
        {
            return;
        }


        currentHearts +=
            amount;


        currentHearts =
            Mathf.Clamp(
                currentHearts,
                0,
                maxHearts
            );


        UpdateHeartUI();


        Debug.Log(
            "HEAL：" +
            currentHearts +
            " / " +
            maxHearts
        );
    }


    // ==================================================
    // 全回復
    // ==================================================

    public void FullHeal()
    {
        if (isDead)
        {
            return;
        }


        currentHearts =
            maxHearts;


        UpdateHeartUI();
    }


    // ==================================================
    // SHIELD獲得
    // PlayerKeyMoverから呼ばれる
    // ==================================================

    public void AddShield()
    {
        if (isDead)
        {
            return;
        }


        // シールド耐久を最大まで回復
        shieldHealth =
            maxShieldHealth;


        UpdateShieldVisual();


        Debug.Log(
            "SHIELD獲得：耐久 " +
            shieldHealth
        );
    }


    // ==================================================
    // SHIELD使用
    // ==================================================

    private void UseShield()
    {
        if (shieldHealth <= 0)
        {
            return;
        }


        // ==========================================
        // 1発分消費
        // ==========================================

        shieldHealth--;


        shieldHealth =
            Mathf.Clamp(
                shieldHealth,
                0,
                maxShieldHealth
            );


        Debug.Log(
            "SHIELDでダメージ無効：" +
            "残り耐久 " +
            shieldHealth
        );


        // ==========================================
        // 表示更新
        // ==========================================

        UpdateShieldVisual();
    }


    // ==================================================
    // シールド表示
    // ==================================================

    private void UpdateShieldVisual()
    {
        bool isActive =
            shieldHealth > 0;


        // ==========================================
        // 表示 / 非表示
        // ==========================================

        if (shieldRenderer1 != null)
        {
            shieldRenderer1.gameObject.SetActive(
                isActive
            );
        }


        if (shieldRenderer2 != null)
        {
            shieldRenderer2.gameObject.SetActive(
                isActive
            );
        }


        // シールドがなければここまで
        if (!isActive)
        {
            return;
        }


        // ==========================================
        // 耐久に応じた色
        // ==========================================

        Color currentColor;


        if (shieldHealth >= 3)
        {
            // --------------------------------------
            // 耐久3：青
            // --------------------------------------

            currentColor =
                shieldBlue;
        }
        else if (shieldHealth == 2)
        {
            // --------------------------------------
            // 耐久2：黄
            // --------------------------------------

            currentColor =
                shieldYellow;
        }
        else
        {
            // --------------------------------------
            // 耐久1：赤
            // --------------------------------------

            currentColor =
                shieldRed;
        }


        // ==========================================
        // シールド画像1
        // ==========================================

        if (shieldRenderer1 != null)
        {
            Color color1 =
                currentColor;


            color1.a =
                shieldAlpha1;


            shieldRenderer1.color =
                color1;
        }


        // ==========================================
        // シールド画像2
        // ==========================================

        if (shieldRenderer2 != null)
        {
            Color color2 =
                currentColor;


            color2.a =
                shieldAlpha2;


            shieldRenderer2.color =
                color2;
        }
    }


    // ==================================================
    // 被弾後無敵
    // ==================================================

    private IEnumerator InvincibleRoutine()
    {
        isInvincible =
            true;


        float timer =
            0f;


        while (timer < invincibleTime)
        {
            // ======================================
            // 非表示
            // ======================================

            if (playerRenderer != null)
            {
                playerRenderer.enabled =
                    false;
            }


            yield return new WaitForSeconds(
                blinkInterval
            );


            timer +=
                blinkInterval;


            // ======================================
            // 表示
            // ======================================

            if (playerRenderer != null)
            {
                playerRenderer.enabled =
                    true;
            }


            yield return new WaitForSeconds(
                blinkInterval
            );


            timer +=
                blinkInterval;
        }


        // ==========================================
        // 最後は必ず表示
        // ==========================================

        if (playerRenderer != null)
        {
            playerRenderer.enabled =
                true;
        }


        isInvincible =
            false;
    }


    // ==================================================
    // ダメージSE
    // ==================================================

    private void PlayDamageSE()
    {
        if (
            audioSource == null ||
            damageSE == null
        )
        {
            return;
        }


        audioSource.PlayOneShot(
            damageSE,
            damageSEVolume
        );
    }


    // ==================================================
    // HeartUI
    // ==================================================

    private void UpdateHeartUI()
    {
        if (HeartUI.Instance == null)
        {
            return;
        }


        HeartUI.Instance.UpdateHearts(
            currentHearts,
            maxHearts
        );
    }


    // ==================================================
    // 死亡
    // ==================================================

    private void Die()
    {
        if (isDead)
        {
            return;
        }


        isDead =
            true;


        isInvincible =
            false;


        // ==========================================
        // シールド解除
        // ==========================================

        shieldHealth =
            0;


        UpdateShieldVisual();


        // ==========================================
        // 点滅中なら表示を戻す
        // ==========================================

        if (playerRenderer != null)
        {
            playerRenderer.enabled =
                true;
        }


        Debug.Log(
            "プレイヤー死亡"
        );


        // ==========================================
        // GameOver
        // ==========================================

        if (GameManager.Instance != null)
        {
            GameManager.Instance
                .GameOver();
        }
    }


    // ==================================================
    // Bulletとの接触
    // ==================================================

    private void OnTriggerEnter2D(
        Collider2D collision
    )
    {
        if (collision == null)
        {
            return;
        }


        // Bullet以外
        if (!collision.CompareTag("Bullet"))
        {
            return;
        }


        // ==========================================
        // 移動中
        // ==========================================

        if (
            playerKeyMover != null &&
            playerKeyMover.isMoving
        )
        {
            return;
        }


        // ==========================================
        // ダメージ
        // ==========================================

        TakeDamage(
            1
        );


        // ==========================================
        // 弾を削除
        // ==========================================

        Destroy(
            collision.gameObject
        );
    }
}