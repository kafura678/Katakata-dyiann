using UnityEngine;

public class bossNormalMove
{
    public Vector3 movePosGet(Vector3 centerPos, float radius)
    {
        float x = radius * Mathf.Sin(Time.time);
        float y = radius * Mathf.Cos(Time.time);

        return new Vector3(centerPos.x + x, centerPos.y + y, centerPos.z);
    }
}
