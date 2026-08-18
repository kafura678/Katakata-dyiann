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
    // 入力
    // ==================================================

    [Header("入力")]

    [Tooltip("falseの間はプレイヤー操作不可")]
    [SerializeField]
    private bool inputEnabled = true;

    public bool InputEnabled => inputEnabled;


    // ==================================================
    // 移動
    // ==================================================

    [Header("移動設定")]

    [SerializeField]
    private float moveDuration = 0.05f;

    public bool isMoving = false;


    // ==================================================
    // Collider
    // ==================================================

    [Header("プレイヤー当たり判定")]

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
    // 通常攻撃
    // ==================================================

    [Header("通常攻撃")]

    [SerializeField]
    private LayerMask enemyLayer;

    [SerializeField]
    private float slashWidth = 0.4f;

    [SerializeField]
    private float baseDamage = 1f;

    [SerializeField]
    private float distanceDamageRate = 0.5f;


    // ==================================================
    // ランダムダメージ
    // ==================================================

    [Header("ランダムダメージ")]

    [SerializeField]
    private float minimumRandomDamageRate = 0.9f;

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

    [SerializeField]
    private GameObject slashEffectPrefab;

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
    // SE
    // ==================================================

    [Header("SE共通")]

    [SerializeField]
    private AudioSource audioSource;


    [Header("移動SE")]

    [SerializeField]
    private AudioClip moveSE;

    [SerializeField, Range(0f, 1f)]
    private float moveSEVolume = 1f;


    [Header("斬撃SE")]

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

    private readonly StringBuilder moveKeyHistory =
        new StringBuilder();


    // ==================================================
    // 超必殺・対象
    // ==================================================

    [Header("超必殺・対象ボス")]

    [SerializeField]
    private boss bossTarget;


    // ==================================================
    // 超必殺・連撃
    // ==================================================

    [Header("超必殺・連続攻撃")]

    [SerializeField]
    private int ultimateRushCount = 8;

    [SerializeField]
    private float ultimateRushDamage = 4f;

    [SerializeField]
    private float ultimateRushMoveDuration = 0.04f;

    [SerializeField]
    private float ultimateRushRadius = 1.2f;

    [SerializeField]
    private float ultimateRushInterval = 0.03f;


    // ==================================================
    // 超必殺・ビーム
    // ==================================================

    [Header("超必殺・ビーム")]

    [SerializeField]
    private GameObject beamPrefab;

    [SerializeField]
    private float beamDamage = 5f;

    [SerializeField]
    private float beamChargeTime = 0.25f;

    [SerializeField]
    private float beamLifeTime = 1f;

    [SerializeField]
    private float beamDamageInterval = 0.1f;

    [Tooltip("敵の左右からどれだけ離れるか")]
    [SerializeField]
    private float beamSideDistance = 2f;

    [SerializeField]
    private float beamPositionMoveDuration = 0.08f;

    [SerializeField]
    private float beamAngleOffset = 0f;


    // ==================================================
    // ビーム出現位置
    // ==================================================

    [Header("ビーム出現位置")]

    [SerializeField]
    private Vector2 beamSpawnOffset =
        Vector2.zero;


    // ==================================================
    // ビーム描画順
    // ==================================================

    [Header("ビーム描画順")]

    [SerializeField]
    private int beamOrderInLayer = 20;


    // ==================================================
    // ビーム終了アニメーション
    // ==================================================

    [Header("ビーム終了アニメーション")]

    [SerializeField]
    private Sprite[] beamEndSprites;

    [SerializeField]
    private float beamEndFrameInterval = 0.05f;


    // ==================================================
    // ビームSE
    // ==================================================

    [Header("ビーム発射SE")]

    [SerializeField]
    private AudioClip beamSE;

    [SerializeField, Range(0f, 1f)]
    private float beamSEVolume = 1f;


    [Header("ビームヒットSE")]

    [SerializeField]
    private AudioClip beamHitSE;

    [SerializeField, Range(0f, 1f)]
    private float beamHitSEVolume = 1f;


    // ==================================================
    // 状態
    // ==================================================

    private bool isUltimatePlaying = false;

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
        // ==========================================
        // 死亡中
        // ==========================================

        if (
            playerHealth != null &&
            playerHealth.IsDead
        )
        {
            return;
        }


        // ==========================================
        // MISSION演出などで操作禁止
        // ==========================================

        if (!inputEnabled)
        {
            return;
        }


        // ==========================================
        // 超必殺中
        // ==========================================

        if (isUltimatePlaying)
        {
            return;
        }


        // ==========================================
        // 移動中
        // ==========================================

        if (isMoving)
        {
            return;
        }


        HandleInput();
    }


    // ==================================================
    // 入力ON / OFF
    // ==================================================

    public void SetInputEnabled(
        bool enabled
    )
    {
        inputEnabled =
            enabled;
    }


    // ==================================================
    // キー入力
    // ==================================================

    private void HandleInput()
    {
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
            // 移動先に敵
            // ==========================================

            if (IsEnemyAtPosition(targetPosition))
            {
                HandleBlockedMove(
                    targetPosition
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


            return;
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


        DamageEnemiesOnLine(
            startPosition,
            endPosition,
            damage
        );


        PlayMoveEffects(
            startPosition,
            endPosition
        );


        yield return StartCoroutine(
            MoveRoutine(
                startPosition,
                endPosition,
                moveDuration
            )
        );


        RegisterMoveKey(
            usedKey
        );


        RestorePlayerColliders();


        isMoving =
            false;
    }


    // ==================================================
    // MISSION開始時の登場移動
    //
    // 通常キー移動と同じ見た目
    // 攻撃・Combo・FLOW・単語入力なし
    // ==================================================

    public IEnumerator MoveForMissionIntro(
        Vector3 targetPosition
    )
    {
        yield return StartCoroutine(
            MoveForMissionIntro(
                targetPosition,
                moveDuration
            )
        );
    }


    public IEnumerator MoveForMissionIntro(
        Vector3 targetPosition,
        float duration
    )
    {
        if (
            playerHealth != null &&
            playerHealth.IsDead
        )
        {
            yield break;
        }


        isMoving =
            true;


        SetPlayerColliders(
            false
        );


        Vector3 startPosition =
            transform.position;


        PlayMoveEffects(
            startPosition,
            targetPosition
        );


        float actualDuration =
            duration > 0f
                ? duration
                : moveDuration;


        yield return StartCoroutine(
            MoveRoutine(
                startPosition,
                targetPosition,
                actualDuration
            )
        );


        RestorePlayerColliders();


        isMoving =
            false;
    }


    // ==================================================
    // 敵に移動先を塞がれた
    // ==================================================

    private void HandleBlockedMove(
        Vector3 blockedPosition
    )
    {
        TakeDamage(
            damageOnBlockedMove
        );


        if (
            playerHealth != null &&
            playerHealth.IsDead
        )
        {
            return;
        }


        Transform safePoint =
            FindNearestSafeMovePoint(
                blockedPosition
            );


        if (safePoint == null)
        {
            return;
        }


        Vector3 safePosition =
            safePoint.position;


        if (IsSamePosition(safePosition))
        {
            return;
        }


        StartCoroutine(
            MoveToSafePoint(
                safePosition
            )
        );
    }


    // ==================================================
    // 安全地点へ退避
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
        // 攻撃なし
        // 軌跡なし
        // Comboなし
        // FLOWなし
        // 単語入力なし
        // ==========================================

        ShowAfterImages(
            startPosition,
            targetPosition
        );


        PlayMoveSE();


        yield return StartCoroutine(
            MoveRoutine(
                startPosition,
                targetPosition,
                moveDuration
            )
        );


        RestorePlayerColliders();


        isMoving =
            false;
    }


    // ==================================================
    // 最寄りの安全地点
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


    private void RestorePlayerColliders()
    {
        if (
            playerHealth != null &&
            playerHealth.IsDead
        )
        {
            return;
        }


        SetPlayerColliders(
            true
        );
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
            distance *
            distanceDamageRate;


        if (ComboManager.Instance != null)
        {
            damage *=
                ComboManager.Instance
                    .GetDamageMultiplier();
        }


        return
            FinalizeDamage(
                damage
            );
    }


    // ==================================================
    // 最終ダメージ
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


        damage *=
            Random.Range(
                minimum,
                maximum
            );


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
    // 移動経路上の敵を攻撃
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


        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
            {
                continue;
            }


            Enemy enemy =
                hit.collider
                    .GetComponentInParent<Enemy>();


            if (enemy == null)
            {
                continue;
            }


            if (damagedEnemies.Contains(enemy))
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


            SpawnSlashEffect(
                hitPosition,
                direction
            );


            PlaySlashSE();


            OnSuccessfulAttack();
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


        Instantiate(
            slashEffectPrefab,
            spawnPosition,
            Quaternion.Euler(
                0f,
                0f,
                angle
            )
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


        int count =
            Mathf.Max(
                0,
                afterImageCount
            );


        for (
            int i = 1;
            i <= count;
            i++
        )
        {
            float t =
                (float)i /
                (count + 1);


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
    // 敵存在判定
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

        foreach (WordCommand command in wordCommands)
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
        // 最後の文字から別コマンド開始
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
                command.word.ToUpper();


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
    }


    // ==================================================
    // 超必殺開始
    // ==================================================

    public void StartUltimateAttack()
    {
        if (!inputEnabled)
        {
            return;
        }


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
        // MISSION開始後に本物の敵が出るため
        // 超必殺時に再検索
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
                "ボスからEnemyが見つかりません"
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
        // 敵の画面位置に応じた側へ移動
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
    // bossからEnemy取得
    // ==================================================

    private Enemy GetBossEnemy()
    {
        if (bossTarget == null)
        {
            return null;
        }


        Enemy enemy =
            bossTarget.GetComponent<Enemy>();


        if (enemy != null)
        {
            return enemy;
        }


        enemy =
            bossTarget.GetComponentInParent<Enemy>();


        if (enemy != null)
        {
            return enemy;
        }


        return
            bossTarget.GetComponentInChildren<Enemy>();
    }


    // ==================================================
    // 超必殺・高速連撃
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


            bossEnemy.TakeDamage(
                FinalizeDamage(
                    ultimateRushDamage
                )
            );


            SpawnSlashEffect(
                hitPosition,
                attackDirection
            );


            PlaySlashSE();


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
    // 敵が画面左半分にいるか
    // ==================================================

    private bool IsBossOnLeftHalf(
        boss targetBoss
    )
    {
        if (targetBoss == null)
        {
            return false;
        }


        Camera mainCamera =
            Camera.main;


        // ==========================================
        // MainCameraが無い場合
        // ワールドX=0を仮の中央として使用
        // ==========================================

        if (mainCamera == null)
        {
            return
                targetBoss.transform.position.x <
                0f;
        }


        Vector3 viewportPosition =
            mainCamera.WorldToViewportPoint(
                targetBoss.transform.position
            );


        // ==========================================
        // Viewport
        //
        // 0.0 = 左端
        // 0.5 = 中央
        // 1.0 = 右端
        // ==========================================

        return
            viewportPosition.x <
            0.5f;
    }


    // ==================================================
    // ビーム発射位置へ移動
    //
    // 敵が画面右半分
    // → 敵の左側
    //
    // 敵が画面左半分
    // → 敵の右側
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


        bool bossIsOnLeftSide =
            IsBossOnLeftHalf(
                targetBoss
            );


        Vector3 targetPosition;


        if (bossIsOnLeftSide)
        {
            // ======================================
            // 敵が画面左
            // → プレイヤーは敵の右へ
            // ======================================

            targetPosition =
                bossPosition +
                Vector3.right *
                beamSideDistance;
        }
        else
        {
            // ======================================
            // 敵が画面右
            // → プレイヤーは敵の左へ
            // ======================================

            targetPosition =
                bossPosition +
                Vector3.left *
                beamSideDistance;
        }


        Vector3 startPosition =
            transform.position;


        ShowAfterImages(
            startPosition,
            targetPosition
        );


        PlayMoveSE();


        yield return StartCoroutine(
            MoveRoutine(
                startPosition,
                targetPosition,
                beamPositionMoveDuration
            )
        );
    }


    // ==================================================
    // 超必殺・ビーム
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


        // ==========================================
        // プレイヤー → 敵
        // ==========================================

        Vector3 bossDirection =
            targetBoss.transform.position -
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
        // ビーム生成位置
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
            targetBoss.transform.position -
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
        // ビーム角度
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
        // 左向きか
        //
        // true
        // → プレイヤーが敵の右側
        // → 左へ発射
        // ==========================================

        bool shootToLeft =
            direction.x <
            0f;


        // ==========================================
        // ビーム生成
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


            SpriteRenderer[] renderers =
                beam.GetComponentsInChildren<SpriteRenderer>(
                    true
                );


            foreach (
                SpriteRenderer renderer
                in renderers
            )
            {
                // ==================================
                // 描画順
                // ==================================

                renderer.sortingOrder =
                    beamOrderInLayer;


                // ==================================
                // 左向きビームの画像反転
                //
                // 右向き画像を180度回転すると
                // 上下も反転して見える場合があるため
                // flipYで補正
                // ==================================

                renderer.flipY =
                    shootToLeft;
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
        // 発射直後に1ヒット
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
        // 継続ダメージ
        // ==========================================

        while (
            lifeTimer <
            beamLifeTime
        )
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
    // ビームダメージ
    // ==================================================

    private void DealBeamDamage(
        Enemy bossEnemy
    )
    {
        if (bossEnemy == null)
        {
            return;
        }


        bossEnemy.TakeDamage(
            FinalizeDamage(
                beamDamage
            )
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


        if (beamRenderer == null)
        {
            Destroy(
                beam
            );


            yield break;
        }


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


        foreach (
            Sprite sprite
            in beamEndSprites
        )
        {
            if (beam == null)
            {
                yield break;
            }


            if (sprite == null)
            {
                continue;
            }


            beamRenderer.sprite =
                sprite;


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
        RestorePlayerColliders();


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