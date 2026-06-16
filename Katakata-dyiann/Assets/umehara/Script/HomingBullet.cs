using UnityEngine;

public class HomingBullet : MonoBehaviour
{
    private Transform target;

    [Header("Move")]
    public float speed = 8f;

    [Tooltip("追尾で曲がる速さ。小さいほどゆるく曲がる")]
    public float rotateSpeed = 90f;

    [Tooltip("発射後、何秒後から追尾を始めるか")]
    public float homingStartDelay = 0.2f;

    [Header("Damage")]
    public float damage = 2f;

    [Header("Life")]
    public float lifeTime = 4f;

    private float timer = 0f;

    public void Initialize(Vector2 firstDirection, float bulletSpeed, float bulletDamage)
    {
        speed = bulletSpeed;
        damage = bulletDamage;

        float angle = Mathf.Atan2(firstDirection.y, firstDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

        FindNearestEnemy();

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer < homingStartDelay)
        {
            transform.position += transform.up * speed * Time.deltaTime;
            return;
        }

        if (target == null)
        {
            FindNearestEnemy();
        }

        if (target == null)
        {
            transform.position += transform.up * speed * Time.deltaTime;
            return;
        }

        Vector2 direction = target.position - transform.position;

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotateSpeed * Time.deltaTime
        );

        transform.position += transform.up * speed * Time.deltaTime;
    }

    private void FindNearestEnemy()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        float nearestDistance = float.MaxValue;
        Transform nearestTarget = null;

        foreach (Enemy enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTarget = enemy.transform;
            }
        }

        target = nearestTarget;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}