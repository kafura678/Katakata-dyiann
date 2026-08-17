using UnityEngine;

public class bossTargetTfsRotation : MonoBehaviour
{
    [SerializeField] Transform targetTfs;
    [SerializeField] float rotateSpeed = 30f;

    void Awake()
    {
        enabled = false;
    }

    void FixedUpdate()
    {
        if (targetTfs == null)
        {
            return;
        }

        targetTfs.Rotate(0f, 0f, rotateSpeed * Time.fixedDeltaTime);
    }

    public void begin()
    {
        enabled = targetTfs != null;
    }

    public void stop()
    {
        enabled = false;
    }
}
