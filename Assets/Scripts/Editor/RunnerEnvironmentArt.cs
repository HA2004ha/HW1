using System.Collections.Generic;
using Runner.Core;
using UnityEngine;

namespace Runner.EditorTools
{
    /// <summary>
    /// The world's backdrop. Every layer is authored to the same tile width so the parallax rig
    /// can wrap them without seams, and the horizon lines are shared so the layers read as one
    /// place rather than four strips of texture.
    /// </summary>
    internal static class RunnerEnvironmentArt
    {
        public const int TileWidth = 640;
        public const int TileHeight = 360;

        public static Sprite Sky { get; private set; }
        public static Sprite FarMountains { get; private set; }
        public static Sprite City { get; private set; }
        public static Sprite Ridge { get; private set; }
        public static Sprite Road { get; private set; }
        public static Sprite Foreground { get; private set; }

        public static Sprite Panel { get; private set; }
        public static Sprite Button { get; private set; }
        public static Sprite RoundedBar { get; private set; }
        public static Sprite Square { get; private set; }
        public static Sprite Heart { get; private set; }

        private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);
        private static readonly Vector2 Bottom = new Vector2(0.5f, 0f);

        public static void Build()
        {
            Sky = SpriteWriter.Save("Environment", "bg_sky", BuildSky(), Center);
            FarMountains = SpriteWriter.Save("Environment", "bg_far", BuildMountains(), Bottom);
            City = SpriteWriter.Save("Environment", "bg_city", BuildCity(), Bottom);
            Ridge = SpriteWriter.Save("Environment", "bg_ridge", BuildRidge(), Bottom);
            Road = SpriteWriter.Save("Environment", "bg_road", BuildRoad(), Bottom);
            Foreground = SpriteWriter.Save("Environment", "bg_foreground", BuildForeground(), Bottom);

            Panel = SpriteWriter.Save("Ui", "ui_panel", BuildPanel(), Center, new Vector4(18, 18, 18, 18));
            Button = SpriteWriter.Save("Ui", "ui_button", BuildButton(), Center, new Vector4(10, 10, 10, 10));
            RoundedBar = SpriteWriter.Save("Ui", "ui_bar", BuildBar(), Center, new Vector4(7, 7, 7, 7));
            Square = SpriteWriter.Save("Ui", "ui_square", BuildSquare(), Center);
            Heart = SpriteWriter.Save("Ui", "ui_heart", BuildHeart(), Center);
        }

        private static PixelCanvas BuildSky()
        {
            var c = new PixelCanvas(TileWidth, TileHeight);

            c.GradientVertical(0, 0, TileWidth, TileHeight, Palette.Night, Palette.WithAlpha(Palette.Violet, 1f));
            c.GradientVertical(0, 0, TileWidth, 150, Palette.Violet, Palette.DeepPurple);
            c.GradientVertical(0, 60, TileWidth, 60, Palette.WithAlpha(Palette.Magenta, 0.85f), Palette.WithAlpha(Palette.Sunset, 0.2f));

            var stars = new RandomState(20260126);
            for (var i = 0; i < 260; i++)
            {
                var x = stars.Range(0, TileWidth);
                var y = stars.Range(170, TileHeight);
                var brightness = stars.NextFloat();
                c.FillRect(x, y, 1 + (brightness > 0.93f ? 1 : 0), 1,
                    Palette.WithAlpha(Palette.White, 0.25f + brightness * 0.65f));
            }

            DrawSun(c, 320, 132, 74);

            c.FillRect(0, 0, TileWidth, 62, Palette.WithAlpha(Palette.Night, 0.55f));
            return c;
        }

        private static void DrawSun(PixelCanvas c, int cx, int cy, int radius)
        {
            for (var y = -radius; y <= radius; y++)
            {
                var halfWidth = Mathf.RoundToInt(Mathf.Sqrt(Mathf.Max(0, radius * radius - y * y)));
                if (halfWidth <= 0) continue;

                var t = (y + radius) / (radius * 2f);
                var color = Color.Lerp(Palette.Magenta, Palette.Amber, t);

                // The classic retro sun: slits widen as they fall.
                var band = cy + y;
                if (y < 0)
                {
                    var depth = -y / (float)radius;
                    if (band % Mathf.Max(3, Mathf.RoundToInt(14 - depth * 9)) < Mathf.RoundToInt(depth * 6f)) continue;
                }

                c.FillRect(cx - halfWidth, band, halfWidth * 2, 1, color);
            }

            for (var y = -radius - 8; y <= radius + 8; y++)
            {
                var halfWidth = Mathf.RoundToInt(Mathf.Sqrt(Mathf.Max(0, (radius + 8) * (radius + 8) - y * y)));
                if (halfWidth < radius - 1 || halfWidth > radius + 3) continue;
                c.FillRect(cx - halfWidth, cy + y, halfWidth * 2, 1, Palette.WithAlpha(Palette.HotPink, 0.18f));
            }
        }

        private static PixelCanvas BuildMountains()
        {
            var c = new PixelCanvas(TileWidth, 200);
            var rng = new RandomState(7717);
            var ridge = new List<Vector2> { new Vector2(0, 0), new Vector2(0, 64) };

            var x = 0;
            while (x < TileWidth)
            {
                var peakWidth = rng.Range(70, 150);
                var peakHeight = rng.Range(96, 178);
                var mid = Mathf.Min(x + peakWidth / 2, TileWidth);
                var end = Mathf.Min(x + peakWidth, TileWidth);

                ridge.Add(new Vector2(mid, peakHeight));
                ridge.Add(new Vector2(end, 64));

                x = end;
            }

            ridge.Add(new Vector2(TileWidth, 0));
            c.FillPolygon(ridge, Palette.Violet);

            for (var i = 1; i < ridge.Count - 1; i += 2)
            {
                var peak = ridge[i];
                if (peak.x <= 0 || peak.x >= TileWidth - 1) continue;

                c.FillPolygon(new List<Vector2>
                {
                    peak,
                    peak + new Vector2(-16, -26),
                    peak + new Vector2(-6, -22),
                    peak + new Vector2(0, -30),
                    peak + new Vector2(8, -20),
                    peak + new Vector2(16, -26)
                }, Palette.WithAlpha(Palette.HotPink, 0.5f));
            }

            return c;
        }

        private static PixelCanvas BuildCity()
        {
            var c = new PixelCanvas(TileWidth, 240);
            var rng = new RandomState(4242);

            var x = 0;
            while (x < TileWidth)
            {
                var width = rng.Range(28, 66);
                var height = rng.Range(70, 210);
                var bx = x;
                var bw = Mathf.Min(width, TileWidth - x);
                if (bw <= 6) break;

                var body = rng.NextFloat() > 0.55f ? Palette.DeepPurple : Palette.WithAlpha(Palette.Night, 0.96f);
                c.FillRect(bx, 0, bw, height, body);
                c.FillRect(bx, height - 2, bw, 2, Palette.WithAlpha(Palette.RoadLine, 0.8f));
                c.FillRect(bx + bw - 1, 0, 1, height, Palette.WithAlpha(Palette.Night, 0.8f));

                for (var wy = 8; wy < height - 6; wy += 9)
                for (var wx = bx + 4; wx < bx + bw - 4; wx += 8)
                {
                    if (rng.NextFloat() < 0.42f) continue;
                    var lit = rng.NextFloat();
                    c.FillRect(wx, wy, 3, 4, lit > 0.72f
                        ? Palette.Amber
                        : Palette.WithAlpha(Palette.IceBlue, 0.35f + lit * 0.4f));
                }

                if (height > 150 && bw > 30)
                {
                    c.FillRect(bx + bw / 2 - 1, height, 2, 14, Palette.ChassisDark);
                    c.FillCircle(bx + bw / 2, height + 15, 2, Palette.Damage);
                }

                x += width + rng.Range(2, 12);
            }

            c.FillRect(0, 0, TileWidth, 10, Palette.Night);
            return c;
        }

        private static PixelCanvas BuildRidge()
        {
            var c = new PixelCanvas(TileWidth, 120);
            var rng = new RandomState(9091);
            var points = new List<Vector2> { new Vector2(0, 0), new Vector2(0, 34) };

            var x = 0;
            while (x < TileWidth)
            {
                var step = rng.Range(46, 96);
                var height = rng.Range(38, 92);
                var end = Mathf.Min(x + step, TileWidth);
                points.Add(new Vector2(Mathf.Min(x + step / 2, TileWidth), height));
                points.Add(new Vector2(end, 34));
                x = end;
            }

            points.Add(new Vector2(TileWidth, 0));
            c.FillPolygon(points, Palette.Night);

            for (var i = 1; i < points.Count - 1; i += 2)
            {
                var p = points[i];
                if (p.x < 2 || p.x > TileWidth - 2) continue;
                c.Line(p, p + new Vector2(rng.Range(-14, 14), rng.Range(6, 20)), 2f,
                    Palette.WithAlpha(Palette.RoadLine, 0.5f));
            }

            return c;
        }

        private static PixelCanvas BuildRoad()
        {
            var c = new PixelCanvas(TileWidth, 128);

            c.FillRect(0, 0, TileWidth, 128, Palette.Road);
            c.GradientVertical(0, 96, TileWidth, 32, Palette.WithAlpha(Palette.Violet, 0.9f), Palette.WithAlpha(Palette.Road, 0f));

            c.FillRect(0, 118, TileWidth, 3, Palette.RoadLine);
            c.FillRect(0, 114, TileWidth, 2, Palette.WithAlpha(Palette.HotPink, 0.55f));

            for (var x = 0; x < TileWidth; x += 32)
            {
                c.FillRect(x, 0, 2, 114, Palette.WithAlpha(Palette.RoadLine, 0.28f));
            }

            for (var y = 8; y < 114; y += 22)
            {
                c.FillRect(0, y, TileWidth, 1, Palette.WithAlpha(Palette.RoadLine, 0.18f));
            }

            var rng = new RandomState(31337);
            for (var i = 0; i < 40; i++)
            {
                var x = rng.Range(0, TileWidth);
                var y = rng.Range(4, 110);
                c.FillRect(x, y, rng.Range(2, 9), 1, Palette.WithAlpha(Palette.Night, 0.5f));
            }

            return c;
        }

        private static PixelCanvas BuildForeground()
        {
            var c = new PixelCanvas(TileWidth, 46);
            var rng = new RandomState(5150);

            c.FillPolygon(new List<Vector2>
            {
                new Vector2(0, 0), new Vector2(TileWidth, 0), new Vector2(TileWidth, 10), new Vector2(0, 10)
            }, Palette.WithAlpha(Palette.Night, 0.92f));

            for (var x = 0; x < TileWidth; x += rng.Range(9, 26))
            {
                var h = rng.Range(12, 40);
                c.FillPolygon(new List<Vector2>
                {
                    new Vector2(x, 8), new Vector2(x + 5, h), new Vector2(x + 11, 8)
                }, Palette.WithAlpha(Palette.Night, 0.9f));
            }

            return c;
        }

        private static PixelCanvas BuildPanel()
        {
            var c = new PixelCanvas(64, 64);
            c.FillRoundRect(0, 0, 64, 64, 16, Palette.WithAlpha(Palette.Night, 0.86f));
            c.FillRoundRect(3, 3, 58, 58, 14, Palette.WithAlpha(Palette.DeepPurple, 0.55f));

            Outline(c, 1, 1, 62, 62, Palette.WithAlpha(Palette.HotPink, 0.75f));
            return c;
        }

        private static PixelCanvas BuildButton()
        {
            var c = new PixelCanvas(40, 28);
            c.FillRoundRect(0, 0, 40, 28, 8, Palette.WithAlpha(Palette.Violet, 0.95f));
            c.FillRoundRect(2, 2, 36, 24, 7, Palette.WithAlpha(Palette.DeepPurple, 0.95f));
            c.FillRoundRect(3, 20, 34, 5, 2, Palette.WithAlpha(Palette.White, 0.12f));
            Outline(c, 0, 0, 39, 27, Palette.RoadLine);
            return c;
        }

        private static PixelCanvas BuildBar()
        {
            var c = new PixelCanvas(22, 14);
            c.FillRoundRect(0, 0, 22, 14, 6, Palette.WithAlpha(Palette.Night, 0.9f));
            c.FillRoundRect(2, 2, 18, 10, 4, Palette.WithAlpha(Palette.White, 0.08f));
            return c;
        }

        private static PixelCanvas BuildSquare()
        {
            var c = new PixelCanvas(8, 8);
            c.Clear(Palette.White);
            return c;
        }

        private static PixelCanvas BuildHeart()
        {
            var c = new PixelCanvas(26, 24);

            c.FillCircle(8, 14, 7, Palette.Damage);
            c.FillCircle(18, 14, 7, Palette.Damage);
            c.FillPolygon(new List<Vector2>
            {
                new Vector2(1, 15), new Vector2(25, 15), new Vector2(13, 1)
            }, Palette.Damage);

            c.FillCircle(9, 16, 3, Palette.WithAlpha(Palette.HotPink, 0.9f));
            return c;
        }

        private static void Outline(PixelCanvas c, int x, int y, int w, int h, Color color)
        {
            c.FillRect(x, y, w, 1, color);
            c.FillRect(x, y + h - 1, w, 1, color);
            c.FillRect(x, y, 1, h, color);
            c.FillRect(x + w - 1, y, 1, h, color);
        }

        /// <summary>Deterministic so re-running the builder never reshuffles the skyline.</summary>
        private struct RandomState
        {
            private uint state;

            public RandomState(uint seed)
            {
                state = seed == 0 ? 1u : seed;
            }

            public int Range(int min, int max) => min + NextInt(max - min);

            public int NextInt(int exclusiveMax)
            {
                if (exclusiveMax <= 0) return 0;
                return (int)(Next() % (uint)exclusiveMax);
            }

            public float NextFloat() => Next() / 4294967295f;

            private uint Next()
            {
                state ^= state << 13;
                state ^= state >> 17;
                state ^= state << 5;
                return state;
            }
        }
    }
}
