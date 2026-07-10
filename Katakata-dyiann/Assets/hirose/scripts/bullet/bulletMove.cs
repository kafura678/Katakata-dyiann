using UnityEngine;

public class bulletMove
{
    public Vector3 movePosGet(Vector3 selfPos, Vector3 dir, float speed)
    {
        selfPos += dir * speed;
        return selfPos;
    }
}
