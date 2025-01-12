using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    [SerializeField] float _explosionVolume = 0.1f;
    ParticleSystem _particleSystem;
    float _completedTime;

    void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    void OnEnable()
    {
        SfxManager.Instance.PlayClip(SoundEffectsClip.Explosion, _explosionVolume);
        _particleSystem.Play();
        _completedTime = Time.time + _particleSystem.main.duration;
    }

    void Update()
    {
        if (Time.time >= _completedTime)
        {
            EventBus.Instance.Raise(new ExplosionCompletedEvent(this));
        }
    }
}