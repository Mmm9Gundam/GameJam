using UnityEngine;

namespace WaterSurvivor
{
    /// <summary>
    /// 【临时占位敌人】只为了让初版能测出"水弹命中敌人"。
    ///
    /// 行为：朝玩家慢慢走；被水弹打中扣血，血没了就销毁。
    /// Inspector 上能改的：moveSpeed（速度）、maxHp（要打几下才死）、target（追谁）。
    ///
    /// ============================================================
    /// 以后要换成正式的水鬼/水灵，只需要遵守两条约定：
    ///
    ///   1. 物体的 Tag 必须是 "Enemy"      ← 水弹靠标签判断"打中了谁"
    ///   2. 想被通知"被打中了"，写一个方法 public void OnHit()
    ///      （水弹是用 SendMessage("OnHit") 调的，没有这个方法也不会报错，
    ///        所以你可以先只做移动，之后再加受击逻辑）
    ///
    /// 想要更多交互（比如被水弹推开、掉血飘字、死亡掉模块），
    /// 就在这个类里加，或者新写一个脚本挂到敌人预制体上。
    /// ============================================================
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class DummyEnemy : MonoBehaviour
    {
        /// <summary>追击速度（单位/秒）。</summary>
        public float moveSpeed = 1.8f;

        /// <summary>要被打几下才死。1 = 一枪一个（初版默认）。</summary>
        public int maxHp = 1;

        /// <summary>追谁——生成时指定为玩家，也可以在 Inspector 里拖。</summary>
        public Transform target;

        Rigidbody2D _rb;
        int _hp;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            _hp = Mathf.Max(1, maxHp);
        }

        void FixedUpdate()
        {
            if (target == null)
            {
                _rb.velocity = Vector2.zero;
                return;
            }

            Vector2 to = (Vector2)target.position - (Vector2)transform.position;

            // 已经贴到玩家身上就别再挤了，否则会把玩家顶得到处跑
            if (to.sqrMagnitude < 0.09f)
            {
                _rb.velocity = Vector2.zero;
                return;
            }

            _rb.velocity = to.normalized * moveSpeed;
        }

        /// <summary>
        /// 被水弹打中时，由 WaterBullet 通过 SendMessage 调用。
        /// 名字必须是 OnHit，参数可以没有，也可以写成 OnHit(int damage)。
        /// </summary>
        public void OnHit()
        {
            OnHit(1);
        }

        /// <summary>带伤害值的版本，方便以后接"攻击力"属性。</summary>
        public void OnHit(int damage)
        {
            _hp -= Mathf.Max(1, damage);

            if (_hp > 0)
            {
                SimpleLog.Log(string.Format(
                    "[占位敌人] {0} 被打中，剩余血量 {1}/{2} | 位置 ({3:F2}, {4:F2})",
                    name, _hp, Mathf.Max(1, maxHp), transform.position.x, transform.position.y));
                return;
            }

            SimpleLog.Log(string.Format(
                "[占位敌人] {0} 被打中 → 死亡销毁 | 位置 ({1:F2}, {2:F2})",
                name, transform.position.x, transform.position.y));

            Destroy(gameObject);
        }
    }
}
