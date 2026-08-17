using UnityEngine;

public class bossSequentialWaveBulletPattern : MonoBehaviour
{
    [SerializeField] bossBulletFactory bulletFactory;
    [SerializeField] float startAngle;
    [SerializeField] float angleStep = 15f;

    float currentAngle;

    void Awake()
    {
        reset();
    }

    public void fire(Transform bossTf)
    {
        if (bossTf == null || bulletFactory == null)
        {
            return;
        }

        float angle = currentAngle * Mathf.Deg2Rad;
        Vector3 moveDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
        bulletFactory.create(bossTf.position, moveDirection);
        currentAngle = Mathf.Repeat(currentAngle + angleStep, 360f);
    }

    public void reset()
    {
        currentAngle = Mathf.Repeat(startAngle, 360f);
    }
}
