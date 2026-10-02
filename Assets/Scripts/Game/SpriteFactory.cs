using UnityEngine;

namespace WaterSurvivor
{
    /// <summary>
    /// 纯代码生成精灵，工程里不用放任何图片（所以永远不会有"资源丢失"的坑）。
    ///
    /// 生成的贴图都是白色的，颜色靠 SpriteRenderer.color 染，
    /// 这样一张圆图能用在水弹、玩家、敌人身上。
    ///
    /// 尺寸约定：生成的精灵宽高都是【1 个世界单位】，
    /// 所以 transform.localScale = 直径，CircleCollider2D.radius = 0.5 就是刚好贴合。
    /// </summary>
    public static class SpriteFactory
    {
        const int TexSize = 64;

        static Sprite _circle;
        static Sprite _square;

        /// <summary>白色实心圆（直径 1 世界单位）。</summary>
        public static Sprite Circle
        {
            get
            {
                if (_circle == null) _circle = Build(CircleTexture(), "GenCircle");
                return _circle;
            }
        }

        /// <summary>白色方块（1x1 世界单位）。</summary>
        public static Sprite Square
        {
            get
            {
                if (_square == null) _square = Build(SquareTexture(), "GenSquare");
                return _square;
            }
        }

        /// <summary>
        /// 一步生成"带精灵的物体"：位置、颜色、直径、渲染层级一次搞定。
        /// </summary>
        public static GameObject SpawnSprite(string name, Sprite sprite, Color color, float diameter, Vector3 position, int sortingOrder)
        {
            GameObject go = new GameObject(name);
            go.transform.position = position;
            go.transform.localScale = new Vector3(diameter, diameter, 1f);

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
