#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using WaterSurvivor;   // 引用运行时脚本（ArenaBootstrap / GameConfig / SimpleLog）

namespace WaterSurvivor.EditorTools
{
    /// <summary>
    /// 编辑器菜单：工具 → 水弹幸存者
    ///
    /// 三个初版测试用得上的小工具。带 #if UNITY_EDITOR 和放在 Assets/Editor 下，
    /// 所以这些代码只存在于编辑器，打包出去的游戏里没有。
    /// </summary>
    public static class WaterSurvivorMenu
    {
        const string MenuRoot = "工具/水弹幸存者/";

        // ------------------------------------------------------------------
        // 1. 在当前场景生成测试对象（想看 Inspector 里的对象时用这个）
        // ------------------------------------------------------------------
        [MenuItem(MenuRoot + "在当前场景生成测试对象", false, 10)]
        static void BuildInScene()
        {
            ArenaBootstrap bootstrap = Object.FindObjectOfType<ArenaBootstrap>();

            if (bootstrap == null)
            {
                GameObject go = new GameObject("ArenaBootstrap");
                Undo.RegisterCreatedObjectUndo(go, "生成水弹幸存者测试对象");
                bootstrap = go.AddComponent<ArenaBootstrap>();
            }

            bootstrap.Build();

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Selection.activeGameObject = bootstrap.gameObject;

            Debug.Log("[水弹幸存者] 已在当前场景生成测试对象（记得 Ctrl+S 保存场景）。");
        }

        // ------------------------------------------------------------------
        // 2. 检查 / 补上 Enemy 标签
        // ------------------------------------------------------------------
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
                "已补上 \"" + GameConfig.EnemyTag + "\" 标签。", "好");
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

        // ------------------------------------------------------------------
        // 3. debug.log 相关
        // ------------------------------------------------------------------
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
