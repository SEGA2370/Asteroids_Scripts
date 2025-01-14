using System.Collections.Generic;
using UnityEngine;

public class GhostParent : MonoBehaviour
{
    private Transform _transform;
    private Renderer _renderer;
    private Collider2D _collider;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider2D>();
        _transform = transform;
    }

    private void OnEnable()
    {
        EnableComponents();
    }

    private void Update()
    {
        HandleScreenWrap();
    }

    private void OnDisable()
    {
        DisableComponents();
    }

    private void EnableComponents()
    {
        _renderer.enabled = true;
        _collider.enabled = true;
    }

    private void DisableComponents()
    {
        _renderer.enabled = false;
        _collider.enabled = false;
    }

    private void HandleScreenWrap()
    {
        if (ViewportHelper.Instance.IsOnScreen(_transform)) return;

        WrapPosition();
    }

    private void WrapPosition()
    {
        _transform.position = ViewportHelper.Instance.WrapAroundScreen(_transform.position);
    }
}
