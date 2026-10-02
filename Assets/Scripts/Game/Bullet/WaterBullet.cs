using UnityEngine;

namespace WaterSurvivor
{
    /// <summary>
    /// 【任务 3 + 4】水弹：
    ///   - 命中敌人 → 销毁 + 打印 debug.log
    ///   - 飞行超过一段时间 → 销毁 + 打印 debug.log
    ///   - 是否命中用【标签】判断，敌人标签是 "Enemy"
    ///
    /// 两种销毁都走 DestroySelf()，日志格式统一，方便初版测试时数命中率。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class WaterBullet : MonoBehaviour
    {
        static int _nextId;

        /// <summary>这一发的编号（从 1 开始，只用来打日志）。</summary>
        public int Id { get; private set; }

        Rigidbody2D _rb;
        Vector2 _direction = Vector2.right;
        float _speed = 14f;
        float _lifetime = 2.5f;
        float _age;          // 已经飞了多久
        bool _destroyed;     // 防止"同一帧里超时和命中都触发"，日志只打一条

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            Id = ++_nextId;
        }

        /// <summary>发射时由 PlayerShooter 调用：往哪飞、多快、活多久。</summary>
        public void Setup(Vector2 direction, float speed, float lifetime)
        {
            _direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
            _speed = speed;
            _lifetime = lifetime;
            _age = 0f;
            _destroyed = false;

            _rb.velocity = _direction * _speed;
        }

        void FixedUpdate()
        {
            if (_destroyed) return;

            // 每帧把速度设回匀速，撞到东西被弹开也能保持直线
            _rb.velocity = _direction * _speed;
        }

        void Update()
        {
            if (_destroyed) return;

            _age += Time.deltaTime;

            // ---------- 销毁方式 1：飞太久 ----------
            if (_age >= _lifetime)
            {
                DestroySelf(string.Format("飞行超时（已飞 {0:F2}s，上限 {1:F2}s）", _age, _lifetime));
            }

            // 如果以后要加"飞出地图就销毁"，在这里再加一个判断即可
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_destroyed) return;

            // ---------- 销毁方式 2：命中敌人 ----------
            // 只看标签，不关心敌人挂的是什么脚本（任务 4）
            if (!TagUtil.HasEnemyTag(other)) return;

            // 顺便通知敌人"我被打中了"。
            // 用 SendMessage 是为了和队友写的敌人脚本解耦：
            // 敌人身上只要有 public void OnHit() 就会被调到；没有也不会报错。
            other.gameObject.SendMessage("OnHit", SendMessageOptions.DontRequireReceiver);

            DestroySelf("命中敌人 [" + other.name + "]");
        }

        void DestroySelf(string reason)
        {
            if (_destroyed) return;
            _destroyed = true;

            if (GameConfig.LogBulletDestroy)
            {
                SimpleLog.Log(string.Format(
                    "[水弹#{0}] {1} → 销毁 | 存活 {2:F2}s | 销毁位置 ({3:F2}, {4:F2})",
                    Id, reason, _age, transform.position.x, transform.position.y));
            }

            Destroy(gameObject);
        }
    }
}
