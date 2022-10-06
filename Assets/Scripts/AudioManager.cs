using System;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : Singleton<AudioManager>
{
    private AudioSource audioSrc;

    public float maxVolume = 2;

    protected override void Awake()
    {
        base.Awake();
        audioSrc = GetComponent<AudioSource>();
        
        audioSrc.PlayDelayed(1);
    }

    private void Update()
    {
        audioSrc.volume = Mathf.Lerp(0, maxVolume, audioSrc.time / 10);
    }

    public void UpdateMasterVolume(Slider volumeSlider)
    {
        maxVolume = volumeSlider.value * volumeSlider.value;
    }
    
    public void StopMusic() => audioSrc.Stop();
}
