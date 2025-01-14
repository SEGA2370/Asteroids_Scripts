using UnityEngine;

public class ViewportHelper : SingletonMonoBehaviour<ViewportHelper>
{
    private Camera _camera;
    private Vector3 _screenBottomLeft;
    private Vector3 _screenTopRight;

    public float ScreenWidth => _screenTopRight.x - _screenBottomLeft.x;
    public float ScreenHeight => _screenTopRight.y - _screenBottomLeft.y;

    protected override void Awake()
    {
        base.Awake(); // Ensure SingletonMonoBehaviour's Awake is called
        _camera = Camera.main;
        UpdateScreenBounds();
    }

    private void UpdateScreenBounds()
    {
        _screenBottomLeft = _camera.ViewportToWorldPoint(Vector3.zero);
        _screenTopRight = _camera.ViewportToWorldPoint(Vector3.one);
    }

    public bool IsOnScreen(Transform tf) => IsOnScreen(tf.position);

    public bool IsOnScreen(Vector3 position)
    {
        return position.x >= _screenBottomLeft.x &&
               position.y >= _screenBottomLeft.y &&
               position.x <= _screenTopRight.x &&
               position.y <= _screenTopRight.y;
    }

    public Vector3 WrapAroundScreen(Vector3 position)
    {
        var wrappedPosition = position;

        if (position.x < _screenBottomLeft.x)
        {
            wrappedPosition.x = _screenTopRight.x;
        }
        else if (position.x > _screenTopRight.x)
        {
            wrappedPosition.x = _screenBottomLeft.x;
        }

        if (position.y < _screenBottomLeft.y)
        {
            wrappedPosition.y = _screenTopRight.y;
        }
        else if (position.y > _screenTopRight.y)
        {
            wrappedPosition.y = _screenBottomLeft.y;
        }

        return wrappedPosition;
    }

    public Vector3 GetRandomVisiblePosition()
    {
        float x = Random.Range(_screenBottomLeft.x, _screenTopRight.x);
        float y = Random.Range(_screenBottomLeft.y, _screenTopRight.y);
        return new Vector3(x, y, 0f);
    }
}
