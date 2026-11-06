using UnityEngine;

public class FireFlicker : MonoBehaviour
{
    public Light fireLight;
    public float minIntensity = 1.8f;
    public float maxIntensity = 3.6f;
    public float flickerSpeed = 12f;

    private float baseIntensity;
    private float noiseSeed;

    private void Start()
    {
        if (fireLight == null) fireLight = GetComponent<Light>();
        noiseSeed = Random.Range(0f, 100f);
        if (fireLight != null) baseIntensity = fireLight.intensity;
    }

    private void Update()
    {
        if (fireLight == null || !fireLight.enabled) return;

        float noise = Mathf.PerlinNoise(noiseSeed, Time.time * flickerSpeed);
        fireLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}
