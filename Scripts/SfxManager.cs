using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SfxManager : SingletonMonoBehaviour<SfxManager>
{
    [SerializeField] List<AudioClip> _sfxClips = new();
    AudioSource _audioSource;

    protected override void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlayClip(SoundEffectsClip clip, float volume = 1f)
    {
        if ((int)clip < _sfxClips.Count && _sfxClips[(int)clip] != null)
            _audioSource.PlayOneShot(_sfxClips[(int)clip], volume);
    }
}

public enum SoundEffectsClip
{
    PlayerBulletFire,
    EnemyBulletFire,
    Explosion,
    ExtraLife
}
