using System.Collections;
using TMPro;
using UnityEngine;

public class WordCommandUI : MonoBehaviour
{
    public static WordCommandUI Instance;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI commandText;

    [Header("Color")]
    [SerializeField]
    private Color inactiveColor =
        new Color(1f, 1f, 1f, 0.25f);

    [SerializeField]
    private Color activeColor =
        new Color(1f, 1f, 1f, 1f);

    [Header("Complete")]
    [SerializeField] private float completeDisplayTime = 0.5f;

    private string currentWord = "";
    private int currentProgress = 0;

    private Coroutine hideCoroutine;

    private void Awake()
    {
        Instance = this;

        Hide();
    }

    public void StartCommand(string word)
    {
        if (commandText == null)
        {
            return;
        }

        currentWord = word;
        currentProgress = 1;

        commandText.gameObject.SetActive(true);

        UpdateDisplay();
    }

    public void UpdateProgress(string word, int progress)
    {
        if (commandText == null)
        {
            return;
        }

        currentWord = word;
        currentProgress = progress;

        commandText.gameObject.SetActive(true);

        UpdateDisplay();
    }

    public void CompleteCommand(string word)
    {
        currentWord = word;
        currentProgress = word.Length;

        UpdateDisplay();

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        hideCoroutine = StartCoroutine(
            HideAfterComplete()
        );
    }

    public void CancelCommand()
    {
        Hide();
    }

    private void UpdateDisplay()
    {
        if (string.IsNullOrEmpty(currentWord))
        {
            Hide();
            return;
        }

        string result = "";

        for (int i = 0; i < currentWord.Length; i++)
        {
            Color color =
                i < currentProgress
                    ? activeColor
                    : inactiveColor;

            string colorHex =
                ColorUtility.ToHtmlStringRGBA(color);

            result +=
                "<color=#" +
                colorHex +
                ">" +
                currentWord[i] +
                "</color>";
        }

        commandText.text = result;
    }

    private IEnumerator HideAfterComplete()
    {
        yield return new WaitForSeconds(
            completeDisplayTime
        );

        Hide();
    }

    private void Hide()
    {
        currentWord = "";
        currentProgress = 0;

        if (commandText != null)
        {
            commandText.text = "";
            commandText.gameObject.SetActive(false);
        }
    }
}