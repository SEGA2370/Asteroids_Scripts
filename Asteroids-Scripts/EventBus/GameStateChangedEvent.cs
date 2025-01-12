public struct GameStateChangedEvent
{
    public GameState GameState;

    public GameStateChangedEvent(GameState gameState)
    {
        GameState = gameState;
    }
}