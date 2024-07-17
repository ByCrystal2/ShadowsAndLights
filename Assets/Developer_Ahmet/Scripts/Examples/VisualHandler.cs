using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualHandler : MonoBehaviour
{
    [SerializeField] private List<Renderer> myRenderers = new List<Renderer>();
    private List<Material[]> myDefaultMaterials = new List<Material[]>();

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

        foreach (Renderer renderer in myRenderers)
        {
            if (renderer != null)
            {
                Material[] newMaterials = new Material[renderer.materials.Length];
                for (int j = 0; j < newMaterials.Length; j++)
                {
                    newMaterials[j] = newMaterial;
                }
                renderer.materials = newMaterials;
            }
        }

        yield return new WaitForSeconds(changeTime);

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
}
