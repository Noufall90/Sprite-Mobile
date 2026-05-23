using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class HighScoreManager : MonoBehaviour
{
    private static HighScoreManager _instance;

    public static HighScoreManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<HighScoreManager>();

                if (_instance == null)
                {
                    GameObject go = new GameObject("HighScoreManager");
                    _instance = go.AddComponent<HighScoreManager>();
                }
            }

            return _instance;
        }
    }

    private int highestScore = 0;

    private TMP_Text mainMenuText;

    public int HighestScore => highestScore;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        highestScore = PlayerPrefs.GetInt("HighestScore", 0);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        highestScore = PlayerPrefs.GetInt("HighestScore", highestScore);
        UpdateMainMenuText();
    }

    public void SetMainMenuText(TMP_Text text)
    {
        mainMenuText = text;
        UpdateMainMenuText();
    }

    private void UpdateMainMenuText()
    {
        if (mainMenuText != null)
        {
            mainMenuText.text = $"Highest Score : {highestScore}";
        }
    }

    public void TrySetNewHighScore(int score)
    {
        if (score > highestScore)
        {
            highestScore = score;
            PlayerPrefs.SetInt("HighestScore", highestScore);
            PlayerPrefs.Save();
            UpdateMainMenuText();
        }
    }

    public void ResetHighScore()
    {
        highestScore = 0;
        PlayerPrefs.SetInt("HighestScore", 0);
        PlayerPrefs.Save();
        UpdateMainMenuText();
    }
}