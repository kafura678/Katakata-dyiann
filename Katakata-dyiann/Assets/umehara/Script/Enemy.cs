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

    private float currentHp;


    // ==================================================
    // ダメージ表示
    // ==================================================

    [Header("ダメージ表示")]

    [Tooltip("DamagePopupのPrefab")]
    [SerializeField]
    private GameObject damagePopupPrefab;


    // ==================================================
    // 死亡エフェクト
    // ==================================================

    [Header("死亡エフェクト")]

    [Tooltip("死亡時に生成する爆発Prefab")]
    [SerializeField]
    private GameObject explosionEffectPrefab;

    [Tooltip("爆発位置の調整")]
    [SerializeField]
    private Vector3 explosionOffset =
        Vector3.zero;


    // ==================================================
    // 状態
    // ==================================================

    private bool isDead = false;


    // ==================================================
    // プロパティ
    // ==================================================

    public float MaxHp
    {
        get
        {
            return maxHp;
        }
    }


    public float CurrentHp
    {
        get
        {
            return currentHp;
        }
    }


    public bool IsDead
    {
        get
        {
            return isDead;
        }
    }


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
        // 死亡済み
        // ==========================================

        if (isDead)
        {
            return;
        }


        // ==========================================
        // 無効なダメージ
        // ==========================================

        if (damage <= 0f)
        {
            return;
        }


        // ==========================================
        // ダメージを受けた瞬間の位置に表示
        // ==========================================

        ShowDamagePopup(
            damage
        );


        // ==========================================
        // HP減少
        // ==========================================

        currentHp -=
            damage;


        currentHp =
            Mathf.Clamp(
                currentHp,
                0f,
                maxHp
            );


        Debug.Log(
            gameObject.name +
            " ダメージ：" +
            damage +
            " / 残りHP：" +
            currentHp
        );


        // ==========================================
        // 死亡判定
        // ==========================================

        if (currentHp <= 0f)
        {
            Die();
        }
    }


    // ==================================================
    // ダメージ表示
    // ==================================================

    private void ShowDamagePopup(float damage)
    {
        if (damagePopupPrefab == null)
        {
            Debug.LogWarning("DamagePopup Prefabが設定されていません");
            return;
        }

        Vector3 spawnPosition = transform.position;

        Debug.Log(
            "DamagePopup生成位置: " +
            spawnPosition
        );

        GameObject popupObject =
            Instantiate(
                damagePopupPrefab,
                spawnPosition,
                Quaternion.identity
            );

        Debug.Log(
            "DamagePopup生成成功: " +
            popupObject.name
        );

        DamagePopup popup =
            popupObject.GetComponent<DamagePopup>();

        if (popup == null)
        {
            Debug.LogWarning(
                "DamagePopup.csがPrefabのルートにありません"
            );

            return;
        }

        popup.Setup(damage);
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


        // ==========================================
        // 爆発
        // ==========================================

        SpawnExplosion();


        Debug.Log(
            gameObject.name +
            " が死亡"
        );


        // ==========================================
        // 敵を削除
        // ==========================================

        Destroy(
            gameObject
        );
    }


    // ==================================================
    // 爆発エフェクト
    // ==================================================

    private void SpawnExplosion()
    {
        if (explosionEffectPrefab == null)
        {
            return;
        }


        Vector3 spawnPosition =
            transform.position
            +
            explosionOffset;


        Instantiate(
            explosionEffectPrefab,
            spawnPosition,
            Quaternion.identity
        );
    }


    // ==================================================
    // HP回復
    // ==================================================

    public void Heal(
        float amount
    )
    {
        if (isDead)
        {
            return;
        }


        if (amount <= 0f)
        {
            return;
        }


        currentHp +=
            amount;


        currentHp =
            Mathf.Clamp(
                currentHp,
                0f,
                maxHp
            );
    }


    // ==================================================
    // HP全回復
    // ==================================================

    public void FullHeal()
    {
        if (isDead)
        {
            return;
        }


        currentHp =
            maxHp;
    }


}