using StateMachineSystem;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Enemy
{
    public int ID;
    public string Name;
    public float Health;
    public int Level;
    public float DeathGold;
    public EnemyType EnemyType;
    protected StateMachine stateMachine;
    public Enemy(int _id, string name, float health, int level, float deathGold, EnemyType enemyType)
    {
        stateMachine = new StateMachine();
        StatesInit();
        ID = _id;
        Name = name;
        Health = health;
        Level = level;
        DeathGold = deathGold;
        EnemyType = enemyType;
    }
    public abstract void StatesInit();
    //public void StatesInit()
    //{
    //    IState idleState = new IdleState();
    //    IState walkState = new WalkState();
    //    IState runState = new RunState();
    //    IState attackState = new AttackState();
    //    IState dyingState = new DyingState();

    //    StateTransition idleToWalk = new StateTransition(idleState, walkState, () => true);
    //    StateTransition walkToIdle = new StateTransition(walkState, idleState, () => true);

    //    StateTransition idleToRun = new StateTransition(idleState, runState, () => true);
    //    StateTransition walkToRun = new StateTransition(walkState, runState, () => true);

    //    StateTransition runToIdle = new StateTransition(runState, idleState, () => true);
    //    StateTransition runToWalk = new StateTransition(runState, walkState, () => true);

    //    StateTransition walkToAttack = new StateTransition(walkState, attackState, () => true);
    //    StateTransition runToAttack = new StateTransition(runState, attackState, () => true);

    //    StateTransition anyToDying = new StateTransition(null, dyingState, () => true);

    //    stateMachine.SetStates(idleToWalk);
    //    stateMachine.SetStates(walkToIdle);
    //    stateMachine.SetStates(idleToRun);
    //    stateMachine.SetStates(walkToRun);
    //    stateMachine.SetStates(runToIdle);
    //    stateMachine.SetStates(runToWalk);
    //    stateMachine.SetStates(walkToAttack);
    //    stateMachine.SetStates(runToAttack);

    //    stateMachine.SetAnyStates(anyToDying);

    //    stateMachine.SetState(idleState);
    //}
}
public enum EnemyType
{
    Spider,
    Mouse
}
[System.Serializable]
public class Spider : Enemy, IMovable, IAttackable, IDamageable, IPatrol, IDie
{
    public Spider(int _id, string name, float health, int level, float deathGold, EnemyType enemyType) : base(_id, name, health, level, deathGold, enemyType)
    {
    }

    public void Move()
    {
        throw new System.NotImplementedException();
    }

    public void Patrol()
    {
        throw new System.NotImplementedException();
    }
    public void Attack()
    {
        throw new System.NotImplementedException();
    }
    public void TakeDamage(int amount)
    {
        throw new System.NotImplementedException();
    }

    public void Die()
    {
        throw new System.NotImplementedException();
    }


    public override void StatesInit()
    {
        IState idleState = new IdleState();
        IState walkState = new WalkState();
        IState runState = new RunState();
        IState attackState = new AttackState();
        IState dyingState = new DyingState();

        StateTransition idleToWalk = new StateTransition(idleState, walkState, () => true);
        StateTransition walkToIdle = new StateTransition(walkState, idleState, () => true);

        StateTransition idleToRun = new StateTransition(idleState, runState, () => true);
        StateTransition walkToRun = new StateTransition(walkState, runState, () => true);

        StateTransition runToIdle = new StateTransition(runState, idleState, () => true);
        StateTransition runToWalk = new StateTransition(runState, walkState, () => true);

        StateTransition walkToAttack = new StateTransition(walkState, attackState, () => true);
        StateTransition runToAttack = new StateTransition(runState, attackState, () => true);

        StateTransition anyToDying = new StateTransition(null, dyingState, () => true);

        stateMachine.SetStates(idleToWalk);
        stateMachine.SetStates(walkToIdle);
        stateMachine.SetStates(idleToRun);
        stateMachine.SetStates(walkToRun);
        stateMachine.SetStates(runToIdle);
        stateMachine.SetStates(runToWalk);
        stateMachine.SetStates(walkToAttack);
        stateMachine.SetStates(runToAttack);

        stateMachine.SetAnyStates(anyToDying);

        stateMachine.SetState(idleState);
    }

    
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