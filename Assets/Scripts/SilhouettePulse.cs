using UnityEngine;

public class SilhouettePulse : MonoBehaviour
{
    private MeshRenderer meshRend;
    private Material runtimeMat;
    private Color baseColor;
    private Color baseEmission;

    private void Start()
    {
        meshRend = GetComponent<MeshRenderer>();
        if (meshRend != null)
        {
            runtimeMat = meshRend.material;
            baseColor = runtimeMat.color;
            baseEmission = runtimeMat.GetColor("_EmissionColor");
        }
    }

    private void Update()
    {
        if (runtimeMat == null) return;

        // Yumuşak nefes alma / parıldama efekti (2.2 rad/s)
        float pulse = Mathf.Sin(Time.time * 2.2f) * 0.18f + 0.82f;
        runtimeMat.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * pulse);
        runtimeMat.SetColor("_EmissionColor", baseEmission * pulse);
    }
}
