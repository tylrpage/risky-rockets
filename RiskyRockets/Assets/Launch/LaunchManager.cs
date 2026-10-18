using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchManager : MonoBehaviour
{
    public static LaunchManager Instance;
    public event Action LaunchStarted;
    public event Action<float> LaunchEnded;

    // Burning exactly this much fuel puts the rocket at orbit height.
    [SerializeField] private float _targetFuel = 150f;
    [SerializeField] private float _orbitHeightMiles = 100f;
    [SerializeField] private float _burnTimePerItem = 0.35f;
    // Pause after one piece finishes burning before the next one starts.
    [SerializeField] private float _delayBetweenBurns = 0.2f;
    // The camera centers on the rocket during this delay.
    [SerializeField] private float _ignitionDelay = 1.5f;
    [SerializeField] private float _driftTime = 2f;
    [SerializeField] private float _worldUnitsPerMile = 10f;
    [SerializeField] private GameObject _dropArea;
    [SerializeField] private Transform _noseCone;
    [SerializeField] private float _noseCloseDuration;
    // Trigger collider on the nose; trash it touches while closing is destroyed.
    [SerializeField] private Collider2D _noseCollider;
    [SerializeField] private float _roskyOriginalOffset;
    [SerializeField] private Transform _rosky;
    [SerializeField] private float _roskyMoveDuration;

    private readonly List<Collider2D> _noseHits = new();

    public float TargetFuel => _targetFuel;
    public float OrbitHeightMiles => _orbitHeightMiles;
    public float BurnTimePerItem => _burnTimePerItem;
    public float DelayBetweenBurns => _delayBetweenBurns;
    public float IgnitionDelay => _ignitionDelay;
    public float WorldUnitsPerMile => _worldUnitsPerMile;

    // Linear drag: dv/dt = thrust - DragK * v. Total distance = total delta-v / DragK,
    // so height is exactly proportional to fuel burned.
    public float DragK => 1f / Mathf.Max(0.01f, _driftTime);

    // Velocity (mi/s) each unit of fuel adds, chosen so TargetFuel ends up at orbit height.
    public float DeltaVPerFuel => _orbitHeightMiles * DragK / Mathf.Max(0.01f, _targetFuel);

    public bool HasLaunched { get; private set; }

    // Small tolerance so hitting the target exactly still counts as orbit.
    public bool ReachedOrbit(float miles) => miles >= _orbitHeightMiles - 0.05f;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        _rosky.localPosition += Vector3.up * _roskyOriginalOffset;
    }

    public void StartLaunch()
    {
        if (HasLaunched) return;
        HasLaunched = true;
        LaunchStarted?.Invoke();
        
        _dropArea.gameObject.SetActive(false);
        StartCoroutine(CloseNoseCoroutine());
    }

    public void EndLaunch(float bestHeightMiles)
    {
        LaunchEnded?.Invoke(bestHeightMiles);
    }

    private IEnumerator CloseNoseCoroutine()
    {
        float t = 0;
        while (t < _noseCloseDuration)
        {
            _noseCone.Rotate(0, 0, (180 / _noseCloseDuration) * Time.deltaTime);
            DestroyTrashTouchingNose();
            t += Time.deltaTime;
            yield return null;
        }
        DestroyTrashTouchingNose();

        t = 0;
        while (t < _roskyMoveDuration)
        {
            _rosky.localPosition -= Vector3.up * (_roskyOriginalOffset * Time.deltaTime);
            t += Time.deltaTime;
            yield return null;
        }
    }

    private void DestroyTrashTouchingNose()
    {
        if (_noseCollider == null)
        {
            return;
        }

        Physics2D.SyncTransforms();
        int count = _noseCollider.Overlap(new ContactFilter2D().NoFilter(), _noseHits);
        for (int i = 0; i < count; i++)
        {
            if (_noseHits[i] != null && _noseHits[i].TryGetComponent(out Trash trash))
            {
                TrashManager.Instance.DestroyTrash(trash);
            }
        }
    }
}
