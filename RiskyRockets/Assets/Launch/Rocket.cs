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
    [SerializeField] private float _shakeDistance = 0.1f;
    [SerializeField] private float _cargoGravityScale = 3f;
    [SerializeField] private Transform[] _scrollingWorld;

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
    private Vector2 _shakeOffset;
    private float _currentShake;
    private float _timeSinceLaunch;
    private bool _isFlying;
    private bool _fuelSpent;

    private void Start()
    {
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
            _launchManager.EndLaunch(BestAltitudeMiles);
        }
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

        Vector3 target = transform.position + (Vector3)_cameraOffset;
        target.z = _cameraStartPosition.z;

        float centerDuration = _launchManager.IgnitionDelay;
        float p = centerDuration > 0f ? Mathf.Clamp01(_timeSinceLaunch / centerDuration) : 1f;
        p = AnimationCurves.CalculateParametric(p);
        _camera.transform.position = Vector3.Lerp(_cameraStartPosition, target, p) + (Vector3)_shakeOffset;
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
