using System.Collections;
using UnityEngine;

public class PlantingSpot : MonoBehaviour
{
    [Header("Spot Info")]
    public string spotName = "Fidan Dikme Alanı";
    public bool isPlanted = false;
    public float interactionRadius = 4.5f;

    [Header("Visual Components")]
    public GameObject unplantedVisual;
    public GameObject plantedSapling;

    [Header("Particles")]
    public ParticleSystem dirtFX;
    public ParticleSystem leavesFX;

    private Vector3 originalSaplingScale;

    private void Awake()
    {
        if (plantedSapling != null)
        {
            originalSaplingScale = plantedSapling.transform.localScale;
            plantedSapling.SetActive(isPlanted);
        }

        if (unplantedVisual != null)
        {
            unplantedVisual.SetActive(!isPlanted);
        }
    }

    public bool PlantSapling()
    {
        if (isPlanted) return false;

        isPlanted = true;

        if (unplantedVisual != null)
        {
            unplantedVisual.SetActive(false);
        }

        if (dirtFX != null)
        {
            dirtFX.Stop();
            dirtFX.Play();
        }

        if (leavesFX != null)
        {
            leavesFX.Stop();
            leavesFX.Play();
        }

        if (plantedSapling != null)
        {
            plantedSapling.SetActive(true);
            StartCoroutine(GrowAnimation());
        }

        // SFX: Play organic sapling planting chime
        EcoAudioManager.PlayPlantSapling();

        // Notify EcoMissionGameManager
        if (EcoMissionGameManager.Instance != null)
        {
            EcoMissionGameManager.Instance.carbonPPM = Mathf.Max(350f, EcoMissionGameManager.Instance.carbonPPM - 35f);
        }

        Debug.Log($"[PlantingSpot] 🌱 {spotName} alanına fidan başarıyla dikildi!");
        return true;
    }

    private IEnumerator GrowAnimation()
    {
        float duration = 0.75f;
        float elapsed = 0f;

        plantedSapling.transform.localScale = Vector3.zero;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Elastic bounce ease-out
            float scaleMultiplier = Mathf.Sin(t * Mathf.PI * 0.5f);
            if (t > 0.7f)
            {
                // gentle overshoot bounce
                float bounceT = (t - 0.7f) / 0.3f;
                scaleMultiplier = 1f + Mathf.Sin(bounceT * Mathf.PI) * 0.12f;
            }

            plantedSapling.transform.localScale = originalSaplingScale * scaleMultiplier;
            yield return null;
        }

        plantedSapling.transform.localScale = originalSaplingScale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isPlanted ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
