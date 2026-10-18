using System;
using System.Collections;
using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager Instance;

    [SerializeField] private AudioSource _rocketSound;
    [SerializeField] private AudioSource _hitSFXSource;
    [SerializeField] private float _rocketRampUpDuration = 1f;
    [SerializeField] private float _rocketRampDownDuration = 1.5f;

    // The volume set in the Inspector is the rocket sound's peak volume.
    private float _rocketTargetVolume;
    private Coroutine _rocketFade;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        _rocketTargetVolume = _rocketSound.volume;
    }

    private void Start()
    {
        LaunchManager.Instance.LaunchStarted += StartRocket;
        TrashManager.Instance.ShipEmptied += StopRocket;
    }

    private void OnDestroy()
    {
        if (LaunchManager.Instance != null) LaunchManager.Instance.LaunchStarted -= StartRocket;
        if (TrashManager.Instance != null) TrashManager.Instance.ShipEmptied -= StopRocket;
    }

    public void PlayCollisionSFX(float velocity)
    {
        _hitSFXSource.volume = RemapClamp(velocity, 0f, 5f, 0f, 0.5f);
        _hitSFXSource.Play();
    }

    private static float RemapClamp(float value, float iMin, float iMax, float oMin, float oMax)
    {
        // InverseLerp outputs 0 to 1 and inherently clamps the value
        float t = Mathf.InverseLerp(iMin, iMax, value);

        // Lerp maps that 0 to 1 value smoothly into your new target range
        return Mathf.Lerp(oMin, oMax, t);
    }

    public void StartRocket()
    {
        _rocketSound.volume = 0f;
        _rocketSound.Play();
        FadeRocketTo(_rocketTargetVolume, _rocketRampUpDuration, false);
    }

    public void StopRocket()
    {
        FadeRocketTo(0f, _rocketRampDownDuration, true);
    }

    private void FadeRocketTo(float to, float duration, bool stopAtEnd)
    {
        // Replace any fade in progress so the new one starts from the current volume.
        if (_rocketFade != null) StopCoroutine(_rocketFade);
        _rocketFade = StartCoroutine(FadeRocket(to, duration, stopAtEnd));
    }

    private IEnumerator FadeRocket(float to, float duration, bool stopAtEnd)
    {
        float from = _rocketSound.volume;
        float t = 0f;
        while (t < duration)
        {
            _rocketSound.volume = Mathf.Lerp(from, to, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        _rocketSound.volume = to;
        if (stopAtEnd) _rocketSound.Stop();
        _rocketFade = null;
    }
}
