using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Splines;

// Attach to a GameObject to have it move along a spline and control its timing, for the launch scene.
public class LaunchActor : MonoBehaviour
{
    private LaunchManager _launchManager;
    [SerializeField] private SplineContainer _splineContainer;
    // Seconds after LaunchStart this actor begins acting
    [SerializeField] private float _startTime;
    // Seconds it taket this actor to reach CompletionPercentage progress along spline
    [SerializeField] private float _duration;
    // Percentage of the way through the spline this actor reaches (0.0f < p <= 1.0f)
    public float CompletionPercentage;
    private bool _isActing;
    private float _timeSinceActingStart;
    private Spline _spline;
    private Vector3 _initialPosition;

    private void Start()
    {
        if (_splineContainer is null)
        {
            _splineContainer = gameObject.GetComponent<SplineContainer>();
        }
        _spline = _splineContainer.Spline;
        _launchManager = LaunchManager.Instance;
        _launchManager.LaunchStarted += BeginTimer;
    }

    private void OnDestroy()
    {
        if (_launchManager != null)
        {
            _launchManager.LaunchStarted -= BeginTimer;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isActing || _spline is null) return;
        
        // If unset in the inspector, actor is assumed to be intended to go the full length of the spline.
        if (CompletionPercentage == 0.0f)
        {
            Act(1.0f);
        }
        else
        {
            Act(CompletionPercentage);
        }
    }

    private void BeginTimer()
    {
        _isActing = true;
        _initialPosition = transform.position;
    }
    
    private void Act(float completionPercentage)
    {
        _timeSinceActingStart += Time.deltaTime;

        if (_timeSinceActingStart < _startTime) return;

        float t = _timeSinceActingStart - _startTime;
        float p = t / _duration;
        p = AnimationCurves.CalculateParametric(p) * completionPercentage;
        
        Vector3 splinePosition = _spline.EvaluatePosition(p);
        Vector3 newPosition = _initialPosition + new Vector3(splinePosition.x, splinePosition.y, 0);
        transform.position = newPosition;

        if (p >= completionPercentage)
        {
            _isActing = false;
        }
    }
}
