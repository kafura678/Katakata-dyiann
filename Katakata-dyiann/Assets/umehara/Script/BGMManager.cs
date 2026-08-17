using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    [Header("通常BGM")]
    [SerializeField]
    private AudioSource normalBGMSource;

    [SerializeField]
    private AudioClip normalBGM;

    [SerializeField, Range(0f, 1f)]
    private float normalBGMVolume = 0.5f;


    [Header("フローモードBGM")]
    [SerializeField]
    private AudioSource flowBGMSource;

    [SerializeField]
    private AudioClip flowBGM;

    [SerializeField, Range(0f, 1f)]
    private float flowBGMVolume = 0.5f;


    private bool isFlowBGMPlaying = false;

    // 通常BGMを止めた位置
    private float normalBGMTime = 0f;


    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    private void Start()
    {
        PlayNormalBGM();
    }


    // ==================================================
    // 通常BGM
    // ==================================================

    public void PlayNormalBGM()
    {
        if (normalBGMSource == null)
        {
            Debug.LogWarning(
                "Normal BGM Sourceが設定されていません"
            );

            return;
        }

        if (normalBGM != null)
        {
            normalBGMSource.clip =
                normalBGM;
        }

        normalBGMSource.volume =
            normalBGMVolume;

        normalBGMSource.loop =
            true;

        normalBGMSource.Play();

        Debug.Log(
            "通常BGM開始"
        );
    }


    // ==================================================
    // FLOW BGM開始
    // ==================================================

    public void StartFlowBGM()
    {
        if (isFlowBGMPlaying)
        {
            return;
        }

        isFlowBGMPlaying =
            true;


        // ==========================================
        // 通常BGMの位置を保存して一時停止
        // ==========================================

        if (normalBGMSource != null)
        {
            normalBGMTime =
                normalBGMSource.time;

            normalBGMSource.Pause();

            Debug.Log(
                "通常BGM一時停止 : " +
                normalBGMTime
            );
        }


        // ==========================================
        // FLOW BGM
        // ==========================================

        if (flowBGMSource == null)
        {
            Debug.LogWarning(
                "Flow BGM Sourceが設定されていません"
            );

            return;
        }

        if (flowBGM != null)
        {
            flowBGMSource.clip =
                flowBGM;
        }

        flowBGMSource.volume =
            flowBGMVolume;

        flowBGMSource.loop =
            true;

        flowBGMSource.Play();

        Debug.Log(
            "FLOW BGM開始"
        );
    }


    // ==================================================
    // FLOW BGM終了
    // ==================================================

    public void EndFlowBGM()
    {
        Debug.Log(
            "EndFlowBGMが呼ばれました"
        );


        isFlowBGMPlaying =
            false;


        // ==========================================
        // FLOW BGM停止
        // ==========================================

        if (flowBGMSource != null)
        {
            flowBGMSource.Stop();
        }


        // ==========================================
        // 通常BGM再開
        // ==========================================

        if (normalBGMSource == null)
        {
            Debug.LogWarning(
                "Normal BGM Sourceがありません"
            );

            return;
        }


        /*
         * Pause状態ならUnPauseだけで再開する。
         *
         * 何らかの理由でStopされていた場合は、
         * 保存していた位置からPlayし直す。
         */

        normalBGMSource.UnPause();


        // UnPauseしても再生されなかった場合
        if (!normalBGMSource.isPlaying)
        {
            if (normalBGMSource.clip == null)
            {
                normalBGMSource.clip =
                    normalBGM;
            }

            if (normalBGMSource.clip != null)
            {
                normalBGMTime =
                    Mathf.Clamp(
                        normalBGMTime,
                        0f,
                        normalBGMSource.clip.length
                    );

                normalBGMSource.time =
                    normalBGMTime;

                normalBGMSource.Play();
            }
        }


        Debug.Log(
            "通常BGM再開 : " +
            normalBGMSource.time +
            " / Playing = " +
            normalBGMSource.isPlaying
        );
    }
}