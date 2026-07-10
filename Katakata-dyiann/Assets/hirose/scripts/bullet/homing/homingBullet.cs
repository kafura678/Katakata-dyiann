using UnityEngine;
using UnityEngine.UIElements;

public class homingBullet : MonoBehaviour
{
    bool isMove = false;
    Vector3 vel;
    float period = 1f;
    Transform playerTf;

    void FixedUpdate()
    {
        if (!isMove) return;

        period -= Time.fixedDeltaTime;
        if (period <= 0) return;

        Vector3 diff = playerTf.localPosition - transform.localPosition;
        Vector3 accele = (diff - vel * period) * 2f / (period * period);

        vel += accele * Time.fixedDeltaTime;
        transform.localPosition = vel * Time.fixedDeltaTime;
    }

    public void homingBulletSet(Transform playerTf, Vector3 startVel, float period)
    {
        this.playerTf = playerTf;
        this.vel = startVel;
        this.period = period;
        isMove = true;
    }
}
