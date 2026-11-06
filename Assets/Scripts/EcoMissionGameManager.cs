using UnityEngine;
using System.Collections.Generic;

public class EcoMissionGameManager : MonoBehaviour
{
    public static EcoMissionGameManager Instance { get; private set; }

    [Header("Mission Timer (10 Minutes)")]
    public float timeRemaining = 600f; // 10 dakika = 600 saniye
    public bool isGameOver = false;
    public bool isVictory = false;

    [Header("Cleanliness & Carbon")]
    public int totalZonesCount = 0;
    public int cleanedZonesCount = 0;
    public float carbonPPM = 650f; // Başlangıç kritik kirli karbon

    [Header("All Mission Zones")]
    public List<EcoMissionZone> allZones = new List<EcoMissionZone>();

    [Header("Audio or Effects")]
    public Material oceanMaterial;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        Time.timeScale = 1f;
        FindAndRegisterAllZones();
    }

    private void Update()
    {
        if (isGameOver) return;

        // 10 Dakikalık Geri Sayım
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                TriggerGameOver(false); // Süre bitti, kaybettik!
            }
        }

        UpdateOceanColor();
    }

    public void RegisterZone(EcoMissionZone zone)
    {
        if (!allZones.Contains(zone))
        {
            allZones.Add(zone);
            totalZonesCount = allZones.Count;
        }
    }

    public void FindAndRegisterAllZones()
    {
        allZones.Clear();
        var found = FindObjectsByType<EcoMissionZone>(FindObjectsSortMode.None);
        foreach (var z in found)
        {
            allZones.Add(z);
        }
        totalZonesCount = allZones.Count;
        cleanedZonesCount = 0;
        foreach (var z in allZones)
        {
            if (z.isCleaned) cleanedZonesCount++;
        }
    }

    public void OnZoneCleaned(EcoMissionZone zone)
    {
        if (isGameOver) return;

        cleanedZonesCount++;
        // Her temizlikte karbon düşer
        float step = (650f - 320f) / Mathf.Max(1, totalZonesCount);
        carbonPPM = Mathf.Max(300f, carbonPPM - step);

        // KAZANMA KONTROLÜ: Her şey temizlendi mi?
        if (cleanedZonesCount >= totalZonesCount && totalZonesCount > 0)
        {
            TriggerGameOver(true); // Kazandık!
        }
    }

    private void TriggerGameOver(bool won)
    {
        isGameOver = true;
        isVictory = won;

        // Oyun dünyasını durdur
        Time.timeScale = 0f;

        // Fare imlecini aç
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void UpdateOceanColor()
    {
        if (oceanMaterial != null)
        {
            float progress = totalZonesCount > 0 ? (float)cleanedZonesCount / totalZonesCount : 0f;
            Color dirtyColor = new Color(0.35f, 0.4f, 0.38f, 0.9f);
            Color cleanColor = new Color(0.08f, 0.58f, 0.78f, 0.85f);
            oceanMaterial.color = Color.Lerp(dirtyColor, cleanColor, progress);
        }
    }
}
