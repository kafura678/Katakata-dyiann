using UnityEngine;

public class bossNormalAttackPhase : bossAttackPhaseBehaviour
{
    [SerializeField] Transform bossTf;
    [SerializeField] Transform moveCenterTf;
    [SerializeField] float moveRadius = 1f;
    [SerializeField] int bulletFireCount = 1;
    [SerializeField] float bulletFireInterval = 1f;
    [SerializeField] bossFireController fireController;

    bossNormalMove normalMove;
    bossFireTimer fireTimer;

    protected override void Awake()
    {
        base.Awake();
        moveRadius = Mathf.Max(0.1f, moveRadius);
        bulletFireCount = Mathf.Max(1, bulletFireCount);
        normalMove = new bossNormalMove();
        fireTimer = new bossFireTimer(bulletFireInterval);
    }

    protected override void onEnter()
    {
        ensureServices();
        fireTimer.reset();
        moveBoss();
    }

    protected override void onUpdatePhase(float deltaTime)
    {
        moveBoss();

        if (fireController != null && bossTf != null && fireTimer.tryFire(deltaTime))
        {
            fireController.bulletsFire(bossTf, bulletFireCount);
        }
    }

    protected override void onExit()
    {
        fireTimer?.reset();
    }

    void moveBoss()
    {
        if (bossTf == null || moveCenterTf == null)
        {
            return;
        }

        bossTf.position = normalMove.movePosGet(moveCenterTf.position, moveRadius);
    }

    void ensureServices()
    {
        if (normalMove == null)
        {
            normalMove = new bossNormalMove();
        }

        if (fireTimer == null)
        {
            fireTimer = new bossFireTimer(bulletFireInterval);
        }
    }
}
