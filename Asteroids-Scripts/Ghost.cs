using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GhostParent;

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

    PolygonCollider2D _collider;
    Renderer _renderer;

    Transform _parentTransform;
    GhostParent _ghostParent;
    GhostPosition _ghostPosition;
    Transform _transform;
    ICollisionParent _collisionParent;

    public void Init(GhostParent ghostParent, GhostPosition ghostPosition)
    {
        _ghostParent = ghostParent;
        _ghostParent.TryGetComponent(out _collisionParent);
        _parentTransform = _ghostParent.transform;
        _ghostPosition = ghostPosition;
        _transform.localScale = _ghostParent.transform.localScale;
        RepositionGhost();
        gameObject.SetActive(true);
    }

    void Awake()
    {
        _transform = transform;
        _renderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<PolygonCollider2D>();
    }

    void OnEnable()
    {
        EnableComponents();
    }

    void OnDisable()
    {
        DisableComponents();
    }

    void Update()
    {
        RepositionGhost();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        _collisionParent?.Collided(collision);
    }

    void EnableComponents()
    {
        _collider.enabled = true;
        _renderer.enabled = true;
    }

    void DisableComponents()
    {
        _collider.enabled = false;
        _renderer.enabled = false;
    }

    void RepositionGhost()
    {
        if (_parentTransform == null) return;
        _transform.SetPositionAndRotation(_parentTransform.position + GhostOffset, _parentTransform.rotation);
        _collider.enabled = _renderer.isVisible;
    }

    Vector3 GhostOffset
    {
        get
        {
            var xOffset = 0f;
            var yOffset = 0f;

            xOffset = _ghostPosition switch
            {
                // Calculate xOffset
                GhostPosition.MiddleRight or GhostPosition.LowerRight or
                    GhostPosition.UpperRight => ViewportHelper.Instance.ScreenWidth,
                GhostPosition.MiddleLeft or GhostPosition.LowerLeft or
                    GhostPosition.UpperLeft => -ViewportHelper.Instance.ScreenWidth,
                _ => xOffset
            };

            yOffset = _ghostPosition switch
            {
                // Calculate yOffset
                GhostPosition.UpperLeft => ViewportHelper.Instance.ScreenHeight,
                GhostPosition.UpperMiddle => ViewportHelper.Instance.ScreenHeight,
                GhostPosition.UpperRight => ViewportHelper.Instance.ScreenHeight,
                GhostPosition.LowerLeft => -ViewportHelper.Instance.ScreenHeight,
                GhostPosition.LowerMiddle => -ViewportHelper.Instance.ScreenHeight,
                GhostPosition.LowerRight => -ViewportHelper.Instance.ScreenHeight,
                _ => yOffset
            };

            return new Vector3(xOffset, yOffset, 0f);
        }
    }

}
