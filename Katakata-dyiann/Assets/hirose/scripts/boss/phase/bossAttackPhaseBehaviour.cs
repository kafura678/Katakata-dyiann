using UnityEngine;

public abstract class bossAttackPhaseBehaviour : MonoBehaviour, bossAttackPhase
{
    [SerializeField] float duration = bossAttackPhaseTimer.defaultDuration;

    bossAttackPhaseTimer phaseTimer;
    bool isRunning;
    bool isPaused;

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
        isPaused = false;
        onEnter();
    }

    public void updatePhase(float deltaTime)
    {
        if (!isRunning || isPaused || deltaTime <= 0f)
        {
            return;
        }

        onUpdatePhase(deltaTime);
        phaseTimer.tick(deltaTime);
    }

    public void pause()
    {
        if (!isRunning || isPaused)
        {
            return;
        }

        isPaused = true;
        onPause();
    }

    public void resume()
    {
        if (!isRunning || !isPaused)
        {
            return;
        }

        isPaused = false;
        onResume();
    }

    public void exit()
    {
        if (!isRunning)
        {
            return;
        }

        onExit();
        isRunning = false;
        isPaused = false;
        phaseTimer.reset();
    }

    protected abstract void onEnter();
    protected abstract void onUpdatePhase(float deltaTime);
    protected abstract void onExit();
    protected virtual void onPause() { }
    protected virtual void onResume() { }

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
