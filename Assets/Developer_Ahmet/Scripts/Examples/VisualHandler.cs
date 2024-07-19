using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ArrowBehaviour;

public class VisualHandler : MonoBehaviour
{
    [SerializeField] private List<Renderer> myRenderers = new List<Renderer>();
    private List<Material[]> myDefaultMaterials = new List<Material[]>();
    [SerializeField] Transform effectContent;

    private void Awake()
    {
        foreach (Renderer renderer in myRenderers)
            if (renderer != null)
                myDefaultMaterials.Add(renderer.materials);
    }

    Coroutine ieSetMaterialEffectCoroutine;
    public void SetMaterialEffect(IHaveVisualEffect haveVisual)
    {
        if (ieSetMaterialEffectCoroutine != null)
            StopCoroutine(ieSetMaterialEffectCoroutine);
        ieSetMaterialEffectCoroutine = StartCoroutine(IESetMaterialEffect(haveVisual.Material, haveVisual.ChangeTime));
    }

    private IEnumerator IESetMaterialEffect(Material newMaterial, float changeTime)
    {
        Debug.Log($"{gameObject.name} objesinin materyalleri güncelleniyor... Material Name => {newMaterial.name}");

        // Var olan materyal dizisine yeni materyali ekle
        foreach (Renderer renderer in myRenderers)
        {
            if (renderer != null)
            {
                Material[] originalMaterials = renderer.materials;
                Material[] newMaterials = new Material[originalMaterials.Length + 1];
                originalMaterials.CopyTo(newMaterials, 0);
                newMaterials[newMaterials.Length - 1] = newMaterial;
                renderer.materials = newMaterials;
            }
        }

        yield return new WaitForSeconds(changeTime);

        // Eski materyalleri geri yükle
        for (int i = 0; i < myRenderers.Count; i++)
        {
            Renderer renderer = myRenderers[i];
            if (renderer != null)
            {
                renderer.materials = myDefaultMaterials[i];
            }
        }

        Debug.Log($"{gameObject.name} objesinin materyalleri sýfýrlandý!");
    }
    public void StartEffect(IHaveVisualEffect haveVisualEffect)
    {
        if (haveVisualEffect.ParticleObject == null) return;
        EffectType effect;
        if (haveVisualEffect is ArrowBehaviour _arrow)
        {
            GameObject particleEffect = _arrow.GetArrow().EffectTheTarget(_arrow.ParticleObject, effectContent);
            StartCoroutine(EffectDamage(particleEffect,haveVisualEffect));
            
        }
            Debug.Log(haveVisualEffect.ParticleObject.name + " adli particle effect instantiate edildi. Gonderilen obje:" + name);
    }
    IEnumerator EffectDamage(GameObject particleObj,IHaveVisualEffect visualEffect)
    {
        for (int i = 0; i < visualEffect.EffectChangeTime; i++)
        {
            GetComponent<HealthHandler>().TakeDamage(visualEffect);
            yield return new WaitForSeconds(1);
        }
        Destroy(particleObj);
    }
}
