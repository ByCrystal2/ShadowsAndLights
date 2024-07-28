using UnityEngine;
using UnityEngine.AI;

public class SpiderBehaviour : Enemy
{
    public override void Idle()
    {
        Debug.Log(enemyData.enemyName + " in idle now.");
    }

    public override void Move()
    {
        Debug.Log("Spider is moving.");
    }

    public override void Patrol()
    {
        Debug.Log("Spider is patrolling.");
    }

    public override void Attack()
    {
        Debug.Log("Spider is attacking.");
    }

    public override void TakeDamage(int amount)
    {
        enemyData.health -= amount;
        Debug.Log("Spider took damage.");
        if (enemyData.health <= 0)
        {
            Die();
        }
    }

    public override void TakeDamage()
    {
        
    }

    public override void Escape()
    {
        
    }

    public override void Die()
    {
        Debug.Log("Spider died.");
    }
}

