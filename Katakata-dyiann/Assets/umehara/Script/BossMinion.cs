using UnityEngine;

public class BossMinion : MonoBehaviour
{
    private BossController boss;
    private float angle;

    [Header("Orbit")]
    public float orbitRadius = 1.7f;
    public float orbitSpeed = 120f;

    [Header("HP")]
    public float hp = 20f;

    [Header("Shot")]
    public GameObject bulletPrefab;
    public float shotInterval = 1.2f;
    public float bulletSpeed = 2.5f;

    private float shotTimer = 0f;
    private bool alternateOffset = false;

    public void Initialize(BossController bossController, float startAngle, bool offset)
    {
        boss = bossController;
        angle = startAngle;
        alternateOffset = offset;

        shotTimer = alternateOffset ? shotInterval * 0.5f : 0f;
    }

    private void Update()
    {
        if (boss == null)
        {
            Destroy(gameObject);
            return;
        }

        OrbitBoss();
        Shoot();
    }

    private void OrbitBoss()
    {
        angle += orbitSpeed * Time.deltaTime;

        float rad = angle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(
            Mathf.Cos(rad) * orbitRadius,
            Mathf.Sin(rad) * orbitRadius,
            0f
        );

        transform.position = boss.transform.position + offset;
    }

    private void Shoot()
    {
        if (FlowManager.Instance != null && !FlowManager.Instance.CanEnemyShoot())
        {
            return;
        }

        if (bulletPrefab == null) return;

        shotTimer += Time.deltaTime;

        if (shotTimer < shotInterval) return;

        shotTimer = 0f;

        Vector2 dir = transform.position - boss.transform.position;

        GameObject bulletObj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        BossBullet bullet = bulletObj.GetComponent<BossBullet>();

        if (bullet != null)
        {
            bullet.Initialize(dir.normalized, bulletSpeed);
        }
    }

    public void TakeDamage(float damage)
    {
        hp -= damage;

        Debug.Log("Minion Damage: " + damage + " / HP: " + hp);

        if (hp <= 0f)
        {
            Destroy(gameObject);
        }
    }
}