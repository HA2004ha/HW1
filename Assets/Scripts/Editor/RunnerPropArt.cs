using System.Collections.Generic;
using Runner.Core;
using UnityEngine;

namespace Runner.EditorTools
{
    /// <summary>
    /// Hazards and pickups. Each one is drawn around its own centre so the prefabs can use a
    /// centred pivot and the colliders line up with what the player actually sees.
    /// </summary>
    internal static class RunnerPropArt
    {
        public static Sprite Spike { get; private set; }
        public static Sprite Crate { get; private set; }
        public static Sprite Saw { get; private set; }
        public static Sprite Drone { get; private set; }
        public static Sprite Mine { get; private set; }
        public static Sprite Coin { get; private set; }
        public static Sprite Shard { get; private set; }
        public static Sprite Pixel { get; private set; }

        private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);
        private static readonly Vector2 Bottom = new Vector2(0.5f, 0f);

        public static void Build()
        {
            Spike = SpriteWriter.Save("Props", "hazard_spike", SpikeSheet(), Bottom);
            Crate = SpriteWriter.Save("Props", "hazard_crate", CrateSheet(), Bottom);
            Saw = SpriteWriter.Save("Props", "hazard_saw", SawSheet(), Center);
            Drone = SpriteWriter.Save("Props", "hazard_drone", DroneSheet(), Center);
            Mine = SpriteWriter.Save("Props", "hazard_mine", MineSheet(), Center);
            Coin = SpriteWriter.Save("Props", "pickup_coin", CoinSheet(), Center);
            Shard = SpriteWriter.Save("Props", "fx_shard", ShardSheet(), Center);
            Pixel = SpriteWriter.Save("Props", "fx_pixel", PixelSheet(), Center);
        }

        private static PixelCanvas SpikeSheet()
        {
            var c = new PixelCanvas(38, 26);

            c.FillRect(1, 0, 36, 5, Palette.ChassisDark);
            c.FillRect(1, 4, 36, 1, Palette.RoadLine);

            for (var i = 0; i < 3; i++)
            {
                var x = 3 + i * 12;
                var blade = new List<Vector2>
                {
                    new Vector2(x, 5), new Vector2(x + 6, 25), new Vector2(x + 12, 5)
                };

                c.FillPolygon(blade, Palette.IceBlue);
                c.FillPolygon(new List<Vector2>
                {
                    new Vector2(x + 1, 5), new Vector2(x + 6, 23), new Vector2(x + 6, 5)
                }, Palette.WithAlpha(Palette.White, 0.55f));
                c.FillPolygon(new List<Vector2>
                {
                    new Vector2(x + 6, 25), new Vector2(x + 12, 5), new Vector2(x + 8, 5)
                }, Palette.WithAlpha(Palette.Night, 0.6f));
            }

            return c;
        }

        private static PixelCanvas CrateSheet()
        {
            var c = new PixelCanvas(30, 30);

            c.FillRoundRect(0, 0, 30, 30, 4, Palette.ChassisDark);
            c.FillRoundRect(2, 2, 26, 26, 3, Palette.Violet);
            c.FillRoundRect(3, 3, 24, 24, 3, Palette.DeepPurple);

            c.Line(new Vector2(4, 4), new Vector2(26, 26), 2f, Palette.RoadLine);
            c.Line(new Vector2(26, 4), new Vector2(4, 26), 2f, Palette.RoadLine);
            c.FillRect(2, 13, 26, 3, Palette.ChassisDark);

            c.FillCircle(6, 6, 1, Palette.Amber);
            c.FillCircle(24, 6, 1, Palette.Amber);
            c.FillCircle(6, 24, 1, Palette.Amber);
            c.FillCircle(24, 24, 1, Palette.Amber);
            c.FillRoundRect(3, 25, 24, 2, 1, Palette.WithAlpha(Palette.HotPink, 0.7f));

            return c;
        }

        private static PixelCanvas SawSheet()
        {
            var c = new PixelCanvas(40, 40);
            var centre = new Vector2(20, 20);
            var teeth = 10;
            var outline = new List<Vector2>();

            for (var i = 0; i < teeth * 2; i++)
            {
                var angle = i / (teeth * 2f) * Mathf.PI * 2f;
                var radius = i % 2 == 0 ? 19f : 13f;
                outline.Add(centre + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius));
            }

            c.FillPolygon(outline, Palette.IceBlue);
            c.FillCircle(20, 20, 12, Palette.ChassisDark);
            c.FillCircle(20, 20, 9, Palette.Chassis);
            c.FillCircle(20, 20, 4, Palette.Amber);

            for (var i = 0; i < 4; i++)
            {
                var a = i * Mathf.PI * 0.5f + 0.4f;
                c.Line(centre + new Vector2(Mathf.Cos(a) * 4f, Mathf.Sin(a) * 4f),
                    centre + new Vector2(Mathf.Cos(a) * 10f, Mathf.Sin(a) * 10f), 2f, Palette.ChassisDark);
            }

            return c;
        }

        private static PixelCanvas DroneSheet()
        {
            var c = new PixelCanvas(46, 28);

            c.FillRect(2, 18, 14, 2, Palette.ChassisDark);
            c.FillRect(30, 18, 14, 2, Palette.ChassisDark);
            c.FillEllipse(9, 21, 7, 2, Palette.WithAlpha(Palette.IceBlue, 0.55f));
            c.FillEllipse(37, 21, 7, 2, Palette.WithAlpha(Palette.IceBlue, 0.55f));

            c.FillRoundRect(11, 4, 24, 16, 6, Palette.ChassisDark);
            c.FillRoundRect(12, 5, 22, 14, 5, Palette.Chassis);
            c.FillRect(14, 15, 18, 2, Palette.WithAlpha(Palette.White, 0.35f));

            c.FillCircle(23, 11, 5, Palette.Night);
            c.FillCircle(23, 11, 4, Palette.Damage);
            c.FillCircle(22, 12, 2, Palette.WithAlpha(Palette.White, 0.85f));

            c.FillRect(15, 1, 3, 4, Palette.Amber);
            c.FillRect(28, 1, 3, 4, Palette.Amber);
            c.FillCircle(23, 2, 2, Palette.WithAlpha(Palette.Damage, 0.6f));

            return c;
        }

        private static PixelCanvas MineSheet()
        {
            var c = new PixelCanvas(32, 32);
            var centre = new Vector2(16, 16);

            for (var i = 0; i < 8; i++)
            {
                var a = i * Mathf.PI * 0.25f;
                c.Line(centre, centre + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 15f, 3f, Palette.ChassisDark);
            }

            c.FillCircle(16, 16, 11, Palette.ChassisDark);
            c.FillCircle(16, 16, 9, Palette.Violet);
            c.FillCircle(16, 16, 5, Palette.Damage);
            c.FillCircle(14, 18, 2, Palette.WithAlpha(Palette.White, 0.9f));

            return c;
        }

        private static PixelCanvas CoinSheet()
        {
            var c = new PixelCanvas(26, 26);

            c.FillCircle(13, 13, 12, Palette.WithAlpha(Palette.Night, 0.55f));
            c.FillCircle(13, 13, 11, Palette.Sunset);
            c.FillCircle(13, 13, 9, Palette.Amber);
            c.FillCircle(13, 13, 6, Palette.WithAlpha(Palette.Night, 0.35f));

            var bolt = new List<Vector2>
            {
                new Vector2(14, 6), new Vector2(9, 14), new Vector2(13, 14),
                new Vector2(11, 20), new Vector2(17, 11), new Vector2(13, 11)
            };

            c.FillPolygon(bolt, Palette.White);
            c.FillCircle(8, 18, 2, Palette.WithAlpha(Palette.White, 0.7f));

            return c;
        }

        private static PixelCanvas ShardSheet()
        {
            var c = new PixelCanvas(8, 8);

            c.FillPolygon(new List<Vector2>
            {
                new Vector2(4, 8), new Vector2(8, 4), new Vector2(4, 0), new Vector2(0, 4)
            }, Palette.White);

            return c;
        }

        private static PixelCanvas PixelSheet()
        {
            var c = new PixelCanvas(4, 4);
            c.Clear(Palette.White);
            return c;
        }
    }
}
