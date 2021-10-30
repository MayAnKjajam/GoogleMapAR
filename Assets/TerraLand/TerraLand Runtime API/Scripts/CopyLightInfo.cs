using UnityEngine;
using System.Collections;

public class CopyLightInfo : MonoBehaviour
{
    public Light sun;
    private Light sunLight;
    private Light currentLight;

	void Start ()
    {
        sunLight = sun.GetComponent<Light>();
        currentLight = transform.GetComponent<Light>();
	}

	void Update ()
    {
        currentLight.color = sunLight.color;
        currentLight.intensity = sunLight.intensity;
	}
}

