using System.Collections.Generic;
using UnityEngine;

public static class EnemyDatabase
{
    public static List<EnemyData> enemyDatas = new List<EnemyData>() 
    {
        new(0, "Bos", new()
        {
            new(){ stat = Stat.None , amount = 0}, 
            new(){ stat = Stat.CurrentHealth , amount = 0}, 
            new(){ stat = Stat.CurrentBatary , amount = 0}, 
            new(){ stat = Stat.Attack , amount = 0}, 
            new(){ stat = Stat.Defence , amount = 0}, 
            new(){ stat = Stat.Health , amount = 0}, 
            new(){ stat = Stat.MoveSpeed , amount = 0}, 
            new(){ stat = Stat.EnemyDexterity , amount = 0}, 
            new(){ stat = Stat.SlowImmune , amount = 0}, 
            new(){ stat = Stat.BurnImmune , amount = 0}, 
            new(){ stat = Stat.PoisonImmune , amount = 0}, 
            new(){ stat = Stat.PatrifyImmune , amount = 0}, 
            new(){ stat = Stat.FreezeImmune , amount = 0}, 
            new(){ stat = Stat.ShockImmune , amount = 0}, 
        }, 0, 0, 0, EnemyType.Spider),

        new(0, "Spider Lv1", new()
        {
            new(){ stat = Stat.None , amount = 0},
            new(){ stat = Stat.CurrentHealth , amount = 30},
            new(){ stat = Stat.CurrentBatary , amount = 0},
            new(){ stat = Stat.Attack , amount = 5},
            new(){ stat = Stat.Defence , amount = 0},
            new(){ stat = Stat.Health , amount = 30},
            new(){ stat = Stat.MoveSpeed , amount = 3},
            new(){ stat = Stat.EnemyDexterity , amount = 1},
            new(){ stat = Stat.SlowImmune , amount = 0},
            new(){ stat = Stat.BurnImmune , amount = 0},
            new(){ stat = Stat.PoisonImmune , amount = 0},
            new(){ stat = Stat.PatrifyImmune , amount = 0},
            new(){ stat = Stat.FreezeImmune , amount = 0},
            new(){ stat = Stat.ShockImmune , amount = 0},
        }, 0, 0, 0, EnemyType.Spider),

        new(0, "Spider Lv2", new()
        {
            new(){ stat = Stat.None , amount = 0},
            new(){ stat = Stat.CurrentHealth , amount = 60},
            new(){ stat = Stat.CurrentBatary , amount = 0},
            new(){ stat = Stat.Attack , amount = 9},
            new(){ stat = Stat.Defence , amount = 0},
            new(){ stat = Stat.Health , amount = 60},
            new(){ stat = Stat.MoveSpeed , amount = 3.5f},
            new(){ stat = Stat.EnemyDexterity , amount = 1.3f},
            new(){ stat = Stat.SlowImmune , amount = 0},
            new(){ stat = Stat.BurnImmune , amount = 0},
            new(){ stat = Stat.PoisonImmune , amount = 0},
            new(){ stat = Stat.PatrifyImmune , amount = 0},
            new(){ stat = Stat.FreezeImmune , amount = 0},
            new(){ stat = Stat.ShockImmune , amount = 0},
        }, 0, 0, 0, EnemyType.Spider),
    };
}
