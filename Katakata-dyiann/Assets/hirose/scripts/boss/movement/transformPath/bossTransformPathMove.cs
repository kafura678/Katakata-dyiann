using System.Collections.Generic;
using UnityEngine;

public class bossTransformPathMove : MonoBehaviour
{
    [SerializeField] Transform bossTf;
    [SerializeField] List<Transform> pathPoints = new List<Transform>();
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] bool loop = true;

    int currentPointIndex;
    bool isMoving;

    public bool isCompleted { get; private set; }

    void Awake()
    {
        moveSpeed = Mathf.Max(0.01f, moveSpeed);
    }

    public bool begin()
    {
        stop();

        int firstPointIndex = getNextValidPointIndex(0);
        if (bossTf == null || firstPointIndex < 0)
        {
            isCompleted = true;
            return false;
        }

        currentPointIndex = firstPointIndex;
        bossTf.position = pathPoints[currentPointIndex].position;
        isMoving = true;
        isCompleted = false;
        advancePoint();
        return true;
    }

    public void move(float deltaTime)
    {
        if (!isMoving || deltaTime <= 0f)
        {
            return;
        }

        Transform targetPoint = pathPoints[currentPointIndex];
        bossTf.position = Vector3.MoveTowards(
            bossTf.position,
            targetPoint.position,
            moveSpeed * deltaTime);

        if (bossTf.position == targetPoint.position)
        {
            advancePoint();
        }
    }

    public void stop()
    {
        isMoving = false;
    }

    void advancePoint()
    {
        int nextPointIndex = getNextValidPointIndex(currentPointIndex + 1);
        if (nextPointIndex >= 0)
        {
            currentPointIndex = nextPointIndex;
            return;
        }

        if (loop)
        {
            nextPointIndex = getNextValidPointIndex(0);
            if (nextPointIndex >= 0)
            {
                currentPointIndex = nextPointIndex;
                return;
            }
        }

        isMoving = false;
        isCompleted = true;
    }

    int getNextValidPointIndex(int startIndex)
    {
        for (int i = startIndex; i < pathPoints.Count; i++)
        {
            if (pathPoints[i] != null)
            {
                return i;
            }
        }

        return -1;
    }
}
