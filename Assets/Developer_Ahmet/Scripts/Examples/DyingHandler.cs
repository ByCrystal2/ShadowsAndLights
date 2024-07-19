using UnityEngine;
using UnityEngine.Events;
[RequireComponent(typeof(VisualHandler), typeof(HealthHandler))]
public class DyingHandler : MonoBehaviour
{
    public UnityEvent OnDying;
    bool isDied;
    public void Die()
    {
        OnDying?.Invoke();
        isDied = true;
        Debug.Log("This entity died. => " + gameObject.name);
    }
    public bool IsDead()
    {
        return isDied;
    }
}