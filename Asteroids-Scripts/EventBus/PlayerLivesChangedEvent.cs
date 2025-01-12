public struct PlayerLivesChangedEvent
{
    public int Lives { get; }

    public PlayerLivesChangedEvent(int lives)
    {
        Lives = lives;
    }
}