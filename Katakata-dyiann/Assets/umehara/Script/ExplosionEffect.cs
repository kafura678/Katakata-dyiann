using System.Collections;
using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    // ==================================================
    // Spriteアニメーション
    // ==================================================

    [Header("爆発アニメーション")]

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private Sprite[] explosionSprites;

    [SerializeField]
    private float frameInterval = 0.05f;


    // ==================================================
    // SE
    // ==================================================

    [Header("爆発SE")]

    [SerializeField]
    private AudioClip explosionSE;

    [SerializeField, Range(0f, 1f)]
    private float explosionSEVolume = 1f;


    // ==================================================
    // Unity
    // ==================================================

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
        // ==========================================
        // 爆発SE
        // ==========================================

        PlayExplosionSE();


        // ==========================================
        // 爆発アニメーション
        // ==========================================

        StartCoroutine(
            ExplosionAnimationRoutine()
        );
    }


    // ==================================================
    // 爆発アニメーション
    // ==================================================

    private IEnumerator ExplosionAnimationRoutine()
    {
        if (spriteRenderer == null)
        {
            Destroy(gameObject);

            yield break;
        }


        if (
            explosionSprites == null ||
            explosionSprites.Length == 0
        )
        {
            Destroy(gameObject);

            yield break;
        }


        float safeInterval =
            Mathf.Max(
                0.01f,
                frameInterval
            );


        foreach (Sprite sprite in explosionSprites)
        {
            if (sprite == null)
            {
                continue;
            }


            spriteRenderer.sprite =
                sprite;


            yield return new WaitForSeconds(
                safeInterval
            );
        }


        Destroy(
            gameObject
        );
    }


    // ==================================================
    // 爆発SE
    // ==================================================

    private void PlayExplosionSE()
    {
        if (explosionSE == null)
        {
            return;
        }


        AudioSource.PlayClipAtPoint(
            explosionSE,
            transform.position,
            explosionSEVolume
        );
    }
}