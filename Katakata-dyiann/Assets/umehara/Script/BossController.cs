using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    private enum BossState
    {
        Normal,
        Special
    }

    [Header("Target")]
    public Transform player;

    [Header("Prefabs")]
    public GameObject bulletPrefab;
    public GameObject minionPrefab;

    [Header("Boss HP")]
    public float hp = 500f;
    public float minionDamageCutRate = 0.25f;

    [Header("Normal Move")]
    public Vector2 centerPosition = Vector2.zero;
    public float normalMoveRadius = 1.2f;
    public float normalMoveSpeed = 2f;
    public float orbitAngleSpeed = 60f;

    [Header("Normal Shot")]
    public float normalShotInterval = 1.4f;
    public int normalWaveBulletCount = 5;
    public float normalBulletSpeed = 2.5f;
    public float normalWaveAngle = 80f;

    [Header("Special Timing")]
    public float normalActionTime = 6f;
    public float specialWaitTime = 0.5f;

    [Header("Special 1: Edge Laser")]
    public float edgeMoveSpeed = 4f;
    public float edgeStopTime = 0.8f;
    public float laserBulletSpeed = 5f;
    public int laserShotCount = 4;
    public float laserShotInterval = 0.5f;
    public float edgeMarginX = 7f;
    public float edgeMarginY = 4f;

    [Header("Special 2: Minions")]
    public int minionCount = 4;
    public float shakeTime = 1f;
    public float shakePower = 0.08f;

    [Header("Special 3: Star Move")]
    public float starMoveSpeed = 4f;
    public float starRadius = 3.5f;
    public float starShotInterval = 0.5f;
    public int starBulletCount = 10;
    public float starBulletSpeed = 3f;

    [Header("Special 4: Dense Slow Bullets")]
    public float denseDuration = 5f;
    public float denseShotInterval = 0.3f;
    public int denseBulletCount = 10;
    public float denseBulletSpeed = 1.6f;

    [Header("画面内移動制限")]
    public bool clampInsideScreen = true;
    public float minX = -6f;
    public float maxX = 6f;
    public float minY = -3.2f;
    public float maxY = 3.2f;

    private BossState state = BossState.Normal;

    private float orbitAngle = 90f;
    private float normalTimer = 0f;
    private float shotTimer = 0f;

    private bool isSpecialRunning = false;

    private BossMinion[] currentMinions = new BossMinion[0];

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        StartCoroutine(BossRoutine());
    }

    private void Update()
    {
        if (state == BossState.Normal && !isSpecialRunning)
        {
            NormalMove();
            NormalShoot();
        }

        transform.position = ClampPosition(transform.position);
    }

    private IEnumerator BossRoutine()
    {
        while (hp > 0f)
        {
            state = BossState.Normal;
            isSpecialRunning = false;
            normalTimer = 0f;

            while (normalTimer < normalActionTime)
            {
                normalTimer += Time.deltaTime;
                yield return null;
            }

            state = BossState.Special;
            isSpecialRunning = true;

            yield return new WaitForSeconds(specialWaitTime);

            int pattern = Random.Range(0, 4);

            switch (pattern)
            {
                case 0:
                    yield return StartCoroutine(Special_EdgeLaser());
                    break;

                case 1:
                    yield return StartCoroutine(Special_SummonMinions());
                    break;

                case 2:
                    yield return StartCoroutine(Special_StarMove());
                    break;

                case 3:
                    yield return StartCoroutine(Special_DenseBullets());
                    break;
            }

            // 特殊攻撃後、中心付近へ一度戻す
            Vector3 returnPos = GetOrbitPosition();
            yield return StartCoroutine(MoveToPosition(returnPos, normalMoveSpeed));

            isSpecialRunning = false;
            state = BossState.Normal;
        }
    }

    private void NormalMove()
    {
        orbitAngle += orbitAngleSpeed * Time.deltaTime;

        Vector3 targetPos = GetOrbitPosition();

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            normalMoveSpeed * Time.deltaTime
        );
    }

    private Vector3 GetOrbitPosition()
    {
        float rad = orbitAngle * Mathf.Deg2Rad;

        float x = centerPosition.x + Mathf.Cos(rad) * normalMoveRadius;
        float y = centerPosition.y + Mathf.Sin(rad) * normalMoveRadius;

        Vector3 pos = new Vector3(x, y, transform.position.z);

        return ClampPosition(pos);
    }

    private void NormalShoot()
    {
        if (FlowManager.Instance != null && !FlowManager.Instance.CanEnemyShoot())
        {
            return;
        }

        shotTimer += Time.deltaTime;

        if (shotTimer < normalShotInterval) return;

        shotTimer = 0f;

        ShootWave(
            transform.position,
            Vector2.down,
            normalWaveBulletCount,
            normalWaveAngle,
            normalBulletSpeed
        );
    }

    private IEnumerator Special_EdgeLaser()
    {
        Vector3 targetPos = GetRandomEdgePosition();

        yield return StartCoroutine(MoveToPosition(targetPos, edgeMoveSpeed));

        for (int i = 0; i < laserShotCount; i++)
        {
            if (FlowManager.Instance != null && !FlowManager.Instance.CanEnemyShoot())
            {
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(edgeStopTime);

            if (player != null)
            {
                Vector2 dir = player.position - transform.position;
                SpawnBullet(transform.position, dir.normalized, laserBulletSpeed);
            }

            yield return new WaitForSeconds(laserShotInterval);
        }
    }

    private IEnumerator Special_SummonMinions()
    {
        yield return StartCoroutine(MoveToPosition(centerPosition, normalMoveSpeed));

        Vector3 originalPos = transform.position;
        float timer = 0f;

        while (timer < shakeTime)
        {
            timer += Time.deltaTime;

            Vector2 randomOffset = Random.insideUnitCircle * shakePower;
            transform.position = originalPos + new Vector3(randomOffset.x, randomOffset.y, 0f);

            yield return null;
        }

        transform.position = originalPos;

        SummonMinions();

        yield return new WaitForSeconds(4f);
    }

    private IEnumerator Special_StarMove()
    {
        Vector3[] points = GetStarPoints();

        for (int i = 0; i < points.Length; i++)
        {
            yield return StartCoroutine(MoveToPosition(points[i], starMoveSpeed));

            if (FlowManager.Instance != null && !FlowManager.Instance.CanEnemyShoot())
            {
                continue;
            }

            ShootCircle(transform.position, starBulletCount, starBulletSpeed);

            yield return new WaitForSeconds(starShotInterval);
        }
    }

    private IEnumerator Special_DenseBullets()
    {
        yield return StartCoroutine(MoveToPosition(centerPosition, normalMoveSpeed));

        float timer = 0f;
        float localShotTimer = 0f;

        while (timer < denseDuration)
        {
            timer += Time.deltaTime;
            localShotTimer += Time.deltaTime;

            if (FlowManager.Instance != null && !FlowManager.Instance.CanEnemyShoot())
            {
                yield return null;
                continue;
            }

            if (localShotTimer >= denseShotInterval)
            {
                localShotTimer = 0f;
                ShootCircle(transform.position, denseBulletCount, denseBulletSpeed);
            }

            yield return null;
        }
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition, float moveSpeed)
    {
        targetPosition = ClampPosition(targetPosition);

        while (Vector3.Distance(transform.position, targetPosition) > 0.02f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetPosition;
    }

    private Vector3 GetRandomEdgePosition()
    {
        int side = Random.Range(0, 4);

        switch (side)
        {
            case 0:
                return ClampPosition(new Vector3(0f, maxY, transform.position.z));      // 上

            case 1:
                return ClampPosition(new Vector3(0f, minY, transform.position.z));      // 下

            case 2:
                return ClampPosition(new Vector3(minX, 0f, transform.position.z));      // 左

            default:
                return ClampPosition(new Vector3(maxX, 0f, transform.position.z));      // 右
        }
    }

    private Vector3[] GetStarPoints()
    {
        Vector3[] outerPoints = new Vector3[5];

        for (int i = 0; i < 5; i++)
        {
            float angle = 90f + 360f / 5f * i;
            Vector2 dir = AngleToDirection(angle);

            Vector3 pos = new Vector3(
                centerPosition.x + dir.x * starRadius,
                centerPosition.y + dir.y * starRadius,
                transform.position.z
            );

            outerPoints[i] = ClampPosition(pos);
        }

        return new Vector3[]
        {
        outerPoints[0],
        outerPoints[2],
        outerPoints[4],
        outerPoints[1],
        outerPoints[3],
        outerPoints[0]
        };
    }

    private void ShootWave(Vector3 position, Vector2 baseDirection, int count, float totalAngle, float speed)
    {
        if (bulletPrefab == null) return;

        if (count <= 1)
        {
            SpawnBullet(position, baseDirection, speed);
            return;
        }

        float startAngle = -totalAngle / 2f;
        float angleStep = totalAngle / (count - 1);
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;

        for (int i = 0; i < count; i++)
        {
            float angle = baseAngle + startAngle + angleStep * i;
            Vector2 dir = AngleToDirection(angle);

            SpawnBullet(position, dir, speed);
        }
    }

    private void ShootCircle(Vector3 position, int count, float speed)
    {
        if (bulletPrefab == null) return;

        for (int i = 0; i < count; i++)
        {
            float angle = 360f / count * i;
            Vector2 dir = AngleToDirection(angle);

            SpawnBullet(position, dir, speed);
        }
    }

    private void SpawnBullet(Vector3 position, Vector2 direction, float speed)
    {
        if (bulletPrefab == null) return;

        GameObject bulletObj = Instantiate(bulletPrefab, position, Quaternion.identity);

        BossBullet bullet = bulletObj.GetComponent<BossBullet>();

        if (bullet != null)
        {
            bullet.Initialize(direction, speed);
        }
    }

    private Vector2 AngleToDirection(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;

        return new Vector2(
            Mathf.Cos(rad),
            Mathf.Sin(rad)
        ).normalized;
    }

    private void SummonMinions()
    {
        if (minionPrefab == null) return;

        currentMinions = new BossMinion[minionCount];

        for (int i = 0; i < minionCount; i++)
        {
            float angle = 360f / minionCount * i;
            Vector2 dir = AngleToDirection(angle);
            Vector3 spawnPos = transform.position + (Vector3)(dir * 1.5f);

            GameObject minionObj = Instantiate(minionPrefab, spawnPos, Quaternion.identity);

            BossMinion minion = minionObj.GetComponent<BossMinion>();

            if (minion != null)
            {
                minion.Initialize(this, angle, i % 2 == 0);
                currentMinions[i] = minion;
            }
        }
    }

    private Vector3 ClampPosition(Vector3 position)
    {
        if (!clampInsideScreen)
        {
            return position;
        }

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);
        position.z = transform.position.z;

        return position;
    }

    public void TakeDamage(float damage)
    {
        float finalDamage = damage;

        if (HasAliveMinion())
        {
            finalDamage *= 1f - minionDamageCutRate;
        }

        hp -= finalDamage;

        Debug.Log("Boss Damage: " + finalDamage + " / HP: " + hp);

        if (hp <= 0f)
        {
            Die();
        }
    }

    private bool HasAliveMinion()
    {
        if (currentMinions == null) return false;

        foreach (BossMinion minion in currentMinions)
        {
            if (minion != null)
            {
                return true;
            }
        }

        return false;
    }

    private void Die()
    {
        Debug.Log("Boss Defeated");
        Destroy(gameObject);
    }
}