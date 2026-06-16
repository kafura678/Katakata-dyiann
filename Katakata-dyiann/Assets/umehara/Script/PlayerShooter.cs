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

    [Header("Bullet Prefabs")]
    public GameObject normalBulletPrefab;
    public GameObject homingBulletPrefab;

    [Header("Fire Point")]
    public Transform firePoint;

    [Header("Normal Fire")]
    public float normalBulletSpeed = 10f;
    public float normalDamage = 1f;

    [Tooltip("通常時の自動発射間隔。小さいほど連射が速い")]
    public float normalFireCooldown = 0.25f;

    [Header("Flow Homing Fire")]
    public float homingBulletSpeed = 8f;
    public float homingDamage = 2f;

    [Tooltip("フローモード中、キー入力ごとに撃てる最短間隔")]
    public float flowKeyFireCooldown = 0.08f;

    [Header("Aim Points")]
    public List<KeyAimPoint> keyAimPoints = new List<KeyAimPoint>();

    private Vector2 currentAimDirection = Vector2.up;
    private bool hasAimDirection = false;

    private float normalFireTimer = 0f;
    private float flowFireTimer = 0f;

    private void Update()
    {
        HandleTimers();
        HandleInput();
        HandleNormalAutoFire();
    }

    private void HandleTimers()
    {
        normalFireTimer += Time.deltaTime;
        flowFireTimer += Time.deltaTime;
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GameManager.Instance.StartShooting();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameManager.Instance.StartFlowMode();
        }

        foreach (KeyAimPoint data in keyAimPoints)
        {
            if (!Input.GetKeyDown(data.key)) continue;

            if (data.aimPoint == null)
            {
                Debug.LogWarning(data.key + " の AimPoint が設定されていません");
                continue;
            }

            Vector2 direction = GetDirectionToAimPoint(data.aimPoint);

            currentAimDirection = direction;
            hasAimDirection = true;

            RotatePlayer(direction);

            if (GameManager.Instance.isFlowMode)
            {
                FireHomingOnce(direction);
            }
            else
            {
                if (!GameManager.Instance.isShooting)
                {
                    GameManager.Instance.AddShotGauge();
                }

                // 通常時は狙いを変えるだけ。発射は自動連射側で行う。
            }

            Debug.Log(data.key + " direction = " + direction);
        }
    }

    private void HandleNormalAutoFire()
    {
        if (GameManager.Instance.isFlowMode) return;
        if (!GameManager.Instance.isShooting) return;
        if (!hasAimDirection) return;

        if (normalFireTimer < normalFireCooldown) return;

        normalFireTimer = 0f;
        FireNormal(currentAimDirection);
    }

    private void FireNormal(Vector2 direction)
    {
        if (normalBulletPrefab == null || firePoint == null) return;

        GameObject bulletObj = Instantiate(
            normalBulletPrefab,
            firePoint.position,
            Quaternion.identity
        );

        Bullet bullet = bulletObj.GetComponent<Bullet>();

        if (bullet != null)
        {
            bullet.Initialize(direction, normalBulletSpeed, normalDamage);
        }
    }

    private void FireHomingOnce(Vector2 direction)
    {
        if (flowFireTimer < flowKeyFireCooldown) return;
        if (homingBulletPrefab == null || firePoint == null) return;

        flowFireTimer = 0f;

        GameObject bulletObj = Instantiate(
            homingBulletPrefab,
            firePoint.position,
            Quaternion.identity
        );

        HomingBullet homingBullet = bulletObj.GetComponent<HomingBullet>();

        if (homingBullet != null)
        {
            homingBullet.Initialize(direction, homingBulletSpeed, homingDamage);
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

    private void RotatePlayer(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }
}