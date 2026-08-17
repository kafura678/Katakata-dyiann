using UnityEngine;

public class homingBullet : MonoBehaviour
{
    bool isMove = false;
    Vector3 vel;
    float period = 1f;
    float homingDuration;
    float elapsedTime;
    bool isDirectionLocked;
    Transform playerTf;

    void FixedUpdate()
    {
        if (!isMove) return;

        if (elapsedTime >= period) return;

        if (!isDirectionLocked && elapsedTime >= homingDuration)
        {
            lockDirection();
        }

        if (!isDirectionLocked && playerTf != null)
        {
            float remainingTime = period - elapsedTime;
            Vector3 diff = playerTf.position - transform.position;
            Vector3 accele = (diff - vel * remainingTime) * 2f / (remainingTime * remainingTime);

            vel += accele * Time.fixedDeltaTime;
        }

        transform.position += vel * Time.fixedDeltaTime;
        elapsedTime += Time.fixedDeltaTime;
    }

    public void homingBulletSet(Transform playerTf, Vector3 startVel, float period, float homingRate)
    {
        this.playerTf = playerTf;
        this.vel = startVel;
        this.period = Mathf.Max(0.1f, period);
        homingDuration = this.period * Mathf.Clamp01(homingRate);
        elapsedTime = 0f;
        isDirectionLocked = false;
        isMove = true;
    }

    void lockDirection()
    {
        isDirectionLocked = true;

        if (playerTf == null)
        {
            return;
        }

        float remainingTime = period - elapsedTime;
        if (remainingTime <= 0f)
        {
            return;
        }

        vel = (playerTf.position - transform.position) / remainingTime;
    }
}
