using UnityEngine;

namespace WaterSurvivor
{
    /// <summary>
    /// 【任务 1】玩家 WASD 移动。
    ///
    /// 用 Rigidbody2D（刚体）移动，而不是直接改 transform.position：
    /// 这样和敌人、以后的水域触发器/水泵都能正常发生物理碰撞。
    ///
    /// 2D 俯视角的要点：把重力关掉（gravityScale = 0），不然物体会一直往下掉。
    ///
    /// 想换按键：改下面 ReadInput() 里的 KeyCode 即可。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMove : MonoBehaviour
    {
        /// <summary>移动速度（单位/秒）。</summary>
        public float moveSpeed = 5.5f;

        Rigidbody2D _rb;
        Vector2 _moveInput;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;                                   // 俯视角：没有重力
            _rb.freezeRotation = true;                               // 撞到敌人不要原地打转
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        void Update()
        {
            // 按键读取放在 Update（和帧率绑定）里，速度施加放在 FixedUpdate（和物理同步）里
            _moveInput = ReadInput();
        }

        void FixedUpdate()
        {
            _rb.velocity = _moveInput * moveSpeed;

            // 初版先简单夹取坐标，别让玩家跑出竞技场就找不到了。
            // 正式版换成地图边界 / 水域逻辑。
            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x, -GameConfig.ArenaHalfWidth, GameConfig.ArenaHalfWidth);
            p.y = Mathf.Clamp(p.y, -GameConfig.ArenaHalfHeight, GameConfig.ArenaHalfHeight);
            transform.position = p;
        }

        /// <summary>把 WASD 读成一个方向向量：W=(0,1) 上，A=(-1,0) 左，以此类推。</summary>
        Vector2 ReadInput()
        {
            float x = 0f;
            float y = 0f;

            if (Input.GetKey(KeyCode.A)) x -= 1f;
            if (Input.GetKey(KeyCode.D)) x += 1f;
            if (Input.GetKey(KeyCode.S)) y -= 1f;
            if (Input.GetKey(KeyCode.W)) y += 1f;

            Vector2 v = new Vector2(x, y);

            // 同时按 W+D 时长度是 1.41，会比只按 W 快 41%，
            // 归一化一下，斜着走和直着走一样快。
            if (v.sqrMagnitude > 1f) v = v.normalized;

            return v;
        }
    }
}
