using UnityEngine;

public class bullet : MonoBehaviour
{
    bool isMove = false;
    float moveSpeed = 1;
    Vector3 moveDir;
    bulletMove bulletMoveRf;

    void Awake()
    {
        bulletMoveRf = new bulletMove();
    }

    void FixedUpdate()
    {
        if (!isMove) return;
        transform.localPosition = bulletMoveRf.movePosGet(transform.localPosition, moveDir, moveSpeed);
    }

    public void moveSet(float moveSpeed, Vector3 moveDir)
    {
        this.moveSpeed = moveSpeed;
        this.moveDir = moveDir;
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg - 90f);
        isMove = true;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
