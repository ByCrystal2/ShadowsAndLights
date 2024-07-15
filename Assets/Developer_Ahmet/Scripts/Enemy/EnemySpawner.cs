using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public List<Enemy> Enemys = new List<Enemy>();

    public void EnemysInit()
    {
        SpidersInit();
    }
    public void SpidersInit()
    {
        Enemy s1 = new Spider(1, "test", 2, 3,2,EnemyType.Spider);
        Enemy s2 = new Spider(2, "test1", 2, 3,2, EnemyType.Spider);
        Enemy s3 = new Spider(3, "test2", 2, 3,2, EnemyType.Spider);
        Enemy s4 = new Spider(4, "test3", 2, 3,2, EnemyType.Spider);
        Enemy s5 = new Spider(5, "test4", 2, 3,2, EnemyType.Spider);
        Enemys.Add(s1);
        Enemys.Add(s2);
        Enemys.Add(s3);
        Enemys.Add(s4);
        Enemys.Add(s5);
    }
}
