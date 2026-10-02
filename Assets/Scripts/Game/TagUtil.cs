using UnityEngine;

namespace WaterSurvivor
{
    /// <summary>
    /// 标签工具。
    ///
    /// 为什么要在外面包一层：如果 "Enemy" 没在
    /// 【编辑 → 项目设置 → 标签和图层】里定义过，
    /// CompareTag("Enemy") 会直接抛异常（UnityException: Tag: Enemy is not defined），
    /// 而不是老老实实返回 false。这里先确认标签存在，不存在就打印一条
    /// 照着做就能修好的报错，而不是每帧刷一屏异常。
    /// </summary>
    public static class TagUtil
    {
        static bool _checked;
        static bool _enemyTagExists;

        /// <summary>当前工程里到底有没有 "Enemy" 这个标签。</summary>
        public static bool EnemyTagExists
        {
            get
            {
                if (!_checked)
                {
                    _checked = true;
                    _enemyTagExists = Exists(GameConfig.EnemyTag);

                    if (!_enemyTagExists)
                    {
                        SimpleLog.Error("[标签] 工程里没有 \"" + GameConfig.EnemyTag + "\" 标签！" +
                            "请到 编辑 → 项目设置 → 标签和图层 里加一个 Enemy（或用菜单：工具 → 水弹幸存者 → 检查/补上 Enemy 标签），" +
                            "否则水弹永远判定不了命中。");
                    }
                }
                return _enemyTagExists;
            }
        }

        /// <summary>标签是否已在工程里定义。</summary>
        public static bool Exists(string tag)
        {
            try
            {
                // 标签没定义时 FindWithTag 会抛异常；定义了但场景里没人用，只会返回 null
                GameObject.FindWithTag(tag);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>这个碰撞体是不是敌人 —— 用标签判断（任务 4）。</summary>
        public static bool HasEnemyTag(Collider2D other)
        {
            if (other == null) return false;
            if (!EnemyTagExists) return false;
            return other.CompareTag(GameConfig.EnemyTag);
        }
    }
}
