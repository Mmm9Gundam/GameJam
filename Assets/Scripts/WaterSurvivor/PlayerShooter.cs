using UnityEngine;

namespace WaterSurvivor
{
    /// <summary>
    /// 【任务 2】按住鼠标左键连发水弹，子弹朝鼠标方向飞。
    /// （任务 4 的"命中判定"在 WaterBullet 里，这里只管发射。）
    ///
    /// 以后接设计文档里的"气压"：
    ///   在 Update() 里加一句"气压够不够"，发射成功后扣气压即可，
    ///   右键打气就是往这个变量里加值。
    /// </summary>
    public class PlayerShooter : MonoBehaviour
    {
        /// <summary>连发间隔（秒）。</summary>
        public float fireInterval = 0.15f;

        /// <summary>子弹速度（单位/秒）。</summary>
        public float bulletSpeed = 14f;

        /// <summary>子弹最长飞行时间（秒）。</summary>
        public float bulletLifetime = 2.5f;

        /// <summary>枪口离玩家中心的距离。</summary>
        public float muzzleOffset = 0.55f;

        float _cooldown;    // 剩余冷却时间
        int _shotCount;     // 一共射了几发（只用来打日志）

        void Update()
        {
            _cooldown -= Time.deltaTime;

            // Input.GetMouseButton(0)：左键【按住】期间一直是 true → 连发。
            // 想改成"点一下打一发"，把它换成 Input.GetMouseButtonDown(0)。
            if (!Input.GetMouseButton(0)) return;
            if (_cooldown > 0f) return;

            _cooldown = Mathf.Max(0.01f, fireInterval);   // 防止填 0 导致一帧射出几百发
            Fire();
        }

        void Fire()
        {
            Vector2 dir = AimDirection();
            Vector3 spawnPos = transform.position + (Vector3)(dir * muzzleOffset);

            // 1. 建一个带圆形精灵的物体当水弹
            GameObject go = SpriteFactory.SpawnSprite(
                "WaterBullet",
                SpriteFactory.Circle,
                GameConfig.ColorBullet,
                GameConfig.BulletRadius * 2f,   // 精灵直径 1 单位，乘 2 就是半径
                spawnPos,
                10);

            // 2. 刚体：负责按速度飞
            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;   // 飞得快也不会"穿"过敌人

            // 3. 碰撞体：勾 isTrigger，这样碰到敌人不会把敌人顶飞，只回调 OnTriggerEnter2D
            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;   // 精灵是 1 单位宽，所以半径 0.5 刚好贴合

            // 4. 挂上水弹逻辑，把"往哪飞、多快、活多久"告诉它
            WaterBullet bullet = go.AddComponent<WaterBullet>();
            bullet.Setup(dir, bulletSpeed, bulletLifetime);

            _shotCount++;
            if (GameConfig.LogBulletSpawn)
            {
                SimpleLog.Log(string.Format(
                    "[水弹#{0}] 发射 | 方向 ({1:F2}, {2:F2}) | 出膛位置 ({3:F2}, {4:F2}) | 第 {5} 发",
                    bullet.Id, dir.x, dir.y, spawnPos.x, spawnPos.y, _shotCount));
            }
        }

        /// <summary>算"玩家 → 鼠标"的单位方向向量。</summary>
        Vector2 AimDirection()
        {
            Camera cam = Camera.main;
            if (cam == null) return Vector2.right;   // 没相机就随便往右（正常不会发生）

            Vector3 mouse = Input.mousePosition;

            // 鼠标给的是屏幕坐标（z = 0），要转成世界坐标。
            // 正交相机下，z 要填"相机到游戏平面的距离"：
            // 相机在 z = -10，游戏平面在 z = 0，所以填 10。
            mouse.z = -cam.transform.position.z;

            Vector2 mouseWorld = cam.ScreenToWorldPoint(mouse);
            Vector2 dir = mouseWorld - (Vector2)transform.position;

            // 鼠标正好压在玩家身上时方向没有意义，随便给个向右，避免算出 NaN
            if (dir.sqrMagnitude < 0.0001f) return Vector2.right;

            return dir.normalized;
        }
    }
}
