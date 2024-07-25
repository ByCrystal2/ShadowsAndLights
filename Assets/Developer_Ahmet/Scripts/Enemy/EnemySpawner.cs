using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public int LevelID;
    public List<Enemy> Enemys = new List<Enemy>();
    public static EnemySpawner instance { get; private set; }
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    public void EnemysInit()
    {
        SpidersInit();
    }
    public void SpidersInit()
    {
        Enemy s1 = new Spider(1, "test", Random.Range(50,100), 1,1,EnemyType.Spider);
        Enemy s2 = new Spider(2, "test1", Random.Range(50, 100), 1,1, EnemyType.Spider);
        Enemy s3 = new Spider(3, "test2", Random.Range(50, 100), 1,1, EnemyType.Spider);
        Enemy s4 = new Spider(4, "test3", Random.Range(50, 100), 1,1, EnemyType.Spider);
        Enemy s5 = new Spider(5, "test4", Random.Range(50, 100), 1,1, EnemyType.Spider);
        Enemys.Add(s1);
        Enemys.Add(s2);
        Enemys.Add(s3);
        Enemys.Add(s4);
        Enemys.Add(s5);
    }
    public (float health, int level, float deathGold) CalculateEnemyValues(float _health, int _level, float _deathGold)
    {
        int gameLevel = LevelID;
        return ((_health * gameLevel), (_level * gameLevel), (_deathGold + gameLevel));
    }
}
