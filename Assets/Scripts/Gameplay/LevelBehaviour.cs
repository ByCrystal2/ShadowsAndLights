using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class LevelBehaviour : MonoBehaviour
{
    public int LevelID;

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
            EndLevel();
            GameManager.instance.AddALevelFinished(LevelID);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            EndLevel();
            isCompleted = true;
        }
    }

    public void EndLevel()
    {
        if (isCompleted)
            return;

        //LevelObjemizin Ilk Childi EndGame Effectlerimiz icin gerekli olup, indexleri onemlidir.
        StopAllCoroutines();

        //EffectObjesini ac.
        StartCoroutine(PlayEffect());

        //Smooth sekilde isigi ac.
        StartCoroutine(IncreaseLightIntensity(transform.GetChild(0).GetChild((int)EndIndex.Light).GetComponent<Light>(), 200, 5));
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
