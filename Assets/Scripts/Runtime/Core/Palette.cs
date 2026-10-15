using UnityEngine;

namespace Runner.Core
{
    /// <summary>
    /// The one place that knows what the game looks like. Sprites are generated from these at
    /// build time and effects reuse them at runtime, so the palette stays coherent everywhere.
    /// </summary>
    public static class Palette
    {
        public static readonly Color Night = new Color32(0x14, 0x0b, 0x2b, 0xff);
        public static readonly Color DeepPurple = new Color32(0x24, 0x11, 0x4a, 0xff);
        public static readonly Color Violet = new Color32(0x3d, 0x1f, 0x6e, 0xff);
        public static readonly Color Magenta = new Color32(0xff, 0x2e, 0x88, 0xff);
        public static readonly Color HotPink = new Color32(0xff, 0x5c, 0xb2, 0xff);
        public static readonly Color Sunset = new Color32(0xff, 0x7a, 0x3c, 0xff);
        public static readonly Color Amber = new Color32(0xff, 0xc7, 0x5c, 0xff);
        public static readonly Color Cyan = new Color32(0x3f, 0xf5, 0xd8, 0xff);
        public static readonly Color IceBlue = new Color32(0x6c, 0xd1, 0xff, 0xff);
        public static readonly Color ChassisDark = new Color32(0x1b, 0x22, 0x44, 0xff);
        public static readonly Color Chassis = new Color32(0x2f, 0x3d, 0x78, 0xff);
        public static readonly Color ChassisLight = new Color32(0x54, 0x6a, 0xb4, 0xff);
        public static readonly Color Road = new Color32(0x18, 0x10, 0x33, 0xff);
        public static readonly Color RoadLine = new Color32(0x7b, 0x3f, 0xff, 0xff);
        public static readonly Color White = new Color32(0xf2, 0xf6, 0xff, 0xff);
        public static readonly Color Damage = new Color32(0xff, 0x33, 0x55, 0xff);

        public static Color WithAlpha(Color color, float alpha) =>
            new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha));
    }
}
