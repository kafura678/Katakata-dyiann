using UnityEngine;

public class bossDensityAttackPhase : bossSpecialAttackPhase
{
    [SerializeField] Transform bossTf;
    [SerializeField] Transform centerTf;
    [SerializeField] bossSequentialWaveBulletPattern bulletPattern;
    [SerializeField] float bulletFireInterval = 0.1f;

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
        keepBossAtCenter();
    }

    protected override void onUpdatePhase(float deltaTime)
    {
        keepBossAtCenter();

        if (bulletPattern != null && fireTimer.tryFire(deltaTime))
        {
            bulletPattern.fire(bossTf);
        }
    }

    protected override void onExit()
    {
        fireTimer?.reset();
        bulletPattern?.reset();
    }

    void ensureFireTimer()
    {
        if (fireTimer == null)
        {
            fireTimer = new bossFireTimer(bulletFireInterval);
        }
    }

    void keepBossAtCenter()
    {
        if (bossTf != null && centerTf != null)
        {
            bossTf.position = centerTf.position;
        }
    }
}
