using Runner.Core;
using UnityEngine;

namespace Runner.EditorTools
{
    /// <summary>
    /// The runner itself, drawn one pixel at a time. A single parametric pose function generates
    /// every frame so the silhouette, palette and proportions cannot drift between animations.
    /// </summary>
    internal static class RunnerCharacterArt
    {
        private const int Width = 44;
        private const int Height = 58;
        private const float HipX = 21f;
        private const float HipY = 25f;
        private const float ShoulderY = 39f;

        private static readonly Color Shell = Palette.Chassis;
        private static readonly Color ShellDark = Palette.ChassisDark;
        private static readonly Color ShellLight = Palette.ChassisLight;
        private static readonly Color Visor = Palette.Cyan;
        private static readonly Color Scarf = Palette.Magenta;
        private static readonly Color ScarfLight = Palette.HotPink;
        private static readonly Color Accent = Palette.Amber;
        private static readonly Color Glow = Palette.IceBlue;

        public static Sprite Idle { get; private set; }
        public static Sprite[] Run { get; } = new Sprite[6];
        public static Sprite Jump { get; private set; }
        public static Sprite Fall { get; private set; }
        public static Sprite Land { get; private set; }
        public static Sprite Hit { get; private set; }
        public static Sprite[] Death { get; } = new Sprite[3];

        private static readonly Vector2 Pivot = new Vector2(0.5f, 0f);

        public static void Build()
        {
            Idle = Frame("runner_idle", 0f, PoseMode.Idle);

            for (var i = 0; i < Run.Length; i++)
            {
                Run[i] = Frame($"runner_run{i}", i / (float)Run.Length * Mathf.PI * 2f, PoseMode.Run);
            }

            Jump = Frame("runner_jump", 0f, PoseMode.Jump);
            Fall = Frame("runner_fall", 0f, PoseMode.Fall);
            Land = Frame("runner_land", 0f, PoseMode.Land);
            Hit = Frame("runner_hit", 0f, PoseMode.Hit);

            for (var i = 0; i < Death.Length; i++)
            {
                Death[i] = Frame($"runner_death{i}", i / 2f, PoseMode.Death);
            }
        }

        private enum PoseMode
        {
            Idle,
            Run,
            Jump,
            Fall,
            Land,
            Hit,
            Death
        }

        private static Sprite Frame(string name, float phase, PoseMode mode)
        {
            var canvas = new PixelCanvas(Width, Height);
            Draw(canvas, phase, mode);
            return SpriteWriter.Save("Player", name, canvas, Pivot);
        }

        private static void Draw(PixelCanvas c, float phase, PoseMode mode)
        {
            if (mode == PoseMode.Death)
            {
                DrawDeath(c, phase);
                return;
            }

            var bob = mode == PoseMode.Run ? Mathf.RoundToInt(Mathf.Abs(Mathf.Sin(phase)) * 2f) : 0;
            var crouch = mode == PoseMode.Land ? 6 : 0;
            var lean = mode == PoseMode.Hit ? -3 : mode == PoseMode.Jump ? 2 : 0;
            var bodyY = -bob - crouch;

            var frontLeg = mode == PoseMode.Run ? phase : phase + 0.4f;
            var backLeg = frontLeg + Mathf.PI;

            DrawScarf(c, phase, bodyY, lean);
            DrawLeg(c, backLeg, bodyY, mode, true);
            DrawPack(c, bodyY, lean);
            DrawTorso(c, bodyY, lean);
            DrawArm(c, backLeg + Mathf.PI, bodyY, lean, true);
            DrawHead(c, bodyY, lean, mode);
            DrawArm(c, frontLeg + Mathf.PI, bodyY, lean, false);
            DrawLeg(c, frontLeg, bodyY, mode, false);
        }

        private static void DrawTorso(PixelCanvas c, int bodyY, int lean)
        {
            c.FillRoundRect(14 + lean, 24 + bodyY, 15, 18, 5, ShellDark);
            c.FillRoundRect(15 + lean, 25 + bodyY, 13, 16, 4, Shell);
            c.FillRect(17 + lean, 34 + bodyY, 9, 3, ShellLight);
            c.FillCircle(21 + lean, 30 + bodyY, 2, Glow);
            c.FillRect(16 + lean, 26 + bodyY, 11, 1, ShellDark);
        }

        private static void DrawHead(PixelCanvas c, int bodyY, int lean, PoseMode mode)
        {
            var y = 42 + bodyY;
            var x = 15 + lean;

            c.FillRoundRect(x, y, 16, 13, 5, ShellDark);
            c.FillRoundRect(x + 1, y + 1, 14, 11, 4, Shell);

            var visorColor = mode == PoseMode.Hit ? Palette.Damage : Visor;
            c.FillRoundRect(x + 5, y + 4, 10, 5, 2, visorColor);
            c.FillRect(x + 6, y + 7, 8, 1, Palette.WithAlpha(Palette.White, 0.65f));

            if (mode == PoseMode.Idle) c.FillRect(x + 8, y + 5, 3, 3, Palette.WithAlpha(Palette.White, 0.85f));

            c.Line(new Vector2(x + 13, y + 13), new Vector2(x + 16, y + 17), 1f, Accent);
            c.FillCircle(x + 16, y + 17, 1, ScarfLight);
        }

        private static void DrawPack(PixelCanvas c, int bodyY, int lean)
        {
            var x = 9 + lean;
            var y = 27 + bodyY;

            c.FillRoundRect(x, y, 7, 12, 2, ShellDark);
            c.FillRect(x + 1, y + 2, 5, 1, Scarf);
            c.FillRect(x + 1, y + 5, 5, 1, Scarf);
            c.FillCircle(x + 3, y - 1, 2, Palette.WithAlpha(Glow, 0.75f));
        }

        private static void DrawScarf(PixelCanvas c, float phase, int bodyY, int lean)
        {
            var anchor = new Vector2(15 + lean, 40 + bodyY);

            for (var i = 0; i < 4; i++)
            {
                var wave = Mathf.Sin(phase * 1.6f + i * 1.1f) * 2.4f;
                var from = anchor + new Vector2(-i * 4f, wave * 0.4f);
                var to = anchor + new Vector2(-(i + 1) * 4f, wave * 0.6f - i * 0.7f);
                c.Line(from, to, 4f - i * 0.6f, i % 2 == 0 ? Scarf : ScarfLight);
            }
        }

        private static void DrawLeg(PixelCanvas c, float t, int bodyY, PoseMode mode, bool back)
        {
            var hip = new Vector2(HipX + (back ? -2f : 2f), HipY + bodyY);
            Vector2 knee;
            Vector2 foot;

            switch (mode)
            {
                case PoseMode.Jump:
                    knee = hip + new Vector2(back ? -1f : 4f, -6f);
                    foot = knee + new Vector2(back ? -5f : 1f, -6f);
                    break;

                case PoseMode.Fall:
                    knee = hip + new Vector2(back ? -4f : 5f, -7f);
                    foot = knee + new Vector2(back ? -2f : 2f, -7f);
                    break;

                case PoseMode.Land:
                    knee = hip + new Vector2(back ? -5f : 6f, -6f);
                    foot = knee + new Vector2(back ? -2f : 2f, -6f);
                    break;

                case PoseMode.Hit:
                    knee = hip + new Vector2(back ? -6f : 3f, -7f);
                    foot = knee + new Vector2(back ? 2f : -4f, -5f);
                    break;

                case PoseMode.Idle:
                    knee = hip + new Vector2(back ? -1f : 1f, -7f);
                    foot = knee + new Vector2(back ? -1f : 1f, -7f);
                    break;

                default:
                    var swing = Mathf.Sin(t);
                    var lift = Mathf.Max(0f, Mathf.Cos(t));
                    knee = new Vector2(hip.x + swing * 5f + 1.5f, 13f + lift * 3f);
                    foot = new Vector2(hip.x + swing * 9.5f, 3.5f + lift * 8f);
                    break;
            }

            var color = back ? ShellDark : Shell;
            c.Line(hip, knee, 5f, color);
            c.Line(knee, foot, 4f, color);
            c.FillRoundRect(Mathf.RoundToInt(foot.x) - 3, Mathf.RoundToInt(foot.y) - 2, 7, 4, 1, back ? ShellDark : ShellLight);
        }

        private static void DrawArm(PixelCanvas c, float t, int bodyY, int lean, bool back)
        {
            var shoulder = new Vector2(23 + lean, ShoulderY + bodyY);
            Vector2 elbow;
            Vector2 hand;

            if (back)
            {
                elbow = shoulder + new Vector2(-3f, -6f);
                hand = elbow + new Vector2(-2f, -6f);
            }
            else
            {
                var swing = Mathf.Sin(t);
                elbow = new Vector2(shoulder.x + swing * 5f + 2f, shoulder.y - 5f);
                hand = new Vector2(elbow.x + swing * 4f + 2f, elbow.y - 4f + Mathf.Abs(swing) * 2f);
            }

            var color = back ? ShellDark : Shell;
            c.Line(shoulder, elbow, 4f, color);
            c.Line(elbow, hand, 4f, color);
            c.FillCircle(Mathf.RoundToInt(hand.x), Mathf.RoundToInt(hand.y), 2, back ? ShellDark : Accent);
        }

        /// <summary>
        /// Three frames: the shell cracking, the parts letting go, and what is left on the road.
        /// </summary>
        private static void DrawDeath(PixelCanvas c, float progress)
        {
            var scatter = progress * 12f;
            var seed = Mathf.RoundToInt(progress * 100f);

            if (progress < 0.34f)
            {
                c.FillRoundRect(14, 24, 15, 18, 5, ShellDark);
                c.FillRoundRect(15, 25, 13, 16, 4, Shell);
                c.FillRoundRect(20, 42, 16, 13, 5, ShellDark);
                c.FillRoundRect(25, 46, 10, 5, 2, Palette.Damage);

                for (var i = 0; i < 5; i++)
                {
                    var a = (i * 1.7f + seed * 0.1f);
                    c.Line(new Vector2(21, 33), new Vector2(21 + Mathf.Cos(a) * 9f, 33 + Mathf.Sin(a) * 11f), 1f, Palette.WithAlpha(Palette.White, 0.8f));
                }
            }
            else if (progress < 0.7f)
            {
                Debris(c, scatter, 0.75f);
            }
            else
            {
                Debris(c, scatter * 1.4f, 0.4f);
                c.FillRoundRect(12, 3, 12, 7, 3, ShellDark);
                c.FillRoundRect(26, 2, 9, 5, 2, Shell);
                c.FillCircle(33, 4, 2, Palette.WithAlpha(Palette.Damage, 0.8f));
            }

            for (var i = 0; i < 9; i++)
            {
                var a = i * 0.7f + progress * 2f;
                var r = 8f + progress * 16f;
                c.FillCircle(Mathf.RoundToInt(22 + Mathf.Cos(a) * r), Mathf.RoundToInt(28 + Mathf.Sin(a) * r * 0.8f), 2,
                    Palette.WithAlpha(i % 2 == 0 ? Accent : Glow, 1f - progress));
            }
        }

        private static void Debris(PixelCanvas c, float scatter, float alpha)
        {
            var chunks = new[]
            {
                new Vector2(21, 33), new Vector2(16, 26), new Vector2(27, 27),
                new Vector2(13, 38), new Vector2(30, 39), new Vector2(22, 46)
            };

            for (var i = 0; i < chunks.Length; i++)
            {
                var angle = i * 1.05f + 0.4f;
                var offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * 0.7f + 0.35f) * scatter;
                var pos = chunks[i] + offset;
                var color = i % 2 == 0 ? Shell : ShellDark;

                var poly = new System.Collections.Generic.List<Vector2>
                {
                    pos, pos + new Vector2(7f, 2f), pos + new Vector2(4f, 8f)
                };

                c.FillPolygon(poly, Palette.WithAlpha(color, alpha));
            }
        }
    }
}
