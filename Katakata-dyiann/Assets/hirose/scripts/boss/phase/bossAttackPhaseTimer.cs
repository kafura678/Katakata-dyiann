public class bossAttackPhaseTimer
{
    public const float defaultDuration = 10f;

    float duration;
    float elapsedTime;

    public bool isCompleted => elapsedTime >= duration;

    public bossAttackPhaseTimer(float duration)
    {
        setDuration(duration);
    }

    public void setDuration(float value)
    {
        duration = value > 0f ? value : defaultDuration;
        reset();
    }

    public void tick(float deltaTime)
    {
        if (deltaTime <= 0f || isCompleted)
        {
            return;
        }

        elapsedTime += deltaTime;
    }

    public void reset()
    {
        elapsedTime = 0f;
    }
}
