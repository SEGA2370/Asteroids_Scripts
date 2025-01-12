using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Assets/_Project/Music/Music Clip Groups/MusicClipGroup", menuName = "Music/Music Clip Group", order = 3)]
public class MusicClipGroup : ScriptableObject
{
    [SerializeField] string _groupName;
    [SerializeField] AudioClip[] _audioClips;

    readonly Queue<AudioClip> _trackQueue = new();

    public AudioClip GetNextMusicClip()
    {
        if (_trackQueue.Count == 0) ShuffleTrackQueue();
        var audioClip = _trackQueue.Dequeue();
        return audioClip;
    }

    void ShuffleTrackQueue()
    {
        if (_trackQueue.Count != 0) return;
        foreach (var audioClip in _audioClips.OrderBy(_ => Random.value))
        {
            _trackQueue.Enqueue(audioClip);
        }
    }
}