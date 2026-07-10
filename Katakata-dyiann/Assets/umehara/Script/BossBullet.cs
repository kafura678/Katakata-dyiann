using UnityEngine;

public class BossBullet : MonoBehaviour
{
    private Vector2 direction;
    private float speed;

    public float lifeTime = 6f;
    public int damage = 1;

    public void Initialize(Vector2 dir, float bulletSpeed)
    {
        direction = dir.normalized;
        speed = bulletSpeed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerKeyMover player = collision.GetComponent<PlayerKeyMover>();

        if (player != null)
        {
            Debug.Log("Player Hit by Boss Bullet");

            Destroy(gameObject);
        }
    }
}