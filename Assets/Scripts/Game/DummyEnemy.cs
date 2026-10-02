using UnityEngine;

namespace WaterSurvivor
{
    /// <summary>
    /// 【临时占位敌人】只为了让初版能测出"水弹命中敌人"这件事。
    ///
    /// 行为：朝玩家慢慢走过去；被水弹打中就销毁。
    ///
    /// 正式的水鬼 / 水灵请替换掉这个脚本，和程序组约定好两条：
    ///   1. 物体的 Tag 必须是 "Enemy"          ← 水弹靠标签判断命中
    ///   2. 想被水弹通知命中，提供一个 public void OnHit()  ← 水弹用 SendMessage 调它
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class DummyEnemy : MonoBehaviour
    {
        /// <summary>追击速度（单位/秒）。</summary>
        public float moveSpeed = 1.8f;

        /// <summary>追谁——生成时由 ArenaBootstrap 指定为玩家。</summary>
        public Transform target;

        Rigidbody2D _rb;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
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

        /// <summary>被水弹打中时，由 WaterBullet 通过 SendMessage 调用。</summary>
        public void OnHit()
        {
            SimpleLog.Log(string.Format(
                "[占位敌人] {0} 被打中 → 销毁 | 位置 ({1:F2}, {2:F2})",
                name, transform.position.x, transform.position.y));

            Destroy(gameObject);
        }
    }
}
