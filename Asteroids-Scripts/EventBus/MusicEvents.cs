public struct StopAllMusicEvent { }

public struct PlayMusicEvent
{
    public string MusicGroupName { get; }

    public PlayMusicEvent(string musicGroupName)
    {
        MusicGroupName = musicGroupName;
    }
}