using UnityEngine;

public class bossBulletFactory : MonoBehaviour
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform bulletParentTf;
    [SerializeField] float bulletSpeed = 0.1f;
    [SerializeField] float bulletLifeTime = 5f;

    void Awake()
    {
        bulletSpeed = Mathf.Max(0.01f, bulletSpeed);
        bulletLifeTime = Mathf.Max(0.05f, bulletLifeTime);
    }

    public void create(Vector3 firePosition, Vector3 moveDirection)
    {
        if (bulletPrefab == null || bulletParentTf == null)
        {
            return;
        }

        GameObject generatedBulletObj = Instantiate(bulletPrefab, bulletParentTf);
        generatedBulletObj.transform.position = firePosition;

        bullet generatedBullet = generatedBulletObj.GetComponent<bullet>();
        if (generatedBullet == null)
        {
            Destroy(generatedBulletObj);
            return;
        }

        generatedBullet.moveSet(bulletSpeed, moveDirection.normalized);
        Destroy(generatedBulletObj, bulletLifeTime);
    }
}
