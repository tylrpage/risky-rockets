using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private BoxCollider2D _scannerTrigger;
    [SerializeField] private GameObject _palletteParent;
    [SerializeField] private int columns;
    [SerializeField] private Vector2 _spacing;
    [SerializeField] private Transform _paletteBottomLeft;
    
    private List<RaycastHit2D> _rigidbodyCastHitsBuffer = new();
    private ContactFilter2D _ghostContactFilter;
    private List<GameObject> _placements = new();
    private List<GameObject> _scanned = new();
    
    [ContextMenu("Get Score")]
    private void GetScore()
    {
        int score = ScanTrash();
        Debug.Log($"Score: {score}");
    }

    [ContextMenu("Build Placements")]
    public void BuildPlacements()
    {
        ClearPlacements();
        
        // Scan the objects
        int totalFuel = ScanTrash();
        _scanned.Shuffle();
        for (int i = 0; i < _scanned.Count; i++)
        {
            RaycastHit2D hit = _rigidbodyCastHitsBuffer[i];
            
            // Create
            int x = i % columns;
            int y = i / columns;
            int rotI = UnityEngine.Random.Range(0, 360);
            Vector3 offset = _paletteBottomLeft.position;
            GameObject hitSource = PrefabUtility.GetCorrespondingObjectFromSource(hit.collider.gameObject);
            GameObject placement = UnityEditor.PrefabUtility.InstantiatePrefab(hitSource) as GameObject;
            if (placement != null)
            {
                _placements.Add(placement);
                
                placement.transform.position = new Vector3(offset.x + x * _spacing.x, offset.y + y * _spacing.y, 0);
                placement.transform.rotation = Quaternion.Euler(Vector3.forward * rotI);
                placement.transform.SetParent(_paletteBottomLeft.transform, true);
                placement.transform.localScale = Vector3.one;
                
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(this.gameObject.scene);
            }
        }

        Debug.Log($"Scanned fuel: {totalFuel}");
    }

    [ContextMenu("Clear Placements")]
    public void ClearPlacements()
    {
        foreach (GameObject placement in _placements)
        {
            DestroyImmediate(placement);
        }
    }

    private int ScanTrash()
    {
        _ghostContactFilter = new ContactFilter2D()
        {
            layerMask = LayerMask.GetMask("Default"),
            useTriggers = true,
        };
        _ghostContactFilter.useLayerMask = true;
        int hits = _scannerTrigger.Cast(Vector2.zero, _ghostContactFilter, _rigidbodyCastHitsBuffer);
        _scanned.Clear();
        int totalFuel = 0;
        for (int i = 0; i < hits; i++)
        {
            RaycastHit2D hit = _rigidbodyCastHitsBuffer[0];

            // Only scan trash
            if (!hit.collider.gameObject.activeInHierarchy || !hit.collider.TryGetComponent(out Trash trash))
            {
                continue;
            }

            totalFuel += trash.FuelValue;
            
            _scanned.Add(hit.collider.gameObject);
        }

        return totalFuel;
    }

    private void Start()
    {
        int score = ScanTrash();
        Debug.Log($"Target Score: {score}");
        
        _palletteParent.SetActive(false);

        _scannerTrigger.GetComponent<SpriteRenderer>().enabled = false;
    }
}
