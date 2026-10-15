namespace Runner.Core
{
    /// <summary>
    /// Fixed draw order for the 2D scene so code-built sprites never fight over depth.
    /// </summary>
    public static class SortOrder
    {
        public const int Sky = -100;
        public const int FarLayer = -90;
        public const int MidLayer = -80;
        public const int NearLayer = -70;
        public const int Ground = -10;
        public const int Backdrop = -20;
        public const int Collectible = 2;
        public const int Obstacle = 3;
        public const int Player = 5;
        public const int PlayerShadow = 4;
        public const int Effect = 10;
        public const int Foreground = 20;
    }
}
