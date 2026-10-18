using System;
using System.Collections;
using UnityEngine;

public class InstructionsController : MonoBehaviour
{
    private const string SEEN_INSTRUCTIONS_PREF_KEY = "SeenInstructions";

    [SerializeField] private float _offset;
    [SerializeField] private float _duration;
    private Vector3 _originalPosition;
    private Coroutine _activeRoutine;
    private bool _isOpen;
    private bool _isMouseOver;

    public bool IsOpen => _isOpen;
    public bool IsMouseOver => _isMouseOver;
    public bool IsMoving { get; private set; }

    private void Start()
    {
        _originalPosition = transform.localPosition;

        if (!PlayerPrefs.HasKey(SEEN_INSTRUCTIONS_PREF_KEY))
        {
            ShowInstructions(false);
        }
    }

    private void OnMouseEnter()
    {
        _isMouseOver = true;
    }

    private void OnMouseExit()
    {
        _isMouseOver = false;
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (_isOpen)
            {
                TryStartOpenCloseRoutine(false);
                PlayerPrefs.SetInt(SEEN_INSTRUCTIONS_PREF_KEY, 1);
            }
            else
            {
                if (_isMouseOver)
                {
                    TryStartOpenCloseRoutine(true);
                }
            }
        }
    }

    private void ShowInstructions(bool instant)
    {
        if (instant)
        {
            transform.localPosition = OpenPosition;
            _isOpen = true;
        }
        else
        {
            TryStartOpenCloseRoutine(true);
        }
    }

    private void HideInstructions()
    {
        TryStartOpenCloseRoutine(false);
    }

    private void TryStartOpenCloseRoutine(bool open)
    {
        if (_activeRoutine != null)
        {
            return;
        }

        _activeRoutine = StartCoroutine(MoveInstructionsCoroutine(open));
    }

    private Vector3 OpenPosition => _originalPosition + Vector3.right * _offset;

    IEnumerator MoveInstructionsCoroutine(bool open)
    {
        Vector3 start = transform.localPosition;
        Vector3 end = open ? OpenPosition : _originalPosition;

        IsMoving = true;
        if (_duration > 0)
        {
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / _duration;
                transform.localPosition = Vector3.Lerp(start, end, t);
                yield return null;
            }
        }

        IsMoving = false;
        transform.localPosition = end;
        _isOpen = open;
        _activeRoutine = null;
    }
}
