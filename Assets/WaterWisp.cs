using UnityEngine;

public class WaterWisp : MonoBehaviour
{
    [Header("攻击数据")]
    public float attackRange = 5f;        // 攻击范围
    public float fireRate = 1f;           // 攻速（大概就是每秒几发）
    public float bulletSpeed = 5f;        // 子弹速度
    public GameObject bulletPrefab;       // 子弹预制体

    [Header("检测")]
    public string playerTag = "Player";   // 玩家标签

    private Transform player;
    private float fireTimer;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null)
        {
            player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange)
        {
            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0f)
            {
                Fire();
                fireTimer = 1f / fireRate;
            }
        }
    }

    void Fire()
    {
        if (bulletPrefab == null || player == null) return;

        // 朝向玩家的方向
        Vector2 dir = (player.position - transform.position).normalized;

        // 关键：子弹生成位置往外偏移，避免卡在敌人自己身体里
        Vector2 spawnPos = (Vector2)transform.position + dir * 0.8f;

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        EnemyBullet eb = bullet.GetComponent<EnemyBullet>();
        if (eb != null)
        {
            eb.SetDirection(dir, player);  // 传玩家进去，子弹就能追踪
            eb.speed = bulletSpeed;
        }
    }

    // 领域展开的范围，迫真
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}