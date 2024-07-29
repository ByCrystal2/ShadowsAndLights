using System.Collections;
using UnityEngine;

public class FlashlightBehaviour : MonoBehaviour
{
    [Header("--Required Component--")]
    [SerializeField] private Light Flashlight;
    [SerializeField] private BoxCollider HitTrigger;

    [Header("--Runtime Datas (DEBUG)--")]
    [SerializeField] private LightPuzzleHandler.LightColor CurrentLightColor;
    [SerializeField] private int BatteryMax;
    [SerializeField] private int BatteryCount;
    [SerializeField] private float MaxBatteryLife;
    [SerializeField] private float BatteryLife;

    [SerializeField] private float CurrentConsumeMultiplier;
    [SerializeField] private bool NoBattery;

    private void Start()
    {
        //BatteryCount = GameManager.instance.currentActiveSaveData.HoldBatary;
        BatteryCount = 3;
        SetBatteryLife();
        SetBatteryCapacity();
    }

    void SetBatteryLife()
    {
        MaxBatteryLife = LightPuzzleHandler.PerBataryCapacityBase;
        BatteryLife = MaxBatteryLife;
        //BatteryLife = GameManager.instance.currentActiveSaveData.CurrentBatteryLife;
    }

    void SetBatteryCapacity()
    {
        BatteryMax = LightPuzzleHandler.MaxBataryCapacityBase;
        MainUIManager.instance.GetLightSwitch().SetBattery(BatteryMax, BatteryCount);
    }

    public void ChangeLightColorTo(LightPuzzleHandler.LightColor _newLight)
    {
        StopAllCoroutines();
        CurrentLightColor = _newLight;

        if (NoBattery && _newLight != LightPuzzleHandler.LightColor.White)
        {
            Flashlight.intensity = 0;
            GameAudioManager.instance.PlayNoBatterySound();
            return;
        }
        StartCoroutine(SmoothSwitch());
        HitTrigger.enabled = false;
        CurrentConsumeMultiplier = LightPuzzleHandler.MainBataryConsumesByLight[(int)_newLight]; // bu degeri skill leveliyle burada hesaplayabiliriz.
    }

    private IEnumerator SmoothSwitch()
    {
        GameAudioManager.instance.PlayFlashlightOffSound();
        float elapsedTime = 0f;
        float duration = 0.25f;

        while (elapsedTime < duration)
        {
            Flashlight.intensity = Mathf.Lerp(25, 0, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.25f);
        GameAudioManager.instance.PlayFlashlightOnSound();

        Flashlight.intensity = 0;

        Flashlight.color = LightPuzzleHandler.GetColorByLight(CurrentLightColor);
        elapsedTime = 0f;
        duration = 0.25f;

        while (elapsedTime < duration)
        {
            Flashlight.intensity = Mathf.Lerp(0, 25, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Flashlight.intensity = 25;
        HitTrigger.enabled = NoBattery ? false : true;
    }

    public float CurrentBataryLife()
    {
        return BatteryLife;
    }

    public int CurrentBataryAmount()
    {
        return BatteryCount;
    }

    public void OnBatteryDead()
    {
        if (BatteryCount > 1)
        {
            BatteryLife = 100;
            BatteryCount--;
        }
        else
        {
            OnLackofBatteries();
        }
    }

    public void OnLackofBatteries()
    {
        NoBattery = true;
        HitTrigger.enabled = false;
        Flashlight.intensity = 0;
    }

    private void Update()
    {
        if (HitTrigger.enabled)
        {
            BatteryLife -= Time.deltaTime * CurrentConsumeMultiplier;
            MainUIManager.instance.GetLightSwitch().UpdateBatteriesUI(BatteryCount, BatteryLife / MaxBatteryLife);
            if (BatteryLife <= 0)
            {
                OnBatteryDead();
            }
        }
    }
}
