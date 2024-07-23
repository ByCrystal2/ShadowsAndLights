using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.Mathematics;

public class LevelBehaviour : MonoBehaviour
{
    public int LevelID;    
    public Transform Spawnpoint;

    private LightsHolder MyLightHolder;
    private DirectorsHolder MyDirectorHolder;
    private TrapsHolder MyTrapHolder;
    private TargetHolder MyTargetHolder;

    private bool isCompleted;

    public void Start()
    {
        MyLightHolder = GetComponentInChildren<LightsHolder>();
        MyDirectorHolder = GetComponentInChildren<DirectorsHolder>();
        MyTrapHolder = GetComponentInChildren<TrapsHolder>();
        MyTargetHolder = GetComponentInChildren<TargetHolder>();

        MyLightHolder.LevelID = LevelID;
        MyDirectorHolder.LevelID = LevelID;
        MyTrapHolder.LevelID = LevelID;
        MyTargetHolder.LevelID = LevelID;

        foreach (Transform item in MyLightHolder.transform)
            item.GetComponent<LightBehaviour>().SetLevel(LevelID);

        foreach (Transform item in MyDirectorHolder.transform)
            item.GetComponent<DirectorBehaviour>().SetLevel(LevelID);

        foreach (Transform item in MyTrapHolder.transform)
            item.GetComponent<TrapBehaviour>().SetLevel(LevelID);

        foreach (Transform item in MyTargetHolder.transform)
            item.GetComponent<TargetBehaviour>().SetLevel(LevelID);
    }

    public void CheckLevelCompleted()
    {
        if (isCompleted)
            return;
        bool allCompleted = true;
        foreach (Transform item in MyTargetHolder.transform)
        {
            if (!item.GetComponent<TargetBehaviour>().IsCompleted())
            {
                allCompleted = false;
                break;  
            }
        }

        if (allCompleted)
        {
            isCompleted = true;
            EndLevel();
#if UNITY_EDITOR
            if (GameManager.instance == null)
                Debug.LogError("Save alinamadi. => GameManager instance sahnede mevcut degil.");
#endif
            GameManager.instance?.AddALevelFinished(LevelID);
        }
    }

    public void OpenExitGate()
    {
        //ExitGate child index = 2;
        Transform exitGateHolder = transform.GetChild(2);
        List<Transform> Doors = new() { exitGateHolder.GetChild(0).GetChild(0), exitGateHolder.GetChild(0).GetChild(1)};

        StopAllCoroutines();
        StartCoroutine(DoorsCoroutine(Doors, 0, 90, 3));
    }

    public void OpenEnterGate()
    {
        //EnterGate child index = 1;
        Transform exitGateHolder = transform.GetChild(1);
        List<Transform> Doors = new() { exitGateHolder.GetChild(0).GetChild(0), exitGateHolder.GetChild(0).GetChild(1) };

        StopAllCoroutines();
        StartCoroutine(DoorsCoroutine(Doors, 0, 90, 3));
    }

    public void CloseExitGate()
    {
        Transform exitGateHolder = transform.GetChild(2);
        List<Transform> Doors = new() { exitGateHolder.GetChild(0).GetChild(0), exitGateHolder.GetChild(0).GetChild(1) };

        StopAllCoroutines();
        StartCoroutine(DoorsCoroutine(Doors, 90, 0, 1));
    }

    public void CloseEnterGate()
    {
        Transform exitGateHolder = transform.GetChild(1);
        List<Transform> Doors = new() { exitGateHolder.GetChild(0).GetChild(0), exitGateHolder.GetChild(0).GetChild(1) };

        StopAllCoroutines();
        StartCoroutine(DoorsCoroutine(Doors, 90, 0, 1));
    }

    public void EndLevel()
    {
        //LevelObjemizin Ilk Childi EndGame Effectlerimiz icin gerekli olup, indexleri onemlidir.
        OpenExitGate();


        //EffectObjesini ac.
        StartCoroutine(PlayEffect());

        //Smooth sekilde isigi ac.
        StartCoroutine(IncreaseLightIntensity(transform.GetChild(0).GetChild((int)EndIndex.Light).GetComponent<Light>(), 200, 5));

        //foreach (Transform item in MyTrapHolder.transform)
        //    item.GetComponent<TrapBehaviour>().GetTrap().TrapCheck(LevelID + 1); => Trap min/max active Checking...
    }

    IEnumerator IncreaseLightIntensity(Light light, float targetValue, float time)
    {
        light.gameObject.SetActive(true);
        float startIntensity = 0;
        float elapsedTime = 0f;

        while (elapsedTime < time)
        {
            light.intensity = Mathf.Lerp(startIntensity, targetValue, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        light.intensity = targetValue;
    }

    IEnumerator PlayEffect()
    {
        Transform EffectParent = transform.GetChild(0).GetChild((int)EndIndex.EffectParent);
        EffectParent.gameObject.SetActive(true);
        yield return null;
        EffectParent.gameObject.SetActive(false);
        yield return null;
        EffectParent.GetChild(0).GetComponent<Magio.MagioObjectEffect>().beginEffectOnStart = true;
        yield return null;
        EffectParent.gameObject.SetActive(true);
    }

    IEnumerator DoorsCoroutine(List<Transform> transforms, float startAngle, float endAngle, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float currentAngle = Mathf.Lerp(startAngle, endAngle, elapsedTime / duration);
            int index = 0;
            foreach (Transform t in transforms)
            {
                t.localRotation = Quaternion.Euler(0, index % 2 == 0 ? -currentAngle : currentAngle, 0);
                index++;
            }
            yield return null;
        }

        int indexFinal = 0;
        foreach (Transform t in transforms)
        {
            t.localRotation = Quaternion.Euler(0, indexFinal % 2 == 0 ? -endAngle : endAngle, 0);
            indexFinal++;
        }
    }

    public LightsHolder GetLightsParent()
    {
        return MyLightHolder;
    }

    public DirectorsHolder GetDirectorsParent()
    {
        return MyDirectorHolder;
    }

    public TrapsHolder GetTrapsHolder()
    {
        return MyTrapHolder;
    }

    public TargetHolder GetTargetsParent()
    {
        return MyTargetHolder;
    }

    public enum EndIndex
    {
        FloorObject,
        EffectParent,
        Light,
    }
}
