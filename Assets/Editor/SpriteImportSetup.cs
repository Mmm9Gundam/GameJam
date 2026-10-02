using System.IO;
using UnityEditor;
using UnityEngine;

namespace WaterSurvivor.EditorTools
{
    /// <summary>
    /// 自动导入设置：只要图片放在名字带 "Art" 的文件夹里
    /// （本工程是 Assets/Resources/Art/），就自动按下面这套设置导入：
    ///
    ///   Texture Type = Sprite        否则代码拿不到 Sprite
    ///   Read/Write   = 勾上          代码要去掉白底、必须能读像素
    ///   压缩          = 不压缩        小图没必要压，避免边缘变脏
    ///   不生成 Mipmap                 2D 游戏用不上
    ///
    /// 美术把 jpg/png 拖进 Assets/Resources/Art/ 就能用，不用手动改 Inspector。
    ///
    /// 注意：Art/Processed/ 是程序烘焙出来的成品（已经抠好白底、裁好边），
    /// 那些文件由 ArtLibrary 自己设置导入参数，这里要跳过，否则会把
    /// "每像素多少单位"这个关键参数冲掉，角色大小就错了。
    /// </summary>
    public class SpriteImportSetup : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            string path = assetPath.Replace('\\', '/');
            if (!path.Contains("/Art/")) return;
            if (path.Contains("/Processed/")) return;      // 成品图不归这里管

            TextureImporter importer = assetImporter as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.isReadable = true;                                  // 代码要读像素抠白底
            importer.textureCompression = TextureImporterCompression.Uncompressed;
        }

        /// <summary>
        /// 源图被换掉/删掉了 → 稍后**按同一路径重新烘焙**成品图。
        ///
        /// 注意这里不能直接删成品图：预制体和场景引用的是成品图，
        /// 删掉的话引用立刻变成 None，角色会在编辑器里当场消失。
        /// 重新烘焙同一路径则引用一直有效，图会自动换成新的。
        /// </summary>
        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            foreach (string p in imported) ScheduleRebake(p);
            foreach (string p in deleted) ScheduleRebake(p);
            foreach (string p in movedFrom) ScheduleRebake(p);
        }

        static void ScheduleRebake(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            path = path.Replace('\\', '/');
            if (!path.Contains("/Art/")) return;
            if (path.Contains("/Processed/")) return;       // 成品自己不触发
            if (!path.StartsWith("Assets/Resources/")) return;
            if (!IsImage(path)) return;

            // delayCall：等这次导入彻底结束再动手，避免导入过程中再导入
            EditorApplication.delayCall += () => ArtLibrary.RebakeFromSource(path);
        }

        static bool IsImage(string path)
        {
            string lower = path.ToLowerInvariant();
            return lower.EndsWith(".png") || lower.EndsWith(".jpg") || lower.EndsWith(".jpeg")
                || lower.EndsWith(".psd") || lower.EndsWith(".tga") || lower.EndsWith(".bmp");
        }
    }

    /// <summary>
    /// 启动自检（解决一个很隐蔽的顺序问题）：
    ///
    /// Unity 第一次打开工程时，有可能**先把图片导入了、才编译出上面的自动导入脚本**，
    /// 那样图片就会带着默认设置（不是 Sprite、也不能读像素），游戏里就变成圆形了。
    ///
    /// 所以这里在编辑器启动后检查一遍 Art 文件夹：
    /// 发现哪张图设置不对，就强制重新导入一次（重新导入会再走一次 OnPreprocessTexture）。
    /// </summary>
    [InitializeOnLoad]
    public static class ArtImportChecker
    {
        const string ArtFolder = "Assets/Resources/Art";

        static ArtImportChecker()
        {
            // delayCall：等编辑器把资源数据库准备好再动手
            EditorApplication.delayCall += FixArtImports;
        }

        static void FixArtImports()
        {
            if (!Directory.Exists(ArtFolder)) return;

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { ArtFolder });
            int fixedCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                // 烘焙成品由 ArtLibrary 管，跳过
                if (path.Replace('\\', '/').Contains("/Processed/")) continue;

                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                bool ok = importer.textureType == TextureImporterType.Sprite && importer.isReadable;
                if (ok) continue;

                // 设置不对 → 强制重导一次，让它走 OnPreprocessTexture
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                fixedCount++;
            }

            if (fixedCount > 0)
            {
                Debug.Log("[水弹幸存者] 已自动修正 " + fixedCount + " 张美术图的导入设置（Sprite + Read/Write）。");
            }
        }
    }
}
