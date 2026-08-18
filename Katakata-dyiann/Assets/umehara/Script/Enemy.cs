using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // ==================================================
    // HP
    // ==================================================

    [Header("HP")]

    [Tooltip("最大HP")]
    [SerializeField]
    private float maxHp = 100f;

    [Tooltip("現在HP")]
    [SerializeField]
    private float currentHp;


    // ==================================================
    // ダメージ表示
    // ==================================================

    [Header("ダメージ表示")]

    [Tooltip("DamagePopupが付いたPrefab")]
    [SerializeField]
    private GameObject damagePopupPrefab;


    // ==================================================
    // 通常の爆発
    // ==================================================

    [Header("爆発エフェクト")]

    [Tooltip("最後に生成する爆発Prefab")]
    [SerializeField]
    private GameObject explosionEffectPrefab;

    [Tooltip("最後の爆発位置の補正")]
    [SerializeField]
    private Vector3 explosionOffset = Vector3.zero;


    // ==================================================
    // 撃破演出
    // ==================================================

    [Header("撃破演出")]

    [Tooltip("敵が消滅するまでの演出時間")]
    [SerializeField]
    private float deathEffectDuration = 2f;

    [Tooltip("撃破中の震え幅")]
    [SerializeField]
    private float deathShakeAmount = 0.08f;


    // ==================================================
    // 撃破中の連続爆発
    // ==================================================

    [Header("撃破中の連続爆発")]

    [Tooltip("周囲で生成する爆発Prefab。未設定ならExplosion Effect Prefabを使用")]
    [SerializeField]
    private GameObject randomExplosionEffectPrefab;

    [Tooltip("爆発を生成する間隔")]
    [SerializeField]
    private float randomExplosionInterval = 0.15f;

    [Tooltip("敵の中心から爆発を発生させる範囲")]
    [SerializeField]
    private float randomExplosionRadius = 1.2f;


    // ==================================================
    // 状態
    // ==================================================

    private bool isDead = false;


    // ==================================================
    // 外部参照
    // ==================================================

    public float MaxHp => maxHp;

    public float CurrentHp => currentHp;

    public bool IsDead => isDead;


    // ==================================================
    // Unity
    // ==================================================

    private void Awake()
    {
        currentHp =
            maxHp;
    }


    // ==================================================
    // ダメージ
    // ==================================================

    public void TakeDamage(
        float damage
    )
    {
        // ==========================================
        // 死亡演出中
        // ==========================================

        if (isDead)
        {
            return;
        }


        // ==========================================
        // 0以下のダメージは無視
        // ==========================================

        if (damage <= 0f)
        {
            return;
        }


        // ==========================================
        // ダメージ表示
        // ==========================================

        SpawnDamagePopup(
            damage
        );


        // ==========================================
        // HP減少
        // ==========================================

        currentHp -=
            damage;


        currentHp =
            Mathf.Max(
                currentHp,
                0f
            );


        // ==========================================
        // 撃破
        // ==========================================

        if (currentHp <= 0f)
        {
            StartCoroutine(
                DeathRoutine()
            );
        }
    }


    // ==================================================
    // ダメージポップアップ
    // ==================================================

    private void SpawnDamagePopup(
        float damage
    )
    {
        if (damagePopupPrefab == null)
        {
            return;
        }


        // ==========================================
        // 敵の現在位置に生成
        // ==========================================

        GameObject popup =
            Instantiate(
                damagePopupPrefab,
                transform.position,
                Quaternion.identity
            );


        DamagePopup damagePopup =
            popup.GetComponent<DamagePopup>();


        if (damagePopup != null)
        {
            damagePopup.Setup(
                damage
            );
        }
    }


    // ==================================================
    // 撃破演出
    // ==================================================

    private IEnumerator DeathRoutine()
    {
        // ==========================================
        // 二重死亡防止
        // ==========================================

        if (isDead)
        {
            yield break;
        }


        isDead =
            true;


        // ==========================================
        // HPを確実に0にする
        // ==========================================

        currentHp =
            0f;


        // ==========================================
        // 当たり判定OFF
        // ==========================================

        DisableColliders();


        // ==========================================
        // SpriteRenderer取得
        // ==========================================

        SpriteRenderer[] spriteRenderers =
            GetComponentsInChildren<SpriteRenderer>(
                true
            );


        // ==========================================
        // 元の色を保存
        //
        // 複数SpriteRendererでも
        // 元々の色を維持したまま透明化
        // ==========================================

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
        // 基準位置
        // ==========================================

        Vector3 originalPosition =
            transform.position;


        float timer =
            0f;


        float explosionTimer =
            0f;


        // ==========================================
        // 演出時間
        // ==========================================

        float duration =
            Mathf.Max(
                0.01f,
                deathEffectDuration
            );


        // ==========================================
        // 撃破演出
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
            // 徐々に透明になる
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

            float safeExplosionInterval =
                Mathf.Max(
                    0.01f,
                    randomExplosionInterval
                );


            while (
                explosionTimer >=
                safeExplosionInterval
            )
            {
                explosionTimer -=
                    safeExplosionInterval;


                SpawnRandomExplosion(
                    originalPosition
                );
            }


            yield return null;
        }


        // ==========================================
        // 最終位置を戻す
        // ==========================================

        transform.position =
            originalPosition;


        // ==========================================
        // 完全に透明にする
        // ==========================================

        foreach (
            SpriteRenderer renderer
            in spriteRenderers
        )
        {
            if (renderer == null)
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
        // 最後の爆発
        // ==========================================

        SpawnFinalExplosion(
            originalPosition
        );


        // ==========================================
        // GAME CLEAR
        //
        // GameManager側で指定秒数待ってから
        // Resultシーンへ移動
        // ==========================================

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameClear();
        }


        // ==========================================
        // 敵削除
        // ==========================================

        Destroy(
            gameObject
        );
    }


    // ==================================================
    // Collider無効化
    // ==================================================

    private void DisableColliders()
    {
        Collider2D[] colliders =
            GetComponentsInChildren<Collider2D>(
                true
            );


        foreach (Collider2D col in colliders)
        {
            if (col == null)
            {
                continue;
            }


            col.enabled =
                false;
        }
    }


    // ==================================================
    // 周囲のランダム爆発
    // ==================================================

    private void SpawnRandomExplosion(
        Vector3 centerPosition
    )
    {
        // ==========================================
        // 専用Prefabがあればそちらを使用
        //
        // 無ければ通常の爆発Prefab
        // ==========================================

        GameObject explosionPrefab =
            randomExplosionEffectPrefab != null
                ? randomExplosionEffectPrefab
                : explosionEffectPrefab;


        if (explosionPrefab == null)
        {
            return;
        }


        // ==========================================
        // ランダム位置
        // ==========================================

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


        // ==========================================
        // 爆発生成
        // ==========================================

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
}