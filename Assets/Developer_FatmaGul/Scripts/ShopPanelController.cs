using UnityEngine;
using UnityEngine.UI;

public class ShopPanelController : MonoBehaviour
{
    [SerializeField] Transform WindowsButtonPanel;
    [SerializeField] Image GoldWindow;
    [SerializeField] Image GemWindow;
    private void Start()
    {
        int childCount = WindowsButtonPanel.childCount;
        for (int i = 0; i < childCount; i++)
        {
            if(WindowsButtonPanel.GetChild(i).TryGetComponent(out Button button))
            {
                if (i==0)
                {
                    button.onClick.AddListener(GoldButtonController);
                }
                else
                {
                    button.onClick.AddListener(GemButtonController);
                }
            }
        }
        GoldButtonController();
    }

    public void GoldButtonController()
    {
        GoldWindow.gameObject.SetActive(true);
        GemWindow.gameObject.SetActive(false);
    }
    public void GemButtonController()
    {
        GoldWindow.gameObject.SetActive(false);
        GemWindow.gameObject.SetActive(true);
    }
}
