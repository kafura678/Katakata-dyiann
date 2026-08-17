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

    [Tooltip("現在のハート数")]
    [SerializeField]
    private int currentHearts;


    // ==================================================
    // 被弾後無敵
    // ==================================================

    [Header("被弾後無敵")]

    [Tooltip("ダメージを受けた後の無敵時間")]
    [SerializeField]
    private float invincibleTime = 1f;

    [Tooltip("無敵中の点滅間隔")]
    [SerializeField]
    private float blinkInterval = 0.1f;


    // ==================================================
    // プレイヤー表示
    // ==================================================

    [Header("プレイヤー表示")]

    [Tooltip("点滅させるSpriteRenderer")]
    [SerializeField]
    private SpriteRenderer playerRenderer;


    // ==================================================
    // シールド
    // ==================================================

    [Header("シールド")]

    [Tooltip("シールド最大耐久")]
    [SerializeField]
    private int maxShieldHealth = 3;

    [Tooltip("現在のシールド耐久")]
    [SerializeField]
    private int shieldHealth = 0;


    // ==================================================
    // シールド表示
    // ==================================================

    [Header("シールド表示")]

    [Tooltip("シールド表示1")]
    [SerializeField]
    private SpriteRenderer shieldRenderer1;

    [Tooltip("シールド表示2")]
    [SerializeField]
    private SpriteRenderer shieldRenderer2;


    [Header("シールド色")]

    [Tooltip("シールド3の時")]
    [SerializeField]
    private Color shieldColor3 = Color.blue;

    [Tooltip("シールド2の時")]
    [SerializeField]
    private Color shieldColor2 = Color.yellow;

    [Tooltip("シールド1の時")]
    [SerializeField]
    private Color shieldColor1 = Color.red;

    [Tooltip("シールドの透明度")]
    [Range(0f, 1f)]
    [SerializeField]
    private float shieldAlpha = 0.5f;


    // ==================================================
    // Collider
    // ==================================================

    [Header("プレイヤー当たり判定")]

    [Tooltip("死亡時に無効化するCollider")]
    [SerializeField]
    private Collider2D[] playerColliders;


    // ==================================================
    // 爆発
    // ==================================================

    [Header("最終爆発")]

    [Tooltip("死亡演出の最後に生成する爆発Prefab")]
    [SerializeField]
    private GameObject explosionEffectPrefab;

    [Tooltip("最終爆発位置の補正")]
    [SerializeField]
    private Vector3 explosionOffset = Vector3.zero;


    // ==================================================
    // 死亡演出
    // ==================================================

    [Header("死亡演出")]

    [Tooltip("死亡演出の長さ")]
    [SerializeField]
    private float deathEffectDuration = 2f;

    [Tooltip("死亡中の震え幅")]
    [SerializeField]
    private float deathShakeAmount = 0.08f;


    // ==================================================
    // 死亡中の連続爆発
    // ==================================================

    [Header("死亡時の連続爆発")]

    [Tooltip(
        "死亡中に周囲で生成する爆発Prefab。" +
        "未設定ならExplosion Effect Prefabを使用"
    )]
    [SerializeField]
    private GameObject randomExplosionEffectPrefab;

    [Tooltip("ランダム爆発の間隔")]
    [SerializeField]
    private float randomExplosionInterval = 0.15f;

    [Tooltip("ランダム爆発が出る範囲")]
    [SerializeField]
    private float randomExplosionRadius = 1.2f;


    // ==================================================
    // SE
    // ==================================================

    [Header("SE共通")]

    [SerializeField]
    private AudioSource audioSource;


    [Header("ダメージSE")]

    [SerializeField]
    private AudioClip damageSE;

    [Range(0f, 1f)]
    [SerializeField]
    private float damageSEVolume = 1f;


    [Header("回復SE")]

    [SerializeField]
    private AudioClip healSE;

    [Range(0f, 1f)]
    [SerializeField]
    private float healSEVolume = 1f;


    [Header("シールド取得SE")]

    [SerializeField]
    private AudioClip shieldGetSE;

    [Range(0f, 1f)]
    [SerializeField]
    private float shieldGetSEVolume = 1f;


    // ==================================================
    // 他スクリプト
    // ==================================================

    private PlayerKeyMover playerKeyMover;


    // ==================================================
    // 状態
    // ==================================================

    private bool isInvincible = false;

    private bool isDead = false;


    // ==================================================
    // 外部参照
    // ==================================================

    public int CurrentHearts => currentHearts;

    public int MaxHearts => maxHearts;

    public int ShieldHealth => shieldHealth;

    public bool IsInvincible => isInvincible;

    public bool IsDead => isDead;


    // ==================================================
    // Unity
    // ==================================================

    private void Awake()
    {
        // ==========================================
        // HP初期化
        // ==========================================

        currentHearts =
            maxHearts;


        // ==========================================
        // Renderer
        // ==========================================

        if (playerRenderer == null)
        {
            playerRenderer =
                GetComponentInChildren<SpriteRenderer>();
        }


        // ==========================================
        // Collider
        // ==========================================

        if (
            playerColliders == null ||
            playerColliders.Length == 0
        )
        {
            playerColliders =
                GetComponentsInChildren<Collider2D>();
        }


        // ==========================================
        // AudioSource
        // ==========================================

        if (audioSource == null)
        {
            audioSource =
                GetComponent<AudioSource>();
        }


        // ==========================================
        // PlayerKeyMover
        // ==========================================

        playerKeyMover =
            GetComponent<PlayerKeyMover>();
    }


    private void Start()
    {
        // ==========================================
        // HP UI初期化
        // ==========================================

        UpdateHeartUI();


        // ==========================================
        // シールド表示初期化
        // ==========================================

        UpdateShieldVisual();
    }


    // ==================================================
    // ダメージ
    // ==================================================

    public void TakeDamage(
        int damage
    )
    {
        // ==========================================
        // 死亡済み
        // ==========================================

        if (isDead)
        {
            return;
        }


        // ==========================================
        // 移動中無敵
        // ==========================================

        if (
            playerKeyMover != null &&
            playerKeyMover.isMoving
        )
        {
            return;
        }


        // ==========================================
        // 被弾後無敵
        // ==========================================

        if (isInvincible)
        {
            return;
        }


        // ==========================================
        // 無効ダメージ
        // ==========================================

        if (damage <= 0)
        {
            return;
        }


        // ==========================================
        // シールド
        //
        // 1回の攻撃を完全に防ぎ
        // シールドを1消費
        // ==========================================

        if (shieldHealth > 0)
        {
            shieldHealth--;


            UpdateShieldVisual();


            // ======================================
            // シールドで防いだ場合も
            // カメラシェイク
            // ======================================

            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake();
            }


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
            CameraShake.Instance.Shake();
        }


        // ==========================================
        // コンボリセット
        // ==========================================

        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.ResetCombo();
        }


        // ==========================================
        // HP UI
        // ==========================================

        UpdateHeartUI();


        // ==========================================
        // 死亡
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
    // 回復
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


        // ==========================================
        // すでに最大HP
        // ==========================================

        if (currentHearts >= maxHearts)
        {
            return;
        }


        int oldHearts =
            currentHearts;


        currentHearts +=
            amount;


        currentHearts =
            Mathf.Clamp(
                currentHearts,
                0,
                maxHearts
            );


        // ==========================================
        // 実際に回復した場合のみSE
        // ==========================================

        if (currentHearts > oldHearts)
        {
            PlayHealSE();
        }


        UpdateHeartUI();
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


        if (currentHearts >= maxHearts)
        {
            return;
        }


        int oldHearts =
            currentHearts;


        currentHearts =
            maxHearts;


        if (currentHearts > oldHearts)
        {
            PlayHealSE();
        }


        UpdateHeartUI();
    }


    // ==================================================
    // シールド取得
    // ==================================================

    public void AddShield()
    {
        if (isDead)
        {
            return;
        }


        // ==========================================
        // シールドを最大まで回復
        // ==========================================

        shieldHealth =
            maxShieldHealth;


        // ==========================================
        // SE
        // ==========================================

        PlayShieldGetSE();


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
        // ==========================================
        // シールドなし
        // ==========================================

        if (shieldHealth <= 0)
        {
            SetShieldRenderer(
                shieldRenderer1,
                false,
                Color.white
            );


            SetShieldRenderer(
                shieldRenderer2,
                false,
                Color.white
            );


            return;
        }


        // ==========================================
        // シールド色
        // ==========================================

        Color shieldColor;


        if (shieldHealth >= 3)
        {
            shieldColor =
                shieldColor3;
        }
        else if (shieldHealth == 2)
        {
            shieldColor =
                shieldColor2;
        }
        else
        {
            shieldColor =
                shieldColor1;
        }


        // ==========================================
        // 2枚とも表示
        // ==========================================

        SetShieldRenderer(
            shieldRenderer1,
            true,
            shieldColor
        );


        SetShieldRenderer(
            shieldRenderer2,
            true,
            shieldColor
        );
    }


    // ==================================================
    // シールドRenderer
    // ==================================================

    private void SetShieldRenderer(
        SpriteRenderer renderer,
        bool active,
        Color color
    )
    {
        if (renderer == null)
        {
            return;
        }


        renderer.enabled =
            active;


        if (!active)
        {
            return;
        }


        color.a =
            shieldAlpha;


        renderer.color =
            color;
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


        float blinkTimer =
            0f;


        bool visible =
            true;


        float safeBlinkInterval =
            Mathf.Max(
                0.01f,
                blinkInterval
            );


        while (timer < invincibleTime)
        {
            float deltaTime =
                Time.deltaTime;


            timer +=
                deltaTime;


            blinkTimer +=
                deltaTime;


            if (
                blinkTimer >=
                safeBlinkInterval
            )
            {
                blinkTimer =
                    0f;


                visible =
                    !visible;


                if (playerRenderer != null)
                {
                    playerRenderer.enabled =
                        visible;
                }
            }


            yield return null;
        }


        // ==========================================
        // 必ず表示状態に戻す
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
    // 死亡開始
    // ==================================================

    private void Die()
    {
        // ==========================================
        // 二重実行防止
        // ==========================================

        if (isDead)
        {
            return;
        }


        isDead =
            true;


        currentHearts =
            0;


        UpdateHeartUI();


        // ==========================================
        // 既存の無敵点滅などを停止
        //
        // DeathRoutineを開始する前に行う
        // ==========================================

        StopAllCoroutines();


        // ==========================================
        // プレイヤー入力禁止
        // ==========================================

        if (playerKeyMover != null)
        {
            playerKeyMover.SetInputEnabled(
                false
            );
        }


        // ==========================================
        // シールド削除
        // ==========================================

        shieldHealth =
            0;


        UpdateShieldVisual();


        // ==========================================
        // 当たり判定OFF
        // ==========================================

        SetPlayerColliders(
            false
        );


        // ==========================================
        // 死亡演出開始
        // ==========================================

        StartCoroutine(
            DeathRoutine()
        );
    }


    // ==================================================
    // 死亡演出
    // ==================================================

    private IEnumerator DeathRoutine()
    {
        // ==========================================
        // SpriteRenderer取得
        //
        // シールドはすでに非表示なので
        // Player本体・子Spriteを透明化
        // ==========================================

        SpriteRenderer[] spriteRenderers =
            GetComponentsInChildren<SpriteRenderer>(
                true
            );


        Color[] originalColors =
            new Color[spriteRenderers.Length];


        for (
            int i = 0;
            i < spriteRenderers.Length;
            i++
        )
        {
            if (spriteRenderers[i] == null)
            {
                continue;
            }


            originalColors[i] =
                spriteRenderers[i].color;
        }


        // ==========================================
        // 点滅でRendererがOFFの可能性があるため
        // 本体を表示状態へ戻す
        // ==========================================

        if (playerRenderer != null)
        {
            playerRenderer.enabled =
                true;
        }


        // ==========================================
        // 元の位置
        // ==========================================

        Vector3 originalPosition =
            transform.position;


        float timer =
            0f;


        float explosionTimer =
            0f;


        float duration =
            Mathf.Max(
                0.01f,
                deathEffectDuration
            );


        float safeExplosionInterval =
            Mathf.Max(
                0.01f,
                randomExplosionInterval
            );


        // ==========================================
        // 死亡演出
        // ==========================================

        while (timer < duration)
        {
            float deltaTime =
                Time.deltaTime;


            timer +=
                deltaTime;


            explosionTimer +=
                deltaTime;


            float progress =
                Mathf.Clamp01(
                    timer /
                    duration
                );


            // ======================================
            // 小刻みに震える
            // ======================================

            Vector2 shakeOffset =
                Random.insideUnitCircle *
                deathShakeAmount;


            transform.position =
                originalPosition +
                new Vector3(
                    shakeOffset.x,
                    shakeOffset.y,
                    0f
                );


            // ======================================
            // 徐々に透明化
            // ======================================

            float alphaRate =
                1f -
                progress;


            for (
                int i = 0;
                i < spriteRenderers.Length;
                i++
            )
            {
                SpriteRenderer renderer =
                    spriteRenderers[i];


                if (renderer == null)
                {
                    continue;
                }


                // シールドは死亡時非表示のまま
                if (
                    renderer == shieldRenderer1 ||
                    renderer == shieldRenderer2
                )
                {
                    continue;
                }


                Color color =
                    originalColors[i];


                color.a =
                    originalColors[i].a *
                    alphaRate;


                renderer.color =
                    color;
            }


            // ======================================
            // 周囲でランダム爆発
            // ======================================

            while (
                explosionTimer >=
                safeExplosionInterval
            )
            {
                explosionTimer -=
                    safeExplosionInterval;


                SpawnRandomDeathExplosion(
                    originalPosition
                );
            }


            yield return null;
        }


        // ==========================================
        // 震えを戻す
        // ==========================================

        transform.position =
            originalPosition;


        // ==========================================
        // 完全透明
        // ==========================================

        for (
            int i = 0;
            i < spriteRenderers.Length;
            i++
        )
        {
            SpriteRenderer renderer =
                spriteRenderers[i];


            if (renderer == null)
            {
                continue;
            }


            if (
                renderer == shieldRenderer1 ||
                renderer == shieldRenderer2
            )
            {
                continue;
            }


            Color color =
                renderer.color;


            color.a =
                0f;


            renderer.color =
                color;
        }


        // ==========================================
        // 最後の大爆発
        // ==========================================

        SpawnFinalExplosion(
            originalPosition
        );


        // ==========================================
        // GAME OVER
        //
        // GameManager側で指定秒数後にResultへ
        // ==========================================

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }


    // ==================================================
    // ランダム爆発
    // ==================================================

    private void SpawnRandomDeathExplosion(
        Vector3 centerPosition
    )
    {
        GameObject explosionPrefab =
            randomExplosionEffectPrefab != null
                ? randomExplosionEffectPrefab
                : explosionEffectPrefab;


        if (explosionPrefab == null)
        {
            return;
        }


        Vector2 randomOffset =
            Random.insideUnitCircle *
            randomExplosionRadius;


        Vector3 spawnPosition =
            centerPosition +
            new Vector3(
                randomOffset.x,
                randomOffset.y,
                0f
            );


        Instantiate(
            explosionPrefab,
            spawnPosition,
            Quaternion.identity
        );
    }


    // ==================================================
    // 最後の爆発
    // ==================================================

    private void SpawnFinalExplosion(
        Vector3 centerPosition
    )
    {
        if (explosionEffectPrefab == null)
        {
            return;
        }


        Instantiate(
            explosionEffectPrefab,
            centerPosition +
            explosionOffset,
            Quaternion.identity
        );
    }


    // ==================================================
    // Collider
    // ==================================================

    private void SetPlayerColliders(
        bool enabled
    )
    {
        if (playerColliders == null)
        {
            return;
        }


        foreach (Collider2D col in playerColliders)
        {
            if (col == null)
            {
                continue;
            }


            col.enabled =
                enabled;
        }
    }


    // ==================================================
    // Heart UI
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
    // SE
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


    private void PlayHealSE()
    {
        if (
            audioSource == null ||
            healSE == null
        )
        {
            return;
        }


        audioSource.PlayOneShot(
            healSE,
            healSEVolume
        );
    }


    private void PlayShieldGetSE()
    {
        if (
            audioSource == null ||
            shieldGetSE == null
        )
        {
            return;
        }


        audioSource.PlayOneShot(
            shieldGetSE,
            shieldGetSEVolume
        );
    }


    // ==================================================
    // 敵弾
    // ==================================================

    private void OnTriggerEnter2D(
        Collider2D collision
    )
    {
        if (isDead)
        {
            return;
        }


        if (collision.CompareTag("EnemyBullet"))
        {
            TakeDamage(
                1
            );
        }
    }
}