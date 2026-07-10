using UnityEngine;

public class boss : MonoBehaviour
{
    [SerializeField] Transform normalMoveCenterTf;
    [SerializeField] float normalMoveRadius;

    [SerializeField] int normalBulletFireCount = 1;
    [SerializeField] float normalBulletFireTime = 1;
    [SerializeField] bossFireController bossFireControllerRf;

    [SerializeField] Transform playerTf;
    [SerializeField] int homingBulletFireCount = 1;
    [SerializeField] float homingBulletFireTime = 1;
    [SerializeField] homingBulletFireController bossHomingBulletFireController;

    bossNormalMove bossMoveRf;
    bossFireTimer bossNormalBulletFireTiemr;
    bossFireTimer bossHomingBulletFireTimer;

    void Awake()
    {
        bossMoveRf = new bossNormalMove();
        bossNormalBulletFireTiemr = new bossFireTimer(normalBulletFireTime);
        bossHomingBulletFireTimer = new bossFireTimer(homingBulletFireTime);

        normalMoveRadius = Mathf.Max(0.1f, normalMoveRadius);
        normalBulletFireCount = Mathf.Max(1, normalBulletFireCount);
    }

    void FixedUpdate()
    {
        transform.localPosition = bossMoveRf.movePosGet(normalMoveCenterTf.localPosition, normalMoveRadius);

        if (bossNormalBulletFireTiemr.tryFire(Time.fixedDeltaTime))
        {
            bossFireControllerRf.bulletsFire(transform, normalBulletFireCount);
        }

        if (bossHomingBulletFireController != null && bossHomingBulletFireTimer.tryFire(Time.fixedDeltaTime))
        {
            bossHomingBulletFireController.homingBulletsFire(transform, playerTf, homingBulletFireCount);
        }
    }
}
