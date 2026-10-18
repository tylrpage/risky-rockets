using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Splines;

// Attach to a GameObject to have it move along a spline and control its timing, for the launch scene.
public class LaunchActor : MonoBehaviour
{
    [SerializeField] private SplineContainer _splineContainer;
    [SerializeField] private float _startTime;
    [SerializeField] private float _duration;
    private LaunchManager _launchManager;
    private bool _isActing;
    private float _timeSinceActingStart;
    private Spline _spline;
    private Vector3 _initialPosition;
    public float CompletionPercentage;

    private void Start()
    {
        if (_splineContainer is null)
        {
            _splineContainer = gameObject.GetComponent<SplineContainer>();
        }
        _spline = _splineContainer.Spline;
        _launchManager = LaunchManager.Instance;
        _launchManager.LaunchStarted += BeginActing;
    }

    private void OnDestroy()
    {
        _launchManager.LaunchStarted -= BeginActing;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isActing) return;
        
        if (CompletionPercentage == 0.0f)
        {
            Act(1.0f);
        }
        else
        {
            Act(CompletionPercentage);
        }
    }

    private void BeginActing()
    {
        Debug.Log("Launch started!");
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
