using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WaterSurvivor
{
    /// <summary>
    /// 把美术图变成游戏里能直接用的 Sprite。
    ///
    /// 【为什么要"烘焙成资源文件"而不是每次在内存里画】
    /// 早先版本是在内存里生成 Sprite 的，运行时没问题，
    /// 但编辑器保存**预制体**时无法保存"内存里临时造出来的对象"的引用，
    /// 结果预制体里的 Sprite 变成 None，场景里角色就没画面了。
    /// 所以现在改成：
    ///   编辑器里（没在跑游戏）→ 把图片处理一遍，**存成真正的 png 资源**（Assets/Resources/Art/Processed/），
    ///                          预制体、场景引用的是这个资源，能正常保存；
    ///   运行时              → 优先直接读那个烘焙好的资源；
    ///   还没烘焙过就按 Play  → 在内存里临时处理一份，保证照样能看到图。
    ///
    /// 美术/策划只需要知道：
    ///     **换图 = 用同名文件覆盖 Assets/Resources/Art/ 下的图，重新点一次菜单或重新 Play。**
    /// 白底会被自动抠掉、四周空白会被自动裁掉、粘连的碎片会被自动忽略。
    /// </summary>
    public static class ArtLibrary
    {
        /// <summary>烘焙结果的资源路径（相对 Assets/Resources/，不带扩展名）。</summary>
        public const string ProcessedResourceFolder = "Art/Processed";

        /// <summary>烘焙结果的工程路径。</summary>
        public const string ProcessedAssetFolder = "Assets/Resources/Art/Processed";

        static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

        /// <summary>取一张美术图，worldSize = 这张图（裁剪后）最长边占多少个世界单位。</summary>
        public static Sprite Get(string name, float worldSize)
        {
            bool ignored;
            return Get(name, worldSize, out ignored);
        }

        /// <summary>同上，usedArt = false 表示没用上美术图（退回成代码画的圆了）。</summary>
        public static Sprite Get(string name, float worldSize, out bool usedArt)
        {
            float size = Mathf.Max(0.01f, worldSize);
            string key = name + "|" + size.ToString("F3");

            Sprite cached;
            if (_cache.TryGetValue(key, out cached) && cached != null)
            {
                usedArt = !IsGeneratedFallback(cached);
                return cached;
            }

            Sprite sprite = null;
            usedArt = false;

#if UNITY_EDITOR
            // 编辑器里（没在运行游戏）：重新烘焙一次，保证资源是最新的
            if (!Application.isPlaying)
            {
                sprite = BakeToAsset(name, size);
                usedArt = sprite != null;
            }
#endif

            // 运行时（或烘焙失败）：直接用烘焙好的资源 —— 预制体里存的引用就是它
            if (sprite == null)
            {
                sprite = Resources.Load<Sprite>(ProcessedResourceName(name));
                usedArt = sprite != null;
            }

            // 还没烘焙过（比如一打开工程就直接 Play）：内存里临时处理一份
            if (sprite == null)
            {
                sprite = BuildInMemory(name, size, out usedArt);
            }

            // 兜底：图找不到也不能让游戏挂掉
            if (sprite == null)
            {
                sprite = SpriteFactory.CircleSized(size);
                usedArt = false;
                SimpleLog.Warn("[美术] 没能用上图片 " + name + "，这个角色先用代码画的圆代替。" +
                               "请检查 Assets/Resources/" + name + ".jpg / .png 是否存在。");
            }

            _cache[key] = sprite;
            return sprite;
        }

        /// <summary>烘焙结果的资源路径，例如 "Art/Processed/Player"。</summary>
        public static string ProcessedResourceName(string name)
        {
            return ProcessedResourceFolder + "/" + Path.GetFileName(name);
        }

        /// <summary>烘焙结果的工程路径，例如 "Assets/Resources/Art/Processed/Player.png"。</summary>
        public static string ProcessedAssetPath(string name)
        {
            return "Assets/Resources/" + ProcessedResourceName(name) + ".png";
        }

        static bool IsGeneratedFallback(Sprite sp)
        {
            return sp != null && sp.name.StartsWith("GenCircle");
        }

        // ==================================================================
        // 内存处理（运行时兜底：不用先跑菜单也能看到图）
        // ==================================================================
        static Sprite BuildInMemory(string name, float worldSize, out bool usedArt)
        {
            usedArt = false;
            if (!GameConfig.UseArtImages) return null;

            Texture2D source = Resources.Load<Texture2D>(name);
            if (source == null) return null;

            Color32[] pixels;
            if (!TryProcess(source, out pixels)) return null;

            int w = source.width;
            int h = source.height;
            Rect rect = FindOpaqueRect(pixels, w, h, GameConfig.ArtCropPadding);

            // 复制一份再改，避免动到 Resources 里的原图资源
            Texture2D copy = new Texture2D(w, h, TextureFormat.RGBA32, false);
            copy.hideFlags = HideFlags.HideAndDontSave;
            copy.wrapMode = TextureWrapMode.Clamp;
            copy.filterMode = FilterMode.Bilinear;
            copy.SetPixels32(pixels);
            copy.Apply();

            // pixelsPerUnit = 裁剪后最长边 / 想要的世界尺寸
            float ppu = Mathf.Max(rect.width, rect.height) / worldSize;
            Sprite sp = Sprite.Create(copy, rect, new Vector2(0.5f, 0.5f), ppu);
            sp.name = Path.GetFileName(name) + "_Runtime";
            usedArt = true;
            return sp;
        }

        // ==================================================================
        // 像素处理：抠白底 + 去碎片
        // ==================================================================
        static bool TryProcess(Texture2D source, out Color32[] pixels)
        {
            pixels = null;
            if (source == null) return false;

            try
            {
                pixels = source.GetPixels32();   // 需要图片勾了 Read/Write
            }
            catch
            {
                SimpleLog.Warn("[美术] 图片 " + source.name + " 不能读像素（Read/Write 没勾）。" +
                               "工程里带了自动导入脚本，正常不会这样；如果碰到了，在 Inspector 里把 Read/Write 勾上即可。");
                return false;
            }

            int w = source.width;
            int h = source.height;

            if (GameConfig.RemoveWhiteBackground)
            {
                KnockOutWhiteBackground(pixels, w, h);

                // 去掉碎片：原图边缘常常粘着别的图的一小块，
                // 留着会把裁剪框撑宽，角色看起来偏到一边、和碰撞体也对不上。
                if (GameConfig.KeepOnlyLargestBlob) KeepLargestBlob(pixels, w, h);
            }

            return true;
        }

        /// <summary>
        /// 把"和图片边缘相连的白色"抠成透明。
        ///
        /// 为什么不是把图里所有白色都变透明：主角的脸本身就是白的，
        /// 那样会把脸也抠掉。所以用"从四条边往里灌"的办法（洪水填充），
        /// 只吃掉和外部连通的背景白，角色内部的白色保留。
        /// </summary>
        static void KnockOutWhiteBackground(Color32[] pixels, int w, int h)
        {
            // 判定"够白"的阈值：1 = 只要不是纯黑都算白，0 = 必须纯白
            float tolerance = Mathf.Clamp01(GameConfig.WhiteBackgroundTolerance);
            byte threshold = (byte)Mathf.RoundToInt(255f * (1f - tolerance));

            bool[] visited = new bool[w * h];
            Stack<int> stack = new Stack<int>(w * 2 + h * 2);

            // 从四条边上的白像素开始往里灌
            for (int x = 0; x < w; x++)
            {
                PushIfWhite(pixels, visited, stack, x, threshold);
                PushIfWhite(pixels, visited, stack, (h - 1) * w + x, threshold);
            }
            for (int y = 0; y < h; y++)
            {
                PushIfWhite(pixels, visited, stack, y * w, threshold);
                PushIfWhite(pixels, visited, stack, y * w + (w - 1), threshold);
            }

            while (stack.Count > 0)
            {
                int index = stack.Pop();
                Color32 c = pixels[index];
                c.a = 0;
                pixels[index] = c;

                int px = index % w;
                int py = index / w;

                if (px > 0) PushIfWhite(pixels, visited, stack, index - 1, threshold);
                if (px < w - 1) PushIfWhite(pixels, visited, stack, index + 1, threshold);
                if (py > 0) PushIfWhite(pixels, visited, stack, index - w, threshold);
                if (py < h - 1) PushIfWhite(pixels, visited, stack, index + w, threshold);
            }
        }

        static void PushIfWhite(Color32[] pixels, bool[] visited, Stack<int> stack, int index, byte threshold)
        {
            if (index < 0 || index >= pixels.Length) return;
            if (visited[index]) return;
            visited[index] = true;

            Color32 c = pixels[index];
            if (c.r >= threshold && c.g >= threshold && c.b >= threshold) stack.Push(index);
        }

        /// <summary>
        /// 只保留最大的一块不透明区域（也就是角色本体），其它零碎的小块全部变透明。
        /// 这样即使原图边缘有杂物、或有 JPEG 噪点，也不会影响角色大小和居中。
        /// </summary>
        static void KeepLargestBlob(Color32[] pixels, int w, int h)
        {
            int n = w * h;
            int[] label = new int[n];              // 0 = 没归过类
            bool[] visited = new bool[n];
            Stack<int> stack = new Stack<int>();

            int currentLabel = 0;
            int bestLabel = 0;
            int bestCount = 0;

            for (int start = 0; start < n; start++)
            {
                if (visited[start]) continue;
                visited[start] = true;

                if (pixels[start].a <= 8) continue;   // 透明像素不参与连通

                currentLabel++;
                int count = 0;

                stack.Push(start);
                while (stack.Count > 0)
                {
                    int index = stack.Pop();
                    label[index] = currentLabel;
                    count++;

                    int px = index % w;
                    int py = index / w;

                    // 上下左右四个方向，把不透明且没归类过的邻居也拉进来
                    if (px > 0) PushBlob(pixels, visited, stack, index - 1);
                    if (px < w - 1) PushBlob(pixels, visited, stack, index + 1);
                    if (py > 0) PushBlob(pixels, visited, stack, index - w);
                    if (py < h - 1) PushBlob(pixels, visited, stack, index + w);
                }

                if (count > bestCount)
                {
                    bestCount = count;
                    bestLabel = currentLabel;
                }
            }

            if (bestLabel == 0) return;   // 整张图都是透明的，不用处理

            for (int i = 0; i < n; i++)
            {
                if (pixels[i].a <= 8) continue;
                if (label[i] == bestLabel) continue;

                Color32 c = pixels[i];
                c.a = 0;
                pixels[i] = c;
            }
        }

        static void PushBlob(Color32[] pixels, bool[] visited, Stack<int> stack, int index)
        {
            if (visited[index]) return;
            visited[index] = true;
            if (pixels[index].a > 8) stack.Push(index);
        }

        /// <summary>
        /// 找出"不透明的像素"所占的矩形区域（也就是角色真正占的地方）。
        /// 全透明的话就退回整张图，避免算出 0 宽的精灵。
        /// </summary>
        static Rect FindOpaqueRect(Color32[] pixels, int w, int h, int padding)
        {
            int minX = w, minY = h, maxX = -1, maxY = -1;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (pixels[y * w + x].a <= 8) continue;   // 基本透明的当背景

                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }

            if (maxX < minX || maxY < minY) return new Rect(0f, 0f, w, h);   // 整张都是透明的

            minX = Mathf.Max(0, minX - padding);
            minY = Mathf.Max(0, minY - padding);
            maxX = Mathf.Min(w - 1, maxX + padding);
            maxY = Mathf.Min(h - 1, maxY + padding);

            return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

#if UNITY_EDITOR
        // ==================================================================
        // 烘焙成真正的资源文件（编辑器专用）
        //
        // 这一步是修好"预制体里图片是空的"的关键：
        // 存成 png 资源之后，预制体/场景才能保存对它的引用。
        // ==================================================================

        /// <summary>
        /// 这张图默认按多大的世界尺寸烘焙。
        /// （源图换了要自动重烘焙时，得知道它属于主角还是小怪。）
        /// </summary>
        public static float DefaultSizeFor(string resourceName)
        {
            if (resourceName == GameConfig.PlayerArtName) return GameConfig.PlayerArtSize;
            if (resourceName == GameConfig.EnemyArtName) return GameConfig.EnemyArtSize;
            return 1f;
        }

        /// <summary>
        /// 源图（Assets/Resources/... 下的原图）有变化时调用：
        /// **按同一路径重新烘焙**，这样预制体/场景里存的引用不会断，图会自动更新。
        /// 源图被删掉了就把成品图也删掉。
        /// </summary>
        public static void RebakeFromSource(string sourceAssetPath)
        {
            if (Application.isPlaying) return;   // 运行中不去动资源

            string resourceName = ToResourceName(sourceAssetPath);
            if (string.IsNullOrEmpty(resourceName)) return;

            try
            {
                _cache.Clear();

                if (Resources.Load<Texture2D>(resourceName) == null)
                {
                    // 源图没了 → 成品图也清掉
                    string stale = ProcessedAssetPath(resourceName);
                    if (AssetDatabase.LoadAssetAtPath<Sprite>(stale) != null) AssetDatabase.DeleteAsset(stale);
                    return;
                }

                BakeToAsset(resourceName, DefaultSizeFor(resourceName));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[水弹幸存者] 自动重新烘焙失败：" + e.Message + "（可以手动点菜单 工具 → 水弹幸存者 → 1）");
            }
        }

        /// <summary>"Assets/Resources/Art/Player.jpg" → "Art/Player"；不是 Resources 下的图返回 null。</summary>
        static string ToResourceName(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return null;

            string p = assetPath.Replace('\\', '/');
            const string root = "Assets/Resources/";
            if (!p.StartsWith(root)) return null;

            p = p.Substring(root.Length);
            string ext = Path.GetExtension(p);
            if (!string.IsNullOrEmpty(ext)) p = p.Substring(0, p.Length - ext.Length);
            return p;
        }

        static Sprite BakeToAsset(string name, float worldSize)
        {
            if (!GameConfig.UseArtImages) return null;

            Texture2D source = Resources.Load<Texture2D>(name);
            if (source == null) return null;

            Color32[] pixels;
            if (!TryProcess(source, out pixels)) return null;

            int w = source.width;
            int h = source.height;
            Rect rect = FindOpaqueRect(pixels, w, h, GameConfig.ArtCropPadding);

            int cw = Mathf.Max(1, Mathf.RoundToInt(rect.width));
            int ch = Mathf.Max(1, Mathf.RoundToInt(rect.height));
            int x0 = Mathf.RoundToInt(rect.x);
            int y0 = Mathf.RoundToInt(rect.y);

            // 只把角色那块抠出来
            Color32[] cropped = new Color32[cw * ch];
            for (int y = 0; y < ch; y++)
            {
                for (int x = 0; x < cw; x++)
                {
                    int sx = Mathf.Clamp(x0 + x, 0, w - 1);
                    int sy = Mathf.Clamp(y0 + y, 0, h - 1);
                    cropped[y * cw + x] = pixels[sy * w + sx];
                }
            }

            Texture2D outTex = new Texture2D(cw, ch, TextureFormat.RGBA32, false);
            outTex.SetPixels32(cropped);
            outTex.Apply();
            byte[] png = outTex.EncodeToPNG();
            Object.DestroyImmediate(outTex);

            if (!AssetDatabase.IsValidFolder(ProcessedAssetFolder))
            {
                AssetDatabase.CreateFolder("Assets/Resources/Art", "Processed");
            }

            string assetPath = ProcessedAssetPath(name);
            File.WriteAllBytes(assetPath, png);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                // 关键：pixelsPerUnit 设成"裁剪后最长边 / 想要的世界尺寸"，
                // 这样预制体里引用这张图时，不用缩放就是正确大小。
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = Mathf.Max(cw, ch) / worldSize;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.isReadable = false;   // 烘焙好的图运行时不用再读像素了
                importer.SaveAndReimport();
            }

            Sprite baked = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (baked != null)
            {
                SimpleLog.Log("[美术] " + name + " 已处理并保存成资源：" + assetPath +
                              "（裁剪后 " + cw + "x" + ch + " 像素，游戏里最长边 " +
                              worldSize.ToString("F2") + " 世界单位）");
            }
            return baked;
        }
#endif
    }
}
