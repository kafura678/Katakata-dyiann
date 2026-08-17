using UnityEngine;

public class bossTargetPointMove : MonoBehaviour
{
    [SerializeField] Transform bossTf;
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float arrivalDistance = 0.01f;

    Transform targetTf;
    bool isMoving;

    public bool isCompleted { get; private set; }

    void Awake()
    {
        moveSpeed = Mathf.Max(0.01f, moveSpeed);
        arrivalDistance = Mathf.Max(0f, arrivalDistance);
    }

    public bool begin(Transform targetTf)
    {
        stop();

        if (bossTf == null || targetTf == null)
        {
            isCompleted = false;
            return false;
        }

        this.targetTf = targetTf;
        isMoving = true;
        isCompleted = false;
        completeIfArrived();
        return true;
    }

    public void move(float deltaTime)
    {
        if (!isMoving || deltaTime <= 0f || targetTf == null)
        {
            return;
        }

        bossTf.position = Vector3.MoveTowards(
            bossTf.position,
            targetTf.position,
            moveSpeed * deltaTime);
        completeIfArrived();
    }

    public void stop()
    {
        targetTf = null;
        isMoving = false;
        isCompleted = false;
    }

    void completeIfArrived()
    {
        if (Vector3.Distance(bossTf.position, targetTf.position) > arrivalDistance)
        {
            return;
        }

        bossTf.position = targetTf.position;
        isMoving = false;
        isCompleted = true;
    }
}
