using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartUI : MonoBehaviour
{
    public static HeartUI Instance;

    [Header("ハート画像")]
    [SerializeField] private List<Image> heartImages = new List<Image>();

    [Header("スプライト")]
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateHearts(int currentHearts, int maxHearts)
    {
        for (int i = 0; i < heartImages.Count; i++)
        {
            if (heartImages[i] == null)
            {
                continue;
            }

            heartImages[i].gameObject.SetActive(i < maxHearts);

            if (i >= maxHearts)
            {
                continue;
            }

            heartImages[i].sprite =
                i < currentHearts ? fullHeartSprite : emptyHeartSprite;
        }
    }
}