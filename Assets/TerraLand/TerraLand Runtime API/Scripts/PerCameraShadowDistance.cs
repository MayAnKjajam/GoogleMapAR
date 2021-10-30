using UnityEngine;
using System.Collections;

public class PerCameraShadowDistance : MonoBehaviour
{
    public Light sun;
    public float shadowDistance = 100f;

    [Range(0f,2f)]
    public float shadowBias = 0f;
    [Range(0f,3f)]
    public float shadowNormalBias = 0f;
    [Range(0.1f,10f)]
    public float shadowNearPlane = 0.1f;

    private float mainShadowDistance;
    private float mainShadowBias;
    private float mainShadowNormalBias;
    private float mainShadowNearPlane;

    void Start ()
    {
        //GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
    }

    void OnPreCull ()
    {
        mainShadowDistance = QualitySettings.shadowDistance;
        mainShadowBias = sun.shadowBias;
        mainShadowNormalBias = sun.shadowNormalBias;
        mainShadowNearPlane = sun.shadowNearPlane;

        QualitySettings.shadowDistance = shadowDistance;
        sun.shadowBias = shadowBias;
        sun.shadowNormalBias = shadowNormalBias;
        sun.shadowNearPlane = shadowNearPlane;
    }

    void OnPostRender ()
    {
        QualitySettings.shadowDistance = mainShadowDistance;
        sun.shadowBias = mainShadowBias;
        sun.shadowNormalBias = mainShadowNormalBias;
        sun.shadowNearPlane = mainShadowNearPlane;
    }
}

