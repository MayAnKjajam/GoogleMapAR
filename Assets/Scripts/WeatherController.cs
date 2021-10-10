using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeatherController : MonoBehaviour
{

    public Toggle Sunny, Rainy, Snowy,Fog;
    public GameObject RainObj, SnowObj , FogSlider;
    public GameObject DirectionalLight;

    // Start is called before the first frame update
    void Start()
    {
        RenderSettings.fog = false;
        Sunny.onValueChanged.AddListener(delegate { isSunny(); });
        Rainy.onValueChanged.AddListener(delegate { isRainy(); });
        Snowy.onValueChanged.AddListener(delegate { isSnowy(); });
        Fog.onValueChanged.AddListener(delegate { isFog(); });
        DirectionalLight = GameObject.Find("Directional Light");
    }

    // Update is called once per frame
    void Update()
    {                                                             
        
    }

    void isSunny()
    {
        if (Sunny.isOn)
        {
            Fog.isOn = false;
            Rainy.isOn = false;
            Snowy.isOn = false;
            RainObj.SetActive(false);
            SnowObj.SetActive(false);
            FogSlider.SetActive(false);
            RenderSettings.fog = false;
            DirectionalLight.GetComponent<Light>().intensity = 1f;
        }
        else
        {
            if (!Rainy.isOn && !Snowy.isOn && !Fog.isOn)
            {
                Sunny.isOn = true;
            }
        }
    }
    void isRainy() 
    {
        if (Rainy.isOn)
        {
            Fog.isOn = false;
            Sunny.isOn = false;
            Snowy.isOn = false;
            RainObj.SetActive(true);
            SnowObj.SetActive(false); 
            FogSlider.SetActive(false);
            RenderSettings.fog = false;
            DirectionalLight.GetComponent<Light>().intensity = 0.17f;
        }
        else
        {
            if (!Sunny.isOn && !Snowy.isOn && !Fog.isOn)
            {
                Rainy.isOn = true;
            }
        }
    }
    void isSnowy()
    {
        if (Snowy.isOn)
        {
            Fog.isOn = false;
            Rainy.isOn = false;
            Sunny.isOn = false;
            RainObj.SetActive(false);
            SnowObj.SetActive(true);
            FogSlider.SetActive(false);
            RenderSettings.fog = false;
            DirectionalLight.GetComponent<Light>().intensity = 0.45f;
        }
        else
        {
            if (!Rainy.isOn && !Sunny.isOn && !Fog.isOn)
            {
                Snowy.isOn = true;
            }
        }
    }
    void isFog()
    {
        if (Fog.isOn)
        {
            Rainy.isOn = false;
            Sunny.isOn = false;
            Snowy.isOn = false;
            RainObj.SetActive(false);
            SnowObj.SetActive(false);
            FogSlider.SetActive(true);
            RenderSettings.fog = true;
            DirectionalLight.GetComponent<Light>().intensity = 0.69f;
        }
        else
        {
            if (!Sunny.isOn && !Snowy.isOn && !Rainy.isOn)
            {
                Fog.isOn = true;
            }
        }
    }
}
