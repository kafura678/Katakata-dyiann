using System.Collections;
using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [SerializeField]
    private TMP_Text damageText;

    [SerializeField]
    private float moveSpeed = 1f;

    [SerializeField]
    private float lifeTime = 1f;


    private void Awake()
    {
        if (damageText == null)
        {
            damageText =
                GetComponentInChildren<TMP_Text>(true);
        }

        if (damageText == null)
        {
            Debug.LogError(
                "DamagePopup: TMP_Textが見つかりません"
            );
        }
    }


    public void Setup(float damage)
    {
        Debug.Log(
            "DamagePopup Setup: " +
            damage
        );

        if (damageText == null)
        {
            return;
        }

        damageText.text =
            Mathf.RoundToInt(damage)
                .ToString();

        damageText.gameObject
            .SetActive(true);

        StartCoroutine(
            PopupRoutine()
        );
    }


    private IEnumerator PopupRoutine()
    {
        float timer = 0f;

        while (timer < lifeTime)
        {
            timer +=
                Time.deltaTime;

            transform.position +=
                Vector3.up *
                moveSpeed *
                Time.deltaTime;

            yield return null;
        }

        Destroy(gameObject);
    }
}