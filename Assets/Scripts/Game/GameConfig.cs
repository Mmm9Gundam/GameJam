using UnityEngine;

namespace WaterSurvivor
{
    /// <summary>
    /// 所有可调数值集中在这里，改完不用去场景里到处找。
    ///
    /// 【重要】如果你在场景里用预制体（Prefab）摆好了玩家和敌人，
    /// 那它们 Inspector 里填的值**不会**被这里覆盖，改预制体就行（推荐做法）。
    /// 只有"代码现场生成"的对象才会读这里的值。
    ///
    /// 数值分三块：
    ///   一、玩法数值（速度、射速、存活时间……）
    ///   二、美术相关（用哪张图、多大、白底要不要抠）
    ///   三、调试相关（日志开关）
    /// </summary>
    public static class GameConfig
    {
        // ================================================================
        // 一、标签与开局
        // ================================================================

        /// <summary>敌人标签。水弹只认这个标签算命中（任务 4）。</summary>
        public const string EnemyTag = "Enemy";

        /// <summary>玩家标签。敌人靠它找玩家。</summary>
        public const string PlayerTag = "Player";

        /// <summary>true = 在任意场景按 Play 都会自动生成测试场地（初版测试用）。</summary>
        public static bool AutoStart = true;

        /// <summary>是否顺手生成几个占位敌人。</summary>
        public static bool SpawnDummyEnemies = true;

        /// <summary>占位敌人数量。</summary>
        public static int DummyEnemyCount = 6;

        // ================================================================
        // 二、速度类数值
        // ================================================================

        /// <summary>玩家移动速度（单位/秒）。</summary>
        public static float PlayerMoveSpeed = 5.5f;

        /// <summary>水弹飞行速度（单位/秒）。</summary>
        public static float BulletSpeed = 14f;

        /// <summary>占位敌人追击速度（单位/秒）。</summary>
        public static float DummyEnemySpeed = 1.8f;

        // ================================================================
        // 三、射击类数值
        // ================================================================

        /// <summary>连发间隔（秒）。0.15 ≈ 每秒 6.7 发。以后接气压时它就是基础射速。</summary>
        public static float FireInterval = 0.15f;

        /// <summary>水弹最长飞行时间（秒），超过就销毁（任务 3）。</summary>
        public static float BulletLifetime = 2.5f;

        /// <summary>水弹半径（世界单位）。</summary>
        public static float BulletRadius = 0.14f;

        /// <summary>枪口离玩家中心的距离，避免子弹一出生就撞到自己。</summary>
        public static float MuzzleOffset = 0.55f;

        // ================================================================
        // 四、碰撞体大小（和图片大小无关，可以各自单独调）
        // ================================================================

        /// <summary>玩家碰撞体半径。图片画得再大，挨打范围也是这个圆。</summary>
        public static float PlayerHitRadius = 0.32f;

        /// <summary>敌人碰撞体半径。</summary>
        public static float EnemyHitRadius = 0.30f;

        // ================================================================
        // 五、美术图（换图看这里）
        // ================================================================

        /// <summary>关掉就用回上一版的纯代码图形（方便对比/排查美术问题）。</summary>
        public static bool UseArtImages = true;

        /// <summary>图片路径，相对 Assets/Resources/，不带扩展名。</summary>
        public static string PlayerArtName = "Art/Player";
        public static string EnemyArtName = "Art/Enemy";

        /// <summary>图片最长边占多少个世界单位（角色在游戏里多大）。</summary>
        public static float PlayerArtSize = 1.15f;
        public static float EnemyArtSize = 1.0f;

        /// <summary>是否自动把白底抠成透明。放白底的 jpg 也能直接用。</summary>
        public static bool RemoveWhiteBackground = true;

        /// <summary>抠白底的宽容度：0 = 只抠纯白，0.25 = 接近白的都抠（推荐 0.12 左右）。</summary>
        public static float WhiteBackgroundTolerance = 0.12f;

        /// <summary>裁掉角色四周空白边时留几个像素的余量，避免边缘被切掉。</summary>
        public static int ArtCropPadding = 2;

        /// <summary>是否只保留最大的一块图形（去掉原图边缘粘着的碎片、噪点）。</summary>
        public static bool KeepOnlyLargestBlob = true;

        // ================================================================
        // 六、场地
        // ================================================================

        /// <summary>竞技场横向半宽（玩家被夹在这个范围内）。</summary>
        public static float ArenaHalfWidth = 9f;

        /// <summary>竞技场纵向半高（同时也决定正交相机的大小）。</summary>
        public static float ArenaHalfHeight = 5f;

        // ================================================================
        // 七、日志
        // ================================================================

        /// <summary>是否把日志同时追加写进磁盘上的 debug.log。</summary>
        public static bool WriteLogFile = true;

        /// <summary>发射时是否打日志（一秒 6 条，嫌刷屏就改 false）。</summary>
        public static bool LogBulletSpawn = true;

        /// <summary>销毁时是否打日志（任务 3 要求，默认开）。</summary>
        public static bool LogBulletDestroy = true;

        // ================================================================
        // 八、配色（只在"没有美术图、退回代码图形"时才会看到）
        // ================================================================

        public static readonly Color ColorPlayer = new Color(0.38f, 0.82f, 1f, 1f);
        public static readonly Color ColorBullet = new Color(0.60f, 0.90f, 1f, 0.95f);
        public static readonly Color ColorEnemy = new Color(0.36f, 0.85f, 0.50f, 1f);
        public static readonly Color ColorFloor = new Color(0.10f, 0.16f, 0.24f, 1f);
        public static readonly Color ColorCameraBg = new Color(0.05f, 0.08f, 0.13f, 1f);
    }
}
