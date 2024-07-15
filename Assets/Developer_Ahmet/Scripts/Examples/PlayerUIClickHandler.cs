using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerUIClickHandler : MonoBehaviour, IPointerClickHandler
{
    CharacterBehaviour player;

    [SerializeField] InteractType interactType;
    private void Awake()
    {
        player = GameObject.FindWithTag("Animal").GetComponent<CharacterBehaviour>();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        player.playerUI.CurrentInteract.Interact(interactType);
        //player.playerUI.CurrentInteract = null;
    }
}
