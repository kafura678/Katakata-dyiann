using UnityEngine;

public class bossSequentialWaveBulletPattern : MonoBehaviour
{
    [SerializeField] bossBulletFactory bulletFactory;
    [SerializeField] float startAngle;
    [SerializeField] float angleStep = 15f;
    [SerializeField] int bulletCountPerFire = 1;
    [SerializeField] float bulletSpreadAngle = 10f;

    float currentAngle;

    void Awake()
    {
        bulletCountPerFire = Mathf.Max(1, bulletCountPerFire);
        reset();
    }

    public void fire(Transform bossTf)
    {
        if (bossTf == null || bulletFactory == null)
        {
            return;
        }

        float startOffset = -(bulletCountPerFire - 1) * bulletSpreadAngle * 0.5f;
        for (int i = 0; i < bulletCountPerFire; i++)
        {
            float angle = (currentAngle + startOffset + bulletSpreadAngle * i) * Mathf.Deg2Rad;
            Vector3 moveDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            bulletFactory.create(bossTf.position, moveDirection);
        }

        currentAngle = Mathf.Repeat(currentAngle + angleStep, 360f);
    }

    public void reset()
    {
        currentAngle = Mathf.Repeat(startAngle, 360f);
    }
}
