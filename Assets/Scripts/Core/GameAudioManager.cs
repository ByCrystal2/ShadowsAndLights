using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager instance { get; private set; }
    public List<AudioSource> AudioManagersforUI = new List<AudioSource>();
    public List<AudioSource> AudioManagersforWorld = new List<AudioSource>();

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void PlayButtonClickSound()
    {
        float targetVol = 0.75f;
        float volGeneral = CalculateSoundPercentGeneral(targetVol);
        float volFinal = CalculateSoundPercentSfx(volGeneral);

        AudioClip clip = Resources.Load<AudioClip>("Sounds/ButtonClick");

        AudioSource source = GetFreeAudioSource(true);
        SetSourceDatas(source, volFinal, Vector3.zero, 1, 1.5f);
        source.PlayOneShot(clip);

        StartCoroutine(ReturnToPool(source, clip.length + 1));
    }

    public void PlayLightSourceSound(Vector3 _pos)
    {
        float targetVol = 0.3f;
        float volGeneral = CalculateSoundPercentGeneral(targetVol);
        float volFinal = CalculateSoundPercentSfx(volGeneral);

        AudioClip clip = Resources.Load<AudioClip>("Sounds/LightSource");

        AudioSource source = GetFreeAudioSource(false);
        SetSourceDatas(source, volFinal, _pos, 1, 3f);
        source.PlayOneShot(clip);

        StartCoroutine(ReturnToPool(source, clip.length + 1));
    }

    public void PlayLightReflectSound(Vector3 _pos)
    {
        float targetVol = 0.5f;
        float volGeneral = CalculateSoundPercentGeneral(targetVol);
        float volFinal = CalculateSoundPercentSfx(volGeneral);

        AudioClip clip = Resources.Load<AudioClip>("Sounds/LightReflect");

        AudioSource source = GetFreeAudioSource(false);
        SetSourceDatas(source, volFinal, _pos, 1, 3f);

        source.PlayOneShot(clip);

        StartCoroutine(ReturnToPool(source, clip.length + 1));
    }

    void SetSourceDatas(AudioSource source, float volFinal, Vector3 _pos, float _pitch, float _maxDistance)
    {
        source.transform.position = _pos;
        source.maxDistance = _maxDistance;
        source.volume = volFinal;
        source.pitch = _pitch;
    }

    float CalculateSoundPercentGeneral(float _vol)
    {
        return (_vol * SettingsManager.instance.ActiveSetting.audioSettings.MainVolume) / 100;
    }

    float CalculateSoundPercentSfx(float _vol)
    {
        return (_vol * SettingsManager.instance.ActiveSetting.audioSettings.SFXVolume) / 100;
    }

    float CalculateSoundPercentBgm(float _vol)
    {
        return (_vol * SettingsManager.instance.ActiveSetting.audioSettings.BGMVolume) / 100;
    }

    AudioSource GetFreeAudioSource(bool _ui)
    {
        if (_ui)
        {
            foreach (var item in AudioManagersforUI)
            {
                if (!item.gameObject.activeSelf)
                {
                    item.gameObject.SetActive(true);
                    return item;
                }
            }

            GameObject newAudio = Instantiate(AudioManagersforUI[0].gameObject, transform);
            newAudio.SetActive(true);
            AudioSource s = newAudio.GetComponent<AudioSource>();
            AudioManagersforUI.Add(s);
            return s;
        }
        else
        {
            foreach (var item in AudioManagersforWorld)
            {
                if (!item.gameObject.activeSelf)
                {
                    item.gameObject.SetActive(true);
                    return item;
                }
            }

            GameObject newAudio = Instantiate(AudioManagersforWorld[0].gameObject, transform);
            newAudio.SetActive(true);
            AudioSource s = newAudio.GetComponent<AudioSource>();
            AudioManagersforWorld.Add(s);
            return s;
        }
    }

    IEnumerator ReturnToPool(AudioSource _s, float _t)
    {
        yield return new WaitForSeconds(_t);
        _s.outputAudioMixerGroup = null;
        _s.gameObject.SetActive(false);
    }

    public void PlayTrapSound(AudioSourceHelper _audioSourceHelper)
    {
        float targetVol = _audioSourceHelper.Volume; //0.75f;
        float volGeneral = CalculateSoundPercentGeneral(targetVol);
        float volFinal = CalculateSoundPercentSfx(volGeneral);

        AudioClip clip = Resources.Load<AudioClip>("Sounds/Traps/Trap_" + _audioSourceHelper.id.ToString());

        AudioSource source = GetFreeAudioSource(false);
        SetSourceDatas(source, volFinal, _audioSourceHelper.Position, _audioSourceHelper.Pitch, 1);

        source.PlayOneShot(clip);

        StartCoroutine(ReturnToPool(source, clip.length + 1));
    }
}
[System.Serializable]
public struct AudioSourceHelper
{
    public int id;
    [HideInInspector] public Vector3 Position;
    public bool Loop;
    public float Priority;
    public float Volume;
    public float Pitch;
    public float StereoPan;
    public float SpatialBlend;
    public float ReverbZoneMix;
    public float DopplerLevel;
    public float Spread;
    public float MinDistance;
    public float MaxDistance;
    public AudioSourceHelper(int _id,Vector3 _position, bool _loop, float _priority, float _volume, float _pitch, float _stereoPan, float _spatialBlend, float _reverbZoneMix, float _dopplerLevel, float _spread, float _minDistance, float _maxDistance)
    {
        id = _id;
        Position = _position;
        Loop = _loop;
        Priority = _priority;
        Volume = _volume;
        Pitch = _pitch;
        StereoPan = _stereoPan;
        SpatialBlend = _spatialBlend;
        ReverbZoneMix = _reverbZoneMix;
        DopplerLevel = _dopplerLevel;
        Spread = _spread;
        MinDistance = _minDistance;
        MaxDistance = _maxDistance;
    }
}

