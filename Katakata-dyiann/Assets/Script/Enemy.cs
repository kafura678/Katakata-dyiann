using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float hp = 3f;
    public float moveSpeed = 2f;

    private Transform target;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            target = player.transform;
        }
    }

    private void Update()
    {
        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        transform.position += direction.normalized * moveSpeed * Time.deltaTime;
    }

    public void TakeDamage(float damage)
    {
        hp -= damage;

        if (hp <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        GameManager.Instance.AddFlowGauge();
        Destroy(gameObject);
    }
}