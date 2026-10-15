namespace Runner.Player
{
    /// <summary>
    /// Animator parameter names, declared once as strings and hashed once, so the state machine
    /// and the code that drives it cannot drift apart.
    /// </summary>
    public static class AnimatorHash
    {
        public const string GroundedName = "Grounded";
        public const string RisingName = "Rising";
        public const string SpeedName = "Speed";
        public const string JumpName = "Jump";
        public const string LandName = "Land";
        public const string DashName = "Dash";
        public const string HitName = "Hit";
        public const string DeathName = "Death";

        public static readonly int Grounded = UnityEngine.Animator.StringToHash(GroundedName);
        public static readonly int Rising = UnityEngine.Animator.StringToHash(RisingName);
        public static readonly int Speed = UnityEngine.Animator.StringToHash(SpeedName);
        public static readonly int Jump = UnityEngine.Animator.StringToHash(JumpName);
        public static readonly int Land = UnityEngine.Animator.StringToHash(LandName);
        public static readonly int Dash = UnityEngine.Animator.StringToHash(DashName);
        public static readonly int Hit = UnityEngine.Animator.StringToHash(HitName);
        public static readonly int Death = UnityEngine.Animator.StringToHash(DeathName);
    }
}
