using UnityEngine;

public class Scorer : MonoBehaviour
{
    public void ScorePoints(Collision2D other)
    {
        if (other.gameObject.TryGetComponent<IScoreable>(out var scoreable))
        {
            GameManager.Instance.AddPoints(scoreable.PointValue);
        }
    }
}