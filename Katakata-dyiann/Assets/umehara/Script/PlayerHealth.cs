using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("ハート設定")]
    [SerializeField] private int maxHearts = 3;
    [SerializeField] private int currentHearts;

    [Header("被弾設定")]
    [SerializeField] private float invincibleTime = 1f;
    [SerializeField] private SpriteRenderer playerRenderer;

    private bool isInvincible;
    private bool isDead;

    public int MaxHearts => maxHearts;
    public int CurrentHearts => currentHearts;
    public bool IsDead => isDead;

    private void Awake()
    {
        currentHearts = maxHearts;
    }

    private void Start()
    {
        UpdateHeartUI();
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible)
        {
            return;
        }

        currentHearts -= damage;
        currentHearts = Mathf.Clamp(currentHearts, 0, maxHearts);

        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.ResetCombo();
        }

        UpdateHeartUI();

        if (currentHearts <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(InvincibleRoutine());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Bullet"))
        {
            return;
        }

        TakeDamage(1);
        Destroy(collision.gameObject);
    }

    private void Die()
    {
        isDead = true;

        Debug.Log("プレイヤー死亡");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    private IEnumerator InvincibleRoutine()
    {
        isInvincible = true;

        float timer = 0f;
        const float blinkInterval = 0.1f;

        while (timer < invincibleTime)
        {
            if (playerRenderer != null)
            {
                playerRenderer.enabled = !playerRenderer.enabled;
            }

            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        if (playerRenderer != null)
        {
            playerRenderer.enabled = true;
        }

        isInvincible = false;
    }

    private void UpdateHeartUI()
    {
        if (HeartUI.Instance != null)
        {
            HeartUI.Instance.UpdateHearts(currentHearts, maxHearts);
        }
    }
}