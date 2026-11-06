using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class EcoMissionCanvasUI : MonoBehaviour
{
    public static EcoMissionCanvasUI Instance { get; private set; }

    [Header("Managers")]
    public EcoMissionGameManager gameManager;
    public FPSController playerController;

    [Header("HUD Elements")]
    public GameObject hudRoot;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI carbonText;
    public Image carbonFillBar;

    [Header("Top Thin Pollution Bar")]
    public Image topPollutionBar;
    public Image topPollutionBarBg;
    public TextMeshProUGUI topPollutionText;

    [Header("Crosshair & Interaction")]
    public Image crosshairImage;
    public GameObject interactPromptObj;
    public TextMeshProUGUI interactPromptText;

    [Header("HUD Minimap Elements")]
    public GameObject minimapPanel;
    public RectTransform minimapInnerArea;

    [Header("Tactical Map Elements")]
    public GameObject mapPanel;
    public RectTransform mapInnerArea;
    public TextMeshProUGUI mapSubtitleText;

    [Header("Map Custom Icons")]
    public Sprite iconFire;
    public Sprite iconTrash;
    public Sprite iconSapling;
    public Sprite iconTree;
    public Sprite iconCleaned;

    [Header("Game Over Elements")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverTitleText;
    public TextMeshProUGUI gameOverDescText;
    public Button restartButton;

    [Header("Pause Menu Elements")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button mainMenuButton;
    public bool isPaused { get; private set; } = false;

    [Header("Planting Spots")]
    public List<PlantingSpot> allPlantingSpots = new List<PlantingSpot>();

    // Ada Gerçek Sınırları (BARRIERS Duvarları)
    private float mapMinX = -30f;
    private float mapMaxX = 251f;
    private float mapMinZ = 68f;
    private float mapMaxZ = 210f;

    private bool isMapOpen = false;

    // Taktik Harita UI Öğeleri
    private List<RectTransform> zoneDotUIList = new List<RectTransform>();
    private List<Image> zoneImgUIList = new List<Image>();
    private List<bool> zoneIsFireList = new List<bool>();

    private List<RectTransform> spotDotUIList = new List<RectTransform>();
    private List<Image> spotImgUIList = new List<Image>();
    private RectTransform playerDotUI;

    // HUD Minimap UI Öğeleri
    private List<RectTransform> miniZoneDotUIList = new List<RectTransform>();
    private List<Image> miniZoneImgUIList = new List<Image>();
    private List<bool> miniZoneIsFireList = new List<bool>();

    private List<RectTransform> miniSpotDotUIList = new List<RectTransform>();
    private List<Image> miniSpotImgUIList = new List<Image>();
    private RectTransform miniPlayerDotUI;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        Time.timeScale = 1f;

        if (gameManager == null) gameManager = EcoMissionGameManager.Instance;
        if (playerController == null) playerController = FindAnyObjectByType<FPSController>();

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        if (mapPanel != null) mapPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        FindAllPlantingSpots();
        InitializeMapDots();
        InitializeMinimapDots();
    }

    private void Update()
    {
        if (gameManager == null) gameManager = EcoMissionGameManager.Instance;
        if (playerController == null) playerController = FindAnyObjectByType<FPSController>();

        HandlePauseToggle();
        HandleMapToggle();
        UpdateHUD();
        UpdateCrosshairAndInteraction();
        UpdateTacticalMap();
        UpdateMinimap();
        UpdateGameOverUI();
    }

    public void FindAllPlantingSpots()
    {
        allPlantingSpots.Clear();
        var spots = FindObjectsByType<PlantingSpot>(FindObjectsSortMode.None);
        allPlantingSpots.AddRange(spots);
    }

    private void HandlePauseToggle()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isMapOpen)
            {
                isMapOpen = false;
                if (mapPanel != null) mapPanel.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (gameManager != null && !gameManager.isGameOver)
            {
                if (isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        EcoAudioManager.PlayClick();
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void GoToMainMenu()
    {
        EcoAudioManager.PlayClick();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    private void HandleMapToggle()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            isMapOpen = !isMapOpen;
            if (mapPanel != null) mapPanel.SetActive(isMapOpen);

            if (isMapOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    private void UpdateHUD()
    {
        if (gameManager == null) return;

        // 1. Geri Sayım Süresi
        int minutes = Mathf.FloorToInt(gameManager.timeRemaining / 60F);
        int seconds = Mathf.FloorToInt(gameManager.timeRemaining - minutes * 60);
        string timeStr = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (timerText != null)
        {
            Color tColor = gameManager.timeRemaining < 120f ? Color.red : (gameManager.timeRemaining < 300f ? new Color(1f, 0.6f, 0.1f) : new Color(0.2f, 0.9f, 0.3f));
            timerText.text = $"⏳ Time: <color=#{ColorUtility.ToHtmlStringRGB(tColor)}><b>{timeStr}</b></color>";
        }

        // 2. Temizlenen Bölge ve Dikilen Fidan Sayısı
        if (progressText != null)
        {
            int plantedCount = 0;
            foreach (var s in allPlantingSpots)
            {
                if (s != null && s.isPlanted) plantedCount++;
            }
            progressText.text = $"🧹 Cleaned: <b>{gameManager.cleanedZonesCount}/{gameManager.totalZonesCount}</b>  |  🌱 Saplings: <b>{plantedCount}/{allPlantingSpots.Count}</b>";
        }

        // 3. Karbon Seviyesi
        if (carbonText != null)
        {
            carbonText.text = $"👣 Carbon: <b>{(int)gameManager.carbonPPM} PPM</b>";
        }

        if (carbonFillBar != null)
        {
            float fill = Mathf.InverseLerp(300f, 650f, gameManager.carbonPPM);
            carbonFillBar.fillAmount = fill;
            carbonFillBar.color = Color.Lerp(new Color(0.2f, 0.8f, 0.3f), new Color(0.9f, 0.2f, 0.2f), fill);
        }

        // 4. En Üstteki İnce Kirlilik Barı
        if (topPollutionBar != null && gameManager.totalZonesCount > 0)
        {
            float remainingRatio = (float)(gameManager.totalZonesCount - gameManager.cleanedZonesCount) / gameManager.totalZonesCount;
            topPollutionBar.fillAmount = remainingRatio;

            float cleanProgress = 1f - remainingRatio;
            Color dynamicColor = Color.Lerp(new Color(0.95f, 0.15f, 0.15f, 0.95f), new Color(0.15f, 0.95f, 0.35f, 0.95f), cleanProgress);
            topPollutionBar.color = dynamicColor;

            if (topPollutionText != null)
            {
                int percent = Mathf.RoundToInt(remainingRatio * 100f);
                topPollutionText.text = $"POLLUTION LEVEL: {percent}%";
            }
        }
    }

    private void UpdateCrosshairAndInteraction()
    {
        if (playerController == null) return;

        bool hasTarget = playerController.activeZoneInReach != null && !playerController.activeZoneInReach.isCleaned;

        // Nişangah Rengi
        if (crosshairImage != null)
        {
            if (playerController.activePlantingSpot != null && !playerController.activePlantingSpot.isPlanted)
            {
                crosshairImage.color = new Color(0.0f, 1f, 0.5f, 1f); // Parlak Zümrüt Yeşili
                crosshairImage.rectTransform.sizeDelta = new Vector2(16, 16);
            }
            else if (hasTarget)
            {
                crosshairImage.color = new Color(0.1f, 1f, 0.3f, 1f);
                crosshairImage.rectTransform.sizeDelta = new Vector2(16, 16);
            }
            else
            {
                crosshairImage.color = new Color(1f, 1f, 1f, 0.65f);
                crosshairImage.rectTransform.sizeDelta = new Vector2(8, 8);
            }
        }

        // Etkileşim Bildirim Paneli
        if (interactPromptObj != null)
        {
            if (playerController.activePlantingSpot != null && !playerController.activePlantingSpot.isPlanted)
            {
                interactPromptObj.SetActive(true);
                if (interactPromptText != null)
                {
                    interactPromptText.text = $"<b>{playerController.activePlantingSpot.spotName}</b>\n<color=#00E676><b>[Press F] 🌱 PLANT SAPLING</b></color>";
                }
            }
            else if (hasTarget)
            {
                interactPromptObj.SetActive(true);
                if (interactPromptText != null)
                {
                    bool isFire = playerController.activeZoneInReach.fireEffect != null || playerController.activeZoneInReach.waterSplashEffect != null;
                    string actionIcon = isFire ? "💧 EXTINGUISH" : "🧹 CLEAN UP";
                    interactPromptText.text = $"<b>{playerController.activeZoneInReach.zoneName}</b>\n<color=#00E676><b>[Press E] {actionIcon}</b></color>";
                }
            }
            else
            {
                interactPromptObj.SetActive(false);
            }
        }
    }

    private bool CheckIsFireZone(EcoMissionZone zone)
    {
        if (zone == null) return false;
        return zone.fireEffect != null || 
               zone.gameObject.name.Contains("Yangın") || 
               zone.zoneName.Contains("Yangın") ||
               (zone.transform.parent != null && zone.transform.parent.name == "Fires");
    }

    private void InitializeMapDots()
    {
        if (mapInnerArea == null || gameManager == null) return;

        for (int i = mapInnerArea.childCount - 1; i >= 0; i--)
        {
            Destroy(mapInnerArea.GetChild(i).gameObject);
        }
        zoneDotUIList.Clear();
        zoneImgUIList.Clear();
        zoneIsFireList.Clear();
        spotDotUIList.Clear();
        spotImgUIList.Clear();

        // 1. Kirlilik & Yangın Odakları (🔥 Ateş / 🗑️ Çöp)
        foreach (var zone in gameManager.allZones)
        {
            if (zone == null) continue;
            bool isFire = CheckIsFireZone(zone);

            GameObject dotObj = new GameObject(isFire ? $"Fire_{zone.zoneName}" : $"Trash_{zone.zoneName}", typeof(RectTransform), typeof(Image));
            dotObj.transform.SetParent(mapInnerArea, false);

            var rt = dotObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(24, 24);

            var img = dotObj.GetComponent<Image>();
            if (isFire && iconFire != null)
            {
                img.sprite = iconFire;
                img.color = Color.white;
            }
            else if (!isFire && iconTrash != null)
            {
                img.sprite = iconTrash;
                img.color = Color.white;
            }
            else
            {
                img.color = isFire ? Color.red : new Color(1f, 0.6f, 0.1f);
            }

            zoneDotUIList.Add(rt);
            zoneImgUIList.Add(img);
            zoneIsFireList.Add(isFire);
        }

        // 2. Fidan Dikim Noktaları (🌱 Fidan)
        for (int i = 0; i < allPlantingSpots.Count; i++)
        {
            var spot = allPlantingSpots[i];
            if (spot == null) continue;

            GameObject dotObj = new GameObject($"Spot_{spot.spotName}", typeof(RectTransform), typeof(Image));
            dotObj.transform.SetParent(mapInnerArea, false);

            var rt = dotObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(26, 26);

            var img = dotObj.GetComponent<Image>();
            if (spot.isPlanted && iconTree != null)
            {
                img.sprite = iconTree;
            }
            else if (iconSapling != null)
            {
                img.sprite = iconSapling;
            }
            else
            {
                img.color = new Color(0.0f, 0.95f, 0.45f, 1f);
            }
            img.color = Color.white;

            spotDotUIList.Add(rt);
            spotImgUIList.Add(img);
        }

        // 3. Oyuncu Noktası (Mavi Daire/Ok)
        GameObject pDotObj = new GameObject("PlayerDot", typeof(RectTransform), typeof(Image));
        pDotObj.transform.SetParent(mapInnerArea, false);
        playerDotUI = pDotObj.GetComponent<RectTransform>();
        playerDotUI.sizeDelta = new Vector2(20, 20);
        var pImg = pDotObj.GetComponent<Image>();
        pImg.color = new Color(0.1f, 0.7f, 1f, 1f);

        // Subtitle legend
        if (mapSubtitleText != null)
        {
            mapSubtitleText.text = "🔥 <b>Fire:</b> Burning Barrel   |   🗑️ <b>Trash:</b> Waste Pile   |   🌱 <b>Sapling:</b> Planting Spot   |   🔵 <b>Blue:</b> You";
        }
    }

    private void InitializeMinimapDots()
    {
        if (minimapInnerArea == null || gameManager == null) return;

        for (int i = minimapInnerArea.childCount - 1; i >= 0; i--)
        {
            Destroy(minimapInnerArea.GetChild(i).gameObject);
        }
        miniZoneDotUIList.Clear();
        miniZoneImgUIList.Clear();
        miniZoneIsFireList.Clear();
        miniSpotDotUIList.Clear();
        miniSpotImgUIList.Clear();

        // 1. Kirlilik & Yangın Noktaları (Minimap)
        foreach (var zone in gameManager.allZones)
        {
            if (zone == null) continue;
            bool isFire = CheckIsFireZone(zone);

            GameObject mDot = new GameObject(isFire ? $"MiniFire_{zone.zoneName}" : $"MiniTrash_{zone.zoneName}", typeof(RectTransform), typeof(Image));
            mDot.transform.SetParent(minimapInnerArea, false);
            var rt = mDot.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(13, 13);
            var img = mDot.GetComponent<Image>();
            
            if (isFire && iconFire != null) img.sprite = iconFire;
            else if (!isFire && iconTrash != null) img.sprite = iconTrash;
            img.color = Color.white;

            miniZoneDotUIList.Add(rt);
            miniZoneImgUIList.Add(img);
            miniZoneIsFireList.Add(isFire);
        }

        // 2. Fidan Noktaları (Minimap)
        foreach (var spot in allPlantingSpots)
        {
            if (spot == null) continue;
            GameObject mSpot = new GameObject($"MiniSpot_{spot.spotName}", typeof(RectTransform), typeof(Image));
            mSpot.transform.SetParent(minimapInnerArea, false);
            var rt = mSpot.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(15, 15);
            var img = mSpot.GetComponent<Image>();

            if (spot.isPlanted && iconTree != null) img.sprite = iconTree;
            else if (iconSapling != null) img.sprite = iconSapling;
            img.color = Color.white;

            miniSpotDotUIList.Add(rt);
            miniSpotImgUIList.Add(img);
        }

        // 3. Oyuncu (Minimap)
        GameObject pDot = new GameObject("MiniPlayerDot", typeof(RectTransform), typeof(Image));
        pDot.transform.SetParent(minimapInnerArea, false);
        miniPlayerDotUI = pDot.GetComponent<RectTransform>();
        miniPlayerDotUI.sizeDelta = new Vector2(11, 11);
        var pImg = pDot.GetComponent<Image>();
        pImg.color = new Color(0.1f, 0.7f, 1f, 1f);
    }

    private void UpdateTacticalMap()
    {
        if (!isMapOpen || mapInnerArea == null || gameManager == null) return;

        // 1. Yangın ve Çöp Simgeleri
        for (int i = 0; i < gameManager.allZones.Count; i++)
        {
            var zone = gameManager.allZones[i];
            if (zone == null || i >= zoneDotUIList.Count) continue;

            var dotRT = zoneDotUIList[i];
            var img = zoneImgUIList[i];
            bool isFire = zoneIsFireList[i];

            Vector2 mapPos = WorldToMapCoords(zone.transform.position, mapInnerArea.rect);
            dotRT.anchoredPosition = mapPos;

            if (zone.isCleaned)
            {
                if (iconCleaned != null)
                {
                    img.sprite = iconCleaned;
                    img.color = Color.white;
                }
                else
                {
                    img.color = new Color(0.2f, 0.9f, 0.35f, 0.85f);
                }
                dotRT.localScale = Vector3.one;
            }
            else
            {
                if (isFire)
                {
                    if (iconFire != null) img.sprite = iconFire;
                    img.color = Color.white;
                    // Ateş için alev titreşimi nabız animasyonu
                    float fPulse = 1f + Mathf.PingPong(Time.time * 3f, 0.22f);
                    dotRT.localScale = new Vector3(fPulse, fPulse, 1f);
                }
                else
                {
                    if (iconTrash != null) img.sprite = iconTrash;
                    img.color = Color.white;
                    dotRT.localScale = Vector3.one;
                }
            }
        }

        // 2. Fidan Dikim Simgeleri (🌱 Fidan / 🌲 Ağaç)
        for (int i = 0; i < allPlantingSpots.Count; i++)
        {
            var spot = allPlantingSpots[i];
            if (spot == null || i >= spotDotUIList.Count) continue;

            var dotRT = spotDotUIList[i];
            var img = spotImgUIList[i];

            Vector2 mapPos = WorldToMapCoords(spot.transform.position, mapInnerArea.rect);
            dotRT.anchoredPosition = mapPos;

            if (spot.isPlanted)
            {
                if (iconTree != null) img.sprite = iconTree;
                img.color = Color.white;
                dotRT.localScale = Vector3.one;
            }
            else
            {
                if (iconSapling != null) img.sprite = iconSapling;
                img.color = Color.white;
                // Fidan için canlı dikkat çekici nabız efekti
                float pulse = 1f + Mathf.PingPong(Time.time * 2.2f, 0.25f);
                dotRT.localScale = new Vector3(pulse, pulse, 1f);
            }
        }

        // 3. Oyuncu Konumu
        if (playerDotUI != null && playerController != null)
        {
            Vector2 playerMapPos = WorldToMapCoords(playerController.transform.position, mapInnerArea.rect);
            playerDotUI.anchoredPosition = playerMapPos;
        }
    }

    private void UpdateMinimap()
    {
        if (minimapInnerArea == null || gameManager == null) return;

        // Minimap Yangın ve Çöp Noktaları
        for (int i = 0; i < gameManager.allZones.Count; i++)
        {
            var zone = gameManager.allZones[i];
            if (zone == null || i >= miniZoneDotUIList.Count) continue;

            var dotRT = miniZoneDotUIList[i];
            var img = miniZoneImgUIList[i];
            bool isFire = miniZoneIsFireList[i];

            dotRT.anchoredPosition = WorldToMapCoords(zone.transform.position, minimapInnerArea.rect);

            if (zone.isCleaned)
            {
                if (iconCleaned != null) img.sprite = iconCleaned;
                img.color = new Color(0.2f, 0.9f, 0.3f, 0.6f);
                dotRT.localScale = Vector3.one;
            }
            else
            {
                if (isFire)
                {
                    if (iconFire != null) img.sprite = iconFire;
                    img.color = Color.white;
                    float fPulse = 1f + Mathf.PingPong(Time.time * 3f, 0.2f);
                    dotRT.localScale = new Vector3(fPulse, fPulse, 1f);
                }
                else
                {
                    if (iconTrash != null) img.sprite = iconTrash;
                    img.color = Color.white;
                    dotRT.localScale = Vector3.one;
                }
            }
        }

        // Minimap Fidan Noktaları
        for (int i = 0; i < allPlantingSpots.Count; i++)
        {
            var spot = allPlantingSpots[i];
            if (spot == null || i >= miniSpotDotUIList.Count) continue;

            var dotRT = miniSpotDotUIList[i];
            var img = miniSpotImgUIList[i];

            dotRT.anchoredPosition = WorldToMapCoords(spot.transform.position, minimapInnerArea.rect);

            if (spot.isPlanted)
            {
                if (iconTree != null) img.sprite = iconTree;
                img.color = Color.white;
                dotRT.localScale = Vector3.one;
            }
            else
            {
                if (iconSapling != null) img.sprite = iconSapling;
                img.color = Color.white;
                float pulse = 1f + Mathf.PingPong(Time.time * 2.2f, 0.25f);
                dotRT.localScale = new Vector3(pulse, pulse, 1f);
            }
        }

        // Minimap Oyuncu Noktası
        if (miniPlayerDotUI != null && playerController != null)
        {
            miniPlayerDotUI.anchoredPosition = WorldToMapCoords(playerController.transform.position, minimapInnerArea.rect);
        }
    }

    private Vector2 WorldToMapCoords(Vector3 worldPos, Rect mapRect)
    {
        float normX = Mathf.InverseLerp(mapMinX, mapMaxX, worldPos.x);
        float normZ = Mathf.InverseLerp(mapMinZ, mapMaxZ, worldPos.z);

        // Rect anchor merkezli (-halfWidth ile +halfWidth)
        float posX = (normX - 0.5f) * mapRect.width;
        float posY = (normZ - 0.5f) * mapRect.height;

        return new Vector2(posX, posY);
    }

    private void UpdateGameOverUI()
    {
        if (gameManager == null || gameOverPanel == null) return;

        if (gameManager.isGameOver && !gameOverPanel.activeSelf)
        {
            gameOverPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (gameManager.isVictory)
            {
                if (gameOverTitleText != null)
                {
                    gameOverTitleText.text = "<color=#2E7D32>🏆 VICTORY! THE ISLAND IS RESTORED!</color>";
                }
                if (gameOverDescText != null)
                {
                    gameOverDescText.text = "You cleared all fire and waste hazards within 10 minutes!\nCarbon emissions dropped to safe levels and the ocean is saved. Victory is yours!";
                }
            }
            else
            {
                if (gameOverTitleText != null)
                {
                    gameOverTitleText.text = "<color=#C62828>⚠️ TIME EXPIRED! CLIMATE COLLAPSE!</color>";
                }
                if (gameOverDescText != null)
                {
                    gameOverDescText.text = "10 minutes have passed and the toxic hazards were not contained in time.\nCarbon levels exceeded the critical tipping point. Mission failed!";
                }
            }
        }
    }

    public void RestartGame()
    {
        EcoAudioManager.PlayClick();
        Time.timeScale = 1f; // Zaman akışını normale döndür
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}
