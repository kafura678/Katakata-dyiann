using System.Collections.Generic;
using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [System.Serializable]
    public class KeyAimPoint
    {
        public KeyCode key;
        public Transform aimPoint;
    }

    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;

    [Header("Fire Settings")]
    public float normalDamage = 1f;
    public float flowDamage = 2f;

    [Tooltip("弾を自動発射する間隔。小さいほど連射が速い")]
    public float fireCooldown = 0.2f;

    [Header("Aim Points")]
    public List<KeyAimPoint> keyAimPoints = new List<KeyAimPoint>();

    private Vector2 currentAimDirection = Vector2.up;
    private bool hasAimDirection = false;
    private float fireTimer = 0f;

    private void Update()
    {
        HandleInput();
        HandleAutoFire();
    }

    private void HandleInput()
    {
        // Enterで射撃開始
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GameManager.Instance.StartShooting();
        }

        // Spaceでフローモード開始
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameManager.Instance.StartFlowMode();
        }

        foreach (KeyAimPoint data in keyAimPoints)
        {
            if (Input.GetKeyDown(data.key))
            {
                if (data.aimPoint == null)
                {
                    Debug.LogWarning(data.key + " の AimPoint が設定されていません");
                    continue;
                }

                currentAimDirection = GetDirectionToAimPoint(data.aimPoint);
                hasAimDirection = true;

                // 射撃中ではない場合は、キー入力でゲージを溜める
                if (!GameManager.Instance.isShooting)
                {
                    GameManager.Instance.AddShotGauge();
                }

                RotatePlayer(currentAimDirection);

                // 狙いを変えた瞬間にすぐ撃てるようにする
                fireTimer = fireCooldown;

                Debug.Log(data.key + " aim direction = " + currentAimDirection);
            }
        }
    }

    private void HandleAutoFire()
    {
        if (!GameManager.Instance.isShooting) return;
        if (!hasAimDirection) return;

        fireTimer += Time.deltaTime;

        if (fireTimer >= fireCooldown)
        {
            fireTimer = 0f;
            Fire(currentAimDirection);
        }
    }

    private Vector2 GetDirectionToAimPoint(Transform aimPoint)
    {
        Vector2 startPos = firePoint != null ? firePoint.position : transform.position;
        Vector2 targetPos = aimPoint.position;

        Vector2 direction = targetPos - startPos;

        if (direction.sqrMagnitude < 0.01f)
        {
            return Vector2.up;
        }

        return direction.normalized;
    }

    private void Fire(Vector2 direction)
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bulletObj = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.identity
        );

        Bullet bullet = bulletObj.GetComponent<Bullet>();

        float damage = GameManager.Instance.isFlowMode ? flowDamage : normalDamage;

        if (bullet != null)
        {
            bullet.Initialize(direction, bulletSpeed, damage);
        }
    }

    private void RotatePlayer(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // スプライトが上向きなら -90f
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }
}