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
    private List<KeyMovePoint> keyMovePoints = new List<KeyMovePoint>();


    // ==================================================
    // 移動
    // ==================================================

    [Header("移動設定")]

    [SerializeField]
    private float moveDuration = 0.05f;

    public bool isMoving = false;


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
    // 通常残像
    // ==================================================

    [Header("残像")]

    [SerializeField]
    private GameObject afterImagePrefab;

    [SerializeField]
    private int afterImageCount = 4;

    [SerializeField]
    private float afterImageLifeTime = 0.2f;


    // ==================================================
    // フロー残像
    // ==================================================

    [Header("フロー中の残像")]

    [Tooltip("フロー中の残像をフロー終了まで残す")]
    [SerializeField]
    private bool keepAfterImageInFlow = true;

    private List<GameObject> flowAfterImages =
        new List<GameObject>();


    // ==================================================
    // SE
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
    // 超必殺
    // ==================================================

    [Header("超必殺・軌跡再演")]

    [Tooltip("軌跡再演時の1区間の移動時間")]
    [SerializeField]
    private float ultimateMoveDuration = 0.03f;

    [Tooltip("超必殺のダメージ倍率")]
    [SerializeField]
    private float ultimateDamageMultiplier = 2f;

    [Tooltip("軌跡再演時の区間ごとの待ち時間")]
    [SerializeField]
    private float ultimateStepWait = 0.02f;

    private List<Vector3> flowRoute =
        new List<Vector3>();

    private bool isUltimateReplay = false;


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

        // 超必殺再演中はプレイヤー入力禁止
        if (isUltimateReplay)
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


            // ------------------------------------------
            // 現在位置と同じキー
            // ------------------------------------------

            if (IsSamePosition(targetPosition))
            {
                // 移動
                // 攻撃
                // SE
                // コンボ
                // FLOW
                // 単語入力

                // 全て発生させない
                return;
            }


            // ------------------------------------------
            // 移動先に敵がいる
            // ------------------------------------------

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
                    Debug.LogWarning(
                        "安全な移動先がありません"
                    );

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

        Vector3 startPosition =
            transform.position;

        Vector3 endPosition =
            targetPosition;


        // ------------------------------------------
        // ダメージ
        // ------------------------------------------

        float damage =
            CalculateDamage(
                startPosition,
                endPosition
            );


        // ------------------------------------------
        // 攻撃
        // ------------------------------------------

        DamageEnemiesOnLine(
            startPosition,
            endPosition,
            damage
        );


        // ------------------------------------------
        // 演出
        // ------------------------------------------

        PlayMoveEffects(
            startPosition,
            endPosition
        );


        // ------------------------------------------
        // 移動
        // ------------------------------------------

        yield return StartCoroutine(
            MoveRoutine(
                startPosition,
                endPosition,
                moveDuration
            )
        );


        // ------------------------------------------
        // フロー軌跡記録
        // ------------------------------------------

        if (
            FlowManager.Instance != null &&
            FlowManager.Instance.isFlowMode &&
            !FlowManager.Instance.isUltimatePlaying
        )
        {
            RecordFlowPosition(
                endPosition
            );
        }


        // ------------------------------------------
        // 単語入力
        // ------------------------------------------

        RegisterMoveKey(
            usedKey
        );

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
            timer += Time.deltaTime;

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

        transform.position = end;
    }


    // ==================================================
    // ダメージ計算
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
            distance * distanceDamageRate;


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
        RaycastHit2D[] hits =
            GetSlashHits(
                start,
                end
            );

        foreach (RaycastHit2D hit in hits)
        {
            bool damaged =
                ApplyDamageToTarget(
                    hit.collider,
                    damage
                );

            if (damaged)
            {
                OnSuccessfulAttack();
            }
        }
    }


    // ==================================================
    // 超必殺斬撃
    // ==================================================

    private void DamageEnemiesOnLineUltimate(
        Vector3 start,
        Vector3 end,
        float damage
    )
    {
        RaycastHit2D[] hits =
            GetSlashHits(
                start,
                end
            );

        foreach (RaycastHit2D hit in hits)
        {
            // 超必殺では
            // コンボやフロー回数を増加させない

            ApplyDamageToTarget(
                hit.collider,
                damage
            );
        }
    }


    // ==================================================
    // 斬撃判定
    // ==================================================

    private RaycastHit2D[] GetSlashHits(
        Vector3 start,
        Vector3 end
    )
    {
        Vector2 direction =
            end - start;

        float distance =
            direction.magnitude;

        if (distance <= 0.01f)
        {
            return new RaycastHit2D[0];
        }

        return Physics2D.CircleCastAll(
            start,
            slashWidth,
            direction.normalized,
            distance,
            enemyLayer
        );
    }


    // ==================================================
    // 敵へダメージ
    // ==================================================

    private bool ApplyDamageToTarget(
        Collider2D target,
        float damage
    )
    {
        if (target == null)
        {
            return false;
        }


        // ------------------------------------------
        // 通常敵
        // ------------------------------------------

        Enemy enemy =
            target.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            return true;
        }


        // ------------------------------------------
        // ボス
        // ------------------------------------------

        BossController boss =
            target.GetComponent<BossController>();

        if (boss != null)
        {
            boss.TakeDamage(damage);
            return true;
        }


        // ------------------------------------------
        // ボスミニオン
        // ------------------------------------------

        BossMinion minion =
            target.GetComponent<BossMinion>();

        if (minion != null)
        {
            minion.TakeDamage(damage);
            return true;
        }

        return false;
    }


    // ==================================================
    // 攻撃成功
    // ==================================================

    private void OnSuccessfulAttack()
    {
        // ------------------------------------------
        // コンボ
        // ------------------------------------------

        if (ComboManager.Instance != null)
        {
            ComboManager.Instance
                .AddCombo();
        }


        // ------------------------------------------
        // フロー
        // ------------------------------------------

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

        bool isFlow =
            FlowManager.Instance != null &&
            FlowManager.Instance.isFlowMode;

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


            // ------------------------------------------
            // フロー中
            // ------------------------------------------

            if (
                isFlow &&
                keepAfterImageInFlow
            )
            {
                // 自動削除しない
                // フロー終了時にまとめて削除

                flowAfterImages.Add(
                    afterImage
                );
            }


            // ------------------------------------------
            // 通常時
            // ------------------------------------------

            else
            {
                Destroy(
                    afterImage,
                    afterImageLifeTime
                );
            }
        }
    }


    // ==================================================
    // フロー残像削除
    // ==================================================

    public void ClearFlowAfterImages()
    {
        foreach (
            GameObject afterImage
            in flowAfterImages
        )
        {
            if (afterImage != null)
            {
                Destroy(afterImage);
            }
        }

        flowAfterImages.Clear();
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
    // 現在位置と同じか
    // ==================================================

    private bool IsSamePosition(
        Vector3 position
    )
    {
        return Vector2.Distance(
            transform.position,
            position
        ) < 0.01f;
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

        return hit != null;
    }


    // ==================================================
    // 一番近い安全な移動先
    // ==================================================

    private Transform FindNearestSafeMovePoint(
        Vector3 blockedPosition
    )
    {
        Transform nearestPoint = null;

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
    // 単語コマンド：キー登録
    // ==================================================

    private void RegisterMoveKey(
        KeyCode key
    )
    {
        string keyString =
            key.ToString()
                .ToUpper();

        // A～Zだけ
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
    // 単語コマンド判定
    // ==================================================

    private void CheckWordCommands()
    {
        if (moveKeyHistory.Length == 0)
        {
            return;
        }

        string history =
            moveKeyHistory.ToString();


        // ------------------------------------------
        // 完成
        // ------------------------------------------

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
                if (
                    WordCommandUI.Instance
                    != null
                )
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


        // ------------------------------------------
        // 入力途中
        // ------------------------------------------

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
                if (
                    WordCommandUI.Instance
                    != null
                )
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


        // ------------------------------------------
        // 最後の文字から新しい単語を探す
        // ------------------------------------------

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

            if (word[0] != lastCharacter)
            {
                continue;
            }

            moveKeyHistory.Clear();

            moveKeyHistory.Append(
                lastCharacter
            );

            if (
                WordCommandUI.Instance
                != null
            )
            {
                WordCommandUI.Instance
                    .StartCommand(
                        word
                    );
            }

            return;
        }


        // ------------------------------------------
        // どれにも該当しない
        // ------------------------------------------

        moveKeyHistory.Clear();

        if (
            WordCommandUI.Instance
            != null
        )
        {
            WordCommandUI.Instance
                .CancelCommand();
        }
    }


    // ==================================================
    // コマンドが有効か
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
    // フロー軌跡記録開始
    // ==================================================

    public void StartFlowRouteRecording()
    {
        // 前回の残像を念のため削除
        ClearFlowAfterImages();

        flowRoute.Clear();

        // フロー開始時の位置
        flowRoute.Add(
            transform.position
        );

        Debug.Log(
            "フロー軌跡記録開始"
        );
    }


    // ==================================================
    // フロー移動位置を保存
    // ==================================================

    private void RecordFlowPosition(
        Vector3 position
    )
    {
        if (flowRoute.Count > 0)
        {
            Vector3 previous =
                flowRoute[
                    flowRoute.Count - 1
                ];

            if (
                Vector2.Distance(
                    previous,
                    position
                ) < 0.01f
            )
            {
                return;
            }
        }

        flowRoute.Add(
            position
        );
    }


    // ==================================================
    // 超必殺開始
    // ==================================================

    public void StartUltimateReplay()
    {
        if (isUltimateReplay)
        {
            return;
        }

        StartCoroutine(
            UltimateReplayRoutine()
        );
    }


    // ==================================================
    // 超必殺：軌跡再演
    // ==================================================

    private IEnumerator UltimateReplayRoutine()
    {
        if (flowRoute.Count < 2)
        {
            Debug.Log(
                "超必殺：軌跡がありません"
            );

            if (FlowManager.Instance != null)
            {
                FlowManager.Instance
                    .UltimateFinished();
            }

            yield break;
        }

        isUltimateReplay = true;
        isMoving = true;


        // ------------------------------------------
        // 最初の記録地点へ戻る
        // ------------------------------------------

        transform.position =
            flowRoute[0];


        // ------------------------------------------
        // 記録した軌跡を再生
        // ------------------------------------------

        for (
            int i = 1;
            i < flowRoute.Count;
            i++
        )
        {
            Vector3 start =
                transform.position;

            Vector3 end =
                flowRoute[i];


            // --------------------------------------
            // ダメージ
            // --------------------------------------

            float damage =
                CalculateDamage(
                    start,
                    end
                );

            damage *=
                ultimateDamageMultiplier;


            // --------------------------------------
            // 超必殺斬撃
            // --------------------------------------

            DamageEnemiesOnLineUltimate(
                start,
                end,
                damage
            );


            // --------------------------------------
            // 演出
            // --------------------------------------

            PlayMoveEffects(
                start,
                end
            );


            // --------------------------------------
            // 高速移動
            // --------------------------------------

            yield return StartCoroutine(
                MoveRoutine(
                    start,
                    end,
                    ultimateMoveDuration
                )
            );


            // --------------------------------------
            // 区間待機
            // --------------------------------------

            if (ultimateStepWait > 0f)
            {
                yield return new WaitForSeconds(
                    ultimateStepWait
                );
            }
        }


        isMoving = false;
        isUltimateReplay = false;


        // ------------------------------------------
        // 軌跡と残像を削除
        // ------------------------------------------

        ClearFlowRoute();


        // ------------------------------------------
        // フロー終了通知
        // ------------------------------------------

        if (FlowManager.Instance != null)
        {
            FlowManager.Instance
                .UltimateFinished();
        }
    }


    // ==================================================
    // フロー軌跡削除
    // ==================================================

    public void ClearFlowRoute()
    {
        flowRoute.Clear();

        ClearFlowAfterImages();
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