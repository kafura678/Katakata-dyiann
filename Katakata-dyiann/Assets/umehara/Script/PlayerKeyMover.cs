using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PlayerKeyMover : MonoBehaviour
{
    // ==================================================
    // 単語コマンド
    // ==================================================

    public enum CommandType
    {
        Heal,
        Shield,
        Guard,
        Power,
        Flow
    }

    [System.Serializable]
    public class WordCommand
    {
        public string word;
        public CommandType commandType;
    }


    // ==================================================
    // キー移動先
    // ==================================================

    [System.Serializable]
    public class KeyMovePoint
    {
        public KeyCode key;
        public Transform movePoint;
    }

    [Header("キーごとの移動先")]
    [SerializeField]
    private List<KeyMovePoint> keyMovePoints =
        new List<KeyMovePoint>();


    // ==================================================
    // 移動
    // ==================================================

    [Header("移動設定")]

    [SerializeField]
    private float moveDuration = 0.05f;

    public bool isMoving = false;


    // ==================================================
    // プレイヤーCollider
    // ==================================================

    [Header("プレイヤー当たり判定")]

    [Tooltip("移動中・超必殺中に無効化するCollider")]
    [SerializeField]
    private Collider2D[] playerColliders;


    // ==================================================
    // 移動先の敵判定
    // ==================================================

    [Header("移動先の敵判定")]

    [Tooltip("移動先に敵がいるか調べる半径")]
    [SerializeField]
    private float movePointCheckRadius = 0.45f;

    [Tooltip("敵がいる位置へ移動しようとした時のダメージ")]
    [SerializeField]
    private int damageOnBlockedMove = 1;


    // ==================================================
    // 通常攻撃
    // ==================================================

    [Header("通常攻撃")]

    [SerializeField]
    private LayerMask enemyLayer;

    [Tooltip("斬撃判定の太さ")]
    [SerializeField]
    private float slashWidth = 0.4f;

    [Tooltip("基本ダメージ")]
    [SerializeField]
    private float baseDamage = 1f;

    [Tooltip("移動距離1あたりの追加ダメージ")]
    [SerializeField]
    private float distanceDamageRate = 0.5f;


    // ==================================================
    // ランダムダメージ
    // ==================================================

    [Header("ランダムダメージ")]

    [Tooltip("最低ダメージ倍率")]
    [SerializeField]
    private float minimumRandomDamageRate = 0.9f;

    [Tooltip("最高ダメージ倍率")]
    [SerializeField]
    private float maximumRandomDamageRate = 1.1f;


    // ==================================================
    // 移動軌跡
    // ==================================================

    [Header("移動軌跡")]

    [SerializeField]
    private LineRenderer slashLine;

    [SerializeField]
    private float slashLineTime = 0.08f;


    // ==================================================
    // 斬撃エフェクト
    // ==================================================

    [Header("斬撃エフェクト")]

    [Tooltip("SlashEffectが付いたPrefab")]
    [SerializeField]
    private GameObject slashEffectPrefab;

    [Tooltip("斬撃画像の角度補正")]
    [SerializeField]
    private float slashEffectAngleOffset = 0f;


    // ==================================================
    // 残像
    // ==================================================

    [Header("残像")]

    [SerializeField]
    private GameObject afterImagePrefab;

    [SerializeField]
    private int afterImageCount = 4;

    [SerializeField]
    private float afterImageLifeTime = 0.2f;


    // ==================================================
    // SE共通
    // ==================================================

    [Header("SE共通")]

    [SerializeField]
    private AudioSource audioSource;


    // ==================================================
    // 移動SE
    // ==================================================

    [Header("移動SE")]

    [SerializeField]
    private AudioClip moveSE;

    [SerializeField, Range(0f, 1f)]
    private float moveSEVolume = 1f;


    // ==================================================
    // 斬撃SE
    // ==================================================

    [Header("斬撃SE")]

    [Tooltip("敵にダメージを与えた時だけ再生")]
    [SerializeField]
    private AudioClip slashSE;

    [SerializeField, Range(0f, 1f)]
    private float slashSEVolume = 1f;


    // ==================================================
    // 単語コマンド
    // ==================================================

    [Header("単語コマンド")]

    [SerializeField]
    private List<WordCommand> wordCommands =
        new List<WordCommand>();

    private StringBuilder moveKeyHistory =
        new StringBuilder();


    // ==================================================
    // 超必殺：ボス
    // ==================================================

    [Header("超必殺・対象ボス")]

    [SerializeField]
    private boss bossTarget;


    // ==================================================
    // 超必殺：高速連撃
    // ==================================================

    [Header("超必殺・連続攻撃")]

    [Tooltip("連続攻撃回数")]
    [SerializeField]
    private int ultimateRushCount = 8;

    [Tooltip("1回あたりの基本ダメージ")]
    [SerializeField]
    private float ultimateRushDamage = 4f;

    [Tooltip("1回の高速移動時間")]
    [SerializeField]
    private float ultimateRushMoveDuration = 0.04f;

    [Tooltip("ボス周辺を移動する半径")]
    [SerializeField]
    private float ultimateRushRadius = 1.2f;

    [Tooltip("連続攻撃の間隔")]
    [SerializeField]
    private float ultimateRushInterval = 0.03f;


    // ==================================================
    // 超必殺：ビーム
    // ==================================================

    [Header("超必殺・ビーム")]

    [SerializeField]
    private GameObject beamPrefab;

    [Tooltip("ビーム1ヒットあたりの基本ダメージ")]
    [SerializeField]
    private float beamDamage = 5f;

    [Tooltip("ビーム発射前の溜め時間")]
    [SerializeField]
    private float beamChargeTime = 0.25f;

    [Tooltip("ビーム本体が攻撃を続ける時間")]
    [SerializeField]
    private float beamLifeTime = 1f;

    [Tooltip("ビームのダメージ間隔")]
    [SerializeField]
    private float beamDamageInterval = 0.1f;

    [Tooltip("ボスから横へ離れる距離")]
    [SerializeField]
    private float beamSideDistance = 2f;

    [Tooltip("ビーム発射位置への移動時間")]
    [SerializeField]
    private float beamPositionMoveDuration = 0.08f;

    [Tooltip("ビーム画像の角度補正")]
    [SerializeField]
    private float beamAngleOffset = 0f;


    // ==================================================
    // ビーム出現位置
    // ==================================================

    [Header("ビーム出現位置")]

    [Tooltip(
        "X：ボス方向への前後\n" +
        "Y：ビーム方向に対する上下"
    )]
    [SerializeField]
    private Vector2 beamSpawnOffset =
        Vector2.zero;


    // ==================================================
    // ビーム描画順
    // ==================================================

    [Header("ビーム描画順")]

    [Tooltip("敵より大きい値に設定")]
    [SerializeField]
    private int beamOrderInLayer = 20;


    // ==================================================
    // ビーム終了アニメーション
    // ==================================================

    [Header("ビーム終了アニメーション")]

    [Tooltip("ビーム終了時に順番に表示するSprite")]
    [SerializeField]
    private Sprite[] beamEndSprites;

    [Tooltip("終了アニメーション1枚あたりの時間")]
    [SerializeField]
    private float beamEndFrameInterval = 0.05f;


    // ==================================================
    // ビーム発射SE
    // ==================================================

    [Header("ビーム発射SE")]

    [SerializeField]
    private AudioClip beamSE;

    [SerializeField, Range(0f, 1f)]
    private float beamSEVolume = 1f;


    // ==================================================
    // ビームヒットSE
    // ==================================================

    [Header("ビームヒットSE")]

    [SerializeField]
    private AudioClip beamHitSE;

    [SerializeField, Range(0f, 1f)]
    private float beamHitSEVolume = 1f;


    // ==================================================
    // 状態
    // ==================================================

    private bool isUltimatePlaying = false;


    // ==================================================
    // 他コンポーネント
    // ==================================================

    private PlayerHealth playerHealth;


    // ==================================================
    // Unity
    // ==================================================

    private void Awake()
    {
        playerHealth =
            GetComponent<PlayerHealth>();


        if (
            playerColliders == null ||
            playerColliders.Length == 0
        )
        {
            playerColliders =
                GetComponentsInChildren<Collider2D>();
        }


        if (audioSource == null)
        {
            audioSource =
                GetComponent<AudioSource>();
        }
    }


    private void Start()
    {
        if (bossTarget == null)
        {
            bossTarget =
                FindFirstObjectByType<boss>();
        }


        SetPlayerColliders(
            true
        );
    }


    private void Update()
    {
        if (
            playerHealth != null &&
            playerHealth.IsDead
        )
        {
            return;
        }


        if (isUltimatePlaying)
        {
            return;
        }


        HandleInput();
    }


    // ==================================================
    // 入力
    // ==================================================

    private void HandleInput()
    {
        if (isMoving)
        {
            return;
        }


        foreach (KeyMovePoint data in keyMovePoints)
        {
            if (!Input.GetKeyDown(data.key))
            {
                continue;
            }


            if (data.movePoint == null)
            {
                Debug.LogWarning(
                    data.key +
                    " のMovePointが設定されていません"
                );

                continue;
            }


            Vector3 targetPosition =
                data.movePoint.position;


            // ==========================================
            // 現在位置と同じ
            // ==========================================

            if (IsSamePosition(targetPosition))
            {
                return;
            }


            // ==========================================
            // 移動先に敵がいる
            // ==========================================

            if (IsEnemyAtPosition(targetPosition))
            {
                // ======================================
                // ダメージ
                // ======================================

                TakeDamage(
                    damageOnBlockedMove
                );


                // ======================================
                // 最も近い安全なキー位置
                // ======================================

                Transform safePoint =
                    FindNearestSafeMovePoint(
                        targetPosition
                    );


                if (safePoint == null)
                {
                    return;
                }


                Vector3 safePosition =
                    safePoint.position;


                // ======================================
                // 現在位置なら移動しない
                // ======================================

                if (IsSamePosition(safePosition))
                {
                    return;
                }


                // ======================================
                // 安地へ退避
                //
                // 攻撃なし
                // 単語入力なし
                // Comboなし
                // FLOWなし
                // ======================================

                StartCoroutine(
                    MoveToSafePoint(
                        safePosition
                    )
                );


                return;
            }


            // ==========================================
            // 通常移動
            // ==========================================

            StartCoroutine(
                MoveAndSlash(
                    targetPosition,
                    data.key
                )
            );


            break;
        }
    }


    // ==================================================
    // 通常移動 + 攻撃
    // ==================================================

    private IEnumerator MoveAndSlash(
        Vector3 targetPosition,
        KeyCode usedKey
    )
    {
        isMoving =
            true;


        SetPlayerColliders(
            false
        );


        Vector3 startPosition =
            transform.position;


        Vector3 endPosition =
            targetPosition;


        float damage =
            CalculateDamage(
                startPosition,
                endPosition
            );


        // ==========================================
        // 攻撃判定
        // ==========================================

        DamageEnemiesOnLine(
            startPosition,
            endPosition,
            damage
        );


        // ==========================================
        // 移動演出
        // ==========================================

        PlayMoveEffects(
            startPosition,
            endPosition
        );


        // ==========================================
        // 移動
        // ==========================================

        yield return StartCoroutine(
            MoveRoutine(
                startPosition,
                endPosition,
                moveDuration
            )
        );


        // ==========================================
        // 単語入力
        // ==========================================

        RegisterMoveKey(
            usedKey
        );


        // ==========================================
        // Collider復帰
        // ==========================================

        if (
            playerHealth == null ||
            !playerHealth.IsDead
        )
        {
            SetPlayerColliders(
                true
            );
        }


        isMoving =
            false;
    }


    // ==================================================
    // 安地へ移動
    // ==================================================

    private IEnumerator MoveToSafePoint(
        Vector3 targetPosition
    )
    {
        isMoving =
            true;


        SetPlayerColliders(
            false
        );


        Vector3 startPosition =
            transform.position;


        // ==========================================
        // 安地移動では残像のみ
        // ==========================================

        ShowAfterImages(
            startPosition,
            targetPosition
        );


        // ==========================================
        // 移動SE
        // ==========================================

        PlayMoveSE();


        // ==========================================
        // 移動
        // ==========================================

        yield return StartCoroutine(
            MoveRoutine(
                startPosition,
                targetPosition,
                moveDuration
            )
        );


        // ==========================================
        // Collider復帰
        // ==========================================

        if (
            playerHealth == null ||
            !playerHealth.IsDead
        )
        {
            SetPlayerColliders(
                true
            );
        }


        isMoving =
            false;
    }


    // ==================================================
    // 最も近い安全なキー位置
    // ==================================================

    private Transform FindNearestSafeMovePoint(
        Vector3 blockedPosition
    )
    {
        Transform nearestPoint =
            null;


        float nearestDistance =
            float.MaxValue;


        foreach (KeyMovePoint data in keyMovePoints)
        {
            if (data.movePoint == null)
            {
                continue;
            }


            Vector3 position =
                data.movePoint.position;


            // ==========================================
            // 敵がいる位置は除外
            // ==========================================

            if (IsEnemyAtPosition(position))
            {
                continue;
            }


            float distance =
                Vector2.Distance(
                    blockedPosition,
                    position
                );


            if (distance < nearestDistance)
            {
                nearestDistance =
                    distance;


                nearestPoint =
                    data.movePoint;
            }
        }


        return nearestPoint;
    }


    // ==================================================
    // 共通移動
    // ==================================================

    private IEnumerator MoveRoutine(
        Vector3 start,
        Vector3 end,
        float duration
    )
    {
        if (duration <= 0f)
        {
            transform.position =
                end;

            yield break;
        }


        float timer =
            0f;


        while (timer < duration)
        {
            timer +=
                Time.deltaTime;


            float t =
                Mathf.Clamp01(
                    timer /
                    duration
                );


            transform.position =
                Vector3.Lerp(
                    start,
                    end,
                    t
                );


            yield return null;
        }


        transform.position =
            end;
    }


    // ==================================================
    // Collider
    // ==================================================

    private void SetPlayerColliders(
        bool enabled
    )
    {
        if (playerColliders == null)
        {
            return;
        }


        foreach (Collider2D col in playerColliders)
        {
            if (col == null)
            {
                continue;
            }


            col.enabled =
                enabled;
        }
    }


    // ==================================================
    // 通常攻撃ダメージ
    // ==================================================

    private float CalculateDamage(
        Vector3 start,
        Vector3 end
    )
    {
        float distance =
            Vector2.Distance(
                start,
                end
            );


        float damage =
            baseDamage +
            distance *
            distanceDamageRate;


        // ==========================================
        // Combo倍率
        // ==========================================

        if (ComboManager.Instance != null)
        {
            damage *=
                ComboManager.Instance
                    .GetDamageMultiplier();
        }


        return FinalizeDamage(
            damage
        );
    }


    // ==================================================
    // 最終ダメージ処理
    // ==================================================

    private float FinalizeDamage(
        float damage
    )
    {
        float minimum =
            Mathf.Min(
                minimumRandomDamageRate,
                maximumRandomDamageRate
            );


        float maximum =
            Mathf.Max(
                minimumRandomDamageRate,
                maximumRandomDamageRate
            );


        // ==========================================
        // ±10%
        // ==========================================

        damage *=
            Random.Range(
                minimum,
                maximum
            );


        // ==========================================
        // 最大ダメージ
        // ==========================================

        if (ComboManager.Instance != null)
        {
            damage =
                ComboManager.Instance
                    .ClampDamage(
                        damage
                    );
        }


        return damage;
    }


    // ==================================================
    // 直線攻撃
    // ==================================================

    private void DamageEnemiesOnLine(
        Vector3 start,
        Vector3 end,
        float damage
    )
    {
        Vector2 direction =
            end -
            start;


        float distance =
            direction.magnitude;


        if (distance <= 0.01f)
        {
            return;
        }


        RaycastHit2D[] hits =
            Physics2D.CircleCastAll(
                start,
                slashWidth,
                direction.normalized,
                distance,
                enemyLayer
            );


        HashSet<Enemy> damagedEnemies =
            new HashSet<Enemy>();


        HashSet<BossMinion> damagedMinions =
            new HashSet<BossMinion>();


        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
            {
                continue;
            }


            // ======================================
            // Enemy
            // ======================================

            Enemy enemy =
                hit.collider
                    .GetComponentInParent<Enemy>();


            if (enemy != null)
            {
                if (
                    damagedEnemies.Contains(
                        enemy
                    )
                )
                {
                    continue;
                }


                Vector3 hitPosition =
                    enemy.transform.position;


                enemy.TakeDamage(
                    damage
                );


                damagedEnemies.Add(
                    enemy
                );


                // ==================================
                // ヒット時のみ
                // ==================================

                SpawnSlashEffect(
                    hitPosition,
                    direction
                );


                PlaySlashSE();


                OnSuccessfulAttack();


                continue;
            }


            // ======================================
            // BossMinion
            // ======================================

            BossMinion minion =
                hit.collider
                    .GetComponentInParent<BossMinion>();


            if (minion != null)
            {
                if (
                    damagedMinions.Contains(
                        minion
                    )
                )
                {
                    continue;
                }


                Vector3 hitPosition =
                    minion.transform.position;


                minion.TakeDamage(
                    damage
                );


                damagedMinions.Add(
                    minion
                );


                SpawnSlashEffect(
                    hitPosition,
                    direction
                );


                PlaySlashSE();


                OnSuccessfulAttack();
            }
        }
    }


    // ==================================================
    // 攻撃成功
    // ==================================================

    private void OnSuccessfulAttack()
    {
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance
                .AddCombo();
        }


        if (FlowManager.Instance == null)
        {
            return;
        }


        if (FlowManager.Instance.isFlowMode)
        {
            FlowManager.Instance
                .CountFlowAttack();
        }
        else
        {
            FlowManager.Instance
                .AddFlowGauge(
                    FlowManager.Instance
                        .flowGainPerHit
                );
        }
    }


    // ==================================================
    // 移動演出
    // ==================================================

    private void PlayMoveEffects(
        Vector3 start,
        Vector3 end
    )
    {
        ShowAfterImages(
            start,
            end
        );


        if (slashLine != null)
        {
            StartCoroutine(
                ShowSlashLine(
                    start,
                    end
                )
            );
        }


        // ==========================================
        // 斬撃SEはここでは鳴らさない
        // ==========================================

        PlayMoveSE();
    }


    // ==================================================
    // 移動軌跡
    // ==================================================

    private IEnumerator ShowSlashLine(
        Vector3 start,
        Vector3 end
    )
    {
        if (slashLine == null)
        {
            yield break;
        }


        slashLine.enabled =
            true;


        slashLine.positionCount =
            2;


        slashLine.SetPosition(
            0,
            start
        );


        slashLine.SetPosition(
            1,
            end
        );


        yield return new WaitForSeconds(
            slashLineTime
        );


        slashLine.enabled =
            false;
    }


    // ==================================================
    // 斬撃エフェクト
    // ==================================================

    private void SpawnSlashEffect(
        Vector3 spawnPosition,
        Vector2 attackDirection
    )
    {
        if (slashEffectPrefab == null)
        {
            return;
        }


        if (
            attackDirection.sqrMagnitude <=
            0.001f
        )
        {
            return;
        }


        float angle =
            Mathf.Atan2(
                attackDirection.y,
                attackDirection.x
            )
            *
            Mathf.Rad2Deg;


        angle +=
            slashEffectAngleOffset;


        Quaternion rotation =
            Quaternion.Euler(
                0f,
                0f,
                angle
            );


        Instantiate(
            slashEffectPrefab,
            spawnPosition,
            rotation
        );
    }


    // ==================================================
    // 残像
    // ==================================================

    private void ShowAfterImages(
        Vector3 start,
        Vector3 end
    )
    {
        if (afterImagePrefab == null)
        {
            return;
        }


        for (
            int i = 1;
            i <= afterImageCount;
            i++
        )
        {
            float t =
                (float)i /
                (afterImageCount + 1);


            Vector3 position =
                Vector3.Lerp(
                    start,
                    end,
                    t
                );


            GameObject afterImage =
                Instantiate(
                    afterImagePrefab,
                    position,
                    transform.rotation
                );


            Destroy(
                afterImage,
                afterImageLifeTime
            );
        }
    }


    // ==================================================
    // SE
    // ==================================================

    private void PlayMoveSE()
    {
        if (
            audioSource == null ||
            moveSE == null
        )
        {
            return;
        }


        audioSource.PlayOneShot(
            moveSE,
            moveSEVolume
        );
    }


    private void PlaySlashSE()
    {
        if (
            audioSource == null ||
            slashSE == null
        )
        {
            return;
        }


        audioSource.PlayOneShot(
            slashSE,
            slashSEVolume
        );
    }


    private void PlayBeamSE()
    {
        if (
            audioSource == null ||
            beamSE == null
        )
        {
            return;
        }


        audioSource.PlayOneShot(
            beamSE,
            beamSEVolume
        );
    }


    private void PlayBeamHitSE()
    {
        if (
            audioSource == null ||
            beamHitSE == null
        )
        {
            return;
        }


        audioSource.PlayOneShot(
            beamHitSE,
            beamHitSEVolume
        );
    }


    // ==================================================
    // 現在位置判定
    // ==================================================

    private bool IsSamePosition(
        Vector3 position
    )
    {
        return
            Vector2.Distance(
                transform.position,
                position
            )
            <
            0.01f;
    }


    // ==================================================
    // 移動先の敵判定
    // ==================================================

    private bool IsEnemyAtPosition(
        Vector3 position
    )
    {
        Collider2D hit =
            Physics2D.OverlapCircle(
                position,
                movePointCheckRadius,
                enemyLayer
            );


        return
            hit != null;
    }


    // ==================================================
    // 単語入力
    // ==================================================

    private void RegisterMoveKey(
        KeyCode key
    )
    {
        string keyString =
            key.ToString()
                .ToUpper();


        if (keyString.Length != 1)
        {
            return;
        }


        moveKeyHistory.Append(
            keyString
        );


        CheckWordCommands();
    }


    // ==================================================
    // 単語判定
    // ==================================================

    private void CheckWordCommands()
    {
        if (moveKeyHistory.Length == 0)
        {
            return;
        }


        string history =
            moveKeyHistory.ToString();


        // ==========================================
        // 完成
        // ==========================================

        foreach (WordCommand command in wordCommands)
        {
            if (!IsValidCommand(command))
            {
                continue;
            }


            string word =
                command.word
                    .ToUpper();


            if (history == word)
            {
                if (WordCommandUI.Instance != null)
                {
                    WordCommandUI.Instance
                        .CompleteCommand(
                            word
                        );
                }


                ExecuteCommand(
                    command
                );


                moveKeyHistory.Clear();

                return;
            }
        }


        // ==========================================
        // 入力途中
        // ==========================================

        foreach (WordCommand command in wordCommands)
        {
            if (!IsValidCommand(command))
            {
                continue;
            }


            string word =
                command.word
                    .ToUpper();


            if (word.StartsWith(history))
            {
                if (WordCommandUI.Instance != null)
                {
                    WordCommandUI.Instance
                        .UpdateProgress(
                            word,
                            history.Length
                        );
                }


                return;
            }
        }


        // ==========================================
        // 最後の文字から別コマンド
        // ==========================================

        char lastCharacter =
            history[
                history.Length - 1
            ];


        foreach (WordCommand command in wordCommands)
        {
            if (!IsValidCommand(command))
            {
                continue;
            }


            string word =
                command.word
                    .ToUpper();


            if (word[0] != lastCharacter)
            {
                continue;
            }


            moveKeyHistory.Clear();


            moveKeyHistory.Append(
                lastCharacter
            );


            if (WordCommandUI.Instance != null)
            {
                WordCommandUI.Instance
                    .StartCommand(
                        word
                    );
            }


            return;
        }


        // ==========================================
        // 失敗
        // ==========================================

        moveKeyHistory.Clear();


        if (WordCommandUI.Instance != null)
        {
            WordCommandUI.Instance
                .CancelCommand();
        }
    }


    private bool IsValidCommand(
        WordCommand command
    )
    {
        return
            command != null &&
            !string.IsNullOrWhiteSpace(
                command.word
            );
    }


    // ==================================================
    // コマンド実行
    // ==================================================

    private void ExecuteCommand(
        WordCommand command
    )
    {
        switch (command.commandType)
        {
            case CommandType.Heal:

                ActivateHeal();

                break;


            case CommandType.Shield:

                ActivateShield();

                break;


            case CommandType.Guard:

                Debug.Log(
                    "GUARD発動"
                );

                break;


            case CommandType.Power:

                Debug.Log(
                    "POWER発動"
                );

                break;


            case CommandType.Flow:

                Debug.Log(
                    "FLOW発動"
                );

                break;
        }
    }


    // ==================================================
    // HEAL
    // ==================================================

    private void ActivateHeal()
    {
        if (playerHealth == null)
        {
            return;
        }


        playerHealth.Heal(
            1
        );


        Debug.Log(
            "HEAL発動"
        );
    }


    // ==================================================
    // SHIELD
    // ==================================================

    private void ActivateShield()
    {
        if (playerHealth == null)
        {
            return;
        }


        playerHealth.AddShield();


        Debug.Log(
            "SHIELD発動"
        );
    }


    // ==================================================
    // 超必殺開始
    // ==================================================

    public void StartUltimateAttack()
    {
        if (isUltimatePlaying)
        {
            return;
        }


        if (
            playerHealth != null &&
            playerHealth.IsDead
        )
        {
            return;
        }


        StartCoroutine(
            UltimateAttackRoutine()
        );
    }


    // ==================================================
    // 超必殺本体
    // ==================================================

    private IEnumerator UltimateAttackRoutine()
    {
        isUltimatePlaying =
            true;


        isMoving =
            true;


        SetPlayerColliders(
            false
        );


        // ==========================================
        // ボス取得
        // ==========================================

        if (bossTarget == null)
        {
            bossTarget =
                FindFirstObjectByType<boss>();
        }


        if (bossTarget == null)
        {
            Debug.LogWarning(
                "bossが見つかりません"
            );


            FinishUltimate();

            yield break;
        }


        Enemy bossEnemy =
            GetBossEnemy();


        if (bossEnemy == null)
        {
            Debug.LogWarning(
                "bossからEnemyが見つかりません"
            );


            FinishUltimate();

            yield break;
        }


        // ==========================================
        // 高速連撃
        // ==========================================

        yield return StartCoroutine(
            UltimateRushAttack(
                bossTarget,
                bossEnemy
            )
        );


        if (
            bossTarget == null ||
            bossEnemy == null
        )
        {
            FinishUltimate();

            yield break;
        }


        // ==========================================
        // ビーム位置へ移動
        // ==========================================

        yield return StartCoroutine(
            MoveBesideBoss(
                bossTarget
            )
        );


        if (
            bossTarget == null ||
            bossEnemy == null
        )
        {
            FinishUltimate();

            yield break;
        }


        // ==========================================
        // 溜め
        // ==========================================

        if (beamChargeTime > 0f)
        {
            yield return new WaitForSeconds(
                beamChargeTime
            );
        }


        if (
            bossTarget == null ||
            bossEnemy == null
        )
        {
            FinishUltimate();

            yield break;
        }


        // ==========================================
        // ビーム
        // ==========================================

        yield return StartCoroutine(
            FireUltimateBeam(
                bossTarget,
                bossEnemy
            )
        );


        FinishUltimate();
    }


    // ==================================================
    // ボスEnemy取得
    // ==================================================

    private Enemy GetBossEnemy()
    {
        if (bossTarget == null)
        {
            return null;
        }


        Enemy enemy =
            bossTarget
                .GetComponent<Enemy>();


        if (enemy != null)
        {
            return enemy;
        }


        enemy =
            bossTarget
                .GetComponentInParent<Enemy>();


        if (enemy != null)
        {
            return enemy;
        }


        return bossTarget
            .GetComponentInChildren<Enemy>();
    }


    // ==================================================
    // 超必殺：高速連撃
    // ==================================================

    private IEnumerator UltimateRushAttack(
        boss targetBoss,
        Enemy bossEnemy
    )
    {
        if (ultimateRushCount <= 0)
        {
            yield break;
        }


        for (
            int i = 0;
            i < ultimateRushCount;
            i++
        )
        {
            if (
                targetBoss == null ||
                bossEnemy == null
            )
            {
                yield break;
            }


            float angle =
                (
                    360f /
                    ultimateRushCount
                )
                *
                i;


            if (i % 2 == 1)
            {
                angle +=
                    140f;
            }


            float radian =
                angle *
                Mathf.Deg2Rad;


            Vector3 offset =
                new Vector3(
                    Mathf.Cos(radian),
                    Mathf.Sin(radian),
                    0f
                )
                *
                ultimateRushRadius;


            Vector3 targetPosition =
                targetBoss.transform.position
                +
                offset;


            Vector3 startPosition =
                transform.position;


            // ======================================
            // 移動
            // ======================================

            yield return StartCoroutine(
                MoveRoutine(
                    startPosition,
                    targetPosition,
                    ultimateRushMoveDuration
                )
            );


            if (
                targetBoss == null ||
                bossEnemy == null
            )
            {
                yield break;
            }


            Vector3 hitPosition =
                targetBoss.transform.position;


            Vector2 attackDirection =
                targetPosition -
                startPosition;


            // ======================================
            // ダメージ
            // ======================================

            bossEnemy.TakeDamage(
                FinalizeDamage(
                    ultimateRushDamage
                )
            );


            // ======================================
            // ヒット演出
            // ======================================

            SpawnSlashEffect(
                hitPosition,
                attackDirection
            );


            PlaySlashSE();


            // ======================================
            // 移動演出
            // ======================================

            ShowAfterImages(
                startPosition,
                targetPosition
            );


            if (slashLine != null)
            {
                StartCoroutine(
                    ShowSlashLine(
                        startPosition,
                        targetPosition
                    )
                );
            }


            PlayMoveSE();


            if (ultimateRushInterval > 0f)
            {
                yield return new WaitForSeconds(
                    ultimateRushInterval
                );
            }
        }
    }


    // ==================================================
    // 超必殺：ビーム位置へ移動
    // ==================================================

    private IEnumerator MoveBesideBoss(
        boss targetBoss
    )
    {
        if (targetBoss == null)
        {
            yield break;
        }


        Vector3 bossPosition =
            targetBoss.transform.position;


        Vector3 leftPosition =
            bossPosition
            +
            Vector3.left *
            beamSideDistance;


        Vector3 rightPosition =
            bossPosition
            +
            Vector3.right *
            beamSideDistance;


        float leftDistance =
            Vector2.Distance(
                transform.position,
                leftPosition
            );


        float rightDistance =
            Vector2.Distance(
                transform.position,
                rightPosition
            );


        Vector3 targetPosition =
            leftDistance <= rightDistance
            ?
            leftPosition
            :
            rightPosition;


        Vector3 startPosition =
            transform.position;


        yield return StartCoroutine(
            MoveRoutine(
                startPosition,
                targetPosition,
                beamPositionMoveDuration
            )
        );


        ShowAfterImages(
            startPosition,
            targetPosition
        );
    }


    // ==================================================
    // 超必殺：連続ビーム
    // ==================================================

    private IEnumerator FireUltimateBeam(
        boss targetBoss,
        Enemy bossEnemy
    )
    {
        if (
            targetBoss == null ||
            bossEnemy == null
        )
        {
            yield break;
        }


        Vector3 bossDirection =
            targetBoss.transform.position
            -
            transform.position;


        bossDirection.z =
            0f;


        if (
            bossDirection.sqrMagnitude <=
            0.001f
        )
        {
            yield break;
        }


        bossDirection.Normalize();


        Vector3 perpendicular =
            new Vector3(
                -bossDirection.y,
                bossDirection.x,
                0f
            );


        // ==========================================
        // 出現位置
        // ==========================================

        Vector3 spawnPosition =
            transform.position
            +
            bossDirection *
            beamSpawnOffset.x
            +
            perpendicular *
            beamSpawnOffset.y;


        Vector3 direction =
            targetBoss.transform.position
            -
            spawnPosition;


        direction.z =
            0f;


        if (
            direction.sqrMagnitude <=
            0.001f
        )
        {
            yield break;
        }


        // ==========================================
        // 回転
        // ==========================================

        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            )
            *
            Mathf.Rad2Deg;


        angle +=
            beamAngleOffset;


        Quaternion rotation =
            Quaternion.Euler(
                0f,
                0f,
                angle
            );


        // ==========================================
        // Beam生成
        // ==========================================

        GameObject beam =
            null;


        if (beamPrefab != null)
        {
            beam =
                Instantiate(
                    beamPrefab,
                    spawnPosition,
                    rotation
                );


            // ======================================
            // 敵より前へ描画
            // ======================================

            SpriteRenderer[] renderers =
                beam.GetComponentsInChildren<SpriteRenderer>(
                    true
                );


            foreach (
                SpriteRenderer renderer
                in renderers
            )
            {
                renderer.sortingOrder =
                    beamOrderInLayer;
            }
        }


        // ==========================================
        // 発射SE
        // ==========================================

        PlayBeamSE();


        float safeDamageInterval =
            Mathf.Max(
                0.01f,
                beamDamageInterval
            );


        float lifeTimer =
            0f;


        float damageTimer =
            0f;


        // ==========================================
        // 最初の1ヒット
        // ==========================================

        if (
            targetBoss != null &&
            bossEnemy != null
        )
        {
            DealBeamDamage(
                bossEnemy
            );
        }


        // ==========================================
        // 連続ダメージ
        // ==========================================

        while (lifeTimer < beamLifeTime)
        {
            float deltaTime =
                Time.deltaTime;


            lifeTimer +=
                deltaTime;


            damageTimer +=
                deltaTime;


            if (
                targetBoss == null ||
                bossEnemy == null
            )
            {
                break;
            }


            while (
                damageTimer >=
                safeDamageInterval
            )
            {
                damageTimer -=
                    safeDamageInterval;


                DealBeamDamage(
                    bossEnemy
                );


                if (
                    targetBoss == null ||
                    bossEnemy == null
                )
                {
                    break;
                }
            }


            yield return null;
        }


        // ==========================================
        // 終了アニメーション
        //
        // ここからダメージなし
        // ==========================================

        if (beam != null)
        {
            yield return StartCoroutine(
                PlayBeamEndAnimation(
                    beam
                )
            );
        }
    }


    // ==================================================
    // ビーム1ヒット
    // ==================================================

    private void DealBeamDamage(
        Enemy bossEnemy
    )
    {
        if (bossEnemy == null)
        {
            return;
        }


        float damage =
            FinalizeDamage(
                beamDamage
            );


        bossEnemy.TakeDamage(
            damage
        );


        PlayBeamHitSE();
    }


    // ==================================================
    // ビーム終了アニメーション
    // ==================================================

    private IEnumerator PlayBeamEndAnimation(
        GameObject beam
    )
    {
        if (beam == null)
        {
            yield break;
        }


        SpriteRenderer beamRenderer =
            beam.GetComponent<SpriteRenderer>();


        if (beamRenderer == null)
        {
            beamRenderer =
                beam.GetComponentInChildren<SpriteRenderer>();
        }


        // ==========================================
        // SpriteRendererなし
        // ==========================================

        if (beamRenderer == null)
        {
            Destroy(
                beam
            );

            yield break;
        }


        // ==========================================
        // 終了画像なし
        // ==========================================

        if (
            beamEndSprites == null ||
            beamEndSprites.Length == 0
        )
        {
            Destroy(
                beam
            );

            yield break;
        }


        float safeInterval =
            Mathf.Max(
                0.01f,
                beamEndFrameInterval
            );


        // ==========================================
        // Spriteを順番に変更
        // ==========================================

        for (
            int i = 0;
            i < beamEndSprites.Length;
            i++
        )
        {
            if (beam == null)
            {
                yield break;
            }


            if (beamEndSprites[i] == null)
            {
                continue;
            }


            beamRenderer.sprite =
                beamEndSprites[i];


            yield return new WaitForSeconds(
                safeInterval
            );
        }


        if (beam != null)
        {
            Destroy(
                beam
            );
        }
    }


    // ==================================================
    // 超必殺終了
    // ==================================================

    private void FinishUltimate()
    {
        if (
            playerHealth == null ||
            !playerHealth.IsDead
        )
        {
            SetPlayerColliders(
                true
            );
        }


        isMoving =
            false;


        isUltimatePlaying =
            false;


        if (FlowManager.Instance != null)
        {
            FlowManager.Instance
                .UltimateFinished();
        }
    }


    // ==================================================
    // 外部ダメージ
    // ==================================================

    public void TakeDamage(
        int damage
    )
    {
        if (playerHealth == null)
        {
            return;
        }


        playerHealth.TakeDamage(
            damage
        );
    }


    // ==================================================
    // Gizmos
    // ==================================================

    private void OnDrawGizmosSelected()
    {
        if (keyMovePoints == null)
        {
            return;
        }


        Gizmos.color =
            Color.red;


        foreach (KeyMovePoint data in keyMovePoints)
        {
            if (data.movePoint == null)
            {
                continue;
            }


            Gizmos.DrawWireSphere(
                data.movePoint.position,
                movePointCheckRadius
            );
        }
    }
}