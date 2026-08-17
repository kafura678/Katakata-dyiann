using UnityEngine;

public abstract class bossAttackPhaseBehaviour : MonoBehaviour, bossAttackPhase
{
    [SerializeField] float duration = bossAttackPhaseTimer.defaultDuration;

    bossAttackPhaseTimer phaseTimer;
    bool isRunning;

    public bool isCompleted => isRunning && phaseTimer != null && phaseTimer.isCompleted;
    public bool isAvailable => isActiveAndEnabled;

    protected virtual void Awake()
    {
        if (duration <= 0f)
        {
            duration = bossAttackPhaseTimer.defaultDuration;
        }

        phaseTimer = new bossAttackPhaseTimer(duration);
    }

    public void enter()
    {
        ensureTimer();
        phaseTimer.reset();
        isRunning = true;
        onEnter();
    }

    public void updatePhase(float deltaTime)
    {
        if (!isRunning || deltaTime <= 0f)
        {
            return;
        }

        onUpdatePhase(deltaTime);
        phaseTimer.tick(deltaTime);
    }

    public void exit()
    {
        if (!isRunning)
        {
            return;
        }

        onExit();
        isRunning = false;
        phaseTimer.reset();
    }

    protected abstract void onEnter();
    protected abstract void onUpdatePhase(float deltaTime);
    protected abstract void onExit();

    void ensureTimer()
    {
        if (phaseTimer == null)
        {
            float normalizedDuration = duration > 0f
                ? duration
                : bossAttackPhaseTimer.defaultDuration;
            phaseTimer = new bossAttackPhaseTimer(normalizedDuration);
        }
    }
}
