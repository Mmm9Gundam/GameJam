using UnityEngine;

namespace WaterSurvivor
{
    /// <summary>
    /// 初版测试用的场地生成器（纯代码，零美术资源）。
    ///
    /// 和你们 NumberZuma 里 GameBootstrap 一个套路：
    /// [RuntimeInitializeOnLoadMethod] 会在场景加载完之后自动执行，
    /// 所以只要脚本编进工程，【在任意场景按 Play】就能直接玩 ——
    /// 不用手动建物体、不用在 Inspector 里拖引用、不会有"资源丢失"。
    ///
    /// 生成内容：正交相机 + 地板 + 玩家（WASD 移动 / 左键连发水弹）+ 几个占位敌人。
    ///
    /// 想改成手动摆：
    ///   1. 把 GameConfig.AutoStart 改成 false
    ///   2. 用菜单 工具 → 水弹幸存者 → 在当前场景生成测试对象
    /// </summary>
    public class ArenaBootstrap : MonoBehaviour
    {
        bool _built;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoStart()
        {
            if (!GameConfig.AutoStart) return;
            if (FindObjectOfType<ArenaBootstrap>() != null) return;   // 场景里已经有了就不重复生成

            GameObject go = new GameObject("ArenaBootstrap");
            go.AddComponent<ArenaBootstrap>();
        }

        void Start()
        {
            Build();
        }

        /// <summary>把相机、地板、玩家、敌人一次性生成到当前场景（编辑器菜单也会调它）。</summary>
        public void Build()
        {
            if (_built) return;
            _built = true;

            SetupCamera();
            CreateFloor();
            GameObject player = CreatePlayer();

            if (GameConfig.SpawnDummyEnemies) CreateEnemies(player.transform);

            SimpleLog.Log("[启动] 就绪：WASD 移动，按住鼠标左键朝鼠标方向连发水弹；" +
                          "命中敌人或飞行 " + GameConfig.BulletLifetime.ToString("F1") + "s 后水弹销毁并记录日志。");
        }

        // ------------------------------------------------------------------
        // 相机
        // ------------------------------------------------------------------
        void SetupCamera()
        {
            Camera cam = Camera.main;

            if (cam == null)
            {
                GameObject camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";        // Camera.main 靠这个标签找相机
                cam = camGo.AddComponent<Camera>();
            }

            cam.orthographic = true;                                  // 2D 用正交相机
            cam.transform.position = new Vector3(0f, 0f, -10f);       // 退到 z = -10，正对 XY 平面
            cam.transform.rotation = Quaternion.identity;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = GameConfig.ColorCameraBg;

            // 正交相机的 orthographicSize 是"竖直方向的半个屏幕高"。
            // 只按半高设的话，窗口一窄（比如 4:3）横向就看不到竞技场两边，
            // 所以再按宽度算一次，取大的那个，保证整个竞技场始终在画面里。
            float sizeForWidth = GameConfig.ArenaHalfWidth / Mathf.Max(0.2f, cam.aspect);
            cam.orthographicSize = Mathf.Max(GameConfig.ArenaHalfHeight, sizeForWidth);
        }

        // ------------------------------------------------------------------
        // 地板（只是给个视觉参照，没有碰撞体，不会挡住子弹）
        // ------------------------------------------------------------------
        void CreateFloor()
        {
            GameObject floor = SpriteFactory.SpawnSprite(
                "Floor", SpriteFactory.Square, GameConfig.ColorFloor,
                1f, Vector3.zero, -100);

            // 方块精灵是 1x1，直接按竞技场尺寸拉伸
            floor.transform.localScale = new Vector3(
                GameConfig.ArenaHalfWidth * 2f,
                GameConfig.ArenaHalfHeight * 2f,
                1f);
        }

        // ------------------------------------------------------------------
        // 玩家
        // ------------------------------------------------------------------
        GameObject CreatePlayer()
        {
            GameObject player = SpriteFactory.SpawnSprite(
                "Player", SpriteFactory.Circle, GameConfig.ColorPlayer,
                GameConfig.PlayerRadius * 2f, Vector3.zero, 5);

            Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CircleCollider2D col = player.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;      // 精灵直径 1 单位 → 半径 0.5 刚好贴合

            // 任务 1：WASD 移动
            PlayerMove move = player.AddComponent<PlayerMove>();
            move.moveSpeed = GameConfig.PlayerMoveSpeed;

            // 任务 2：按住左键连发水弹
            PlayerShooter shooter = player.AddComponent<PlayerShooter>();
            shooter.fireInterval = GameConfig.FireInterval;
            shooter.bulletSpeed = GameConfig.BulletSpeed;
            shooter.bulletLifetime = GameConfig.BulletLifetime;
            shooter.muzzleOffset = GameConfig.MuzzleOffset;

            try { player.tag = "Player"; }
            catch { /* "Player" 是引擎自带标签，正常不会失败；失败也不影响功能 */ }

            return player;
        }

        // ------------------------------------------------------------------
        // 占位敌人：围着玩家撒一圈，方便马上打到
        // ------------------------------------------------------------------
        void CreateEnemies(Transform player)
        {
            int count = Mathf.Max(0, GameConfig.DummyEnemyCount);
            if (count == 0) return;

            for (int i = 0; i < count; i++)
            {
                float angle = 360f / count * i + 15f;      // 均匀分布在圆周上
                float rad = angle * Mathf.Deg2Rad;
                float ring = 4.5f;

                Vector3 pos = new Vector3(Mathf.Cos(rad) * ring, Mathf.Sin(rad) * ring, 0f);
                pos.x = Mathf.Clamp(pos.x, -GameConfig.ArenaHalfWidth + 0.6f, GameConfig.ArenaHalfWidth - 0.6f);
                pos.y = Mathf.Clamp(pos.y, -GameConfig.ArenaHalfHeight + 0.6f, GameConfig.ArenaHalfHeight - 0.6f);

                GameObject enemy = SpriteFactory.SpawnSprite(
                    "Enemy_" + i, SpriteFactory.Circle, GameConfig.ColorEnemy,
                    0.7f, pos, 4);

                Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.freezeRotation = true;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

                CircleCollider2D col = enemy.AddComponent<CircleCollider2D>();
                col.radius = 0.5f;

                DummyEnemy dummy = enemy.AddComponent<DummyEnemy>();
                dummy.target = player;
                dummy.moveSpeed = 1.8f;

                // 任务 4 的前提：敌人必须挂 "Enemy" 标签
                if (!SetEnemyTag(enemy)) return;
            }
        }

        static bool SetEnemyTag(GameObject go)
        {
            if (!TagUtil.EnemyTagExists)
            {
                SimpleLog.Error("[启动] 没能给 " + go.name + " 设置 Enemy 标签，" +
                                "水弹将无法判定命中。请先补上标签（菜单：工具 → 水弹幸存者 → 检查/补上 Enemy 标签）。");
                return false;
            }

            // 标签确认存在后再赋值，这样不会抛异常
            go.tag = GameConfig.EnemyTag;
            return true;
        }
    }
}
