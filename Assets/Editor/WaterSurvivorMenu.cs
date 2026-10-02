#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using WaterSurvivor;

namespace WaterSurvivor.EditorTools
{
    /// <summary>
    /// 编辑器菜单：**工具 → 水弹幸存者**
    ///
    /// 这个文件是给你的"手动挡"：点几下菜单，就能把
    /// 预制体、场景、标签、日志这些准备工作一次性做好，
    /// 之后所有调整都在 Inspector 和场景里完成，不需要改代码。
    ///
    /// 带 #if UNITY_EDITOR 且放在 Assets/Editor 下，
    /// 所以这些代码只存在于编辑器，打包出来的游戏里没有。
    /// </summary>
    public static class WaterSurvivorMenu
    {
        const string MenuRoot = "工具/水弹幸存者/";
        const string PrefabDir = "Assets/Prefabs";
        const string SceneDir = "Assets/Scenes";
        const string TestScenePath = SceneDir + "/WaterSurvivorTest.scene";
        const string PlayerPrefabPath = PrefabDir + "/Player.prefab";
        const string EnemyPrefabPath = PrefabDir + "/Enemy.prefab";

        // ==================================================================
        // 1. 生成预制体（组长要的"玩家预制体"）
        // ==================================================================
        [MenuItem(MenuRoot + "1. 生成预制体（Player / Enemy）", false, 1)]
        static void CreatePrefabs()
        {
            EnsurePrefabs();

            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            Selection.activeObject = playerPrefab;
            EditorGUIUtility.PingObject(playerPrefab);

            Debug.Log("[水弹幸存者] 预制体已就绪：\n  " + PlayerPrefabPath + "\n  " + EnemyPrefabPath +
                      "\n在 Project 窗口双击它们，就能在 Inspector 里换图片、改速度、加组件。");
        }

        /// <summary>
        /// 没有预制体就现场生成；**已有但图片引用是空的也重建**。
        ///
        /// 为什么要检查"图片是不是空的"：
        /// 早期版本把"内存里临时造的图片"塞进预制体，编辑器保存不下来，
        /// 预制体里的 Sprite 就变成 None，游戏里角色有逻辑但没画面。
        /// 现在图片会先烘焙成真正的资源文件再引用，所以重建一次就好了。
        /// </summary>
        static GameObject[] EnsurePrefabs()
        {
            if (!Directory.Exists(PrefabDir)) AssetDatabase.CreateFolder("Assets", "Prefabs");

            if (PrefabNeedsRebuild(PlayerPrefabPath))
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath) != null)
                {
                    Debug.Log("[水弹幸存者] Player.prefab 里的图片引用是空的（早期版本生成的问题），正在重建…");
                }
                GameObject temp = ActorFactory.BuildPlayer(Vector3.zero);
                PrefabUtility.SaveAsPrefabAsset(temp, PlayerPrefabPath);
                Object.DestroyImmediate(temp);
            }

            if (PrefabNeedsRebuild(EnemyPrefabPath))
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath) != null)
                {
                    Debug.Log("[水弹幸存者] Enemy.prefab 里的图片引用是空的（早期版本生成的问题），正在重建…");
                }
                // 敌人在预制体里先不填 target，放到场景里由 ArenaBootstrap 牵线
                GameObject temp = ActorFactory.BuildEnemy(Vector3.zero, null);
                PrefabUtility.SaveAsPrefabAsset(temp, EnemyPrefabPath);
                Object.DestroyImmediate(temp);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return new GameObject[]
            {
                AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath),
                AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath)
            };
        }

        /// <summary>预制体不存在、或者里面的图片引用是空的，就该重建。</summary>
        static bool PrefabNeedsRebuild(string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) return true;

            SpriteRenderer sr = prefab.GetComponentInChildren<SpriteRenderer>();
            return sr == null || sr.sprite == null;
        }

        // ==================================================================
        // 2. 生成测试场景（推荐路线：之后都在场景里改）
        // ==================================================================
        [MenuItem(MenuRoot + "2. 生成测试场景（推荐）", false, 2)]
        static void CreateTestScene()
        {
            // 先问一下要不要保存当前场景，避免把别人没存的东西弄丢
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            GameObject[] prefabs = EnsurePrefabs();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            PopulateScene(prefabs[0], prefabs[1]);

            if (!Directory.Exists(SceneDir)) AssetDatabase.CreateFolder("Assets", "Scenes");

            if (!EditorSceneManager.SaveScene(scene, TestScenePath))
            {
                Debug.LogError("[水弹幸存者] 场景保存失败：" + TestScenePath);
                return;
            }

            AddSceneToBuildSettings(TestScenePath);

            Debug.Log("[水弹幸存者] 测试场景已生成：" + TestScenePath +
                      "\n相机、地板、玩家、6 个敌人都在 Hierarchy 里，直接按 Play 就能玩；" +
                      "\n想换图片：Project 窗口双击 Assets/Prefabs/Player.prefab，改 Sprite Renderer 的 Sprite。");
        }

        // ==================================================================
        // 3. 在当前场景生成（不新建场景）
        // ==================================================================
        [MenuItem(MenuRoot + "3. 在当前场景生成测试对象", false, 3)]
        static void BuildInCurrentScene()
        {
            GameObject[] prefabs = EnsurePrefabs();
            PopulateScene(prefabs[0], prefabs[1]);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[水弹幸存者] 已在当前场景生成测试对象（记得 Ctrl+S 保存场景）。");
        }

        /// <summary>往当前场景里摆相机、地板、玩家、敌人，并挂一个 ArenaBootstrap 负责牵线。</summary>
        static void PopulateScene(GameObject playerPrefab, GameObject enemyPrefab)
        {
            // ---- 相机 ----
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
            }
            cam.orthographic = true;
            cam.orthographicSize = GameConfig.ArenaHalfHeight;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.transform.rotation = Quaternion.identity;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = GameConfig.ColorCameraBg;

            // ---- 地板 ----
            ActorFactory.BuildFloor();

            // ---- 玩家（实例化预制体，方便你在 Inspector 里改它）----
            GameObject player = null;
            if (playerPrefab != null)
            {
                player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                player.transform.position = Vector3.zero;
                player.name = ActorFactory.PlayerName;
            }

            // ---- 敌人：围着玩家撒一圈 ----
            int count = Mathf.Max(0, GameConfig.DummyEnemyCount);
            for (int i = 0; i < count; i++)
            {
                if (enemyPrefab == null) continue;

                float angle = 360f / Mathf.Max(1, count) * i + 15f;
                float rad = angle * Mathf.Deg2Rad;
                float ring = 4.5f;

                Vector3 pos = new Vector3(Mathf.Cos(rad) * ring, Mathf.Sin(rad) * ring, 0f);
                pos.x = Mathf.Clamp(pos.x, -GameConfig.ArenaHalfWidth + 0.6f, GameConfig.ArenaHalfWidth - 0.6f);
                pos.y = Mathf.Clamp(pos.y, -GameConfig.ArenaHalfHeight + 0.6f, GameConfig.ArenaHalfHeight - 0.6f);

                GameObject enemy = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab);
                enemy.transform.position = pos;
                enemy.name = "Enemy_" + i;
            }

            // ---- 一个 GameRoot：记住用哪个预制体，并把"追谁"告诉每个敌人 ----
            GameObject root = new GameObject("GameRoot");
            ArenaBootstrap bootstrap = root.AddComponent<ArenaBootstrap>();
            bootstrap.playerPrefab = playerPrefab;
            bootstrap.enemyPrefab = enemyPrefab;

            // 场景里已经摆好了对象，按 Play 时不要再生成一遍，否则会翻倍
            bootstrap.buildOnStart = false;
            bootstrap.spawnEnemies = false;

            if (player != null)
            {
                foreach (DummyEnemy enemy in Object.FindObjectsOfType<DummyEnemy>())
                {
                    enemy.target = player.transform;
                }
            }
        }

        static void AddSceneToBuildSettings(string path)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            foreach (EditorBuildSettingsScene s in scenes)
            {
                if (s.path == path) return;   // 已经在列表里了，别重复加
            }

            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        // ==================================================================
        // 4. Enemy 标签
        // ==================================================================
        [MenuItem(MenuRoot + "检查/补上 Enemy 标签", false, 11)]
        static void EnsureEnemyTag()
        {
            if (HasTag(GameConfig.EnemyTag))
            {
                EditorUtility.DisplayDialog("水弹幸存者",
                    "工程里已经有 \"" + GameConfig.EnemyTag + "\" 标签，不用补。", "好");
                return;
            }

            AddTag(GameConfig.EnemyTag);
            EditorUtility.DisplayDialog("水弹幸存者",
                "已补上 \"" + GameConfig.EnemyTag + "\" 标签。\n" +
                "（水弹靠这个标签判断打中了谁，敌人身上必须挂它，详见 README）", "好");
        }

        static bool HasTag(string tag)
        {
            string[] tags = InternalEditorUtility.tags;
            for (int i = 0; i < tags.Length; i++)
            {
                if (tags[i] == tag) return true;
            }
            return false;
        }

        static void AddTag(string tag)
        {
            // Unity 没有公开的 API 加标签，标准做法是拿 TagManager.asset 当序列化对象改
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0)
            {
                Debug.LogError("[水弹幸存者] 找不到 ProjectSettings/TagManager.asset，标签没加上。");
                return;
            }

            SerializedObject so = new SerializedObject(assets[0]);
            SerializedProperty tagsProp = so.FindProperty("tags");

            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;

            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        // ==================================================================
        // 5. debug.log 相关
        // ==================================================================
        [MenuItem(MenuRoot + "打开 debug.log 所在文件夹", false, 30)]
        static void RevealLog()
        {
            string path = SimpleLog.ResolveFilePath();
            string dir = Path.GetDirectoryName(path);

            if (File.Exists(path))
            {
                EditorUtility.RevealInFinder(path);
            }
            else if (Directory.Exists(dir))
            {
                Debug.Log("[水弹幸存者] debug.log 还没生成，先打开所在文件夹：" + dir + "（按 Play 跑一下就有内容了）");
                EditorUtility.RevealInFinder(dir);
            }
            else
            {
                Debug.LogWarning("[水弹幸存者] 目录不存在：" + dir);
            }
        }

        [MenuItem(MenuRoot + "清空 debug.log", false, 31)]
        static void ClearLog()
        {
            string path = SimpleLog.ResolveFilePath();

            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    Debug.Log("[水弹幸存者] 已删除 " + path);
                }
                else
                {
                    Debug.Log("[水弹幸存者] debug.log 本来就不存在：" + path);
                }
            }
            catch (System.Exception e)
            {
                // Play 模式下文件正被游戏占着，删不掉很正常
                Debug.LogWarning("[水弹幸存者] 删除失败（Play 模式下会被占用，先停止运行再删）：" + e.Message);
            }
        }
    }
}
#endif
