using TMPro;
using UnityEngine;

public class SC_UI : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _score;

    [SerializeField]
    SC_GameVariables _variables;

    float _displayScore;

    void Awake()
    {
        _score.text = _displayScore.ToString("0");
        _variables.ScoreChanged += HandleScoreChanged;
    }

    void HandleScoreChanged(int score)
    {
        _displayScore = Mathf.Lerp(_displayScore, score, SC_GameVariables.Instance.scoreSpeed * Time.deltaTime);
        _score.text = _displayScore.ToString("0");
    }
}
