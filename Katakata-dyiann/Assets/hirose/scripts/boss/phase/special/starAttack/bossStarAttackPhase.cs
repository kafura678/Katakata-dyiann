using UnityEngine;

public class bossStarAttackPhase : bossSpecialAttackPhase
{
    [SerializeField] bossTransformPathMove pathMove;
    [SerializeField] bossTargetTfsRotation targetTfsRotation;
    [SerializeField] bossSimultaneousBulletPattern bulletPattern;
    [SerializeField] Transform bossTf;
    [SerializeField] float bulletFireInterval = 1f;

    bossFireTimer fireTimer;

    protected override void Awake()
    {
        base.Awake();
        fireTimer = new bossFireTimer(bulletFireInterval);
    }

    protected override void onEnter()
    {
        ensureFireTimer();
        fireTimer.reset();
        bulletPattern?.reset();
        targetTfsRotation?.begin();
        pathMove?.begin();
    }

    protected override void onUpdatePhase(float deltaTime)
    {
        pathMove?.move(deltaTime);

        if (bulletPattern != null && fireTimer.tryFire(deltaTime))
        {
            bulletPattern.fire(bossTf);
        }
    }

    protected override void onExit()
    {
        pathMove?.stop();
        targetTfsRotation?.stop();
        bulletPattern?.reset();
        fireTimer?.reset();
    }

    void ensureFireTimer()
    {
        if (fireTimer == null)
        {
            fireTimer = new bossFireTimer(bulletFireInterval);
        }
    }
}
