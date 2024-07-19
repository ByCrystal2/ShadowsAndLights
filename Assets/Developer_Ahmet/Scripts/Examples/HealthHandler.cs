using System.Collections;
using UnityEngine;

[RequireComponent(typeof(VisualHandler))]
public class HealthHandler : MonoBehaviour
{
    [SerializeField, Range(0,150)] float maxHealth = 100;
    private float currentHealth;
    private void Awake()
    {
        currentHealth = maxHealth;
    }
    public void TakeDamage(ICanDamage damage)
    {
        currentHealth -= damage.Damage;
        GetComponent<VisualHandler>()?.SetMaterialEffect((IHaveVisualEffect)damage);
        Debug.Log($"Damage of {damage.Damage} dealt to the entity with object name {gameObject.name}. ({currentHealth})");
    }
}
