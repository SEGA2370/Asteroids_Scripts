using UnityEngine;

[CreateAssetMenu(menuName = "Create AsteroidSettings", fileName = "AsteroidSettings", order = 0)]
public class AsteroidSettings : ScriptableObject
{
    public AsteroidSize Size => _size;
    public float MinimumSpeed => _minimumSpeed;
    public float MaximumSpeed => _maximumSpeed;
    public float MinimumRotation => _minimumRotation;
    public float MaximumRotation => _maximumRotation;
    public int Points => _points;

    [SerializeField] AsteroidSize _size = AsteroidSize.Small;
    [SerializeField] float _minimumSpeed = 0.1f;
    [SerializeField] float _maximumSpeed = 2f;
    [SerializeField] float _minimumRotation = 10f;
    [SerializeField] float _maximumRotation = 50f;
    [SerializeField] int _points = 20;
}

public enum AsteroidSize
{
    Small = 0,
    Medium,
    Large
}
