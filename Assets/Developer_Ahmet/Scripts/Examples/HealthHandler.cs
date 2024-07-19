using System.Collections;
using UnityEngine;

[RequireComponent(typeof(VisualHandler),typeof(DyingHandler))]
public class HealthHandler : MonoBehaviour
{
    [SerializeField, Range(0,150)] float maxHealth = 100;
    private float currentHealth;
    VisualHandler visualHandler;
    DyingHandler dyingHandler;
    private void Awake()
    {
        currentHealth = maxHealth;
        visualHandler = GetComponent<VisualHandler>();
        dyingHandler = GetComponent<DyingHandler>();
    }
    public void TakeDamage(ICanDamage damage)
    {
        if (dyingHandler.IsDead()) return;
        currentHealth -= damage.Damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
            dyingHandler.Die();
            return;
        }
        visualHandler.SetMaterialEffect((IHaveVisualEffect)damage);
        Debug.Log($"Damage of {damage.Damage} dealt to the entity with object name {gameObject.name}. ({currentHealth})");
    }
    public void TakeDamage(IHaveVisualEffect effectDamage)
    {
        if (dyingHandler.IsDead()) return;
        currentHealth -= effectDamage.EffectDamage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
            dyingHandler.Die();
            return;
        }
        Debug.Log($"Effect Damage of {effectDamage.EffectDamage} dealt to the entity with object name {gameObject.name}. ({currentHealth})\n Which effect => {effectDamage.ParticleObject.name}");
    }
}
