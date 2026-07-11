using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float hp = 3f;

    public void TakeDamage(float damage)

    {

        hp -= damage;

        Debug.Log(gameObject.name + " に " + damage + " ダメージ");

        if (hp <= 0f)

        {
            Die();
        }

    }

    private void Die()
    {
        Debug.Log("Boss Defeated");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(1000);
            GameManager.Instance.AddDefeatedEnemyCount();
            GameManager.Instance.GameClear();
        }

        Destroy(gameObject);
    }

    /*public void TakeDamage(float damage)
    {
        hp -= damage;

        if (hp <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddFlowGauge();
        }

        Destroy(gameObject);
    }*/
}