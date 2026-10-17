using System;
using UnityEngine;
using UnityEngine.Serialization;

public class TrashDragging : MonoBehaviour
{
    [SerializeField] private float MovedTrashTransparency;
    [SerializeField] private BoxCollider2D DropArea;
    
    private Trash _grabbedTrash;
    private Trash _grabbedTrashCopy;
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (_grabbedTrash == null)
            {
                // Try grab trash
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
            
                RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

                if (hit.collider != null && hit.collider.TryGetComponent(out Trash trash))
                {
                    if (trash.Dropped)
                    {
                        Debug.LogError($"Can't pickup trash that's already dropped");
                    }
                    else
                    {
                        // Make copy of trash
                        _grabbedTrash = trash;
                        _grabbedTrashCopy = Instantiate(trash.gameObject).GetComponent<Trash>();
                        _grabbedTrashCopy.GetComponent<Rigidbody2D>().simulated = false;
                        
                        // Color the trash we are moving
                        SpriteRenderer trashSr = _grabbedTrashCopy.GetComponent<SpriteRenderer>();
                        Color trashColor = new Color(trashSr.color.r, trashSr.color.g, trashSr.color.b, MovedTrashTransparency);
                        _grabbedTrash.GetComponent<SpriteRenderer>().color = trashColor;
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
                    float originalZ = _grabbedTrash.transform.position.z;
                    _grabbedTrash.transform.position = new Vector3(mouseWorldPos.x, mouseWorldPos.y, originalZ);
                
                    // Restore trash original color
                    SpriteRenderer trashSr = _grabbedTrashCopy.GetComponent<SpriteRenderer>();
                    Color trashColor = new Color(trashSr.color.r, trashSr.color.g, trashSr.color.b, 1);
                    _grabbedTrash.GetComponent<SpriteRenderer>().color = trashColor;
                
                    // Delete the copy
                    Destroy(_grabbedTrashCopy.gameObject);
                
                    _grabbedTrash.Dropped = true;
                    _grabbedTrash = null;
                    _grabbedTrashCopy = null;
                }
                else
                {
                    Debug.LogError($"Can only drop in drop area");
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (_grabbedTrashCopy != null)
        {
            // Move around our trash copy
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float originalZ = _grabbedTrash.transform.position.z;
            _grabbedTrashCopy.transform.position = new Vector3(mouseWorldPos.x, mouseWorldPos.y, originalZ);
        }
    }
}
