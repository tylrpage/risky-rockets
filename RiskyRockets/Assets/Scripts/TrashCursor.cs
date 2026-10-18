using System;
using UnityEngine;

public class TrashCursor : MonoBehaviour
{
    public enum CursorType
    {
        Pointer,
        Grabber,
        GrabberGood,
        GrabberBad,
    }
    
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Sprite _pointer;
    [SerializeField] private Sprite _grabber;
    [SerializeField] private Sprite _grabberGood;
    [SerializeField] private Sprite _grabberBad;
    [SerializeField] private TrashDragging _trashDragging;
    [SerializeField] private InstructionsController _instructionsController;
    private CursorType? _currentCursorType;

    private void Start()
    {
        Cursor.visible = false;
        SetCursor(CursorType.Pointer);
    }

    private void SetCursor(CursorType cursorType)
    {
        if (_currentCursorType != null && _currentCursorType == cursorType)
        {
            return;
        }
        
        switch (cursorType)
        {
            case CursorType.Pointer:
                _spriteRenderer.sprite = _pointer;
                break;
            case CursorType.Grabber:
                _spriteRenderer.sprite = _grabber;
                break;
            case CursorType.GrabberGood:
                _spriteRenderer.sprite = _grabberGood;
                break;
            case CursorType.GrabberBad:
                _spriteRenderer.sprite = _grabberBad;
                break;
        }

        _currentCursorType = cursorType;
    }

    private void LateUpdate()
    {
        // Position
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.transform.position = new Vector3(mouseWorldPos.x, mouseWorldPos.y, -1);

        if (_instructionsController.IsOpen || _instructionsController.IsMouseOver)
        {
            SetCursor(_instructionsController.IsMoving ? CursorType.Pointer : CursorType.Grabber);
        }
        else if (_trashDragging.IsDraggingTrash())
        {
            SetCursor(_trashDragging.IsOverDropArea() ? CursorType.GrabberGood : CursorType.GrabberBad);
        }
        else if (_trashDragging.IsHoveringDraggableTrash)
        {
            SetCursor(CursorType.Grabber);
        }
        else
        {
            SetCursor(CursorType.Pointer);
        }
    }
}
