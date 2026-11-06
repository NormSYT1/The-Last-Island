using UnityEngine;

public class EcoMissionZone : MonoBehaviour
{
    [Header("Mission Spot Info")]
    public string zoneName = "Kirlilik ve Yangın Odağı";
    public string description = "Karbon salımı ve aktif yangın";
    public bool isCleaned = false;

    [Header("Realistic Fire & Smoke FX")]
    public ParticleSystem fireEffect;      // Gerçekçi alevler
    public ParticleSystem smokeEffect;     // Yoğun is dumanı
    public ParticleSystem sparksEffect;    // Havaya fırlayan kıvılcımlar
    public Light fireLight;                // Çevreyi aydınlatan titreşen ateş ışığı

    [Header("Extinguish Water & Steam FX")]
    public ParticleSystem waterSplashEffect;   // Su sıçrama ve dökülme efekti
    public ParticleSystem steamEffect;         // Buharlaşma dumanı ("tısss" efekti)

    [Header("Props")]
    public GameObject dirtyProp;           // Kirli / yanan nesne
    public GameObject cleanedProp;         // Temizlenince çıkan fidan / yeşil alan

    private void Start()
    {
        UpdateStateVisuals();
    }

    public void CleanZone()
    {
        if (isCleaned) return;

        isCleaned = true;
        UpdateStateVisuals();

        // Su sıçraması ve söndürme buharı animasyonlarını patlat
        if (waterSplashEffect != null)
        {
            waterSplashEffect.gameObject.SetActive(true);
            waterSplashEffect.Stop();
            waterSplashEffect.Play();
        }

        if (steamEffect != null)
        {
            steamEffect.gameObject.SetActive(true);
            steamEffect.Stop();
            steamEffect.Play();
        }

        // SFX: E etkileşim sesi (Ateş ise söndürme/su/cızırtı, çöp ise temizleme/pop sesi)
        bool isFire = fireEffect != null || waterSplashEffect != null || zoneName.ToLower().Contains("fire") || zoneName.ToLower().Contains("yang");
        if (isFire)
        {
            EcoAudioManager.PlayExtinguish();
        }
        else
        {
            EcoAudioManager.PlayCleanTrash();
        }

        // 10 Dakikalık Görev Yöneticisine bildir
        if (EcoMissionGameManager.Instance != null)
        {
            EcoMissionGameManager.Instance.OnZoneCleaned(this);
        }
    }

    public void UpdateStateVisuals()
    {
        // 1. Alev Efekti
        if (fireEffect != null)
        {
            if (isCleaned) fireEffect.Stop();
            else if (!fireEffect.isPlaying) fireEffect.Play();
        }

        // 2. İs Dumanı Efekti
        if (smokeEffect != null)
        {
            if (isCleaned) smokeEffect.Stop();
            else if (!smokeEffect.isPlaying) smokeEffect.Play();
        }

        // 3. Kıvılcım Efekti
        if (sparksEffect != null)
        {
            if (isCleaned) sparksEffect.Stop();
            else if (!sparksEffect.isPlaying) sparksEffect.Play();
        }

        // 4. Ateş Işığı
        if (fireLight != null)
        {
            fireLight.enabled = !isCleaned;
        }

        // 5. Objelerin Durumu
        if (dirtyProp != null) dirtyProp.SetActive(!isCleaned);
        if (cleanedProp != null) cleanedProp.SetActive(isCleaned);
    }
}
