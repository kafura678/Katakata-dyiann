using UnityEngine;

public class bossSimultaneousBulletPattern : MonoBehaviour
{
    [SerializeField] bossBulletFactory bulletFactory;
    [SerializeField] int bulletCount = 5;
    [SerializeField] float baseAngle;
    [SerializeField] float rotateAnglePerFire = 10f;

    float currentAngle;

    void Awake()
    {
        bulletCount = Mathf.Max(1, bulletCount);
        reset();
    }

    public void fire(Transform bossTf)
    {
        if (bossTf == null || bulletFactory == null)
        {
            return;
        }

        float angleInterval = 360f / bulletCount;
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = (currentAngle + angleInterval * i) * Mathf.Deg2Rad;
            Vector3 moveDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            bulletFactory.create(bossTf.position, moveDirection);
        }

        currentAngle = Mathf.Repeat(currentAngle + rotateAnglePerFire, 360f);
    }

    public void reset()
    {
        currentAngle = Mathf.Repeat(baseAngle, 360f);
    }
}
