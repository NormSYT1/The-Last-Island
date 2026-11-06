using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("The name of the scene to load when clicking Play / Başla")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("UI Buttons (Optional direct binding)")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playButton != null)
        {
            playButton.onClick.RemoveListener(PlayGame);
            playButton.onClick.AddListener(PlayGame);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitGame);
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    /// <summary>
    /// Starts the game by loading the GameScene.
    /// </summary>
    public void PlayGame()
    {
        EcoAudioManager.PlayClick();
        Debug.Log($"[MainMenuManager] Başla tıklandı. '{gameSceneName}' sahnesi yükleniyor...");
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// Exits the game application.
    /// </summary>
    public void QuitGame()
    {
        EcoAudioManager.PlayClick();
        Debug.Log("[MainMenuManager] Çıkış tıklandı. Oyundan çıkılıyor...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
