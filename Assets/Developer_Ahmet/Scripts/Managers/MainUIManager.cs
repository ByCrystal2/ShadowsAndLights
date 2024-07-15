using UnityEngine;

public class MainUIManager : MonoBehaviour
{
    [SerializeField] GameObject joyStickUI;
    public static MainUIManager instance {  get; private set; }

    private void Awake()
    {
        if (instance != null) 
        { 
            Destroy(instance);
            return;
        }
        instance = this;
    }
    public void LockPlayer()
    {
        joyStickUI.SetActive(false);
    }
    public void UnLockPlayer()
    {
        joyStickUI.SetActive(true);
    }
}
