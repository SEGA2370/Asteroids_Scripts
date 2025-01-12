public struct ExplosionCompletedEvent
{
    public ExplosionEffect Explosion { get; private set; }
    
    public ExplosionCompletedEvent(ExplosionEffect explosion)
    {
        Explosion = explosion;
    }
}