using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField] private float _scrollSpeed = 0.1f; // Speed at which the layer moves to the left
    [SerializeField] private float _resetPositionX = -20f; // X position to reset the layer
   // [SerializeField] private float _startPositionX = 20f; // X position to start the layer

    private Vector3 _startPosition;

    void Start()
    {
        // Store the initial position of the layer
        _startPosition = transform.position;
    }

    void Update()
    {
        // Move the layer to the left
        transform.position += Vector3.left * _scrollSpeed * Time.deltaTime;

        // Check if the layer has moved past the reset position
        if (transform.position.x < _resetPositionX)
        {
            // Reset the position to the starting position
            transform.position = new Vector3(_startPosition.x, transform.position.y, transform.position.z);
        }
    }
}
