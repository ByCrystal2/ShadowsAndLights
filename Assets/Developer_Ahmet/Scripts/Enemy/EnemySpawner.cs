using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public int LevelID;
    public List<Enemy> MyEnemies = new List<Enemy>();

    private void FixedUpdate()
    {
        foreach (var item in MyEnemies)
        {
            item.HandleState();
        }
    }
}
