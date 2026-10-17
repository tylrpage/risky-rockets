using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Splines;

// Attach to a GameObject to have it move along a spline and control its timing, for the launch scene.
public class LaunchActor : MonoBehaviour
{
    private SplineContainer _splineContainer;
    [SerializeField] private float _startTime;
    [SerializeField] private float _duration;
    private LaunchManager _launchManager;
    private bool _isActing;
    private float _timeSinceActingStart;
    private Spline _spline;

    private void Start()
    {
        _splineContainer = gameObject.GetComponent<SplineContainer>();
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

        Act();
    }

    private void BeginActing()
    {
        Debug.Log("Launch started!");
        _isActing = true;
    }
    
    private void Act()
    {
        _timeSinceActingStart += Time.deltaTime;

        if (_timeSinceActingStart < _startTime) return;

        float t = _timeSinceActingStart - _startTime;
        float p = t / _duration;
        
        Vector3 splinePosition = _spline.EvaluatePosition(p);
        Vector3 newPosition = new Vector3(splinePosition.x, splinePosition.y, transform.position.z);
        transform.position = newPosition;

        if (p >= 1)
        {
            _isActing = false;
        }
    }
}
