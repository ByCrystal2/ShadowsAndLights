using UnityEngine;

public class MainUIManager : MonoBehaviour
{
    [SerializeField] GameObject joyStickUI;
    [SerializeField] BatteryUI batteryUI;
    [SerializeField] FlashlightBehaviour flashlight;
    [SerializeField] LightSwitchUI LightSwitch;
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

    public BatteryUI GetBatteryUI()
    {
        return batteryUI;
    }
    
    public FlashlightBehaviour GetFlashlight()
    {
        return flashlight;
    }
    
    public LightSwitchUI GetLightSwitch()
    {
        return LightSwitch;
    }
}
