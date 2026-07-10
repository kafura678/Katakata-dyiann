using UnityEngine;

public class bossFireController : MonoBehaviour
{
    [SerializeField] GameObject originalBulletObj;
    [SerializeField] Transform bulletParentTf;
    [SerializeField] float fireRotateSpeed;
    [SerializeField] float bulletSpeed;

    fireService fireServiceRf;

    void Awake()
    {
        fireServiceRf = new fireService();

        fireRotateSpeed = Mathf.Max(0.01f, fireRotateSpeed);
        bulletSpeed = Mathf.Max(0.01f, bulletSpeed);
    }

    public void bulletsFire(Transform bossTf, int fireCount)
    {
        for (int i = 1; i <= fireCount; i++)
        {
            bulletFire(bossTf, i * ((2 * Mathf.PI) / fireCount));
        }
    }

    public void bulletFire(Transform bossTf, float rotateValue)
    {
        GameObject generatedBulletObj = Instantiate(originalBulletObj, bulletParentTf);
        generatedBulletObj.transform.localPosition = fireServiceRf.circleFirePosGet(bossTf.localPosition, fireRotateSpeed, rotateValue);

        bullet generatedBullet = generatedBulletObj.GetComponent<bullet>();
        generatedBullet.moveSet(bulletSpeed, (generatedBulletObj.transform.localPosition - bossTf.localPosition).normalized);

        Destroy(generatedBulletObj, 5f);
    }
}
