using System.Collections.Generic;
using UnityEngine;

namespace Runner.EditorTools
{
    /// <summary>
    /// A tiny software pixel buffer. Every sprite in the game is authored here so the project
    /// ships with no binary art and the whole look can be re-generated from one place.
    /// </summary>
    public sealed class PixelCanvas
    {
        public int Width { get; }
        public int Height { get; }
        public float PixelsPerUnit { get; set; } = SpriteWriter.PixelsPerUnit;

        private readonly Color32[] pixels;

        public PixelCanvas(int width, int height)
        {
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            pixels = new Color32[Width * Height];
        }

        public void Clear(Color color)
        {
            var c32 = color;
            for (var i = 0; i < pixels.Length; i++) pixels[i] = c32;
        }

        public Color32 Get(int x, int y) =>
            x < 0 || y < 0 || x >= Width || y >= Height ? new Color32(0, 0, 0, 0) : pixels[y * Width + x];

        /// <summary>Alpha-blended write; out-of-bounds pixels are dropped.</summary>
        public void Set(int x, int y, Color color)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height || color.a <= 0f) return;

            var index = y * Width + x;
            var a = color.a;

            if (a >= 0.999f)
            {
                pixels[index] = color;
                return;
            }

            var target = pixels[index];
            var r = Mathf.LerpUnclamped(target.r / 255f, color.r, a);
            var g = Mathf.LerpUnclamped(target.g / 255f, color.g, a);
            var b = Mathf.LerpUnclamped(target.b / 255f, color.b, a);
            var alpha = Mathf.Clamp01(target.a / 255f + a);
            pixels[index] = new Color(r, g, b, alpha);
        }

        public void FillRect(int x, int y, int w, int h, Color color)
        {
            for (var py = y; py < y + h; py++)
            for (var px = x; px < x + w; px++) Set(px, py, color);
        }

        public void FrameRect(int x, int y, int w, int h, Color color)
        {
            FillRect(x, y, w, 1, color);
            FillRect(x, y + h - 1, w, 1, color);
            FillRect(x, y, 1, h, color);
            FillRect(x + w - 1, y, 1, h, color);
        }

        public void FillRoundRect(int x, int y, int w, int h, int radius, Color color)
        {
            FillRect(x, y + radius, w, h - radius * 2, color);
            FillRect(x + radius, y, w - radius * 2, h, color);
            FillCircle(x + radius, y + radius, radius, color);
            FillCircle(x + w - radius - 1, y + radius, radius, color);
            FillCircle(x + radius, y + h - radius - 1, radius, color);
            FillCircle(x + w - radius - 1, y + h - radius - 1, radius, color);
        }

        public void FillCircle(int cx, int cy, int radius, Color color)
        {
            var r2 = radius * radius;
            for (var y = -radius; y <= radius; y++)
            for (var x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= r2) Set(cx + x, cy + y, color);
            }
        }

        public void FillEllipse(int cx, int cy, int rx, int ry, Color color)
        {
            if (rx <= 0 || ry <= 0) return;

            for (var y = -ry; y <= ry; y++)
            for (var x = -rx; x <= rx; x++)
            {
                var nx = (float)x / rx;
                var ny = (float)y / ry;
                if (nx * nx + ny * ny <= 1f) Set(cx + x, cy + y, color);
            }
        }

        public void Line(Vector2 a, Vector2 b, float thickness, Color color)
        {
            var delta = b - a;
            var steps = Mathf.Max(1, Mathf.CeilToInt(Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y))));
            var half = thickness * 0.5f;

            for (var i = 0; i <= steps; i++)
            {
                var t = (float)i / steps;
                var p = a + delta * t;

                for (var oy = Mathf.FloorToInt(-half); oy <= Mathf.CeilToInt(half); oy++)
                for (var ox = Mathf.FloorToInt(-half); ox <= Mathf.CeilToInt(half); ox++)
                {
                    if (ox * ox + oy * oy > half * half + 0.35f) continue;
                    Set(Mathf.RoundToInt(p.x) + ox, Mathf.RoundToInt(p.y) + oy, color);
                }
            }
        }

        /// <summary>Even-odd polygon fill, which is what most of the silhouettes are made of.</summary>
        public void FillPolygon(IList<Vector2> points, Color color)
        {
            if (points == null || points.Count < 3) return;

            var minY = int.MaxValue;
            var maxY = int.MinValue;

            for (var i = 0; i < points.Count; i++)
            {
                minY = Mathf.Min(minY, Mathf.FloorToInt(points[i].y));
                maxY = Mathf.Max(maxY, Mathf.CeilToInt(points[i].y));
            }

            var intersections = new List<float>();

            for (var y = Mathf.Max(0, minY); y <= Mathf.Min(Height - 1, maxY); y++)
            {
                var scan = y + 0.5f;
                intersections.Clear();

                for (var i = 0; i < points.Count; i++)
                {
                    var a = points[i];
                    var b = points[(i + 1) % points.Count];

                    if (a.y == b.y) continue;
                    if (scan < Mathf.Min(a.y, b.y) || scan >= Mathf.Max(a.y, b.y)) continue;

                    intersections.Add(a.x + (scan - a.y) / (b.y - a.y) * (b.x - a.x));
                }

                intersections.Sort();

                for (var n = 0; n + 1 < intersections.Count; n += 2)
                {
                    var x0 = Mathf.Max(0, Mathf.RoundToInt(intersections[n]));
                    var x1 = Mathf.Min(Width - 1, Mathf.RoundToInt(intersections[n + 1]));
                    for (var x = x0; x <= x1; x++) Set(x, y, color);
                }
            }
        }

        public void GradientVertical(int x, int y, int w, int h, Color top, Color bottom)
        {
            for (var py = 0; py < h; py++)
            {
                var t = h <= 1 ? 0f : (float)py / (h - 1);
                FillRect(x, y + py, w, 1, Color.Lerp(bottom, top, t));
            }
        }

        public void DitherBand(int x, int y, int w, int h, Color color, float density)
        {
            for (var py = 0; py < h; py++)
            for (var px = 0; px < w; px++)
            {
                var hash = ((px + x) * 19 + (py + y) * 37) % 100 / 100f;
                if (hash < density) Set(x + px, y + py, color);
            }
        }

        public Texture2D ToTexture()
        {
            var texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "runtime_canvas"
            };

            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        public Sprite ToSprite(Vector2 pivot)
        {
            var texture = ToTexture();
            return Sprite.Create(texture, new Rect(0, 0, Width, Height), pivot, PixelsPerUnit, 0u,
                SpriteMeshType.FullRect);
        }

        /// <summary>Copies another canvas on top of this one, treating negative coordinates as clipping.</summary>
        public void Blit(PixelCanvas source, int offsetX, int offsetY)
        {
            for (var y = 0; y < source.Height; y++)
            for (var x = 0; x < source.Width; x++)
            {
                var color = source.Get(x, y);
                if (color.a < 0.01f) continue;
                Set(offsetX + x, offsetY + y, color);
            }
        }
    }
}
