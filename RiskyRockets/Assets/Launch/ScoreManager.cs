using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    private int _score;
    [SerializeField] private TMP_Text _scoreText;

    public int Score
    {
        get
        {
            return _score;
        }
        set
        {
            _score = value;
            _scoreText.text = value.ToString();
        }
    }

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this.gameObject);
        }
        Instance = this.gameObject.GetComponent<ScoreManager>();
    }
}
