using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.VFX;

public class PlayerWeapons : MonoBehaviour
{
    [SerializeField] BulletBase _bulletPrefab;
    [SerializeField] Transform _muzzle, _bulletsParent;
    [SerializeField] SoundEffectsClip _fireSound = SoundEffectsClip.PlayerBulletFire;
    [SerializeField] float _fireSoundVolume = 0.25f;

    IObjectPool<BulletBase> _bulletPool;
    Rigidbody2D _rigidBody;

    public void FireBullet()
    {
        SfxManager.Instance.PlayClip(_fireSound, _fireSoundVolume);
        var bullet = _bulletPool.Get();
        bullet.transform.position = _muzzle.position;
        bullet.transform.rotation = transform.rotation;
        bullet.OnBulletDestroyed += DestroyBullet;
        bullet.gameObject.SetActive(true);
        if (bullet.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.velocity += _rigidBody.velocity;
        }
    }

    void Awake()
    {
        _bulletPool = new ObjectPool<BulletBase>(
            SpawnBullet, OnGetBullet, OnReleaseBullet, OnDestroyBullet,
            true, 10, 20);
        _rigidBody = GetComponentInParent<Rigidbody2D>();
    }

    BulletBase SpawnBullet()
    {
        var bullet = Instantiate(_bulletPrefab, _muzzle.position, transform.rotation);
        bullet.transform.SetParent(_bulletsParent);
        return bullet;
    }

    void OnGetBullet(BulletBase bullet)
    {
        if (!bullet.TryGetComponent<Rigidbody2D>(out var rb)) return;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    void OnReleaseBullet(BulletBase bullet)
    {
        bullet.gameObject.SetActive(false);
    }

    void OnDestroyBullet(BulletBase bullet) { }

    void DestroyBullet(BulletBase bullet)
    {
        bullet.OnBulletDestroyed -= DestroyBullet;
        bullet.gameObject.SetActive(false);
        _bulletPool.Release(bullet);
    }
}
