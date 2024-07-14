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
        source.volume = volFinal;
        source.pitch = 1f;
        source.PlayOneShot(clip);

        StartCoroutine(ReturnToPool(source, clip.length + 1));
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
}
