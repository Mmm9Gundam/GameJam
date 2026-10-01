using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("子弹数据")]
    public float speed = 5f;          // 速度
    public float lifeTime = 3f;       // 存在时间
    public float rotateSpeed = 5f;    // 转向速度，要是斜着快点的话，我直接吓哭了

    private Transform target;         // 追踪目标（玩家）
    private Vector2 moveDirection;    // 当前飞行方向

    // 设置初始方向和追踪目标
    public void SetDirection(Vector2 dir, Transform targetTransform = null)
    {
        moveDirection = dir.normalized;
        target = targetTransform;
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        Debug.Log("子弹移动：" + moveDirection + " 速度：" + speed);
        // 追踪玩家
        if (target != null)
        {
            Vector2 dirToTarget = (target.position - transform.position).normalized;
            moveDirection = Vector2.Lerp(moveDirection, dirToTarget, rotateSpeed * Time.deltaTime).normalized;
        }

        // 飞行
        transform.Translate(moveDirection * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 确保不会帅哥子弹不会触发怪物的碰撞
        if (other.CompareTag("Player"))
        {
            Debug.Log("敌人子弹命中玩家：" + other.name);
            Destroy(gameObject);
        }
    }
}