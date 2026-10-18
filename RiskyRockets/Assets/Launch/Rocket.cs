using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The rocket itself stays still (so its cargo is plain physics); the world scrolls down to show the climb.
public class Rocket : MonoBehaviour
{
    private const float SHAKE_INTERVAL = 0.05f;
    // Flight ends once the distance left to coast is below this; the remainder is added so the final height is exact.
    private const float STOP_DISTANCE_MILES = 0.05f;

    [SerializeField] private Camera _camera;
    [SerializeField] private Vector2 _cameraOffset = new Vector2(1f, 0f);
    // After the flight ends, how long the camera takes to stop following the rocket.
    [SerializeField] private float _cameraReleaseDuration = 0.75f;
    [SerializeField] private float _fallDelay = 0.3f;
    [SerializeField] private float _fallStartSpeed = 1f;
    [SerializeField] private float _fallAcceleration = 3f;
    [SerializeField] private float _fallTiltDegrees = 20f;
    [SerializeField] private float _fallTiltDuration = 1.5f;
    [SerializeField] private float _shakeDistance = 0.1f;
    [SerializeField] private float _cargoGravityScale = 3f;
    [SerializeField] private Transform[] _scrollingWorld;
    [SerializeField] private SpriteRenderer _smallFire;
    [SerializeField] private SpriteRenderer _bigFire;
    // Trash worth more fuel than this flashes the big fire.
    [SerializeField] private int _bigFireFuelThreshold = 10;

    public float AltitudeMiles { get; private set; }
    public float VelocityMilesPerSecond { get; private set; }
    public float BestAltitudeMiles { get; private set; }

    private struct Burn
    {
        public float Thrust;
        public float TimeLeft;
    }

    private readonly List<Burn> _burns = new();
    private readonly List<(Transform transform, Vector3 start)> _scrolling = new();
    private LaunchManager _launchManager;
    private Vector3 _cameraStartPosition;
    private Vector3 _followTarget;
    private Vector3 _lastRocketTarget;
    private bool _cameraReleased;
    private float _timeSinceRelease;
    private Vector2 _shakeOffset;
    private float _currentShake;
    private float _timeSinceLaunch;
    private bool _isFlying;
    private bool _fuelSpent;
    private Coroutine _smallFireFade;
    private Coroutine _bigFireFade;

    private void Start()
    {
        SetAlpha(_smallFire, 0f);
        SetAlpha(_bigFire, 0f);

        _launchManager = LaunchManager.Instance;
        _launchManager.LaunchStarted += OnLaunchStarted;
        TrashManager.Instance.ShipEmptied += OnShipEmptied;
    }

    private void OnDestroy()
    {
        if (_launchManager != null) _launchManager.LaunchStarted -= OnLaunchStarted;
        if (TrashManager.Instance != null) TrashManager.Instance.ShipEmptied -= OnShipEmptied;
    }

    private void OnLaunchStarted()
    {
        _cameraStartPosition = _camera.transform.position;
        _timeSinceLaunch = 0f;
        _isFlying = true;
        _cameraReleased = false;

        TrashManager.Instance.PrepareCargo(_cargoGravityScale);

        _scrolling.Clear();
        if (_scrollingWorld != null)
        {
            foreach (Transform t in _scrollingWorld)
            {
                if (t != null) _scrolling.Add((t, t.position));
            }
        }
        foreach (Transform t in TrashManager.Instance.FreezeWorldTrash())
        {
            _scrolling.Add((t, t.position));
        }

        _currentShake = _shakeDistance;
        StartCoroutine(Shake());
    }

    private void OnShipEmptied()
    {
        _fuelSpent = true;
    }

    public void AddBurn(float deltaV, float duration)
    {
        if (duration <= 0f)
        {
            VelocityMilesPerSecond += deltaV;
            return;
        }
        _burns.Add(new Burn { Thrust = deltaV / duration, TimeLeft = duration });
    }

    private void Update()
    {
        if (_launchManager == null || !_launchManager.HasLaunched) return;
        _timeSinceLaunch += Time.deltaTime;
        if (!_isFlying) return;

        float k = _launchManager.DragK;

        // Split the frame wherever a burn ends so each burn delivers exactly its delta-v.
        float remaining = Time.deltaTime;
        while (remaining > 0f)
        {
            float step = remaining;
            float thrust = 0f;
            foreach (Burn burn in _burns)
            {
                thrust += burn.Thrust;
                step = Mathf.Min(step, burn.TimeLeft);
            }

            Integrate(thrust, k, step);

            for (int i = _burns.Count - 1; i >= 0; i--)
            {
                Burn burn = _burns[i];
                burn.TimeLeft -= step;
                if (burn.TimeLeft <= 1e-6f) _burns.RemoveAt(i);
                else _burns[i] = burn;
            }
            remaining -= step;
        }

        bool finished = _fuelSpent && _burns.Count == 0 && VelocityMilesPerSecond / k < STOP_DISTANCE_MILES;
        if (finished)
        {
            AltitudeMiles += VelocityMilesPerSecond / k;
            VelocityMilesPerSecond = 0f;
        }

        BestAltitudeMiles = Mathf.Max(BestAltitudeMiles, AltitudeMiles);
        ScrollWorld();
        ScoreManager.Instance.SetHeight(BestAltitudeMiles);

        if (finished)
        {
            _isFlying = false;
            _cameraReleased = true;
            _timeSinceRelease = 0f;
            _launchManager.EndLaunch(BestAltitudeMiles);
            if (!_launchManager.ReachedOrbit(BestAltitudeMiles)) StartCoroutine(Fall());
        }
    }

    private IEnumerator Fall()
    {
        yield return new WaitForSeconds(_fallDelay);

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Quaternion startRotation = transform.rotation;
        float speed = _fallStartSpeed;
        float t = 0f;

        while (true)
        {
            float dt = Time.deltaTime;
            t += dt;
            speed += _fallAcceleration * dt;
            transform.position += Vector3.down * (speed * dt);

            float tilt = _fallTiltDuration > 0f ? Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / _fallTiltDuration)) : 1f;
            transform.rotation = startRotation * Quaternion.Euler(0f, 0f, -_fallTiltDegrees * tilt);

            float viewBottom = _camera.transform.position.y - _camera.orthographicSize;
            if (TopOf(renderers) < viewBottom)
            {
                gameObject.SetActive(false);
                yield break;
            }
            yield return null;
        }
    }

    private static float TopOf(Renderer[] renderers)
    {
        float top = float.MinValue;
        foreach (Renderer r in renderers)
        {
            if (r != null && r.enabled && r.gameObject.activeInHierarchy) top = Mathf.Max(top, r.bounds.max.y);
        }
        return top;
    }

    // Exact solution of dv/dt = a - k*v over dt, so results don't depend on frame rate.
    private void Integrate(float a, float k, float dt)
    {
        float terminal = a / k;
        float decay = Mathf.Exp(-k * dt);
        float v0 = VelocityMilesPerSecond;
        AltitudeMiles += terminal * dt + (v0 - terminal) * (1f - decay) / k;
        VelocityMilesPerSecond = terminal + (v0 - terminal) * decay;
    }

    private void ScrollWorld()
    {
        Vector3 scroll = Vector3.down * (AltitudeMiles * _launchManager.WorldUnitsPerMile);
        foreach ((Transform t, Vector3 start) in _scrolling)
        {
            if (t != null) t.position = start + scroll;
        }
    }

    private void LateUpdate()
    {
        if (_launchManager == null || !_launchManager.HasLaunched) return;

        Vector3 rocketTarget = transform.position + (Vector3)_cameraOffset;
        rocketTarget.z = _cameraStartPosition.z;

        if (!_cameraReleased)
        {
            _followTarget = rocketTarget;
        }
        else
        {
            // Follow the rocket's movement less and less until the camera stops.
            _timeSinceRelease += Time.deltaTime;
            float released = _cameraReleaseDuration > 0f ? Mathf.Clamp01(_timeSinceRelease / _cameraReleaseDuration) : 1f;
            float weight = 1f - Mathf.SmoothStep(0f, 1f, released);
            _followTarget += (rocketTarget - _lastRocketTarget) * weight;
        }
        _lastRocketTarget = rocketTarget;

        float centerDuration = _launchManager.IgnitionDelay;
        float p = centerDuration > 0f ? Mathf.Clamp01(_timeSinceLaunch / centerDuration) : 1f;
        p = AnimationCurves.CalculateParametric(p);
        _camera.transform.position = Vector3.Lerp(_cameraStartPosition, _followTarget, p) + (Vector3)_shakeOffset;
    }

    private IEnumerator Shake()
    {
        while (_currentShake > 0f)
        {
            _shakeOffset = new Vector2(Random.Range(-_currentShake, _currentShake), Random.Range(-_currentShake, _currentShake));
            yield return new WaitForSeconds(SHAKE_INTERVAL);
        }
        _shakeOffset = Vector2.zero;
    }

    public void FlashFire(int fuel, float duration)
    {
        if (fuel > _bigFireFuelThreshold) FlashFire(_bigFire, ref _bigFireFade, duration);
        else FlashFire(_smallFire, ref _smallFireFade, duration);
    }

    private void FlashFire(SpriteRenderer fire, ref Coroutine fade, float duration)
    {
        if (fire == null) return;
        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(FadeFire(fire, duration));
    }

    private static IEnumerator FadeFire(SpriteRenderer fire, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            SetAlpha(fire, 1f - t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        SetAlpha(fire, 0f);
    }

    private static void SetAlpha(SpriteRenderer sr, float alpha)
    {
        if (sr == null) return;
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }

    public void StopShake()
    {
        StartCoroutine(EndShake());
    }

    private IEnumerator EndShake()
    {
        while (_currentShake > 0f)
        {
            yield return new WaitForSeconds(0.3f);
            _currentShake -= 0.01f;
        }
    }
}
