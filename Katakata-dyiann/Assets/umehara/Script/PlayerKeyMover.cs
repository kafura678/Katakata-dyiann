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

    [Header("Move Points")]
    public List<KeyMovePoint> keyMovePoints = new List<KeyMovePoint>();

    [Header("Slash")]
    public LineRenderer slashLine;
    public float slashVisibleTime = 0.08f;

    [Header("Damage")]
    public float baseDamage = 1f;
    public float distanceDamageRate = 0.5f;
    public float slashWidth = 0.4f;
    public LayerMask enemyLayer;

    private void Update()
    {
        HandleKeyMove();
    }

    private void HandleKeyMove()
    {
        foreach (KeyMovePoint data in keyMovePoints)
        {
            if (!Input.GetKeyDown(data.key)) continue;

            if (data.movePoint == null)
            {
                Debug.LogWarning(data.key + " の MovePoint が設定されていません");
                continue;
            }

            MoveAndSlash(data.movePoint.position);
        }
    }

    private void MoveAndSlash(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = targetPosition;

        float distance = Vector2.Distance(startPosition, endPosition);
        float damage = baseDamage + distance * distanceDamageRate;

        DamageEnemiesOnLine(startPosition, endPosition, damage);

        transform.position = endPosition;

        if (slashLine != null)
        {
            StartCoroutine(ShowSlashLine(startPosition, endPosition));
        }

        Debug.Log("移動距離: " + distance + " / ダメージ: " + damage);
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
            Enemy enemy = hit.collider.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    private IEnumerator ShowSlashLine(Vector3 start, Vector3 end)
    {
        slashLine.enabled = true;
        slashLine.positionCount = 2;
        slashLine.SetPosition(0, start);
        slashLine.SetPosition(1, end);

        yield return new WaitForSeconds(slashVisibleTime);

        slashLine.enabled = false;
    }
}