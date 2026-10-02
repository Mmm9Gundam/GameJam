using System.Collections.Generic;
using UnityEngine;

namespace WaterSurvivor
{
    /// <summary>
    /// 纯代码生成精灵，工程里不用放任何图片也能跑。
    ///
    /// 现在工程里已经有美术图了（Assets/Resources/Art/ 下），
    /// 这个类留在两处用：
    ///   1. 水弹、地板这些小东西还是代码画（省得美术做一堆小图）；
    ///   2. 图片万一丢了/没导入成功，角色会退回成代码画的圆，游戏不会崩。
    ///
    /// 尺寸约定：下面 [名字带 Sized] 的方法生成的精灵，
    /// 最长边正好等于你传入的 worldSize 个世界单位，
    /// 所以用它的时候 transform.localScale 保持 (1,1,1) 就行 ——
    /// 这一点和美术图（ArtLibrary）的口径一致，方便互换。
    /// </summary>
    public static class SpriteFactory
    {
        const int TexSize = 64;

        static Sprite _circle;
        static Sprite _square;
        static readonly Dictionary<float, Sprite> _sizedCircles = new Dictionary<float, Sprite>();
        static readonly Dictionary<float, Sprite> _sizedSquares = new Dictionary<float, Sprite>();

        /// <summary>白色实心圆（直径固定 1 世界单位，用 localScale 缩放）。</summary>
        public static Sprite Circle
        {
            get
            {
                if (_circle == null) _circle = Build(CircleTexture(), "GenCircle");
                return _circle;
            }
        }

        /// <summary>白色方块（固定 1x1 世界单位，用 localScale 缩放）。</summary>
        public static Sprite Square
        {
            get
            {
                if (_square == null) _square = Build(SquareTexture(), "GenSquare");
                return _square;
            }
        }

        /// <summary>生成"最长边 = worldSize"的圆，和美术图的尺寸口径一致。</summary>
        public static Sprite CircleSized(float worldSize)
        {
            return Sized(_sizedCircles, worldSize, "GenCircle");
        }

        /// <summary>生成"最长边 = worldSize"的方块。</summary>
        public static Sprite SquareSized(float worldSize)
        {
            return Sized(_sizedSquares, worldSize, "GenSquare");
        }

        static Sprite Sized(Dictionary<float, Sprite> cache, float worldSize, string name)
        {
            worldSize = Mathf.Max(0.01f, worldSize);

            Sprite cached;
            if (cache.TryGetValue(worldSize, out cached) && cached != null) return cached;

            // pixelsPerUnit = 贴图边长 / 想要的世界尺寸，于是整张图正好是 worldSize 个单位
            Sprite sp = Sprite.Create(
                name == "GenCircle" ? CircleTexture() : SquareTexture(),
                new Rect(0f, 0f, TexSize, TexSize),
                new Vector2(0.5f, 0.5f),
                TexSize / worldSize);
            sp.name = name + "_" + worldSize.ToString("F2");

            cache[worldSize] = sp;
            return sp;
        }

        /// <summary>
        /// 一步生成"带精灵的物体"：位置、颜色、直径、渲染层级一次搞定。
        /// 注意这里传的是会写到 localScale 上的缩放值，配 Circle/Square 用。
        /// </summary>
        public static GameObject SpawnSprite(string name, Sprite sprite, Color color, float scale, Vector3 position, int sortingOrder)
        {
            GameObject go = new GameObject(name);
            go.transform.position = position;
            go.transform.localScale = new Vector3(scale, scale, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = sortingOrder;   // 数字越大越靠前：地板 -100 < 敌人 4 < 玩家 5 < 子弹 10

            return go;
        }

        static Sprite Build(Texture2D tex, string name)
        {
            // 第 4 个参数 pixelsPerUnit = 贴图宽度，于是整张图正好是 1 个世界单位
            Sprite sp = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width);
            sp.name = name;
            return sp;
        }

        static Texture2D NewTexture()
        {
            Texture2D tex = new Texture2D(TexSize, TexSize, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.hideFlags = HideFlags.HideAndDontSave;   // 不参与场景保存，也不显示在 Project 窗口里
            return tex;
        }

        static Texture2D CircleTexture()
        {
            Texture2D tex = NewTexture();

            float radius = TexSize * 0.5f - 1f;
            Vector2 center = new Vector2(TexSize * 0.5f, TexSize * 0.5f);
            Color[] pixels = new Color[TexSize * TexSize];

            for (int y = 0; y < TexSize; y++)
            {
                for (int x = 0; x < TexSize; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    // 边缘 1 像素做渐变，圆边不至于全是锯齿
                    float alpha = Mathf.Clamp01(radius - dist + 0.5f);
                    pixels[y * TexSize + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        static Texture2D SquareTexture()
        {
            Texture2D tex = NewTexture();
            Color[] pixels = new Color[TexSize * TexSize];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
    }
}
