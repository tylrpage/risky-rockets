using System.Collections;
using System.Numerics;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    private UnityEngine.Vector2 _initialPosition;
    private UnityEngine.Vector2 _anchoredPosition;
    private const float SHAKE_DISTANCE = 0.1f;
    private const float SHAKE_INTERVAL = 0.05f;
    private float _shakeInterval;
    private const float BOOST_DURATION = 3.0f;
    private const float BOOST_DISTANCE = 4.0f;

    private void Start()
    {
        LaunchManager.Instance.LaunchStarted += BeginShake;
    }

    public IEnumerator Boost()
    {
        float t = 0.0f;
        while (t < BOOST_DURATION / 5f)
        {
            yield return new WaitForEndOfFrame();
            float p = t / (BOOST_DURATION / 5f);
            p = AnimationCurves.CalculateParametric(p);
            _anchoredPosition = _initialPosition + new UnityEngine.Vector2(0, BOOST_DISTANCE * p);
            t += Time.deltaTime;
        }
        while (t < BOOST_DURATION)
        {
            yield return new WaitForEndOfFrame();
            float p = (t - 3 * BOOST_DURATION / 5f) / (3 * BOOST_DURATION / 5f);
            p = AnimationCurves.CalculateParametric(p);
            _anchoredPosition = _initialPosition + new UnityEngine.Vector2(0, BOOST_DISTANCE) - new UnityEngine.Vector2(0, BOOST_DISTANCE * p);
            t += Time.deltaTime;
        }
        _anchoredPosition = _initialPosition;
    }

    private void BeginShake()
    {
        _initialPosition = transform.position;
        _anchoredPosition = _initialPosition;
        StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
        while (true)
        {
            Debug.Log("shake!");
            yield return new WaitForSeconds(SHAKE_INTERVAL);
            UnityEngine.Vector2 randomOffset = new UnityEngine.Vector2(Random.Range(-SHAKE_DISTANCE, SHAKE_DISTANCE), Random.Range(-SHAKE_DISTANCE, SHAKE_DISTANCE)); 
            transform.position = _anchoredPosition + randomOffset;
        }
    }
}
