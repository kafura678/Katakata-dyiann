using System.Collections;
using UnityEngine;

public class SlashEffect : MonoBehaviour
{
    [Header("斬撃アニメーション")]

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [Tooltip("斬撃画像を再生順に入れる")]
    [SerializeField]
    private Sprite[] slashSprites;


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
            PlaySlash()
        );
    }


    private IEnumerator PlaySlash()
    {
        if (
            spriteRenderer == null ||
            slashSprites == null ||
            slashSprites.Length == 0
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
            i < slashSprites.Length;
            i++
        )
        {
            spriteRenderer.sprite =
                slashSprites[i];


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