using UnityEngine;

public class homingBulletFireController : MonoBehaviour
{
    [SerializeField] GameObject originalBulletObj;
    [SerializeField] Transform bulletParentTf;

    [SerializeField] float backWeight = 1f;
    [SerializeField] float sideWeight = 1f;
    [SerializeField] float startVelocityValue = 2f;
    [SerializeField] float period = 1f;
    [SerializeField, Range(0f, 1f)] float homingRate = 0.6f;

    fireService fireServiceRf;

    void Awake()
    {
        fireServiceRf = new fireService();

        period = Mathf.Max(0.1f, period);
        homingRate = Mathf.Clamp01(homingRate);
    }

    public void homingBulletsFire(Transform bossTf, Transform playerTf, int fireCount)
    {
        for (int i = 0; i < fireCount; i++)
        {
            homingBulletFire(bossTf, playerTf);
        }
    }

    public void homingBulletFire(Transform bossTf, Transform playerTf)
    {
        GameObject generatedBulletObj = Instantiate(originalBulletObj, bulletParentTf);
        generatedBulletObj.transform.position = bossTf.position;

        homingBullet generatedBullet = generatedBulletObj.GetComponent<homingBullet>();
        generatedBullet.homingBulletSet(playerTf, fireServiceRf.homingBulletStartVelocity(bossTf.position, playerTf.position, backWeight, sideWeight) * startVelocityValue, period, homingRate);

        Destroy(generatedBulletObj, period);
    }
}
