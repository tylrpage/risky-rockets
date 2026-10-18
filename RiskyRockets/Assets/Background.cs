using UnityEngine;

// Stops once the top of the highest Background tile reaches the top of the view, so it never runs out.
// Runs after Rocket so it uses this frame's camera position.
[DefaultExecutionOrder(100)]
public class Background : MonoBehaviour
{
    [SerializeField] private Rocket _rocket;

    private Camera _camera;
    private Vector3 _startPosition;
    private float _highestTileTop;
    private bool _isScrolling;

    private void Start()
    {
        if (_rocket == null) _rocket = FindAnyObjectByType<Rocket>();
        LaunchManager.Instance.LaunchStarted += OnLaunchStarted;
    }

    private void OnDestroy()
    {
        if (LaunchManager.Instance != null) LaunchManager.Instance.LaunchStarted -= OnLaunchStarted;
    }

    private void OnLaunchStarted()
    {
        _camera = Camera.main;
        _startPosition = transform.position;

        // All tiles stop at the same scroll amount, so they stay stacked.
        _highestTileTop = float.MinValue;
        foreach (Background tile in FindObjectsByType<Background>(FindObjectsSortMode.None))
        {
            _highestTileTop = Mathf.Max(_highestTileTop, tile.GetComponent<SpriteRenderer>().bounds.max.y);
        }
        _isScrolling = true;
    }

    private void LateUpdate()
    {
        if (!_isScrolling || _rocket == null) return;

        float scroll = _rocket.AltitudeMiles * LaunchManager.Instance.WorldUnitsPerMile;
        float viewTop = _camera.transform.position.y + _camera.orthographicSize;
        float maxScroll = Mathf.Max(0f, _highestTileTop - viewTop);
        scroll = Mathf.Min(scroll, maxScroll);

        transform.position = _startPosition + Vector3.down * scroll;
    }
}
