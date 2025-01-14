using UnityEngine;

public class Ghost : MonoBehaviour
{
    public enum GhostPosition
    {
        UpperRight = 0,
        MiddleRight,
        LowerRight,
        LowerMiddle,
        LowerLeft,
        MiddleLeft,
        UpperLeft,
        UpperMiddle,
    }

    private Transform _parentTransform;
    private Transform _transform;

    public void Init(GhostParent ghostParent, GhostPosition ghostPosition)
    {
        _parentTransform = ghostParent.transform;
        RepositionGhost(ghostPosition);
        gameObject.SetActive(true);
    }

    private void Awake()
    {
        _transform = transform;
    }

    private void RepositionGhost(GhostPosition ghostPosition)
    {
        if (_parentTransform == null) return;

        var offset = CalculateOffset(ghostPosition);
        _transform.position = _parentTransform.position + offset;
    }

    private Vector3 CalculateOffset(GhostPosition position)
    {
        float xOffset = position switch
        {
            GhostPosition.MiddleRight or GhostPosition.LowerRight or GhostPosition.UpperRight => ViewportHelper.Instance.ScreenWidth,
            GhostPosition.MiddleLeft or GhostPosition.LowerLeft or GhostPosition.UpperLeft => -ViewportHelper.Instance.ScreenWidth,
            _ => 0f
        };

        float yOffset = position switch
        {
            GhostPosition.UpperLeft or GhostPosition.UpperMiddle or GhostPosition.UpperRight => ViewportHelper.Instance.ScreenHeight,
            GhostPosition.LowerLeft or GhostPosition.LowerMiddle or GhostPosition.LowerRight => -ViewportHelper.Instance.ScreenHeight,
            _ => 0f
        };

        return new Vector3(xOffset, yOffset, 0f);
    }
}
