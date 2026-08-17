using System.Collections;
using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    [Header("爆発アニメーション")]

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [Tooltip("爆発画像を順番に15枚入れる")]
    [SerializeField]
    private Sprite[] explosionSprites;


    [Header("再生設定")]

    [Tooltip("1枚あたりの表示時間")]
    [SerializeField]
    private float frameInterval = 0.05f;

    [Tooltip("再生終了後に自動削除")]
    [SerializeField]
    private bool destroyAfterPlay = true;


    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponent<SpriteRenderer>();
        }
    }


    private void Start()
    {
        StartCoroutine(
            PlayExplosion()
        );
    }


    private IEnumerator PlayExplosion()
    {
        if (
            spriteRenderer == null ||
            explosionSprites == null ||
            explosionSprites.Length == 0
        )
        {
            if (destroyAfterPlay)
            {
                Destroy(gameObject);
            }

            yield break;
        }


        for (
            int i = 0;
            i < explosionSprites.Length;
            i++
        )
        {
            spriteRenderer.sprite =
                explosionSprites[i];


            yield return new WaitForSeconds(
                frameInterval
            );
        }


        if (destroyAfterPlay)
        {
            Destroy(gameObject);
        }
    }
}