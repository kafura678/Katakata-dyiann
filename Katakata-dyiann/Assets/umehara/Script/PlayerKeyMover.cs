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

    [Header("フローモード")]
    public bool isFlowMode = false;
    public float flowGauge = 0f;
    public float maxFlowGauge = 100f;
    public float flowGainPerSlash = 10f;
    public float flowDrainPerSecond = 20f;

    private void Update()
    {
        UpdateFlowMode();
        HandleInput();
    }

    private void HandleInput()
    {
        if (isMoving) return;

        // Shift + A でフローモード発動
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.A))
        {
            StartFlowMode();
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

            StartCoroutine(MoveAndSlash(data.movePoint.position));
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

        if (isFlowMode)
        {
            damage *= 1.5f;
        }

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
                AddFlowGauge(flowGainPerSlash);
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

    private void AddFlowGauge(float amount)
    {
        if (isFlowMode) return;

        flowGauge += amount;
        flowGauge = Mathf.Clamp(flowGauge, 0f, maxFlowGauge);
    }

    private void StartFlowMode()
    {
        if (isFlowMode) return;
        if (flowGauge < maxFlowGauge) return;

        isFlowMode = true;
        Debug.Log("フローモード開始");
    }

    private void UpdateFlowMode()
    {
        if (!isFlowMode) return;

        flowGauge -= flowDrainPerSecond * Time.deltaTime;

        if (flowGauge <= 0f)
        {
            flowGauge = 0f;
            isFlowMode = false;
            Debug.Log("フローモード終了");
        }
    }
}