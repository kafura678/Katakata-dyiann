using UnityEngine;

public class fireService
{
    public Vector3 circleFirePosGet(Vector3 centerPos, float rotateSpeed, float rotateValue)
    {
        float x = Mathf.Sin(Time.time * rotateSpeed + rotateValue) / 10f; //座標調整のため/10fを行っている
        float y = Mathf.Cos(Time.time * rotateSpeed + rotateValue) / 10f;
        return new Vector3(centerPos.x + x, centerPos.y + y, centerPos.z);
    }

    public Vector3 homingBulletStartVelocity(Vector3 firePos, Vector3 targetPos, float backWeight, float sideWeight)
    {
        Vector3 dir = (targetPos - firePos).normalized;

        Vector3 backDir = -dir;
        Vector3 sideDir = new Vector3(-dir.y, dir.x, dir.z);

        float sideDirSign = Random.value < 0.5f ? -1f : 1f;
        return (backDir * backWeight + sideDirSign * sideDir * sideWeight).normalized;
    }
}
