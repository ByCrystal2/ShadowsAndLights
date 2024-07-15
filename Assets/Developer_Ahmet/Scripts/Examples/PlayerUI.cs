using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] List<PlayerUIHelper> InteractObject = new List<PlayerUIHelper>();
    [SerializeField] GameObject PlayerJostick;
    private IInteractable currentInteract;
    public IInteractable CurrentInteract { get { return currentInteract; } set { currentInteract = value; if (value == null) CloseInteractUIS();}}
    private void Awake()
    {
        CloseInteractUIS();
    }
    public void CloseInteractUIS()
    {
        foreach (var interactObject in InteractObject)
            interactObject.UI.FadeOut();
    }
    public void SetInteract(IInteractable _interactObj)
    {
        if (currentInteract == null)
        {
            CurrentInteract = _interactObj;
            foreach (InteractType interactType in CurrentInteract.InteractTypes)
                ShowInteractUIs(interactType);
        }
    }
    private void ShowInteractUIs(InteractType _interactType)
    {
        PlayerUIHelper interactUI = InteractObject.Where(x => x.InteractType == _interactType).SingleOrDefault();
        if (interactUI.UI != null)
        {
            if (interactUI.InteractType != InteractType.Dropable)
                interactUI.UI.gameObject.SetActive(true);           
        }
        else
            Debug.LogWarning("Istenilen InteractType turunde bir UI objesi playerde bulunmamaktadir.");
    }
    public void ShowDropable()
    {
        InteractObject.Where(x => x.InteractType == InteractType.Dropable).SingleOrDefault().UI.gameObject.SetActive(true);
    }
}
[System.Serializable]
public struct PlayerUIHelper
{
    public UIFade UI;
    public InteractType InteractType;
}