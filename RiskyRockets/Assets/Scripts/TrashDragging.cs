using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class TrashDragging : MonoBehaviour
{
    [SerializeField] private float MovedTrashTransparency;
    [SerializeField] private BoxCollider2D DropArea;
    [SerializeField] private float RotationSpeed;
    
    public bool IsHoveringDraggableTrash { get; private set; }
    private Trash _grabbedTrash;
    private Trash _grabbedTrashCopy;
    private Transform _grabbedTrashHitGhost;
    private bool _isOverDropArea;
    private List<RaycastHit2D> _rigidbodyCastHitsBuffer = new();
    private ContactFilter2D _ghostContactFilter;

    private void Start()
    {
        _ghostContactFilter = new ContactFilter2D()
        {
            layerMask = LayerMask.GetMask("Default"),
            useLayerMask = true,
        };
    }

    private Trash GetHoveredTrash()
    {
        // Try grab trash
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
            
        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

        if (hit.collider != null && hit.collider.TryGetComponent(out Trash trash))
        {
            return trash;
        }

        return null;
    }

    private void RestoreTrash()
    {
        // Restore trash original color
        SpriteRenderer trashSr = _grabbedTrashCopy.GetComponent<SpriteRenderer>();
        Color trashColor = new Color(trashSr.color.r, trashSr.color.g, trashSr.color.b, 1);
        _grabbedTrash.GetComponent<SpriteRenderer>().color = trashColor;
        
        // Delete the copy
        Destroy(_grabbedTrashCopy.gameObject);
        _grabbedTrashCopy = null;
        
        // Delete drop ghost
        Destroy(_grabbedTrashHitGhost.gameObject);
        _grabbedTrashHitGhost = null;
        
        _grabbedTrash = null;
    }
    
    private void Update()
    {
        Trash hoveredTrash = GetHoveredTrash();
        IsHoveringDraggableTrash = hoveredTrash != null && !hoveredTrash.Dropped;

        if (_grabbedTrash != null)
        {
            // Rotate
            float rotationSign = -Input.GetAxis("Horizontal");
            _grabbedTrashCopy.transform.Rotate(Vector3.forward * RotationSpeed * rotationSign * Time.deltaTime);
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            if (_grabbedTrash == null)
            {
                if (hoveredTrash != null)
                {
                    if (hoveredTrash.Dropped)
                    {
                        Debug.LogError($"Can't pickup trash that's already dropped");
                    }
                    else
                    {
                        // Make copy of trash
                        _grabbedTrash = hoveredTrash;
                        _grabbedTrashCopy = Instantiate(hoveredTrash.gameObject).GetComponent<Trash>();
                        _grabbedTrashCopy.GetComponent<Rigidbody2D>().simulated = false;
                        
                        // Color the trash we are moving
                        SpriteRenderer trashSr = _grabbedTrashCopy.GetComponent<SpriteRenderer>();
                        Color trashColor = new Color(trashSr.color.r, trashSr.color.g, trashSr.color.b, MovedTrashTransparency);
                        _grabbedTrash.GetComponent<SpriteRenderer>().color = trashColor;
                        
                        // Create ghost
                        _grabbedTrashHitGhost = Instantiate(hoveredTrash.gameObject).transform;
                        Color trashGhostColor = new Color(trashSr.color.r, trashSr.color.g, trashSr.color.b, MovedTrashTransparency);
                        _grabbedTrash.GetComponent<SpriteRenderer>().color = trashColor;
                        _grabbedTrashHitGhost.GetComponent<SpriteRenderer>().color = trashGhostColor;
                        _grabbedTrashHitGhost.GetComponent<Rigidbody2D>().simulated = false;
                    }    
                }
            }
            else
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                // Check if we are in drop area
                if (DropArea.OverlapPoint(mouseWorldPos))
                {
                    // Drop trash
                    // Move trash original
                    _grabbedTrash.transform.position = _grabbedTrashCopy.transform.position;
                    _grabbedTrash.transform.rotation = _grabbedTrashCopy.transform.rotation;
                
                    // Restore trash original color
                    SpriteRenderer trashSr = _grabbedTrashCopy.GetComponent<SpriteRenderer>();
                    Color trashColor = new Color(trashSr.color.r, trashSr.color.g, trashSr.color.b, 1);
                    _grabbedTrash.GetComponent<SpriteRenderer>().color = trashColor;
                
                    // Delete the copy
                    Destroy(_grabbedTrashCopy.gameObject);
                    _grabbedTrashCopy = null;
        
                    // Delete drop ghost
                    Destroy(_grabbedTrashHitGhost.gameObject);
                    _grabbedTrashHitGhost = null;
                
                    _grabbedTrash.Dropped = true;
                    _grabbedTrash = null;
                    _grabbedTrashCopy = null;
                }
                else
                {
                    RestoreTrash();
                }
            }
        }
    }

    private void LateUpdate()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _isOverDropArea = DropArea.OverlapPoint(mouseWorldPos);

        if (_grabbedTrashCopy != null)
        {
            // Move around our trash copy
            float originalZ = _grabbedTrash.transform.position.z;
            _grabbedTrashCopy.transform.position = new Vector3(mouseWorldPos.x, mouseWorldPos.y, originalZ);

            bool shouldGhostBeEnabled = false;
            if (_isOverDropArea)
            {
                Transform ghostTransform = _grabbedTrashCopy.transform;
                int hits = _grabbedTrashCopy.GetComponent<Rigidbody2D>().Cast(_grabbedTrashCopy.transform.position, ghostTransform.eulerAngles.z, Vector2.down, _ghostContactFilter, _rigidbodyCastHitsBuffer);
                if (hits > 0)
                {
                    shouldGhostBeEnabled = true;
                    _grabbedTrashHitGhost.position = _grabbedTrashCopy.transform.position +
                                                     Vector3.down * _rigidbodyCastHitsBuffer[0].distance;
                    // Debug.Log($"ghost drop distance {_rigidbodyCastHitsBuffer[0].distance}, name: {_rigidbodyCastHitsBuffer[0].collider.name}");
                    _grabbedTrashHitGhost.rotation = _grabbedTrashCopy.transform.rotation;
                }
            }

            if (shouldGhostBeEnabled != _grabbedTrashHitGhost.gameObject.activeSelf)
            {
                _grabbedTrashHitGhost.gameObject.SetActive(shouldGhostBeEnabled);
            }
        }
    }

    public bool IsDraggingTrash()
    {
        return _grabbedTrash != null;
    }

    public bool IsOverDropArea()
    {
        return _isOverDropArea;
    }
}
