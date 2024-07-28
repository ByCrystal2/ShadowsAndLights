using StateMachineSystem;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Enemy : MonoBehaviour, IIDle, IMovable, IAttackable, IDamageable, IPatrol, IDie, IEscape
{
    [SerializeField] protected EnemyData enemyData;

    public EnemyState currentState;

    void FixedUpdate()
    {
        HandleState();
    }

    private void HandleState()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Move:
                Move();
                break;
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Attack:
                Attack();
                break;
            case EnemyState.TakeDamage:
                TakeDamage();
                break;
            case EnemyState.Die:
                Die();
                break;
            case EnemyState.Escape:
                Escape();
                break;
        }
    }

    public abstract void Idle();
    public abstract void Move();
    public abstract void Patrol();
    public abstract void Attack();
    public abstract void TakeDamage();
    public abstract void TakeDamage(int amount);
    public abstract void Die();
    public abstract void Escape();
}

[System.Serializable]
public class EnemyData
{
    public int id;
    public string enemyName;
    public float health;
    public int level;
    public float deathGold;
    public float escapeGold;
    public EnemyType enemyType;

    public EnemyData(int _id, string name, float health, int level, float deathGold, float escapeGold, EnemyType enemyType)
    {
        this.id = _id;
        this.enemyName = name;
        this.health = health;
        this.level = level;
        this.deathGold = deathGold;
        this.escapeGold = escapeGold;
        this.enemyType = enemyType;
    }
}

public enum EnemyState
{
    Idle,
    Move,
    Patrol,
    Attack,
    TakeDamage,
    Die,
    Escape,
}

public enum EnemyType
{
    Spider,
    Mouse
}

public interface IIDle
{
    void Idle();
}

public interface IMovable
{
    void Move();
}

public interface IAttackable
{
    void Attack();
}

public interface IDamageable
{
    void TakeDamage(int amount);
}

public interface IPatrol
{
    void Patrol();
}

public interface IDie
{
    void Die();
}
public interface IEscape
{
    void Escape();
}

[System.Serializable]
public struct EnemyTarget
{
    public Transform TargetObject;
    public Vector3 TargetPoint;
    public EnemyTargetType TargetType;
}

public enum EnemyTargetType
{
    None,
    Player,
    Director,
}