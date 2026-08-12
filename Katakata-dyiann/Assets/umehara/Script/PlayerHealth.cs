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

    [Header("被弾SE")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip damageSE;
    [SerializeField, Range(0f, 1f)] private float damageSEVolume = 1f;

    private bool isInvincible;
    private bool isDead;

    public int MaxHearts => maxHearts;
    public int CurrentHearts => currentHearts;
    public bool IsDead => isDead;

    private PlayerKeyMover playerKeyMover;

    private void Awake()
    {
        currentHearts = maxHearts;
        playerKeyMover = GetComponent<PlayerKeyMover>();
    }

    private void Start()
    {
        UpdateHeartUI();
    }

    public void TakeDamage(int damage)
    {
        // 移動中は被弾しない
        if (playerKeyMover != null && playerKeyMover.isMoving)
        {
            return;
        }

        if (isDead || isInvincible)
        {
            return;
        }

        currentHearts -= damage;
        currentHearts = Mathf.Clamp(currentHearts, 0, maxHearts);

        if (audioSource != null && damageSE != null)
        {
            audioSource.PlayOneShot(damageSE, damageSEVolume);
        }

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake();
        }

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

    public void Heal(int amount)
    {
        if (isDead)
        {
            return;
        }

        currentHearts += amount;
        currentHearts = Mathf.Clamp(
            currentHearts,
            0,
            maxHearts
        );

        UpdateHeartUI();
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