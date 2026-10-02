using UnityEngine;

namespace WaterSurvivor
{
    /// <summary>
    /// 测试场地生成器。**两条路都支持**，你挑一条用就行：
    ///
    /// 路线 A（现在默认的，最省事）：
    ///   什么都不用做，在任意场景按 Play，它会自动生成
    ///   相机 + 地板 + 玩家 + 一圈敌人。美术图会自动从
    ///   Assets/Resources/Art/ 里读。
    ///
    /// 路线 B（推荐给要长期改的人）：用预制体
    ///   1. 菜单 工具 → 水弹幸存者 → 生成预制体（Player / Enemy）
    ///   2. 菜单 工具 → 水弹幸存者 → 生成测试场景（会自动摆好并引用预制体）
    ///   之后你在 Project 窗口双击 Player 预制体，就能在 Inspector 里
    ///   直接改图片、速度、碰撞体；场景里的对象也是普通 GameObject，
    ///   可以随便拖、随便加组件。
    ///
    /// 两条路的关系：**Inspector 里填了预制体就走预制体，没填才代码生成**。
    /// 走预制体时，本脚本不会覆盖你在 Inspector 里改的任何值。
    /// </summary>
    public class ArenaBootstrap : MonoBehaviour
    {
        [Header("预制体（留空 = 用代码现场生成）")]
        [Tooltip("玩家的预制体。拖 Assets/Prefabs/Player.prefab 到这里。")]
        public GameObject playerPrefab;

        [Tooltip("敌人的预制体。拖 Assets/Prefabs/Enemy.prefab 到这里。")]
        public GameObject enemyPrefab;

        [Header("开关")]
        [Tooltip("按 Play 时是否自动生成场地。场景里已经摆好对象就关掉它。")]
        public bool buildOnStart = true;

        [Tooltip("是否生成一圈测试敌人。")]
        public bool spawnEnemies = true;

        [Tooltip("生成几个测试敌人。")]
        public int enemyCount = 6;

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
            if (buildOnStart) Build();
        }

        /// <summary>把相机、地板、玩家、敌人一次性生成到当前场景（编辑器菜单也会调它）。</summary>
        public void Build()
        {
            if (_built) return;
            _built = true;

            SetupCamera();
            CreateFloor();

            GameObject player = CreatePlayer();

            if (spawnEnemies && enemyCount > 0) CreateEnemies(player.transform);

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
        // 地板（视觉参照，没有碰撞体，不会挡住子弹）
        // ------------------------------------------------------------------
        void CreateFloor()
        {
            ActorFactory.BuildFloor();
        }

        // ------------------------------------------------------------------
        // 玩家：有预制体用预制体，没有就代码生成
        // ------------------------------------------------------------------
        GameObject CreatePlayer()
        {
            if (playerPrefab != null)
            {
                GameObject prefabPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
                prefabPlayer.name = ActorFactory.PlayerName;
                return prefabPlayer;   // 走预制体就不改它的任何 Inspector 值
            }

            return ActorFactory.BuildPlayer(Vector3.zero);
        }

        // ------------------------------------------------------------------
        // 敌人：围着玩家撒一圈
        // ------------------------------------------------------------------
        void CreateEnemies(Transform player)
        {
            int count = Mathf.Max(0, enemyCount);

            for (int i = 0; i < count; i++)
            {
                float angle = 360f / count * i + 15f;      // 均匀分布在圆周上
                float rad = angle * Mathf.Deg2Rad;
                float ring = 4.5f;

                Vector3 pos = new Vector3(Mathf.Cos(rad) * ring, Mathf.Sin(rad) * ring, 0f);
                pos.x = Mathf.Clamp(pos.x, -GameConfig.ArenaHalfWidth + 0.6f, GameConfig.ArenaHalfWidth - 0.6f);
                pos.y = Mathf.Clamp(pos.y, -GameConfig.ArenaHalfHeight + 0.6f, GameConfig.ArenaHalfHeight - 0.6f);

                GameObject enemy;

                if (enemyPrefab != null)
                {
                    enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
                }
                else
                {
                    enemy = ActorFactory.BuildEnemy(pos, player);

                    // 代码生成时顺手确认一下标签，标签不对水弹就打不中
                    if (!TagUtil.EnemyTagExists) return;
                }

                enemy.name = "Enemy_" + i;
                WireEnemyTarget(enemy, player);
            }
        }

        /// <summary>
        /// 把"追谁"告诉敌人。这一步是必须的联动：
        /// 预制体自己不知道场景里的玩家是谁，要在生成的时候牵线。
        /// 以后你的敌人脚本不叫 DummyEnemy，只要也有 target 字段，在这里加一行即可。
        /// </summary>
        static void WireEnemyTarget(GameObject enemy, Transform player)
        {
            DummyEnemy dummy = enemy.GetComponent<DummyEnemy>();
            if (dummy != null) dummy.target = player;
        }
    }
}
