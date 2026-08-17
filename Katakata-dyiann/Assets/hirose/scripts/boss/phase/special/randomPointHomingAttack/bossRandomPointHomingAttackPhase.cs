using UnityEngine;

public class bossRandomPointHomingAttackPhase : bossSpecialAttackPhase
{
    enum sequenceState
    {
        moving,
        firing
    }

    [SerializeField] Transform bossTf;
    [SerializeField] Transform playerTf;
    [SerializeField] bossRandomMoveTargetSelector moveTargetSelector;
    [SerializeField] bossTargetPointMove targetPointMove;
    [SerializeField] homingBulletFireController fireController;
    [SerializeField] float fireInterval = 0.25f;

    bossFireTimer fireTimer;
    sequenceState currentState;

    protected override void Awake()
    {
        base.Awake();
        fireTimer = new bossFireTimer(fireInterval);
    }

    protected override void onEnter()
    {
        ensureFireTimer();
        fireTimer.reset();
        beginInitialMove();
    }

    protected override void onUpdatePhase(float deltaTime)
    {
        switch (currentState)
        {
            case sequenceState.moving:
                updateMoving(deltaTime);
                break;
            case sequenceState.firing:
                updateFiring(deltaTime);
                break;
        }
    }

    protected override void onExit()
    {
        targetPointMove?.stop();
        fireTimer?.reset();
        currentState = sequenceState.moving;
    }

    void updateMoving(float deltaTime)
    {
        if (targetPointMove == null)
        {
            return;
        }

        targetPointMove.move(deltaTime);
        if (targetPointMove.isCompleted)
        {
            beginFiring();
        }
    }

    void updateFiring(float deltaTime)
    {
        if (!fireTimer.tryFire(deltaTime))
        {
            return;
        }

        if (fireController != null && bossTf != null && playerTf != null)
        {
            fireController.homingBulletFire(bossTf, playerTf);
        }
    }

    void beginInitialMove()
    {
        Transform selectedTargetTf = moveTargetSelector?.select();
        if (selectedTargetTf == null || targetPointMove == null || !targetPointMove.begin(selectedTargetTf))
        {
            targetPointMove?.stop();
            currentState = sequenceState.moving;
            return;
        }

        currentState = sequenceState.moving;
    }

    void beginFiring()
    {
        fireTimer.reset();
        currentState = sequenceState.firing;
    }

    void ensureFireTimer()
    {
        if (fireTimer == null)
        {
            fireTimer = new bossFireTimer(fireInterval);
        }
    }
}
