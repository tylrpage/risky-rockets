using System.Collections;
using System.Numerics;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Analytics;

public class Rocket : MonoBehaviour
{
    private UnityEngine.Vector3 _initialCameraPosition;
    private UnityEngine.Vector2 _rocketPosition;
    public const float BOOST_DURATION = 3.0f;
    private const float BOOST_DISTANCE = 6.0f;
    private float _shakeDistance = 0.1f;
    private const float SHAKE_INTERVAL = 0.05f;
    private float _shakeInterval;
    [SerializeField] private Camera _camera;

    private void Start()
    {
        LaunchManager.Instance.LaunchStarted += BeginShake;
    }

    public IEnumerator Boost()
    {
        float t = 0.0f;
        _initialCameraPosition = _camera.transform.position;
        while (t < BOOST_DURATION / 5f)
        {
            yield return new WaitForEndOfFrame();
            float p = t / (BOOST_DURATION / 5f);
            p = AnimationCurves.CalculateParametric(p);

            _camera.transform.position = _initialCameraPosition - new UnityEngine.Vector3(0, BOOST_DISTANCE * p, 0);
            t += Time.deltaTime;
        }
        while (t < 3 * BOOST_DURATION / 5f)
        {
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
        }
        while (t < BOOST_DURATION)
        {
            yield return new WaitForEndOfFrame();
            float p = (t - 3 * BOOST_DURATION / 5f) / (2 * BOOST_DURATION / 5f);
            p = AnimationCurves.CalculateParametric(p);
            _camera.transform.position = _initialCameraPosition - new UnityEngine.Vector3(0, BOOST_DISTANCE * (1 - p), 0);
            t += Time.deltaTime;
        }
        _camera.transform.position = _initialCameraPosition;
    }

    private void BeginShake()
    {
        _rocketPosition = transform.position;
        StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
        yield return new WaitForSeconds(5f);
        _rocketPosition = transform.position;
        while (true)
        {
            yield return new WaitForSeconds(SHAKE_INTERVAL);
            UnityEngine.Vector2 randomOffset = new UnityEngine.Vector2(Random.Range(-_shakeDistance, _shakeDistance), Random.Range(-_shakeDistance, _shakeDistance)); 
            transform.position = _rocketPosition + randomOffset;
        }
    }

    public void StopShake()
    {
        StartCoroutine(EndShake());
    }

    private IEnumerator EndShake()
    {
        while (_shakeDistance > 0)
        {
            yield return new WaitForSeconds(0.3f);
            _shakeDistance -= 0.01f;
        }
    }
}
