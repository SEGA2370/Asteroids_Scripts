using UnityEngine;

public class FastEnemyShip : EnemyShip
{
    protected override Vector3 GetFireDirection()
    {
        var player = GameManager.Instance.PlayerShip;
        if (player == null) return base.GetFireDirection();
        var direction = (player.transform.position - transform.position).normalized;
        return Quaternion.LookRotation(Vector3.forward, direction).eulerAngles;
    }
}