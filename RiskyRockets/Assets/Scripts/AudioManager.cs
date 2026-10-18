using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

public class AudioManager : MonoBehaviour
{
    public AudioSource _buildingMusic;
    public AudioSource _launchingMusic;

    public void Start()
    {
        _buildingMusic.Play();
        _launchingMusic.Play();
        _launchingMusic.volume = 0.0f;
        LaunchManager.Instance.LaunchStarted += SwitchToLaunchingMusic;
    }

    public void SwitchToLaunchingMusic()
    {
        StartCoroutine(FadeMusic());
    }

    public IEnumerator FadeMusic()
    {
        float t = 0.0f;
        while (t <= 4.0f)
        {
            yield return new WaitForEndOfFrame();
            _buildingMusic.volume = 1 - (t / 4.0f);
            _launchingMusic.volume = t / 4.0f;
            t += Time.deltaTime;
        }
        _launchingMusic.volume = 1f;
    }
}
