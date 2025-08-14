using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "SampleScene";
    [SerializeField] private GameObject instructionsPanel;

    void Awake()
    {
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
    }

    void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuMusic();
        }
    }

    public void StartGame()
    {
        if (!string.IsNullOrEmpty(gameSceneName))
            SceneManager.LoadScene(gameSceneName);
        else
            Debug.LogWarning("MainMenu: gameSceneName is empty.");
    }

    public void ToggleInstructions()
    {
        if (instructionsPanel == null)
        {
            Debug.LogWarning("MainMenu: instructionsPanel not assigned.");
            return;
        }
        instructionsPanel.SetActive(!instructionsPanel.activeSelf);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}