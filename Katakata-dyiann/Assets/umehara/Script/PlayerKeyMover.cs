using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PlayerKeyMover : MonoBehaviour
{
    // ==================================================
    // コマンド
    // ==================================================

    public enum CommandType
    {
        Heal,
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
    // プレイヤー当たり判定
    // ==================================================

    [Header("プレイヤー当たり判定")]

    [Tooltip("移動中に無効化するCollider")]
    [SerializeField]
    private Collider2D[] playerColliders;


    // ==================================================
    // 移動先の敵判定
    // ==================================================

    [Header("移動先の敵判定")]

    [SerializeField]
    private float movePointCheckRadius = 0.45f;

    [SerializeField]
    private int damageOnBlockedMove = 1;


    // ==================================================
    // 斬撃
    // ==================================================

    [Header("斬撃設定")]

    [SerializeField]
    private LayerMask enemyLayer;

    [SerializeField]
    private float slashWidth = 0.4f;

    [SerializeField]
    private float baseDamage = 1f;

    [SerializeField]
    private float distanceDamageRate = 0.5f;


    // ==================================================
    // 斬撃ライン
    // ==================================================

    [Header("斬撃ライン")]

    [SerializeField]
    private LineRenderer slashLine;

    [SerializeField]
    private float slashLineTime = 0.08f;


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
    // 移動SE
    // ==================================================

    [Header("移動SE")]

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip moveSE;

    [SerializeField, Range(0f, 1f)]
    private float moveSEVolume = 1f;


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
    // 超必殺：対象ボス
    // ==================================================

    [Header("超必殺・対象ボス")]

    [SerializeField]
    private boss bossTarget;


    // ==================================================
    // 超必殺：連続攻撃
    // ==================================================

    [Header("超必殺・連続攻撃")]

    [Tooltip("連続攻撃回数")]
    [SerializeField]
    private int ultimateRushCount = 8;

    [Tooltip("連続攻撃1回のダメージ")]
    [SerializeField]
    private float ultimateRushDamage = 4f;

    [Tooltip("1回の高速移動にかかる時間")]
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

    [Tooltip("ビーム画像のPrefab")]
    [SerializeField]
    private GameObject beamPrefab;

    [Tooltip("ビームダメージ")]
    [SerializeField]
    private float beamDamage = 30f;

    [Tooltip("ビーム発射前の溜め時間")]
    [SerializeField]
    private float beamChargeTime = 0.25f;

    [Tooltip("ビームの表示時間")]
    [SerializeField]
    private float beamLifeTime = 0.5f;

    [Tooltip("ボスから横へ離れる距離")]
    [SerializeField]
    private float beamSideDistance = 2f;

    [Tooltip("ボス横への移動時間")]
    [SerializeField]
    private float beamPositionMoveDuration = 0.08f;

    [Tooltip("ビーム画像の角度補正")]
    [SerializeField]
    private float beamAngleOffset = 0f;


    // ==================================================
    // 超必殺状態
    // ==================================================

    private bool isUltimatePlaying = false;


    // ==================================================
    // 他スクリプト
    // ==================================================

    private PlayerHealth playerHealth;


    // ==================================================
    // Unity
    // ==================================================

    private void Awake()
    {
        playerHealth =
            GetComponent<PlayerHealth>();


        // InspectorでColliderを設定していなければ自動取得
        if (
            playerColliders == null ||
            playerColliders.Length == 0
        )
        {
            playerColliders =
                GetComponentsInChildren<Collider2D>();
        }
    }


    private void Start()
    {
        // InspectorでBossを設定していなければ取得
        if (bossTarget == null)
        {
            bossTarget =
                FindFirstObjectByType<boss>();
        }


        // 開始時は当たり判定ON
        SetPlayerColliders(true);
    }


    private void Update()
    {
        // 死亡中
        if (
            playerHealth != null &&
            playerHealth.IsDead
        )
        {
            return;
        }


        // 超必殺中
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
        // 移動中
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
            // 現在位置と同じキー
            // ==========================================

            if (IsSamePosition(targetPosition))
            {
                // 移動なし
                // 攻撃なし
                // SEなし
                // コマンド入力なし
                return;
            }


            // ==========================================
            // 移動先に敵がいる
            // ==========================================

            if (IsEnemyAtPosition(targetPosition))
            {
                TakeDamage(
                    damageOnBlockedMove
                );


                Transform safePoint =
                    FindNearestSafeMovePoint(
                        targetPosition
                    );


                if (safePoint == null)
                {
                    return;
                }


                targetPosition =
                    safePoint.position;


                if (IsSamePosition(targetPosition))
                {
                    return;
                }
            }


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
    // 通常移動 + 斬撃
    // ==================================================

    private IEnumerator MoveAndSlash(
        Vector3 targetPosition,
        KeyCode usedKey
    )
    {
        isMoving = true;


        // ==========================================
        // 移動中は当たり判定OFF
        // ==========================================

        SetPlayerColliders(false);


        Vector3 startPosition =
            transform.position;

        Vector3 endPosition =
            targetPosition;


        // ==========================================
        // ダメージ計算
        // ==========================================

        float damage =
            CalculateDamage(
                startPosition,
                endPosition
            );


        // ==========================================
        // 攻撃
        // ==========================================

        DamageEnemiesOnLine(
            startPosition,
            endPosition,
            damage
        );


        // ==========================================
        // 演出
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
        // 移動終了
        // ==========================================

        SetPlayerColliders(true);

        isMoving = false;
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
            transform.position = end;

            yield break;
        }


        float timer = 0f;


        while (timer < duration)
        {
            timer +=
                Time.deltaTime;


            float t =
                Mathf.Clamp01(
                    timer / duration
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
    // Collider切り替え
    // ==================================================

    private void SetPlayerColliders(
        bool enabled
    )
    {
        if (playerColliders == null)
        {
            return;
        }


        foreach (
            Collider2D col
            in playerColliders
        )
        {
            if (col != null)
            {
                col.enabled =
                    enabled;
            }
        }
    }


    // ==================================================
    // 通常ダメージ計算
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


        // コンボ倍率
        if (ComboManager.Instance != null)
        {
            damage *=
                ComboManager.Instance
                    .GetDamageMultiplier();
        }


        return damage;
    }


    // ==================================================
    // 通常斬撃
    // ==================================================

    private void DamageEnemiesOnLine(
        Vector3 start,
        Vector3 end,
        float damage
    )
    {
        Vector2 direction =
            end - start;


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


        // 同じEnemyに複数Colliderがあっても
        // 1回だけダメージ
        HashSet<Enemy> damagedEnemies =
            new HashSet<Enemy>();


        // Enemyを継承していないMinion用
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


                enemy.TakeDamage(
                    damage
                );


                damagedEnemies.Add(
                    enemy
                );


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


                minion.TakeDamage(
                    damage
                );


                damagedMinions.Add(
                    minion
                );


                OnSuccessfulAttack();
            }
        }
    }


    // ==================================================
    // 攻撃成功
    // ==================================================

    private void OnSuccessfulAttack()
    {
        // ==========================================
        // コンボ
        // ==========================================

        if (ComboManager.Instance != null)
        {
            ComboManager.Instance
                .AddCombo();
        }


        // ==========================================
        // FLOW
        // ==========================================

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


        PlayMoveSE();
    }


    // ==================================================
    // 斬撃ライン
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


        slashLine.enabled = true;

        slashLine.positionCount = 2;


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


        slashLine.enabled = false;
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
    // 移動SE
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


    // ==================================================
    // 同じ位置か
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
    // 移動先に敵がいるか
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
    // 一番近い安全なキー位置
    // ==================================================

    private Transform FindNearestSafeMovePoint(
        Vector3 blockedPosition
    )
    {
        Transform nearestPoint =
            null;


        float nearestDistance =
            float.MaxValue;


        foreach (
            KeyMovePoint data
            in keyMovePoints
        )
        {
            if (data.movePoint == null)
            {
                continue;
            }


            Vector3 position =
                data.movePoint.position;


            // 敵がいる場所は除外
            if (IsEnemyAtPosition(position))
            {
                continue;
            }


            float distance =
                Vector2.Distance(
                    blockedPosition,
                    position
                );


            if (
                distance <
                nearestDistance
            )
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
    // 単語入力
    // ==================================================

    private void RegisterMoveKey(
        KeyCode key
    )
    {
        string keyString =
            key.ToString()
                .ToUpper();


        // A～Zのみ
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
        // 完成した単語
        // ==========================================

        foreach (
            WordCommand command
            in wordCommands
        )
        {
            if (!IsValidCommand(command))
            {
                continue;
            }


            string word =
                command.word.ToUpper();


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

        foreach (
            WordCommand command
            in wordCommands
        )
        {
            if (!IsValidCommand(command))
            {
                continue;
            }


            string word =
                command.word.ToUpper();


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
        // 最後の文字から新しい単語
        // ==========================================

        char lastCharacter =
            history[
                history.Length - 1
            ];


        foreach (
            WordCommand command
            in wordCommands
        )
        {
            if (!IsValidCommand(command))
            {
                continue;
            }


            string word =
                command.word.ToUpper();


            if (
                word[0] !=
                lastCharacter
            )
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
        // どの単語にも該当しない
        // ==========================================

        moveKeyHistory.Clear();


        if (WordCommandUI.Instance != null)
        {
            WordCommandUI.Instance
                .CancelCommand();
        }
    }


    // ==================================================
    // コマンド確認
    // ==================================================

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


        playerHealth.Heal(1);


        Debug.Log(
            "HEAL発動：HPを1回復"
        );
    }


    // ==================================================
    // 超必殺開始
    // FlowManagerから呼ばれる
    // ==================================================

    public void StartUltimateAttack()
    {
        if (isUltimatePlaying)
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


        // ==========================================
        // 超必殺中は当たり判定OFF
        // ==========================================

        SetPlayerColliders(
            false
        );


        // ==========================================
        // boss取得
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


        // ==========================================
        // Enemy取得
        // ==========================================

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
        // 連続高速攻撃
        // ==========================================

        yield return StartCoroutine(
            UltimateRushAttack(
                bossTarget,
                bossEnemy
            )
        );


        // 連撃中にボスが倒れた
        if (bossTarget == null)
        {
            FinishUltimate();

            yield break;
        }


        // ==========================================
        // ボス横へ移動
        // ==========================================

        yield return StartCoroutine(
            MoveBesideBoss(
                bossTarget
            )
        );


        if (bossTarget == null)
        {
            FinishUltimate();

            yield break;
        }


        // ==========================================
        // ビーム溜め
        // ==========================================

        if (beamChargeTime > 0f)
        {
            yield return new WaitForSeconds(
                beamChargeTime
            );
        }


        if (bossTarget == null)
        {
            FinishUltimate();

            yield break;
        }


        // ==========================================
        // ビーム発射
        // ==========================================

        FireUltimateBeam(
            bossTarget,
            bossEnemy
        );


        // ==========================================
        // ビーム表示中
        // ==========================================

        if (beamLifeTime > 0f)
        {
            yield return new WaitForSeconds(
                beamLifeTime
            );
        }


        FinishUltimate();
    }


    // ==================================================
    // bossからEnemy取得
    // ==================================================

    private Enemy GetBossEnemy()
    {
        if (bossTarget == null)
        {
            return null;
        }


        // 同じオブジェクト
        Enemy enemy =
            bossTarget
                .GetComponent<Enemy>();


        if (enemy != null)
        {
            return enemy;
        }


        // 親
        enemy =
            bossTarget
                .GetComponentInParent<Enemy>();


        if (enemy != null)
        {
            return enemy;
        }


        // 子
        enemy =
            bossTarget
                .GetComponentInChildren<Enemy>();


        return enemy;
    }


    // ==================================================
    // 超必殺：連続攻撃
    // ==================================================

    private IEnumerator UltimateRushAttack(
        boss targetBoss,
        Enemy bossEnemy
    )
    {
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


            // ======================================
            // ボス周辺の位置を決める
            // ======================================

            float angle =
                (360f /
                ultimateRushCount)
                *
                i;


            // 単純な円運動にならないようずらす
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
                targetBoss
                    .transform
                    .position
                +
                offset;


            Vector3 startPosition =
                transform.position;


            // ======================================
            // 高速移動
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


            // ======================================
            // 連撃ダメージ
            // ======================================

            bossEnemy.TakeDamage(
                ultimateRushDamage
            );


            // ======================================
            // 残像
            // ======================================

            ShowAfterImages(
                startPosition,
                targetPosition
            );


            // ======================================
            // 斬撃ライン
            // ======================================

            if (slashLine != null)
            {
                StartCoroutine(
                    ShowSlashLine(
                        startPosition,
                        targetPosition
                    )
                );
            }


            // ======================================
            // SE
            // ======================================

            PlayMoveSE();


            // ======================================
            // 次の攻撃
            // ======================================

            if (
                ultimateRushInterval >
                0f
            )
            {
                yield return new WaitForSeconds(
                    ultimateRushInterval
                );
            }
        }
    }


    // ==================================================
    // 超必殺：ボス横へ移動
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
            targetBoss
                .transform
                .position;


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


        // ==========================================
        // 近い方を選ぶ
        // ==========================================

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
    // 超必殺：ビーム
    // ==================================================

    private void FireUltimateBeam(
        boss targetBoss,
        Enemy bossEnemy
    )
    {
        if (
            targetBoss == null ||
            bossEnemy == null
        )
        {
            return;
        }


        Vector3 direction =
            targetBoss
                .transform
                .position
            -
            transform.position;


        direction.z =
            0f;


        if (
            direction.sqrMagnitude <=
            0.001f
        )
        {
            return;
        }


        // ==========================================
        // ビーム画像
        // ==========================================

        if (beamPrefab != null)
        {
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


            GameObject beam =
                Instantiate(
                    beamPrefab,
                    transform.position,
                    rotation
                );


            Destroy(
                beam,
                beamLifeTime
            );
        }


        // ==========================================
        // ビームダメージ
        // ==========================================

        bossEnemy.TakeDamage(
            beamDamage
        );
    }


    // ==================================================
    // 超必殺終了
    // ==================================================

    private void FinishUltimate()
    {
        // 当たり判定を戻す
        SetPlayerColliders(
            true
        );


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
    // 外部からダメージ
    // BossBulletなどから使用
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
    // Sceneビュー
    // ==================================================

    private void OnDrawGizmosSelected()
    {
        if (keyMovePoints == null)
        {
            return;
        }


        Gizmos.color =
            Color.red;


        foreach (
            KeyMovePoint data
            in keyMovePoints
        )
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