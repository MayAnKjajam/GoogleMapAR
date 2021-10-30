using UnityEngine;
using UnityEngine.UI;

public class OfflineManager : MonoBehaviour
{
    public GameObject fadeIn;
    public float fadeTime = 5f;
    private RawImage fade;
    private float startTime = 0;
    public static bool releaseDisplay = false;

    void Awake()
    {
        fade = fadeIn.GetComponent<RawImage>();
        fade.color = new UnityEngine.Color(fade.color.r, fade.color.g, fade.color.b, 1f);
    }

    void Update()
    {
        if(releaseDisplay && startTime == 0)
            startTime = Time.timeSinceLevelLoad;

        if (releaseDisplay && Time.timeSinceLevelLoad > startTime + 2 && fade.enabled)
        {
            float fadeAmount = 1f - Mathf.InverseLerp(0f, fadeTime, Time.timeSinceLevelLoad - (startTime + 2));
            fade.color = new UnityEngine.Color(fade.color.r, fade.color.g, fade.color.b, fadeAmount);
        }

        if (fade.color.a == 0f)
            fade.enabled = false;
    }
}

