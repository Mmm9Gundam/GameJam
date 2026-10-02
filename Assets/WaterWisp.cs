using UnityEngine;

public class WaterWisp : MonoBehaviour
{
    [Header("攻击数据")]
    public float attackRange = 5f;        // 攻击范围
    public float fireRate = 1f;           // 攻速（大概是每秒几发）
    public float bulletSpeed = 5f;        // 子弹速度
    public GameObject bulletPrefab;       // 子弹预制体

    [Header("检测")]
    public string playerTag = "Player";   // 玩家标签
    public float spawnOffset = 0.8f;      // 子弹生成偏移，避免卡身体

    private Transform player;
    private float fireTimer;

    void Start()
    {
        FindPlayer();
    }

    void Update()
    {
        // 雷达开了
        if (player == null)
        {
            FindPlayer();
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange)
        {
            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0f)
            {
                Fire();
                fireTimer = fireRate > 0f ? 1f / fireRate : 0.1f;
            }
        }
        else
        {
            // 离开范围重置计时，再进范围需等一个完整间隔
            fireTimer = fireRate > 0f ? 1f / fireRate : 0.1f;
        }
    }

    void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null)
        {
            player = p.transform;
        }
    }

    void Fire()
    {
        if (bulletPrefab == null || player == null) return;

        // 发射前瞄准玩家，做到直线攻击的效果，应该效果还行
        Vector2 dir = (player.position - transform.position).normalized;

        // 防呆操作
        Vector2 spawnPos = (Vector2)transform.position + dir * spawnOffset;

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        EnemyBullet eb = bullet.GetComponent<EnemyBullet>();
        if (eb != null)
        {
            eb.SetDirection(dir);
            eb.speed = bulletSpeed;
        }
    }

    // 攻击范围具象化，方便你们调数据，感觉不赖
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}