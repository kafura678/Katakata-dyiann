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
    // キー移動
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
    // 移動設定
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
    // 斬撃設定
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
    private List<WordCommand> wordCommands = new List<WordCommand>();

    private StringBuilder moveKeyHistory = new StringBuilder();


    // ==================================================
    // 他スクリプト
    // ==================================================

    private PlayerHealth playerHealth;


    // ==================================================
    // Unity
    // ==================================================

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (playerHealth != null && playerHealth.IsDead)
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
        // 移動中は入力しない
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
                    data.key + " の MovePoint が設定されていません"
                );

                continue;
            }

            Vector3 targetPosition = data.movePoint.position;

            // ==============================================
            // 現在位置と同じキー
            // ==============================================

            if (IsSamePosition(targetPosition))
            {
                // 移動なし
                // 攻撃なし
                // コマンド入力にも追加しない
                return;
            }

            // ==============================================
            // 移動先に敵がいる
            // ==============================================

            if (IsEnemyAtPosition(targetPosition))
            {
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(
                        damageOnBlockedMove
                    );
                }

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

                // 安全地点が現在位置なら移動しない
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

            // 1フレーム1入力
            break;
        }
    }


    // ==================================================
    // 移動 + 斬撃
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

        // ==============================================
        // ダメージ計算
        // ==============================================

        float distance =
            Vector2.Distance(
                startPosition,
                endPosition
            );

        float damage =
            baseDamage +
            distance * distanceDamageRate;

        damage =
            GetComboDamage(damage);


        // ==============================================
        // 攻撃判定
        // ==============================================

        DamageEnemiesOnLine(
            startPosition,
            endPosition,
            damage
        );


        // ==============================================
        // 演出
        // ==============================================

        ShowAfterImages(
            startPosition,
            endPosition
        );

        if (slashLine != null)
        {
            StartCoroutine(
                ShowSlashLine(
                    startPosition,
                    endPosition
                )
            );
        }

        PlayMoveSE();


        // ==============================================
        // 移動
        // ==============================================

        float timer = 0f;

        while (timer < moveDuration)
        {
            timer += Time.deltaTime;

            float t =
                timer / moveDuration;

            transform.position =
                Vector3.Lerp(
                    startPosition,
                    endPosition,
                    t
                );

            yield return null;
        }

        transform.position =
            endPosition;


        // ==============================================
        // コマンド入力
        // ==============================================

        RegisterMoveKey(usedKey);

        isMoving = false;
    }


    // ==================================================
    // 斬撃
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

        foreach (RaycastHit2D hit in hits)
        {
            // ==============================================
            // 通常敵
            // ==============================================

            Enemy enemy =
                hit.collider.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);

                OnSuccessfulAttack();

                continue;
            }


            // ==============================================
            // ボス
            // ==============================================

            BossController boss =
                hit.collider.GetComponent<BossController>();

            if (boss != null)
            {
                boss.TakeDamage(damage);

                OnSuccessfulAttack();

                continue;
            }


            // ==============================================
            // ボス小型敵
            // ==============================================

            BossMinion minion =
                hit.collider.GetComponent<BossMinion>();

            if (minion != null)
            {
                minion.TakeDamage(damage);

                OnSuccessfulAttack();
            }
        }
    }


    // ==================================================
    // 攻撃成功
    // ==================================================

    private void OnSuccessfulAttack()
    {
        // ==============================================
        // コンボ
        // ==============================================

        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.AddCombo();
        }


        // ==============================================
        // フロー
        // ==============================================

        if (FlowManager.Instance != null)
        {
            if (FlowManager.Instance.isFlowMode)
            {
                FlowManager.Instance.CountFlowAttack();
            }
            else
            {
                FlowManager.Instance.AddFlowGauge(
                    FlowManager.Instance.flowGainPerHit
                );
            }
        }
    }


    // ==================================================
    // コンボダメージ
    // ==================================================

    private float GetComboDamage(float damage)
    {
        if (ComboManager.Instance == null)
        {
            return damage;
        }

        return damage *
            ComboManager.Instance.GetDamageMultiplier();
    }


    // ==================================================
    // 同じ位置か確認
    // ==================================================

    private bool IsSamePosition(
        Vector3 targetPosition
    )
    {
        return Vector2.Distance(
            transform.position,
            targetPosition
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
    // 一番近い安全なキーを取得
    // ==================================================

    private Transform FindNearestSafeMovePoint(
        Vector3 blockedPosition
    )
    {
        Transform nearestPoint = null;

        float nearestDistance =
            float.MaxValue;

        foreach (KeyMovePoint data in keyMovePoints)
        {
            if (data.movePoint == null)
            {
                continue;
            }

            Vector3 pointPosition =
                data.movePoint.position;

            if (IsEnemyAtPosition(pointPosition))
            {
                continue;
            }

            float distance =
                Vector2.Distance(
                    blockedPosition,
                    pointPosition
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
    // 移動したキーを記録
    // ==================================================

    private void RegisterMoveKey(
        KeyCode key
    )
    {
        string keyString =
            key.ToString().ToUpper();

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
    // コマンド判定
    // ==================================================

    private void CheckWordCommands()
    {
        if (moveKeyHistory.Length == 0)
        {
            return;
        }

        string history =
            moveKeyHistory.ToString();

        WordCommand matchingCommand = null;


        // ==============================================
        // 完成・入力途中を探す
        // ==============================================

        foreach (WordCommand command in wordCommands)
        {
            if (
                command == null ||
                string.IsNullOrEmpty(command.word)
            )
            {
                continue;
            }

            string word =
                command.word.ToUpper();


            // ==========================================
            // コマンド完成
            // ==========================================

            if (history == word)
            {
                if (WordCommandUI.Instance != null)
                {
                    WordCommandUI.Instance.CompleteCommand(
                        word
                    );
                }

                ExecuteCommand(command);

                moveKeyHistory.Clear();

                return;
            }


            // ==========================================
            // まだ入力途中
            // ==========================================

            if (word.StartsWith(history))
            {
                matchingCommand = command;

                break;
            }
        }


        // ==============================================
        // 入力途中のコマンドがある
        // ==============================================

        if (matchingCommand != null)
        {
            if (WordCommandUI.Instance != null)
            {
                WordCommandUI.Instance.UpdateProgress(
                    matchingCommand.word.ToUpper(),
                    history.Length
                );
            }

            return;
        }


        // ==============================================
        // 現在の文字列では成立しない
        // 最後の文字から新しい単語を探す
        // ==============================================

        char lastCharacter =
            history[history.Length - 1];

        foreach (WordCommand command in wordCommands)
        {
            if (
                command == null ||
                string.IsNullOrEmpty(command.word)
            )
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
                WordCommandUI.Instance.StartCommand(
                    word
                );
            }

            return;
        }


        // ==============================================
        // どの単語にも当てはまらない
        // ==============================================

        moveKeyHistory.Clear();

        if (WordCommandUI.Instance != null)
        {
            WordCommandUI.Instance.CancelCommand();
        }
    }


    // ==================================================
    // コマンド効果
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
    // 外部からのダメージ
    // BossBulletなどから使用
    // ==================================================

    public void TakeDamage(int damage)
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
    // Sceneビューのデバッグ表示
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