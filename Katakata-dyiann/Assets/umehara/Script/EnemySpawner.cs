using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    public float spawnInterval = 1.5f;
    public float spawnRadius = 8f;

    private float timer = 0f;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("enemyPrefab が設定されていません");
            return;
        }

        // 下方向から出ないように、上側だけに制限
        // 20度〜160度：右上〜上〜左上
        float angleDeg = Random.Range(20f, 160f);
        float angle = angleDeg * Mathf.Deg2Rad;

        Vector3 spawnPos = new Vector3(
            Mathf.Cos(angle) * spawnRadius,
            Mathf.Sin(angle) * spawnRadius,
            0f
        );

        GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        Debug.Log("Enemy Spawned at " + spawnPos);

        Enemy enemy = enemyObj.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.hp = Random.Range(2f, 6f);
            enemy.moveSpeed = Random.Range(1.2f, 2.5f);

            float scale = Mathf.Lerp(0.6f, 1.4f, enemy.hp / 6f);
            enemyObj.transform.localScale = new Vector3(scale, scale, 1f);
        }
        else
        {
            Debug.LogWarning("EnemyPrefab に Enemy.cs が付いていません");
        }
    }
}