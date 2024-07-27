using UnityEngine;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour
{
    [SerializeField] Image imageContent; 
    [SerializeField] Image imageContentShadow;
    private ICollectInventory currentItem;
    public bool IsFull;
    public void SetItem(ICollectInventory collectInventory)
    {
        SetImage(collectInventory.MySprite);
        currentItem = collectInventory;
        IsFull = true;
        AlphaControl(true);
    }
    public void FreeSlot()
    {
        AlphaControl(false);
        currentItem = null;
        IsFull = false;
    }
    private void SetImage(Sprite _sprite)
    {
        imageContent.sprite = _sprite;
        imageContentShadow.sprite = _sprite;
    }
    private void AlphaControl(bool active)
    {
        Color defaultColor1 = imageContent.color;
        Color defaultColor2 = imageContentShadow.color;
        if (active)
        {
            imageContent.gameObject.SetActive(true);
            imageContentShadow.gameObject.SetActive(true);
            imageContent.enabled = true;
            imageContentShadow.enabled = true;
            defaultColor1.a = 255;
            defaultColor2.a = 50;
        }
        else
        {
            imageContent.gameObject.SetActive(false);
            imageContentShadow.gameObject.SetActive(false);
            imageContent.enabled = false;
            imageContentShadow.enabled = false;
            defaultColor1.a = 0;
            defaultColor2.a = 0;
        }
        imageContent.color = defaultColor1;
        imageContentShadow.color = defaultColor2;
    }
    public ICollectInventory GetItem() => currentItem;
}
