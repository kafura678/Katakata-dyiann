using UnityEngine;

public class bossFireTimer
{
    readonly float fireTime;
    float elapsedTime = 0;

    public bossFireTimer(float value)
    {
        fireTime = Mathf.Max(0.05f, value);
    }

    public bool tryFire(float deltaTime)
    {
        elapsedTime += deltaTime;
        if (fireTime <= elapsedTime)
        {
            elapsedTime = 0;
            return true;
        }
        return false;
    }

    public void reset()
    {
        elapsedTime = 0f;
    }
}
