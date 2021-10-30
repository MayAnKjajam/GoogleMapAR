using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ARToggle : MonoBehaviour
{

    public GameObject ARCamera;
    public GameObject NormalCamera;
    public Toggle ARMode;
    public GameObject Joystick;
    public TMP_Dropdown Dropdown;
    public GameObject SpawnerParent;
    public Slider VisiblitySlider;
    public GameObject MainCanvas;
    // Start is called before the first frame update
    void Start()
    {
        ARMode.onValueChanged.AddListener(delegate { ModeToggle(); });
        ARCamera = GameObject.Find("AR Camera");
        MainCanvas = GameObject.Find("CanvasM");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void ModeToggle()
    {
        if (ARMode.isOn)
        {
            Dropdown.interactable = false;
            Dropdown.value = 0;
            NormalCamera.SetActive(false);
            ARCamera.SetActive(true);
            Joystick.SetActive(false);
            MainCanvas.SetActive(true);
            RenderSettings.fogStartDistance = 0.1f;
            RenderSettings.fogEndDistance = 1f;
            VisiblitySlider.minValue = 0.1f;
            VisiblitySlider.maxValue = 0.9f;
            VisiblitySlider.value = VisiblitySlider.minValue;
        }
        else
        {
            Dropdown.interactable = true;
           ARCamera.SetActive(false);
            Joystick.SetActive(true); 
            for (int i = 0; i < SpawnerParent.transform.childCount; i++)
            {
                SpawnerParent.transform.GetChild(i).GetComponent<ObjectInformation>().CockPitCamera.SetActive(false);
                SpawnerParent.transform.GetChild(i).GetComponent<CapsuleCollider>().enabled = true;
            }
            RenderSettings.fogStartDistance = 0.1f;
            RenderSettings.fogEndDistance = 1f;
            VisiblitySlider.minValue = 0.1f;
            VisiblitySlider.maxValue = 0.9f;
            VisiblitySlider.value = VisiblitySlider.minValue;
            NormalCamera.SetActive(true);
            MainCanvas.SetActive(false);
        }
    }
}
