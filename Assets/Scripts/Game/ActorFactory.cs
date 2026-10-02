using UnityEngine;

namespace WaterSurvivor
{
    /// <summary>
    /// 角色"拼装工厂"：玩家和敌人长什么样、挂哪些组件，全在这里定义。
    ///
    /// 为什么要单独抽一个类：
    ///   1. 代码现场生成（ArenaBootstrap）和编辑器菜单生成预制体，用的是**同一套**拼装逻辑，
    ///      不会出现"预制体和代码生成的不一样"这种坑；
    ///   2. 以后要给角色加组件（比如血量、动画、音效），只改这里一处，
    ///      代码生成和预制体都不会漏。
    ///
    /// 美术/策划不需要看这个文件，你们直接在预制体上改 Inspector 就行。
    /// </summary>
    public static class ActorFactory
    {
        public const string PlayerName = "Player";
        public const string EnemyName = "Enemy";

        // 渲染层级：地板 -100 < 敌人 4 < 玩家 5 < 子弹 10
        const int PlayerSortingOrder = 5;
        const int EnemySortingOrder = 4;

        // ------------------------------------------------------------------
        // 玩家
        // ------------------------------------------------------------------
        public static GameObject BuildPlayer(Vector3 position)
        {
            GameObject go = new GameObject(PlayerName);
            go.transform.position = position;

            ApplyArt(go, GameConfig.PlayerArtName, GameConfig.PlayerArtSize, GameConfig.ColorPlayer, PlayerSortingOrder);
            ApplyBody(go, GameConfig.PlayerHitRadius);

            // 任务 1：WASD 移动
            PlayerMove move = go.AddComponent<PlayerMove>();
            move.moveSpeed = GameConfig.PlayerMoveSpeed;

            // 任务 2：按住左键连发水弹
            PlayerShooter shooter = go.AddComponent<PlayerShooter>();
            shooter.fireInterval = GameConfig.FireInterval;
            shooter.bulletSpeed = GameConfig.BulletSpeed;
            shooter.bulletLifetime = GameConfig.BulletLifetime;
            shooter.muzzleOffset = GameConfig.MuzzleOffset;

            SetTag(go, GameConfig.PlayerTag);
            return go;
        }

        // ------------------------------------------------------------------
        // 敌人（现在挂的是临时占位脚本 DummyEnemy）
        // ------------------------------------------------------------------
        public static GameObject BuildEnemy(Vector3 position, Transform target)
        {
            GameObject go = new GameObject(EnemyName);
            go.transform.position = position;

            ApplyArt(go, GameConfig.EnemyArtName, GameConfig.EnemyArtSize, GameConfig.ColorEnemy, EnemySortingOrder);
            ApplyBody(go, GameConfig.EnemyHitRadius);

            DummyEnemy enemy = go.AddComponent<DummyEnemy>();
            enemy.target = target;
            enemy.moveSpeed = GameConfig.DummyEnemySpeed;

            SetTag(go, GameConfig.EnemyTag);
            return go;
        }

        // ------------------------------------------------------------------
        // 地板（动画/场景都要用，放这里统一）
        // ------------------------------------------------------------------
        public static GameObject BuildFloor()
        {
            // 方块精灵是 1x1，先建成 1x1 再按竞技场尺寸拉伸
            GameObject floor = SpriteFactory.SpawnSprite(
                "Floor", SpriteFactory.Square, GameConfig.ColorFloor,
                1f, Vector3.zero, -100);

            floor.transform.localScale = new Vector3(
                GameConfig.ArenaHalfWidth * 2f,
                GameConfig.ArenaHalfHeight * 2f,
                1f);

            return floor;
        }

        // ------------------------------------------------------------------
        // 内部小工具
        // ------------------------------------------------------------------

        /// <summary>挂上美术图和 SpriteRenderer。图找不到时会退回代码画的圆。</summary>
        static void ApplyArt(GameObject go, string artName, float artSize, Color fallbackColor, int sortingOrder)
        {
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();

            bool usedArt;
            sr.sprite = ArtLibrary.Get(artName, artSize, out usedArt);

            // 用的是美术图就不要染色（保持图片原样）；退回代码图形时才上色区分
            sr.color = usedArt ? Color.white : fallbackColor;
            sr.sortingOrder = sortingOrder;

            // ArtLibrary 返回的图已经按 artSize 做好了世界尺寸，所以这里不用缩放
            go.transform.localScale = Vector3.one;
        }

        /// <summary>挂上刚体和圆形碰撞体：2D 俯视角必须没有重力。</summary>
        static void ApplyBody(GameObject go, float hitRadius)
        {
            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.radius = Mathf.Max(0.05f, hitRadius);
        }

        static void SetTag(GameObject go, string tag)
        {
            if (!TagUtil.Exists(tag))
            {
                SimpleLog.Error("[标签] 工程里没有 \"" + tag + "\" 标签，请到 " +
                                "编辑 → 项目设置 → 标签和图层 里补一个（菜单：工具 → 水弹幸存者 → 检查/补上 Enemy 标签）。");
                return;
            }
            go.tag = tag;
        }
    }
}
