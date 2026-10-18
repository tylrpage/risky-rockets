using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    [SerializeField] private TMP_Text _scoreText;

    public float BestHeightMiles { get; private set; }

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
        LaunchManager.Instance.LaunchEnded += ShowFinal;
        
        _scoreText.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (LaunchManager.Instance != null) LaunchManager.Instance.LaunchEnded -= ShowFinal;
    }

    public void SetHeight(float miles)
    {
        if (!_scoreText.gameObject.activeSelf)
        {
            _scoreText.gameObject.SetActive(true);
        }
        
        BestHeightMiles = miles;
        _scoreText.text = $"{miles:0.0} mi";
    }

    private void ShowFinal(float bestMiles)
    {
        BestHeightMiles = bestMiles;
        // Small tolerance so hitting the target exactly still counts as orbit.
        bool reachedOrbit = bestMiles >= LaunchManager.Instance.OrbitHeightMiles - 0.05f;
        _scoreText.text = reachedOrbit ? $"{bestMiles:0.0} mi\nROSKY HAS REACHED ORBIT!!!" : $"{bestMiles:0.0} mi\nRIP ROSKY";
    }
}
