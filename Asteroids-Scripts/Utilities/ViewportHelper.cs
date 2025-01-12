using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewportHelper : SingletonMonoBehaviour<ViewportHelper>
{
    Camera _camera => Camera.main;
    Vector3 ScreenBottomLeft => _camera.ViewportToWorldPoint(Vector3.zero);
    Vector3 ScreenTopRight => _camera.ViewportToWorldPoint(Vector3.one);

    public float ScreenWidth => ScreenTopRight.x - ScreenBottomLeft.x;

    public float ScreenHeight => ScreenTopRight.y - ScreenBottomLeft.y;

    public bool IsOnScreen(Transform tf)
    {
        return IsOnScreen(tf.position);
    }

    public bool IsOnScreen(Vector3 position)
    {
        var isOnScreen = position.x >= ScreenBottomLeft.x &&
                         position.y >= ScreenBottomLeft.y &&
                         position.x <= ScreenTopRight.x &&
                         position.y <= ScreenTopRight.y;
        return isOnScreen;
    }

    public Vector3 GetRandomVisiblePosition()
    {
        var x = Random.Range(ScreenBottomLeft.x, ScreenTopRight.x);
        var y = Random.Range(ScreenBottomLeft.y, ScreenTopRight.y);
        return new Vector3(x, y, 0f);
    }

}

