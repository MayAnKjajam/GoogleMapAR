using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;


public class FogController : MonoBehaviour
{
    public Slider FogSlider;
    // Start is called before the first frame update
    void Start()
    {
        FogSlider.onValueChanged.AddListener(delegate{Fogsetting();}) ;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Fogsetting()
    {
        RenderSettings.fogStartDistance = FogSlider.value;
    }
}
