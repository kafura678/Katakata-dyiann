using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKeyMover : MonoBehaviour
{
    [System.Serializable]
    public class KeyMovePoint
    {
        public KeyCode key;
        public Transform movePoint;
    }

    [Header("キーごとの移動先")]
    public List<KeyMovePoint> keyMovePoints = new List<KeyMovePoint>();

    [Header("移動設定")]
    public float moveDuration = 0.05f;
    public bool isMoving = false;

    [Header("移動先の敵チェック")]
    public float movePointCheckRadius = 0.45f;

    [Header("プレイヤーHP")]
    public int playerHP = 5;
    public int damageOnBlockedMove = 1;

    [Header("斬撃設定")]
    public LayerMask enemyLayer;
    public float slashWidth = 0.4f;
    public float baseDamage = 1f;
    public float distanceDamageRate = 0.5f;

    [Header("斬撃ライン")]
    public LineRenderer slashLine;
    public float slashLineTime = 0.08f;

    [Header("残像")]
    public GameObject afterImagePrefab;
    public int afterImageCount = 4;



    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (isMoving) return;

        // Shift + A でフローモード発動
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.A))
        {
            return;
        }

        foreach (KeyMovePoint data in keyMovePoints)
        {
            if (!Input.GetKeyDown(data.key)) continue;

            if (data.movePoint == null)
            {
                Debug.LogWarning(data.key + " の MovePoint が設定されていません");
                continue;
            }

            Vector3 targetPosition = data.movePoint.position;

            // 移動先に敵がいるか確認
            if (IsEnemyAtPosition(targetPosition))
            {
                TakeDamage(damageOnBlockedMove);

                Transform safePoint = FindNearestSafeMovePoint(targetPosition);

                if (safePoint != null)
                {
                    Debug.Log("移動先に敵がいたため、近くの安全なキー位置へ移動します");
                    targetPosition = safePoint.position;
                }
                else
                {
                    Debug.LogWarning("安全な移動先がありません");
                    return;
                }
            }

            StartCoroutine(MoveAndSlash(targetPosition));
            break;
        }
    }

    private IEnumerator MoveAndSlash(Vector3 targetPosition)
    {
        isMoving = true;

        Vector3 startPosition = transform.position;
        Vector3 endPosition = targetPosition;

        float distance = Vector2.Distance(startPosition, endPosition);
        float damage = baseDamage + distance * distanceDamageRate;

        DamageEnemiesOnLine(startPosition, endPosition, damage);
        ShowAfterImages(startPosition, endPosition);
        StartCoroutine(ShowSlashLine(startPosition, endPosition));

        float timer = 0f;

        while (timer < moveDuration)
        {
            timer += Time.deltaTime;
            float t = timer / moveDuration;

            transform.position = Vector3.Lerp(startPosition, endPosition, t);

            yield return null;
        }

        transform.position = endPosition;
        isMoving = false;
    }

    private bool IsEnemyAtPosition(Vector3 position)
    {
        Collider2D hit = Physics2D.OverlapCircle(
            position,
            movePointCheckRadius,
            enemyLayer
        );

        return hit != null;
    }

    private Transform FindNearestSafeMovePoint(Vector3 blockedPosition)
    {
        Transform nearestPoint = null;
        float nearestDistance = float.MaxValue;

        foreach (KeyMovePoint data in keyMovePoints)
        {
            if (data.movePoint == null) continue;

            Vector3 pointPosition = data.movePoint.position;

            // 敵がいる場所は候補から外す
            if (IsEnemyAtPosition(pointPosition)) continue;

            float distance = Vector2.Distance(blockedPosition, pointPosition);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPoint = data.movePoint;
            }
        }

        return nearestPoint;
    }

    private void DamageEnemiesOnLine(Vector3 start, Vector3 end, float damage)
    {
        Vector2 direction = end - start;
        float distance = direction.magnitude;

        if (distance <= 0.01f) return;

        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            start,
            slashWidth,
            direction.normalized,
            distance,
            enemyLayer
        );

        foreach (RaycastHit2D hit in hits)
        {
            float finalDamage = GetComboDamage(damage);

            Enemy enemy = hit.collider.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(finalDamage);
                AddCombo();
                AddFlowByHit();
                continue;
            }

            BossController boss = hit.collider.GetComponent<BossController>();

            if (boss != null)
            {
                boss.TakeDamage(finalDamage);
                AddCombo();
                AddFlowByHit();
                continue;
            }

            BossMinion minion = hit.collider.GetComponent<BossMinion>();

            if (minion != null)
            {
                minion.TakeDamage(finalDamage);
                AddCombo();
                AddFlowByHit();
            }
        }
    }

    private IEnumerator ShowSlashLine(Vector3 start, Vector3 end)
    {
        if (slashLine == null) yield break;

        slashLine.enabled = true;
        slashLine.positionCount = 2;
        slashLine.SetPosition(0, start);
        slashLine.SetPosition(1, end);

        yield return new WaitForSeconds(slashLineTime);

        slashLine.enabled = false;
    }

    private void ShowAfterImages(Vector3 start, Vector3 end)
    {
        if (afterImagePrefab == null) return;

        for (int i = 1; i <= afterImageCount; i++)
        {
            float t = (float)i / (afterImageCount + 1);
            Vector3 pos = Vector3.Lerp(start, end, t);

            GameObject afterImage = Instantiate(afterImagePrefab, pos, transform.rotation);
            Destroy(afterImage, 0.2f);
        }
    }

    private float GetComboDamage(float baseDamage)
    {
        if (ComboManager.Instance == null)
        {
            return baseDamage;
        }

        return baseDamage * ComboManager.Instance.GetDamageMultiplier();
    }

    private void AddCombo()
    {
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.AddCombo();
        }
    }

    private void AddFlowByHit()
    {
        if (FlowManager.Instance == null) return;

        if (FlowManager.Instance.isFlowMode)
        {
            FlowManager.Instance.CountFlowAttack();
        }
        else
        {
            FlowManager.Instance.AddFlowGauge(FlowManager.Instance.flowGainPerHit);
        }
    }

    public void TakeDamage(int damage)
    {
        playerHP -= damage;

        if (playerHP < 0)
        {
            playerHP = 0;
        }

        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.ResetCombo();
        }

        Debug.Log("プレイヤーHP: " + playerHP);

        if (playerHP <= 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (keyMovePoints == null) return;

        Gizmos.color = Color.red;

        foreach (KeyMovePoint data in keyMovePoints)
        {
            if (data.movePoint == null) continue;

            Gizmos.DrawWireSphere(data.movePoint.position, movePointCheckRadius);
        }
    }
}