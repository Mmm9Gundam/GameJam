using UnityEngine;

namespace WaterSurvivor
{
    /// <summary>
    /// 初版测试用的全部可调数值，集中放这里，改完不用去 Inspector 里到处找。
    ///
    /// 目前只覆盖第一批任务：
    ///   1. WASD 移动
    ///   2. 按住鼠标左键连发水弹（朝鼠标方向）
    ///   3. 水弹命中敌人 / 飞行超时后销毁并打印 debug.log
    ///   4. 命中判定用标签，敌人标签是 "Enemy"
    ///
    /// 设计文档里的气压、模块、热力值、水泵、水域等还没做，
    /// 以后接进来时直接在这个文件里加字段就行。
    /// </summary>
    public static class GameConfig
    {
        // ---------------- 标签 ----------------
        /// <summary>敌人标签。水弹只认这个标签算命中（任务 4）。</summary>
        public const string EnemyTag = "Enemy";

        // ---------------- 开局 ----------------
        /// <summary>true = 在任意场景按 Play 都会自动生成测试场地（初版测试用）。</summary>
        public static bool AutoStart = true;

        /// <summary>是否顺手生成几个占位敌人，方便打中看日志。</summary>
        public static bool SpawnDummyEnemies = true;

        /// <summary>占位敌人数量。</summary>
        public static int DummyEnemyCount = 6;

        // ---------------- 玩家 ----------------
        /// <summary>玩家移动速度（单位/秒）。</summary>
        public static float PlayerMoveSpeed = 5.5f;

        /// <summary>玩家身体半径（世界单位，用来算碰撞体和显示大小）。</summary>
        public static float PlayerRadius = 0.4f;

        // ---------------- 水弹 ----------------
        /// <summary>连发间隔（秒）。0.15 ≈ 每秒 6.7 发。以后接气压时它就是基础射速。</summary>
        public static float FireInterval = 0.15f;

        /// <summary>水弹飞行速度（单位/秒）。</summary>
        public static float BulletSpeed = 14f;

        /// <summary>水弹最长飞行时间（秒），超过就销毁（任务 3）。</summary>
        public static float BulletLifetime = 2.5f;

        /// <summary>水弹半径（世界单位）。</summary>
        public static float BulletRadius = 0.14f;

        /// <summary>枪口离玩家中心的距离，避免子弹一出生就撞到自己。</summary>
        public static float MuzzleOffset = 0.55f;

        // ---------------- 场地 ----------------
        /// <summary>竞技场横向半宽（固定竞技场，玩家被夹在这个范围内）。</summary>
        public static float ArenaHalfWidth = 9f;

        /// <summary>竞技场纵向半高（同时也决定正交相机的大小）。</summary>
        public static float ArenaHalfHeight = 5f;

        // ---------------- 日志 ----------------
        /// <summary>是否把日志同时追加写进磁盘上的 debug.log。</summary>
        public static bool WriteLogFile = true;

        /// <summary>发射时是否打日志（一秒 6 条，嫌刷屏就改 false）。</summary>
        public static bool LogBulletSpawn = true;

        /// <summary>销毁时是否打日志（任务 3 要求，默认开）。</summary>
        public static bool LogBulletDestroy = true;

        // ---------------- 配色（纯代码生成，不需要美术资源） ----------------
        public static readonly Color ColorPlayer = new Color(0.38f, 0.82f, 1f, 1f);
        public static readonly Color ColorBullet = new Color(0.60f, 0.90f, 1f, 0.95f);
        public static readonly Color ColorEnemy = new Color(0.36f, 0.85f, 0.50f, 1f);
        public static readonly Color ColorFloor = new Color(0.10f, 0.16f, 0.24f, 1f);
        public static readonly Color ColorCameraBg = new Color(0.05f, 0.08f, 0.13f, 1f);
    }
}
