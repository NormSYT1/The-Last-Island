using UnityEngine;
using UnityEngine.UI;

public class EcoAudioManager : MonoBehaviour
{
    private static EcoAudioManager _instance;
    public static EcoAudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var existing = FindAnyObjectByType<EcoAudioManager>();
                if (existing != null)
                {
                    _instance = existing;
                }
                else
                {
                    var go = new GameObject("EcoAudioManager");
                    _instance = go.AddComponent<EcoAudioManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [Header("SFX Audio Clips")]
    public AudioClip clickClip;
    public AudioClip extinguishClip;
    public AudioClip cleanTrashClip;
    public AudioClip plantSaplingClip;

    [Header("Volume Controls")]
    [Range(0f, 1f)] public float buttonClickVolume = 0.45f; // %50 azaltıldı (0.90 -> 0.45)
    [Range(0f, 1f)] public float extinguishVolume = 1.0f;
    [Range(0f, 1f)] public float cleanTrashVolume = 1.0f;
    [Range(0f, 1f)] public float plantSaplingVolume = 1.0f;

    private AudioSource sfxSource;
    private AudioSource uiSource; // Dedicated for UI sounds (unaffected by pause/Time.timeScale)

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        SetupSources();
        LoadDefaultClipsIfNeeded();
        AutoHookAllButtonsInScene();
    }

    private void SetupSources()
    {
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f; // 2D crisp sound
        }

        if (uiSource == null)
        {
            uiSource = gameObject.AddComponent<AudioSource>();
            uiSource.playOnAwake = false;
            uiSource.spatialBlend = 0f;
            uiSource.ignoreListenerPause = true; // Plays even during pause menu
        }
    }

    private void LoadDefaultClipsIfNeeded()
    {
#if UNITY_EDITOR
        if (clickClip == null)
            clickClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX_ButtonClick.wav");
        if (extinguishClip == null)
            extinguishClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX_Extinguish.wav");
        if (cleanTrashClip == null)
            cleanTrashClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX_CleanTrash.wav");
        if (plantSaplingClip == null)
            plantSaplingClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX_PlantSapling.wav");
#endif
    }

    /// <summary>
    /// Automatically attaches click sound listeners to all buttons currently active in the scene.
    /// </summary>
    public void AutoHookAllButtonsInScene()
    {
        var buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var btn in buttons)
        {
            btn.onClick.RemoveListener(PlayClick);
            btn.onClick.AddListener(PlayClick);
        }
    }

    public static void PlayClick()
    {
        if (Instance == null) return;
        Instance.PlayUIClip(Instance.clickClip, Instance.buttonClickVolume);
    }

    public static void PlayExtinguish()
    {
        if (Instance == null) return;
        Instance.PlaySFXClip(Instance.extinguishClip, Instance.extinguishVolume);
    }

    public static void PlayCleanTrash()
    {
        if (Instance == null) return;
        Instance.PlaySFXClip(Instance.cleanTrashClip, Instance.cleanTrashVolume);
    }

    public static void PlayPlantSapling()
    {
        if (Instance == null) return;
        Instance.PlaySFXClip(Instance.plantSaplingClip, Instance.plantSaplingVolume);
    }

    public void PlaySFXClip(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        SetupSources();
        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayUIClip(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        SetupSources();
        uiSource.PlayOneShot(clip, volume);
    }
}
