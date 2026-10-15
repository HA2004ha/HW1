namespace Runner.Core
{
    /// <summary>Lifecycle stages the game moves through. Only Playing scrolls the world.</summary>
    public enum GamePhase
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }
}
